using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace LetsMoveIt
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool CopyMode { get; set; } = false;
        public bool MultiSelect { get; set; } = false;
        public bool DisableOnEvent { get; set; } = true;

        //Switch Components
        public bool MoveCropWithoutTile { get; set; } = true;
        public bool MoveCropWithoutIndoorPot { get; set; } = false;

        //Enable|Disable Components
        public bool EnableMoveBuilding { get; set; } = true;
        public bool EnableMoveEntity { get; set; } = true;
        public bool EnableMoveCrop { get; set; } = true;
        //Objects
        public bool EnableMoveObject { get; set; } = true;
        public bool EnableMovePlaceableObject { get; set; } = true;
        public bool EnableMoveCollectibleObject { get; set; } = true;
        public bool EnableMoveGeneratedObject { get; set; } = true;
        //Resource Clumps
        public bool EnableMoveResourceClump { get; set; } = true;
        public bool EnableMoveGiantCrop { get; set; } = true;
        public bool EnableMoveStump { get; set; } = true;
        public bool EnableMoveHollowLog { get; set; } = true;
        public bool EnableMoveBoulder { get; set; } = true;
        public bool EnableMoveMeteorite { get; set; } = true;
        //Terrain Features
        public bool EnableMoveTerrainFeature { get; set; } = true;
        public bool EnableMoveFlooring { get; set; } = true;
        public bool EnableMoveTree { get; set; } = true;
        public bool EnableMoveFruitTree { get; set; } = true;
        public bool EnableMoveGrass { get; set; } = true;
        public bool EnableMoveFarmland { get; set; } = true;
        public bool EnableMoveBush { get; set; } = true;

        //Sound
        public string Sound { get; set; } = "shwip";

        //Keybinding
        public KeybindList ModKey { get; set; } = new(SButton.LeftAlt);
        public KeybindList MoveKey { get; set; } = new(SButton.MouseLeft);
        public KeybindList OverwriteKey { get; set; } = new(SButton.LeftControl);
        public KeybindList CancelKey { get; set; } = new(SButton.Escape);
        public KeybindList RemoveKey { get; set; } = new(SButton.Delete);
        public KeybindList ToggleCopyModeKey { get; set; } = new(SButton.None);
        public KeybindList ToggleMultiSelectKey { get; set; } = new(SButton.None);
        public KeybindList ToggleCropTileKey { get; set; } = new(SButton.None);
        public KeybindList ToggleCropPotKey { get; set; } = new(SButton.None);
    }
}
