using System;
using LetsMoveIt.Menus;
using LetsMoveIt.SelectionUtils;
using LetsMoveIt.TargetData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using static LetsMoveIt.ConfigUtils;

namespace LetsMoveIt
{
    /// <summary>The mod entry point.</summary>
    internal class ModEntry : Mod
    {
        private const int ToolbarMessageHeight = 140;
        private bool Select = false;
        private Vector2 StartCursorTile;
        private Rectangle SelectedArea = Rectangle.Empty;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            ConfigUtils.Load(helper);
            I18n.Init(helper.Translation);
            Target.Init(Monitor);
            //Selection.Init(helper.Data);

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
                return;

            if (Config.ModKey.IsDown() && !Config.MultiSelect && Config.EnableMoveEntity)
            {
                foreach (var c in Game1.currentLocation.characters)
                {
                    Mod1.DrawBoundingBox(e.SpriteBatch, c.GetBoundingBox(), Color.White);
                }
                foreach (var c in Game1.currentLocation.animals.Values)
                {
                    Mod1.DrawBoundingBox(e.SpriteBatch, c.GetBoundingBox(), Color.White);
                }
                if (Game1.currentLocation is Forest forest)
                {
                    foreach (var c in forest.marniesLivestock)
                    {
                        Mod1.DrawBoundingBox(e.SpriteBatch, c.GetBoundingBox(), Color.White);
                    }
                }
                Mod1.DrawBoundingBox(e.SpriteBatch, Game1.player.GetBoundingBox(), Color.White);
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

            Selection.Draw(e.SpriteBatch, Game1.currentLocation, Game1.currentCursorTile);
        }

        private void OnRenderingHud(object? sender, RenderingHudEventArgs e)
        {
            if (Selection.CurrentSelection is null)
                return;

            string toolbarMessage = $"{Config.CancelKey} {I18n.Message("Info.Cancel")} | {Config.OverwriteKey} {I18n.Message("Info.Force")} | {Config.RemoveKey} {I18n.Message("Info.Remove")}";
            Vector2 bounds = Game1.smallFont.MeasureString(toolbarMessage);
            Vector2 msgPosition = new(Game1.uiViewport.Width / 2 - bounds.X / 2, Game1.uiViewport.Height - ToolbarMessageHeight);
            Utility.drawTextWithColoredShadow(e.SpriteBatch, toolbarMessage, Game1.smallFont, msgPosition, Color.White, Color.Black);
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (Config.ModMenuKey.JustPressed())
            {
                if (Game1.activeClickableMenu is ModMenu modMenu)
                {
                    modMenu.Exit();
                }
                else Game1.activeClickableMenu ??= new ModMenu();
                return;
            }
            if (!Context.IsPlayerFree && Game1.activeClickableMenu is not CarpenterMenu)
                return;
            if (Config.DisableOnEvent && Game1.eventUp)
                return;
            if (ToggleKey())
                return;

            if (Selection.CurrentSelection is not null)
            {
                if (Config.CancelKey.JustPressed())
                {
                    Helper.Input.Suppress(e.Button);
                    ClearSelection();
                    Mod1.PlaySound();
                    return;
                }
                if (Config.RemoveKey.JustPressed())
                {
                    Helper.Input.Suppress(e.Button);
                    RemoveDialogue();
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
                if (Selection.CurrentSelection is not null)
                {
                    Helper.Input.Suppress(e.Button);
                    Selection.CurrentSelection.TargetAction();
                }
                return;
            }
        }

        private void OnButtonReleased(object? sender, ButtonReleasedEventArgs e)
        {
            if (Config.MoveKey.GetState() == SButtonState.Released)
            {
                Select = false;
                if (Config.ModKey.IsDown() && Config.MultiSelect)
                {
                    Selection.TrySelect(Game1.currentLocation, SelectedArea, true);
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

        private void RemoveDialogue()
        {
            string select = I18n.Dialogue("Remove.Select1");
            if (Selection.CurrentSelection?.SingleTarget?.TargetObject is Character)
                select = I18n.Dialogue("Remove.Select2");
            if (Selection.CurrentSelection?.SingleTarget?.TargetObject is Building)
                select = I18n.Dialogue("Remove.Select3");
            Game1.player.currentLocation.createQuestionDialogue(I18n.Dialogue("Remove", new { select }), Mod1.YesNoResponses(), RemoveDialogAction);
        }

        private void RemoveDialogAction(Farmer f, string response)
        {
            if (response != "Yes")
                return;

            Selection.CurrentSelection?.RemoveTargetAction();
            
            ClearSelection();
            Game1.playSound("trashcan");
        }

        private void SelectTargetAction(ButtonPressedEventArgs e)
        {
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
                Selection.TrySelect(Game1.currentLocation, Game1.currentCursorTile.ToRectangle(), false, Config.EnableMoveEntity);
            }
        }

        private void ClearSelection()
        {
            Selection.ClearActiveSelection();
            SelectedArea = Rectangle.Empty;
            Select = false;
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
            ConfigUtils.RegisterMenu(Helper, ModManifest);
        }
    }
}
