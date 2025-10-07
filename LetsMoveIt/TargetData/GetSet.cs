using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace LetsMoveIt.TargetData
{
    internal partial class Target
    {
        /// <summary>Get target.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="tile">The current tile position.</param>
        /// <param name="map">The current map position.</param>
        public static Target Get(GameLocation location, Vector2 tile, Point map)
        {
            var t = new Target(location, tile, map);
            if (Config.EnableMoveEntity && !Config.MultiSelect)
            {
                foreach (var c in location.characters)
                {
                    // bessere Methode um NPC zu selecten finden
                    //var bb = c.GetBoundingBox();
                    //bb = new Rectangle(bb.Location - new Point(0, 64), new Point(c.Sprite.getWidth() * 4, c.Sprite.getHeight() * 4));
                    if (c.GetBoundingBox().Contains(map))
                    {
                        t.Set(c, c.currentLocation, tile);
                        return t;
                    }
                }
                foreach (var a in location.animals.Values)
                {
                    if (a.GetBoundingBox().Contains(map))
                    {
                        t.Set(a, a.currentLocation, tile);
                        return t;
                    }
                }
                if (location is Forest forest)
                {
                    foreach (var a in forest.marniesLivestock)
                    {
                        if (a.GetBoundingBox().Contains(map))
                        {
                            t.Set(a, a.currentLocation, tile, true);
                            return t;
                        }
                    }
                }
                if (Game1.player.GetBoundingBox().Contains(map))
                {
                    Game1.player.forceCanMove();
                    t.Set(Game1.player, location, tile);
                    return t;
                }
            }
            if (location.objects.TryGetValue(tile, out var obj))
            {
                if ((obj is IndoorPot pot) && Config.MoveCropWithoutIndoorPot)
                {
                    if (pot.bush.Value is not null && Config.EnableMoveBush)
                    {
                        var b = pot.bush.Value;
                        t.Set(b, b.Location, b.Tile);
                        return t;
                    }
                    if (pot.hoeDirt.Value.crop is not null && Config.EnableMoveCrop)
                    {
                        var cp = pot.hoeDirt.Value.crop;
                        t.Set(cp, cp.currentLocation, cp.tilePosition);
                        return t;
                    }
                }

                if (!Config.EnableMoveObject)
                    goto SkipObjects;
                if (obj.isPlaceable() && !Config.EnableMovePlaceableObject)
                    goto SkipObjects;
                if (obj.IsSpawnedObject && !Config.EnableMoveCollectibleObject)
                    goto SkipObjects;
                if (!obj.isPlaceable() && !obj.IsSpawnedObject && !Config.EnableMoveGeneratedObject)
                    goto SkipObjects;

                t.Set(obj, obj.Location, obj.TileLocation);
                return t;
            }
            SkipObjects:
            foreach (var rc in location.resourceClumps)
            {
                if (rc.occupiesTile((int)tile.X, (int)tile.Y) && Config.EnableMoveResourceClump)
                {
                    int rcIndex = rc.parentSheetIndex.Value;
                    if ((rc is GiantCrop) && !Config.EnableMoveGiantCrop)
                        goto SkipResourceClumps;
                    if ((rcIndex is ResourceClump.stumpIndex) && !Config.EnableMoveStump)
                        goto SkipResourceClumps;
                    if ((rcIndex is ResourceClump.hollowLogIndex) && !Config.EnableMoveHollowLog)
                        goto SkipResourceClumps;
                    if ((rcIndex is ResourceClump.boulderIndex or ResourceClump.quarryBoulderIndex or ResourceClump.mineRock1Index or ResourceClump.mineRock2Index or ResourceClump.mineRock3Index or ResourceClump.mineRock4Index) && !Config.EnableMoveBoulder)
                        goto SkipResourceClumps;
                    if ((rcIndex is ResourceClump.meteoriteIndex) && !Config.EnableMoveMeteorite)
                        goto SkipResourceClumps;

                    t.Set(rc, rc.Location, rc.Tile);
                    return t;
                }
            }
            SkipResourceClumps:
            if (location.isCropAtTile((int)tile.X, (int)tile.Y) && Config.MoveCropWithoutTile && Config.EnableMoveCrop)
            {
                var cp = ((HoeDirt)location.terrainFeatures[tile]).crop;
                t.Set(cp, cp.currentLocation, cp.tilePosition);
                return t;
            }
            if (location.largeTerrainFeatures is not null && Config.EnableMoveTerrainFeature)
            {
                foreach (var ltf in location.largeTerrainFeatures)
                {
                    if (ltf.getBoundingBox().Contains((int)tile.X * 64, (int)tile.Y * 64))
                    {
                        if ((ltf is Bush) && !Config.EnableMoveBush)
                            goto SkipLargeTerrainFeatures;

                        t.Set(ltf, ltf.Location, ltf.Tile);
                        return t;
                    }
                }
            }
            SkipLargeTerrainFeatures:
            if (location.terrainFeatures.TryGetValue(tile, out var tf) && Config.EnableMoveTerrainFeature)
            {
                if ((tf is Flooring) && !Config.EnableMoveFlooring)
                    goto SkipTerrainFeatures;
                if ((tf is Tree) && !Config.EnableMoveTree)
                    goto SkipTerrainFeatures;
                if ((tf is FruitTree) && !Config.EnableMoveFruitTree)
                    goto SkipTerrainFeatures;
                if ((tf is Grass) && !Config.EnableMoveGrass)
                    goto SkipTerrainFeatures;
                if ((tf is HoeDirt) && !Config.EnableMoveFarmland)
                    goto SkipTerrainFeatures;
                if ((tf is Bush) && !Config.EnableMoveBush) // Tea Bush
                    goto SkipTerrainFeatures;

                t.Set(tf, tf.Location, tf.Tile);
                return t;
            }
            SkipTerrainFeatures:
            if (location.IsTileOccupiedBy(tile, CollisionMask.Buildings) && Config.EnableMoveBuilding)
            {
                var building = location.getBuildingAt(tile);
                if (building != null)
                {
                    Vector2 buildingTile = new(building.tileX.Value, building.tileY.Value);
                    t.Set(building.buildingType.Value, building, location, buildingTile, tile - buildingTile);
                    return t;
                }
            }
            return null!;
        }

        /// <summary>
        /// Get alle Targets on one Tile.
        /// </summary>
        public static List<Target> GetAllTargets(GameLocation location, Vector2 tile, Point map)
        {
            var targets = new List<Target>();

            // Charaktere (NPCs, Tiere, Spieler) funktioniert nicht bei MultiSelect
            //if (Config.EnableMoveEntity)
            //{
            //    foreach (var c in location.characters)
            //    {
            //        if (c.GetBoundingBox().Contains(map))
            //            targets.Add(new Target(location, tile, map));
            //    }
            //    foreach (var a in location.animals.Values)
            //    {
            //        if (a.GetBoundingBox().Contains(map))
            //            targets.Add(new Target(location, tile, map));
            //    }
            //    if (location is Forest forest)
            //    {
            //        foreach (var a in forest.marniesLivestock)
            //        {
            //            if (a.GetBoundingBox().Contains(map))
            //                targets.Add(new Target(location, tile, map));
            //        }
            //    }
            //    if (Game1.player.GetBoundingBox().Contains(map))
            //        targets.Add(new Target(location, tile, map));
            //}

            // TerrainFeatures
            if (location.terrainFeatures.TryGetValue(tile, out var tf) && Config.EnableMoveTerrainFeature)
            {
                if ((tf is Flooring) && !Config.EnableMoveFlooring) { }
                else if ((tf is Tree) && !Config.EnableMoveTree) { }
                else if ((tf is FruitTree) && !Config.EnableMoveFruitTree) { }
                else if ((tf is Grass) && !Config.EnableMoveGrass) { }
                else if ((tf is HoeDirt) && !Config.EnableMoveFarmland) { }
                else if ((tf is HoeDirt hoeDirt) && (hoeDirt?.crop is not null) && Config.MoveCropWithoutTile) { }
                else if ((tf is Bush) && !Config.EnableMoveBush) { }
                else
                {
                    var t = new Target(location, tile, map);
                    t.Set(tf, tf.Location, tf.Tile);
                    targets.Add(t);
                }
            }

            // Objekte
            if (location.objects.TryGetValue(tile, out var obj))
            {
                if (obj is IndoorPot pot && Config.MoveCropWithoutIndoorPot)
                {
                    if (pot.bush.Value is not null && Config.EnableMoveBush)
                    {
                        var t = new Target(location, tile, map);
                        t.Set(pot.bush.Value, pot.bush.Value.Location, pot.bush.Value.Tile);
                        targets.Add(t);
                    }
                    if (pot.hoeDirt.Value.crop is not null && Config.EnableMoveCrop)
                    {
                        var t = new Target(location, tile, map);
                        t.Set(pot.hoeDirt.Value.crop, pot.hoeDirt.Value.crop.currentLocation, pot.hoeDirt.Value.crop.tilePosition);
                        targets.Add(t);
                    }
                }
                if (Config.EnableMoveObject)
                {
                    if (obj.isPlaceable() && Config.EnableMovePlaceableObject ||
                        obj.IsSpawnedObject && Config.EnableMoveCollectibleObject ||
                        !obj.isPlaceable() && !obj.IsSpawnedObject && Config.EnableMoveGeneratedObject)
                    {
                        var t = new Target(location, tile, map);
                        t.Set(obj, obj.Location, obj.TileLocation);
                        targets.Add(t);
                    }
                }
            }

            // ResourceClumps
            foreach (var rc in location.resourceClumps)
            {
                if (rc.occupiesTile((int)tile.X, (int)tile.Y) && Config.EnableMoveResourceClump)
                {
                    int rcIndex = rc.parentSheetIndex.Value;
                    if ((rc is GiantCrop) && !Config.EnableMoveGiantCrop) continue;
                    if ((rcIndex is ResourceClump.stumpIndex) && !Config.EnableMoveStump) continue;
                    if ((rcIndex is ResourceClump.hollowLogIndex) && !Config.EnableMoveHollowLog) continue;
                    if ((rcIndex is ResourceClump.boulderIndex or ResourceClump.quarryBoulderIndex or ResourceClump.mineRock1Index or ResourceClump.mineRock2Index or ResourceClump.mineRock3Index or ResourceClump.mineRock4Index) && !Config.EnableMoveBoulder) continue;
                    if ((rcIndex is ResourceClump.meteoriteIndex) && !Config.EnableMoveMeteorite) continue;

                    var t = new Target(location, tile, map);
                    t.Set(rc, rc.Location, rc.Tile);
                    targets.Add(t);
                }
            }

            // Crop auf HoeDirt
            if (location.isCropAtTile((int)tile.X, (int)tile.Y) && Config.MoveCropWithoutTile && Config.EnableMoveCrop)
            {
                var cp = ((HoeDirt)location.terrainFeatures[tile]).crop;
                var t = new Target(location, tile, map);
                t.Set(cp, cp.currentLocation, cp.tilePosition);
                targets.Add(t);
            }

            // LargeTerrainFeatures
            if (location.largeTerrainFeatures is not null && Config.EnableMoveTerrainFeature)
            {
                foreach (var ltf in location.largeTerrainFeatures)
                {
                    if (ltf.getBoundingBox().Contains((int)tile.X * 64, (int)tile.Y * 64))
                    {
                        if ((ltf is Bush) && !Config.EnableMoveBush) continue;
                        var t = new Target(location, tile, map);
                        t.Set(ltf, ltf.Location, ltf.Tile);
                        targets.Add(t);
                    }
                }
            }

            // Gebäude
            if (location.IsTileOccupiedBy(tile, CollisionMask.Buildings) && Config.EnableMoveBuilding)
            {
                var building = location.getBuildingAt(tile);
                if (building != null)
                {
                    Vector2 buildingTile = new(building.tileX.Value, building.tileY.Value);
                    var t = new Target(location, tile, map);
                    t.Set(building.buildingType.Value, building, location, buildingTile, tile - buildingTile);
                    targets.Add(t);
                }
            }

            return targets;
        }

        /// <summary>Set target</summary>
        private void Set(object obj, GameLocation lastLocation, Vector2 cursorTile, Vector2? offset = null)
        {
            Set(null, obj, lastLocation, cursorTile, offset ?? Vector2.Zero);
        }
        private void Set(object obj, GameLocation lastLocation, Vector2 cursorTile, bool marniesLivestock)
        {
            Set(null, obj, lastLocation, cursorTile, Vector2.Zero, marniesLivestock);
        }
        private void Set(string? name, object obj, GameLocation lastLocation, Vector2 cursorTile, Vector2 offset, bool marniesLivestock = false)
        {
            Name = name;
            MarniesLivestock = marniesLivestock;
            TargetObject = obj;
            TargetLocation = lastLocation;
            TilePosition = cursorTile;
            TileOffset = offset;
        }
    }
}
