using System;
using System.Collections.Generic;
using System.Linq;
using LetsMoveIt.TargetData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;

namespace LetsMoveIt
{
    /// <summary>The mod entry point.</summary>
    internal class ModEntry : Mod
    {
        private ModConfig Config = null!; // Initialized in Entry()

        private const int ToolbarMessageHeight = 140;
        private string ToolbarMessage = string.Empty;
        private Vector2 Bounds;

        private readonly Dictionary<Vector2, List<Target>> MultipleTargets = [];
        private Target? SingleTarget;

        private bool Select = false;
        private Vector2 StartCursorTile;
        private Rectangle SelectedArea = Rectangle.Empty;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = helper.ReadConfig<ModConfig>();
            I18n.Init(helper.Translation);
            Target.Init(Config, Monitor);

            if (!Config.ModEnabled)
                return;
            
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.Display.MenuChanged += OnMenuChanged;
            helper.Events.Player.Warped += OnWarped;
            helper.Events.Display.RenderingHud += OnRenderingHud;
            helper.Events.Display.RenderedWorld += OnRenderedWorld;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.Input.ButtonReleased += OnButtonReleased;
        }

        private void OnRenderedWorld(object? sender, RenderedWorldEventArgs e)
        {
            if (!Config.ModEnabled)
            {
                ClearSelection();
                return;
            }
            if (Config.ModKey.IsDown() && Select && Config.MultiSelect)
            {
                Game1.player.canOnlyWalk = true;
                SelectedArea = new Rectangle(
                    (int)Math.Min(Game1.currentCursorTile.X, StartCursorTile.X),
                    (int)Math.Min(Game1.currentCursorTile.Y, StartCursorTile.Y),
                    (int)Math.Abs(Game1.currentCursorTile.X - StartCursorTile.X) + 1,
                    (int)Math.Abs(Game1.currentCursorTile.Y - StartCursorTile.Y) + 1);

                Vector2 basePosition = Game1.GlobalToLocal(new Vector2(SelectedArea.X, SelectedArea.Y) * Game1.tileSize);
                for (int x_offset = 0; x_offset < SelectedArea.Width; x_offset++)
                {
                    for (int y_offset = 0; y_offset < SelectedArea.Height; y_offset++)
                    {
                        Vector2 position = basePosition + new Vector2(x_offset, y_offset) * Game1.tileSize;
                        e.SpriteBatch.Draw(Game1.mouseCursors, position, new Rectangle(194, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1);
                    }
                }
            }
            if (SingleTarget is not null)
            {
                if (SingleTarget.TargetObject is null)
                {
                    SingleTarget = null;
                    return;
                }
                SingleTarget.Render(e.SpriteBatch, Game1.currentLocation, Game1.currentCursorTile);
            }
            if (MultipleTargets.Count > 0)
            {
                foreach (var targetList in MultipleTargets.Values)
                {
                    foreach (var t in targetList)
                    {
                        if (t.TargetObject is null)
                            continue;
                        Vector2 renderTile = Game1.currentCursorTile + (t.TilePosition - new Vector2(SelectedArea.X, SelectedArea.Y));
                        t.Render(e.SpriteBatch, Game1.currentLocation, renderTile);
                    }
                }
            }
        }

        private void OnRenderingHud(object? sender, RenderingHudEventArgs e)
        {
            if (SingleTarget is not null || MultipleTargets.Count > 0)
            {
                Vector2 msgPosition = new(Game1.uiViewport.Width / 2 - Bounds.X / 2, Game1.uiViewport.Height - ToolbarMessageHeight);
                Utility.drawTextWithColoredShadow(e.SpriteBatch, ToolbarMessage, Game1.smallFont, msgPosition, Color.White, Color.Black);
            }
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (!Context.IsPlayerFree && Game1.activeClickableMenu is not CarpenterMenu)
                return;
            if (Config.DisableOnEvent && Game1.eventUp)
                return;
            if (ToggleKey())
                return;
            if (SingleTarget is not null || MultipleTargets.Count > 0)
            {
                if (Config.CancelKey.JustPressed())
                {
                    Helper.Input.Suppress(e.Button);
                    ClearSelection();
                    PlaySound();
                    return;
                }
                if (Config.RemoveKey.JustPressed())
                {
                    Helper.Input.Suppress(e.Button);
                    RemoveTargetAction();
                    return;
                }
            }
            if (Config.MoveKey.JustPressed())
            {
                if (Config.ModKey.IsDown())
                {
                    SelectTargetAction(e);
                    return;
                }
                if (SingleTarget is not null)
                {
                    SingleTargetAction(e);
                }
                if (MultipleTargets.Count > 0)
                {
                    MultipleTargetsAction(e);
                }
            }
        }

        private void OnButtonReleased(object? sender, ButtonReleasedEventArgs e)
        {
            if (Config.MoveKey.GetState() == SButtonState.Released)
            {
                Select = false;
                if (Config.ModKey.IsDown() && Config.MultiSelect)
                {
                    var seenObjects = new HashSet<object>();
                    for (int x = 0; x < SelectedArea.Width; x++)
                    {
                        for (int y = 0; y < SelectedArea.Height; y++)
                        {
                            Vector2 tile = new(SelectedArea.X + x, SelectedArea.Y + y);

                            // Sammle alle Targets auf diesem Tile
                            List<Target>? targetsOnTile = Target.GetAllTargets(Game1.currentLocation, tile, Game1.GlobalToLocal(tile).ToPoint());
                            if (targetsOnTile is not null && targetsOnTile.Count > 0)
                            {
                                if (!MultipleTargets.TryGetValue(tile, out var list))
                                {
                                    list = [];
                                    MultipleTargets[tile] = list;
                                }
                                foreach (var t in targetsOnTile)
                                {
                                    // Nur hinzufügen, wenn das selbe Objekt nicht doppelt vorkommt
                                    if (t.TargetObject is not null && seenObjects.Add(t.TargetObject))
                                        list.Add(t);
                                }
                            }
                        }
                    }
                    if (MultipleTargets.Count > 0)
                    {
                        var allKeys = MultipleTargets.Keys;
                        int minX = (int)allKeys.Min(k => k.X);
                        int minY = (int)allKeys.Min(k => k.Y);
                        int maxX = (int)allKeys.Max(k => k.X);
                        int maxY = (int)allKeys.Max(k => k.Y);
                        SelectedArea = new Rectangle(minX, minY, maxX - minX, maxY - minY);
                        PlaySound();
                    }
                }
            }
        }

        private bool ToggleKey()
        {
            if (Config.ToggleCopyModeKey.JustPressed())
            {
                Config.CopyMode = Config.CopyMode.Toggle();
                return true;
            }
            if (Config.ToggleMultiSelectKey.JustPressed())
            {
                Config.MultiSelect = Config.MultiSelect.Toggle();
                return true;
            }
            if (Config.ToggleCropTileKey.JustPressed())
            {
                Config.MoveCropWithoutTile = Config.MoveCropWithoutTile.Toggle();
                return true;
            }
            if (Config.ToggleCropPotKey.JustPressed())
            {
                Config.MoveCropWithoutIndoorPot = Config.MoveCropWithoutIndoorPot.Toggle();
                return true;
            }
            return false;
        }

        private void RemoveTargetAction()
        {
            string select = I18n.Dialogue("Remove.Select1");
            if (SingleTarget?.TargetObject is Character)
                select = I18n.Dialogue("Remove.Select2");
            if (SingleTarget?.TargetObject is Building)
                select = I18n.Dialogue("Remove.Select3");
            Game1.player.currentLocation.createQuestionDialogue(I18n.Dialogue("Remove", new { select }), Mod1.YesNoResponses(), RemoveDialogAction);
        }

        private void RemoveDialogAction(Farmer f, string response)
        {
            if (response != "Yes")
                return;

            SingleTarget?.Remove();
            foreach (var targetList in MultipleTargets.Values)
            {
                foreach (var t in targetList)
                {
                    if (t.TargetObject is not null)
                        t.Remove();
                }
            }
            ClearSelection();
            Game1.playSound("trashcan");
        }

        private void SelectTargetAction(ButtonPressedEventArgs e)
        {
            // toolbar message and bounds
            ToolbarMessage = $"{Config.CancelKey} {I18n.Message("Info.Cancel")} | {Config.OverwriteKey} {I18n.Message("Info.Force")} | {Config.RemoveKey} {I18n.Message("Info.Remove")}";
            Bounds = Game1.smallFont.MeasureString(ToolbarMessage);

            Game1.player.canOnlyWalk = true;
            ClearSelection();
            if (Config.MultiSelect)
            {
                Select = true;
                StartCursorTile = Game1.currentCursorTile;
            }
            else
            {
                Helper.Input.Suppress(e.Button);
                SingleTarget = Target.Get(Game1.currentLocation, Game1.currentCursorTile, Mod1.GetGlobalMousePosition());
                if (SingleTarget?.TargetObject is null)
                {
                    SingleTarget = null;
                    return;
                }
                PlaySound();
            }
        }

        private void SingleTargetAction(ButtonPressedEventArgs e)
        {
            Helper.Input.Suppress(e.Button);
            bool overwriteTile = Config.OverwriteKey.IsDown();

            if (SingleTarget!.IsOccupied(Game1.currentLocation, Game1.currentCursorTile) && !overwriteTile)
            {
                Game1.playSound("cancel");
                return;
            }

            if (Config.CopyMode)
            {
                SingleTarget.CopyTo(Game1.currentLocation, Game1.currentCursorTile, overwriteTile);
            }
            else
            {
                SingleTarget.MoveTo(Game1.currentLocation, Game1.currentCursorTile, overwriteTile);
                PlaySound();
            }
        }

        private void MultipleTargetsAction(ButtonPressedEventArgs e)
        {
            Helper.Input.Suppress(e.Button);
            bool overwriteTile = Config.OverwriteKey.IsDown();

            foreach (var key in MultipleTargets.Keys.ToList())
            {
                var targetList = MultipleTargets[key];
                var toRemove = new List<Target>();

                foreach (var t in targetList)
                {
                    if (t.TargetObject is null)
                        continue;

                    Vector2 targetTile = Game1.currentCursorTile + (t.TilePosition - new Vector2(SelectedArea.X, SelectedArea.Y));

                    if (t.IsOccupied(Game1.currentLocation, targetTile) && !overwriteTile)
                        continue;

                    if (Config.CopyMode)
                    {
                        t.CopyTo(Game1.currentLocation, targetTile, overwriteTile);
                    }
                    else
                    {
                        t.MoveTo(Game1.currentLocation, targetTile, overwriteTile);
                    }

                    if (t.TargetObject is null)
                        toRemove.Add(t);
                }

                foreach (var t in toRemove)
                    targetList.Remove(t);
            }

            var emptyKeys = MultipleTargets
                .Where(pair => pair.Value.Count == 0)
                .Select(pair => pair.Key)
                .ToList();

            foreach (var key in emptyKeys)
                MultipleTargets.Remove(key);

            PlaySound();
        }

        private void ClearSelection()
        {
            MultipleTargets.Clear();
            SingleTarget = null;
            SelectedArea = Rectangle.Empty;
            Select = false;
        }

        public void PlaySound()
        {
            if (!string.IsNullOrEmpty(Config.Sound))
                Game1.playSound(Config.Sound);
        }

        private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
        {
            if (Game1.activeClickableMenu is null)
                return;
            if (Game1.activeClickableMenu is DialogueBox)
                return;
            ClearSelection();
        }

        private void OnWarped(object? sender, WarpedEventArgs e)
        {
            if (Config.DisableOnEvent && Game1.eventUp)
                ClearSelection();
        }

        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            ClearSelection();
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );
            // Config
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("ModEnabled"),
                tooltip: () => I18n.Config("ModEnabled.Tooltip"),
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("DisableOnEvent"),
                getValue: () => Config.DisableOnEvent,
                setValue: value => Config.DisableOnEvent = value
            );
            configMenu.AddKeybindList(
                mod: ModManifest,
                name: () => I18n.Config("ModKey"),
                getValue: () => Config.ModKey,
                setValue: value => Config.ModKey = value
            );
            configMenu.AddKeybindList(
                mod: ModManifest,
                name: () => I18n.Config("MoveKey"),
                getValue: () => Config.MoveKey,
                setValue: value => Config.MoveKey = value
            );
            configMenu.AddKeybindList(
                mod: ModManifest,
                name: () => I18n.Config("OverwriteKey"),
                tooltip: () => I18n.Config("OverwriteKey.Tooltip"),
                getValue: () => Config.OverwriteKey,
                setValue: value => Config.OverwriteKey = value
            );
            configMenu.AddKeybindList(
                mod: ModManifest,
                name: () => I18n.Config("CancelKey"),
                getValue: () => Config.CancelKey,
                setValue: value => Config.CancelKey = value
            );
            configMenu.AddKeybindList(
                mod: ModManifest,
                name: () => I18n.Config("RemoveKey"),
                getValue: () => Config.RemoveKey,
                setValue: value => Config.RemoveKey = value
            );
            configMenu.AddKeybindList(
                mod: ModManifest,
                name: () => I18n.Config("ToggleCopyModeKey"),
                getValue: () => Config.ToggleCopyModeKey,
                setValue: value => Config.ToggleCopyModeKey = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("CopyMode"),
                getValue: () => Config.CopyMode,
                setValue: value => Config.CopyMode = value
            );
            configMenu.AddKeybindList(
                mod: ModManifest,
                name: () => I18n.Config("ToggleMultiSelectKey"),
                getValue: () => Config.ToggleMultiSelectKey,
                setValue: value => Config.ToggleMultiSelectKey = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("MultiSelect"),
                tooltip: () => I18n.Config("MultiSelect.Tooltip"),
                getValue: () => Config.MultiSelect,
                setValue: value => Config.MultiSelect = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => I18n.Config("Sound"),
                getValue: () => Config.Sound,
                setValue: value => Config.Sound = value
            );
            // Prioritize Crops
            configMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => I18n.Config("PrioritizeCrops")
            );
            configMenu.AddKeybindList(
                mod: ModManifest,
                name: () => I18n.Config("ToggleCropTileKey"),
                getValue: () => Config.ToggleCropTileKey,
                setValue: value => Config.ToggleCropTileKey = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("MoveCropWithoutTile"),
                getValue: () => Config.MoveCropWithoutTile,
                setValue: value => Config.MoveCropWithoutTile = value
            );
            configMenu.AddKeybindList(
                mod: ModManifest,
                name: () => I18n.Config("ToggleCropPotKey"),
                getValue: () => Config.ToggleCropPotKey,
                setValue: value => Config.ToggleCropPotKey = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("MoveCropWithoutIndoorPot"),
                getValue: () => Config.MoveCropWithoutIndoorPot,
                setValue: value => Config.MoveCropWithoutIndoorPot = value
            );
            // Enable & Disable Components Page
            configMenu.AddPageLink(
                mod: ModManifest,
                pageId: "Components",
                text: () => I18n.Config("Page.Components.Link"),
                tooltip: () => I18n.Config("Page.Components.Link.Tooltip")
            );
            configMenu.AddPage(
                mod: ModManifest,
                pageId: "Components",
                pageTitle: () => I18n.Config("Page.Components.Title")
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("EnableMoveBuilding"),
                getValue: () => Config.EnableMoveBuilding,
                setValue: value => Config.EnableMoveBuilding = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("EnableMoveEntity"),
                tooltip: () => I18n.Config("EnableMoveEntity.Tooltip"),
                getValue: () => Config.EnableMoveEntity,
                setValue: value => Config.EnableMoveEntity = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("EnableMoveCrop"),
                getValue: () => Config.EnableMoveCrop,
                setValue: value => Config.EnableMoveCrop = value
            );

            configMenu.AddParagraph(
                mod: ModManifest,
                text: () => "________________" // SPACE
            );
            // Objects
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("EnableMoveObject"),
                getValue: () => Config.EnableMoveObject,
                setValue: value => Config.EnableMoveObject = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("EnableMoveFurniture"),
                getValue: () => Config.EnableMoveFurniture,
                setValue: value => Config.EnableMoveFurniture = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("EnableMovePlaceableObject"),
                tooltip: () => I18n.Config("EnableMovePlaceableObject.Tooltip"),
                getValue: () => Config.EnableMovePlaceableObject,
                setValue: value => Config.EnableMovePlaceableObject = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("EnableMoveCollectibleObject"),
                tooltip: () => I18n.Config("EnableMoveCollectibleObject.Tooltip"),
                getValue: () => Config.EnableMoveCollectibleObject,
                setValue: value => Config.EnableMoveCollectibleObject = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("EnableMoveGeneratedObject"),
                tooltip: () => I18n.Config("EnableMoveGeneratedObject.Tooltip"),
                getValue: () => Config.EnableMoveGeneratedObject,
                setValue: value => Config.EnableMoveGeneratedObject = value
            );

            configMenu.AddParagraph(
                mod: ModManifest,
                text: () => "________________" // SPACE
            );
            // Resource Clumps
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("EnableMoveResourceClump"),
                getValue: () => Config.EnableMoveResourceClump,
                setValue: value => Config.EnableMoveResourceClump = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("EnableMoveGiantCrop"),
                getValue: () => Config.EnableMoveGiantCrop,
                setValue: value => Config.EnableMoveGiantCrop = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("EnableMoveStump"),
                getValue: () => Config.EnableMoveStump,
                setValue: value => Config.EnableMoveStump = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("EnableMoveHollowLog"),
                getValue: () => Config.EnableMoveHollowLog,
                setValue: value => Config.EnableMoveHollowLog = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("EnableMoveBoulder"),
                getValue: () => Config.EnableMoveBoulder,
                setValue: value => Config.EnableMoveBoulder = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("EnableMoveMeteorite"),
                getValue: () => Config.EnableMoveMeteorite,
                setValue: value => Config.EnableMoveMeteorite = value
            );

            configMenu.AddParagraph(
                mod: ModManifest,
                text: () => "________________" // SPACE
            );
            // Terrain Features
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("EnableMoveTerrainFeature"),
                getValue: () => Config.EnableMoveTerrainFeature,
                setValue: value => Config.EnableMoveTerrainFeature = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("EnableMoveFlooring"),
                getValue: () => Config.EnableMoveFlooring,
                setValue: value => Config.EnableMoveFlooring = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("EnableMoveTree"),
                getValue: () => Config.EnableMoveTree,
                setValue: value => Config.EnableMoveTree = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("EnableMoveFruitTree"),
                getValue: () => Config.EnableMoveFruitTree,
                setValue: value => Config.EnableMoveFruitTree = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("EnableMoveGrass"),
                getValue: () => Config.EnableMoveGrass,
                setValue: value => Config.EnableMoveGrass = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("EnableMoveFarmland"),
                getValue: () => Config.EnableMoveFarmland,
                setValue: value => Config.EnableMoveFarmland = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Config("EnableMoveBush"),
                getValue: () => Config.EnableMoveBush,
                setValue: value => Config.EnableMoveBush = value
            );
        }
    }
}
