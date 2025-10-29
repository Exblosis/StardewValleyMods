using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
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
            try
            {
                Target target = new();
                if (Config.EnableMoveEntity && !Config.MultiSelect)
                {
                    foreach (var c in location.characters)
                    {
                        // bessere Methode um NPC zu selecten finden
                        //var bb = c.GetBoundingBox();
                        //bb = new Rectangle(bb.Location - new Point(0, 64), new Point(c.Sprite.getWidth() * 4, c.Sprite.getHeight() * 4));
                        if (c.GetBoundingBox().Contains(map))
                        {
                            target.Set(c, c.currentLocation, tile);
                            return target;
                        }
                    }
                    foreach (var a in location.animals.Values)
                    {
                        if (a.GetBoundingBox().Contains(map))
                        {
                            target.Set(a, a.currentLocation, tile);
                            return target;
                        }
                    }
                    if (location is Forest forest)
                    {
                        foreach (var a in forest.marniesLivestock)
                        {
                            if (a.GetBoundingBox().Contains(map))
                            {
                                target.Set(a, a.currentLocation, tile, true);
                                return target;
                            }
                        }
                    }
                    if (Game1.player.GetBoundingBox().Contains(map))
                    {
                        Game1.player.forceCanMove();
                        target.Set(Game1.player, location, tile);
                        return target;
                    }
                }
                if (location.objects.TryGetValue(tile, out var obj))
                {
                    if ((obj is IndoorPot pot) && Config.MoveCropWithoutIndoorPot)
                    {
                        if (pot.bush.Value is not null && Config.EnableMoveBush)
                        {
                            var b = pot.bush.Value;
                            target.Set(b, b.Location, b.Tile);
                            return target;
                        }
                        if (pot.hoeDirt.Value.crop is not null && Config.EnableMoveCrop)
                        {
                            var cp = pot.hoeDirt.Value.crop;
                            target.Set(cp, cp.currentLocation, cp.tilePosition);
                            return target;
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

                    target.Set(obj, obj.Location, obj.TileLocation);
                    return target;
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

                        target.Set(rc, rc.Location, rc.Tile);
                        return target;
                    }
                }
                SkipResourceClumps:
                if (location.isCropAtTile((int)tile.X, (int)tile.Y) && Config.MoveCropWithoutTile && Config.EnableMoveCrop)
                {
                    var cp = ((HoeDirt)location.terrainFeatures[tile]).crop;
                    target.Set(cp, cp.currentLocation, cp.tilePosition);
                    return target;
                }
                if (location.largeTerrainFeatures is not null && Config.EnableMoveTerrainFeature)
                {
                    foreach (var ltf in location.largeTerrainFeatures)
                    {
                        if (ltf.getBoundingBox().Contains((int)tile.X * 64, (int)tile.Y * 64))
                        {
                            if ((ltf is Bush) && !Config.EnableMoveBush)
                                goto SkipLargeTerrainFeatures;

                            target.Set(ltf, ltf.Location, ltf.Tile);
                            return target;
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

                    target.Set(tf, tf.Location, tf.Tile);
                    return target;
                }
                SkipTerrainFeatures:
                if (location.IsTileOccupiedBy(tile, CollisionMask.Buildings) && Config.EnableMoveBuilding)
                {
                    var building = location.getBuildingAt(tile);
                    if (building != null)
                    {
                        Vector2 buildingTile = new(building.tileX.Value, building.tileY.Value);
                        target.Set(building.buildingType.Value, building, location, buildingTile, tile - buildingTile);
                        return target;
                    }
                }
                return null!;
            }
            catch (System.Exception ex)
            {
                Monitor.Log($"Fehler beim Select: {ex}", LogLevel.Error);
                return null!;
            }
        }

        /// <summary>
        /// Get alle Targets on one Tile.
        /// </summary>
        public static List<Target> GetAllTargets(GameLocation location, Vector2 tile, Point map)
        {
            try
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
                    else if ((tf is HoeDirt { crop: not null }) && Config.MoveCropWithoutTile) { }
                    else if ((tf is Bush) && !Config.EnableMoveBush) { }
                    else
                    {
                        Target target = new();
                        target.Set(tf, tf.Location, tf.Tile);
                        targets.Add(target);
                    }
                }

                // Objekte
                if (location.objects.TryGetValue(tile, out var obj))
                {
                    if (obj is IndoorPot pot && Config.MoveCropWithoutIndoorPot)
                    {
                        if (pot.bush.Value is not null && Config.EnableMoveBush)
                        {
                            Target target = new();
                            target.Set(pot.bush.Value, pot.bush.Value.Location, pot.bush.Value.Tile);
                            targets.Add(target);
                        }
                        if (pot.hoeDirt.Value.crop is not null && Config.EnableMoveCrop)
                        {
                            Target target = new();
                            target.Set(pot.hoeDirt.Value.crop, pot.hoeDirt.Value.crop.currentLocation, pot.hoeDirt.Value.crop.tilePosition);
                            targets.Add(target);
                        }
                    }
                    if (Config.EnableMoveObject)
                    {
                        if (obj.isPlaceable() && Config.EnableMovePlaceableObject ||
                            obj.IsSpawnedObject && Config.EnableMoveCollectibleObject ||
                            !obj.isPlaceable() && !obj.IsSpawnedObject && Config.EnableMoveGeneratedObject)
                        {
                            Target target = new();
                            target.Set(obj, obj.Location, obj.TileLocation);
                            targets.Add(target);
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

                        Target target = new();
                        target.Set(rc, rc.Location, rc.Tile);
                        targets.Add(target);
                    }
                }

                // Crop auf HoeDirt
                if (location.isCropAtTile((int)tile.X, (int)tile.Y) && Config.MoveCropWithoutTile && Config.EnableMoveCrop)
                {
                    var cp = ((HoeDirt)location.terrainFeatures[tile]).crop;
                    Target target = new();
                    target.Set(cp, cp.currentLocation, cp.tilePosition);
                    targets.Add(target);
                }

                // LargeTerrainFeatures
                if (location.largeTerrainFeatures is not null && Config.EnableMoveTerrainFeature)
                {
                    foreach (var ltf in location.largeTerrainFeatures)
                    {
                        if (ltf.getBoundingBox().Contains((int)tile.X * 64, (int)tile.Y * 64))
                        {
                            if ((ltf is Bush) && !Config.EnableMoveBush) continue;
                            Target target = new();
                            target.Set(ltf, ltf.Location, ltf.Tile);
                            targets.Add(target);
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
                        Target target = new();
                        target.Set(building.buildingType.Value, building, location, buildingTile, tile - buildingTile);
                        targets.Add(target);
                    }
                }

                return targets;
            }
            catch (System.Exception ex)
            {
                Monitor.Log($"Fehler beim Select: {ex}", LogLevel.Error);
                return null!;
            }
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
