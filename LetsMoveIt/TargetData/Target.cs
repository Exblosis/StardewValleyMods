using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace LetsMoveIt.TargetData
{
    internal partial class Target
    {
        private static ModConfig Config = null!;
        private static IModHelper Helper = null!;
        private static IMonitor Monitor = null!;

        public string? Name;
        //public static string? Index;
        //public static Guid? Guid;
        public bool MarniesLivestock;
        public object? TargetObject;
        public GameLocation TargetLocation = null!;
        public Vector2 TilePosition;
        public Vector2 TileOffset;
        private readonly HashSet<Vector2> BoundingBoxTile = [];

        //public Target(GameLocation location, Vector2 tile, Point map)
        //{
        //    Get(location, tile, map);
        //}
        public Target(GameLocation location, Vector2 tile, Point map)
        {
            Initialize(location, tile, map);
        }

        public Target(GameLocation location, Vector2 tile, object obj)
        {
            TargetObject = obj;
            TargetLocation = location;
            TilePosition = tile;
        }

        private void Initialize(GameLocation location, Vector2 tile, Point map)
        {
            TargetLocation = location;
            TilePosition = tile;
        }

        /// <summary>Only for set values</summary>
        public static void Init(ModConfig config, IModHelper helper, IMonitor monitor)
        {
            Config = config;
            Helper = helper;
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

        //public bool IsOccupied(GameLocation location, Vector2 tile)
        //{
        //    bool occupied = false;
        //    if (!location.isTilePassable(tile) || !location.isTileOnMap(tile) || location.isTileHoeDirt(tile) || location.isCropAtTile((int)tile.X, (int)tile.Y) || location.IsTileBlockedBy(tile, ignorePassables: CollisionMask.All))
        //    {
        //        if (TargetObject is Crop && location.isTileHoeDirt(tile))
        //        {
        //            occupied = false;
        //        }
        //        else if (TargetObject is SObject sObject && sObject.IsTapper())
        //        {
        //            if (location.terrainFeatures.TryGetValue(tile, out var tf) && tf is Tree)
        //            {
        //                occupied = false;
        //            }
        //            else
        //            {
        //                occupied = true;
        //            }
        //        }
        //        else
        //        {
        //            occupied = true;
        //        }
        //    }
        //    if (BoundingBoxTile.Count != 0)
        //    {
        //        BoundingBoxTile.ToList().ForEach(t =>
        //        {
        //            if (!location.isTilePassable(t) || !location.isTileOnMap(t) || location.isTileHoeDirt(t) || location.isCropAtTile((int)t.X, (int)t.Y) || location.IsTileBlockedBy(t, ignorePassables: CollisionMask.All))
        //            {
        //                if (BoundingBoxTile.Count == 1)
        //                {
        //                    if (TargetObject is Bush bush && bush.size.Value == 3 && location.getObjectAtTile((int)tile.X, (int)tile.Y) is IndoorPot)
        //                    {
        //                        occupied = false;
        //                    }
        //                    else
        //                    {
        //                        occupied = true;
        //                    }
        //                }
        //                else
        //                {
        //                    occupied = true;
        //                }
        //            }
        //        });
        //    }
        //    return occupied;
        //}
    }
}
