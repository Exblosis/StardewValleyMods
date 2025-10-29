using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace LetsMoveIt.TargetData
{
    internal partial class Target
    {
        private static ModConfig Config = null!;
        private static IMonitor Monitor = null!;

        public string? Name;
        //public static string? Index;
        //public static Guid? Guid;
        public bool MarniesLivestock;
        public object? TargetObject;
        public GameLocation TargetLocation = null!;
        public Vector2 TilePosition;
        public Vector2 TileOffset;

        /// <summary>Relative to Render(tile), used by ResourceClump and TerrainFeature.</summary>
        private readonly HashSet<Vector2> BoundingBoxTile = [];

        /// <summary>Create Empty Target</summary>
        public Target() { }

        /// <summary>Create Simple Target</summary>
        public Target(object obj, GameLocation location, Vector2 tile)
        {
            TargetObject = obj;
            TargetLocation = location;
            TilePosition = tile;
        }

        /// <summary>Use in ModEntry.Entry() | Only for set values.</summary>
        public static void Init(ModConfig config, IMonitor monitor)
        {
            Config = config;
            Monitor = monitor;
        }

        public bool IsOccupied(GameLocation location, Vector2 tile)
        {
            if (IsTileOccupied(location, tile))
                return true;

            if (BoundingBoxTile.Count > 0)
            {
                return BoundingBoxTile.Any(t => IsTileOccupied(location, t));
            }
            
            return false;
        }

        private bool IsTileOccupied(GameLocation location, Vector2 tile)
        {
            if (!location.isTilePassable(tile) || !location.isTileOnMap(tile) ||
                location.isTileHoeDirt(tile) || location.isCropAtTile((int)tile.X, (int)tile.Y) ||
                location.IsTileBlockedBy(tile, ignorePassables: CollisionMask.All))
            {
                if (TargetObject is Crop && location.isTileHoeDirt(tile))
                    return false;

                if (TargetObject is SObject sObject && sObject.IsTapper())
                {
                    if (location.terrainFeatures.TryGetValue(tile, out var tf) && tf is Tree)
                        return false;
                    return true;
                }

                return true;
            }
            return false;
        }
    }
}
