using StardewModdingAPI;

namespace LetsMoveIt
{
    internal static class ConfigUtils
    {
        /// <summary>Die aktive Konfiguration. Wird in Entry() geladen, danach global lesbar.</summary>
        public static ModConfig Config { get; private set; } = null!;

        /// <summary>Lädt die Config von der Festplatte. Einmalig in Entry() aufrufen.</summary>
        public static void Load(IModHelper helper)
        {
            Config = helper.ReadConfig<ModConfig>();
        }

        /// <summary>Registriert die Config bei Generic Mod Config Menu, falls installiert.</summary>
        public static void RegisterMenu(IModHelper helper, IManifest modManifest)
        {
            var configMenu = helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;
            // Register mod
            configMenu.Register(
                mod: modManifest,
                reset: () => Config = new ModConfig(),
                save: () => helper.WriteConfig(Config)
            );
            // Config
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => I18n.Config("ModEnabled"),
                tooltip: () => I18n.Config("ModEnabled.Tooltip"),
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => I18n.Config("DisableOnEvent"),
                getValue: () => Config.DisableOnEvent,
                setValue: value => Config.DisableOnEvent = value
            );
            configMenu.AddKeybindList(
                mod: modManifest,
                name: () => I18n.Config("ModMenuKey"),
                getValue: () => Config.ModMenuKey,
                setValue: value => Config.ModMenuKey = value
            );
            configMenu.AddKeybindList(
                mod: modManifest,
                name: () => I18n.Config("ModKey"),
                getValue: () => Config.ModKey,
                setValue: value => Config.ModKey = value
            );
            configMenu.AddKeybindList(
                mod: modManifest,
                name: () => I18n.Config("MoveKey"),
                getValue: () => Config.MoveKey,
                setValue: value => Config.MoveKey = value
            );
            configMenu.AddKeybindList(
                mod: modManifest,
                name: () => I18n.Config("OverwriteKey"),
                tooltip: () => I18n.Config("OverwriteKey.Tooltip"),
                getValue: () => Config.OverwriteKey,
                setValue: value => Config.OverwriteKey = value
            );
            configMenu.AddKeybindList(
                mod: modManifest,
                name: () => I18n.Config("CancelKey"),
                getValue: () => Config.CancelKey,
                setValue: value => Config.CancelKey = value
            );
            configMenu.AddKeybindList(
                mod: modManifest,
                name: () => I18n.Config("RemoveKey"),
                getValue: () => Config.RemoveKey,
                setValue: value => Config.RemoveKey = value
            );
            configMenu.AddKeybindList(
                mod: modManifest,
                name: () => I18n.Config("ToggleCopyModeKey"),
                getValue: () => Config.ToggleCopyModeKey,
                setValue: value => Config.ToggleCopyModeKey = value
            );
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => I18n.Config("CopyMode"),
                getValue: () => Config.CopyMode,
                setValue: value => Config.CopyMode = value
            );
            configMenu.AddKeybindList(
                mod: modManifest,
                name: () => I18n.Config("ToggleMultiSelectKey"),
                getValue: () => Config.ToggleMultiSelectKey,
                setValue: value => Config.ToggleMultiSelectKey = value
            );
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => I18n.Config("MultiSelect"),
                tooltip: () => I18n.Config("MultiSelect.Tooltip"),
                getValue: () => Config.MultiSelect,
                setValue: value => Config.MultiSelect = value
            );
            configMenu.AddTextOption(
                mod: modManifest,
                name: () => I18n.Config("Sound"),
                getValue: () => Config.Sound,
                setValue: value => Config.Sound = value
            );
            // Prioritize Crops
            configMenu.AddSectionTitle(
                mod: modManifest,
                text: () => I18n.Config("PrioritizeCrops")
            );
            configMenu.AddKeybindList(
                mod: modManifest,
                name: () => I18n.Config("ToggleCropTileKey"),
                getValue: () => Config.ToggleCropTileKey,
                setValue: value => Config.ToggleCropTileKey = value
            );
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => I18n.Config("MoveCropWithoutTile"),
                getValue: () => Config.MoveCropWithoutTile,
                setValue: value => Config.MoveCropWithoutTile = value
            );
            configMenu.AddKeybindList(
                mod: modManifest,
                name: () => I18n.Config("ToggleCropPotKey"),
                getValue: () => Config.ToggleCropPotKey,
                setValue: value => Config.ToggleCropPotKey = value
            );
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => I18n.Config("MoveCropWithoutIndoorPot"),
                getValue: () => Config.MoveCropWithoutIndoorPot,
                setValue: value => Config.MoveCropWithoutIndoorPot = value
            );
            // Enable & Disable Components Page
            configMenu.AddPageLink(
                mod: modManifest,
                pageId: "Components",
                text: () => I18n.Config("Page.Components.Link"),
                tooltip: () => I18n.Config("Page.Components.Link.Tooltip")
            );
            configMenu.AddPage(
                mod: modManifest,
                pageId: "Components",
                pageTitle: () => I18n.Config("Page.Components.Title")
            );
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => I18n.Config("EnableMoveBuilding"),
                getValue: () => Config.EnableMoveBuilding,
                setValue: value => Config.EnableMoveBuilding = value
            );
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => I18n.Config("EnableMoveEntity"),
                tooltip: () => I18n.Config("EnableMoveEntity.Tooltip"),
                getValue: () => Config.EnableMoveEntity,
                setValue: value => Config.EnableMoveEntity = value
            );
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => I18n.Config("EnableMoveCrop"),
                getValue: () => Config.EnableMoveCrop,
                setValue: value => Config.EnableMoveCrop = value
            );

            configMenu.AddParagraph(
                mod: modManifest,
                text: () => "________________" // SPACE
            );
            // Objects
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => I18n.Config("EnableMoveObject"),
                getValue: () => Config.EnableMoveObject,
                setValue: value => Config.EnableMoveObject = value
            );
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => I18n.Config("EnableMoveFurniture"),
                getValue: () => Config.EnableMoveFurniture,
                setValue: value => Config.EnableMoveFurniture = value
            );
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => I18n.Config("EnableMovePlaceableObject"),
                tooltip: () => I18n.Config("EnableMovePlaceableObject.Tooltip"),
                getValue: () => Config.EnableMovePlaceableObject,
                setValue: value => Config.EnableMovePlaceableObject = value
            );
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => I18n.Config("EnableMoveCollectibleObject"),
                tooltip: () => I18n.Config("EnableMoveCollectibleObject.Tooltip"),
                getValue: () => Config.EnableMoveCollectibleObject,
                setValue: value => Config.EnableMoveCollectibleObject = value
            );
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => I18n.Config("EnableMoveGeneratedObject"),
                tooltip: () => I18n.Config("EnableMoveGeneratedObject.Tooltip"),
                getValue: () => Config.EnableMoveGeneratedObject,
                setValue: value => Config.EnableMoveGeneratedObject = value
            );

            configMenu.AddParagraph(
                mod: modManifest,
                text: () => "________________" // SPACE
            );
            // Resource Clumps
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => I18n.Config("EnableMoveResourceClump"),
                getValue: () => Config.EnableMoveResourceClump,
                setValue: value => Config.EnableMoveResourceClump = value
            );
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => I18n.Config("EnableMoveGiantCrop"),
                getValue: () => Config.EnableMoveGiantCrop,
                setValue: value => Config.EnableMoveGiantCrop = value
            );
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => I18n.Config("EnableMoveStump"),
                getValue: () => Config.EnableMoveStump,
                setValue: value => Config.EnableMoveStump = value
            );
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => I18n.Config("EnableMoveHollowLog"),
                getValue: () => Config.EnableMoveHollowLog,
                setValue: value => Config.EnableMoveHollowLog = value
            );
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => I18n.Config("EnableMoveBoulder"),
                getValue: () => Config.EnableMoveBoulder,
                setValue: value => Config.EnableMoveBoulder = value
            );
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => I18n.Config("EnableMoveMeteorite"),
                getValue: () => Config.EnableMoveMeteorite,
                setValue: value => Config.EnableMoveMeteorite = value
            );

            configMenu.AddParagraph(
                mod: modManifest,
                text: () => "________________" // SPACE
            );
            // Terrain Features
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => I18n.Config("EnableMoveTerrainFeature"),
                getValue: () => Config.EnableMoveTerrainFeature,
                setValue: value => Config.EnableMoveTerrainFeature = value
            );
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => I18n.Config("EnableMoveFlooring"),
                getValue: () => Config.EnableMoveFlooring,
                setValue: value => Config.EnableMoveFlooring = value
            );
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => I18n.Config("EnableMoveTree"),
                getValue: () => Config.EnableMoveTree,
                setValue: value => Config.EnableMoveTree = value
            );
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => I18n.Config("EnableMoveFruitTree"),
                getValue: () => Config.EnableMoveFruitTree,
                setValue: value => Config.EnableMoveFruitTree = value
            );
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => I18n.Config("EnableMoveGrass"),
                getValue: () => Config.EnableMoveGrass,
                setValue: value => Config.EnableMoveGrass = value
            );
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => I18n.Config("EnableMoveFarmland"),
                getValue: () => Config.EnableMoveFarmland,
                setValue: value => Config.EnableMoveFarmland = value
            );
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => I18n.Config("EnableMoveBush"),
                getValue: () => Config.EnableMoveBush,
                setValue: value => Config.EnableMoveBush = value
            );
        }
    }
}
