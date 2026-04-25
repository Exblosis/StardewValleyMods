using System;
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
        private static IMonitor Monitor = null!;

        public string? Name { get; set; }
        public bool MarniesLivestock { get; set; }
        public object? TargetObject { get; private set; }
        public GameLocation TargetLocation { get; private set; } = null!;
        public Vector2 TilePosition { get; private set; }
        public Vector2 TileOffset { get; private set; }

        /// <summary>Relative to Render(tile), used by ResourceClump and TerrainFeature.</summary>
        private readonly HashSet<Vector2> BoundingBoxTile = [];

        /// <summary>Create Empty Target</summary>
        public Target() {}

        /// <summary>Create Simple Target</summary>
        public Target(object obj, GameLocation location, Vector2 tile)
        {
            TargetObject = obj;
            TargetLocation = location ?? throw new ArgumentNullException(nameof(location));
            TilePosition = tile;
        }

        /// <summary>Use in ModEntry.Entry() | Only for set values.</summary>
        public static void Init(ModConfig config, IMonitor monitor)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            Monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
        }

        public bool IsOccupied(GameLocation location, Vector2 tile)
        {
            if (location == null)
                throw new ArgumentNullException(nameof(location));

            if (BoundingBoxTile.Count > 0)
            {
                foreach (var t in BoundingBoxTile)
                {
                    if (!location.isTileOnMap(t))
                        return true;

                    if (IsTileOccupied(location, t))
                        return true;
                }
                return BoundingBoxTile.Any(t => IsTileOccupied(location, t));
            }
            else
            {
                if (!location.isTileOnMap(tile))
                    return true;

                if (IsTileOccupied(location, tile))
                    return true;
            }
            
            return false;
        }

        private bool IsTileOccupied(GameLocation location, Vector2 tile)
        {
            bool passable = location.isTilePassable(tile);
            bool hoeDirt = location.isTileHoeDirt(tile);
            bool hasCrop = location.isCropAtTile((int)tile.X, (int)tile.Y);
            bool blockedBy = location.IsTileBlockedBy(tile, ignorePassables: CollisionMask.All);

            if (TargetObject is Furniture furniture)
                return !furniture.canBePlacedHere(location, tile);

            if (!passable || hoeDirt || hasCrop || blockedBy)
            {
                if (TargetObject is Crop && hoeDirt)
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
