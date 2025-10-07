using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace LetsMoveIt.TargetData
{
    internal partial class Target
    {
        /// <summary>Move the current target.</summary>
        /// <param name="location">The current location.</param>
        /// <param name="tile">The current tile position.</param>
        /// <param name="overwriteTile">To Overwrite existing Object.</param>
        public void CopyTo(GameLocation location, Vector2 tile, bool overwriteTile)
        {
            if (!Config.ModEnabled)
            {
                TargetObject = null;
                return;
            }
            if (TargetObject is null)
                return;

            try
            {
                switch (TargetObject)
                {
                    case Farmer:
                        Game1.playSound("Duck");
                        break;
                    case NPC:
                        Game1.addHUDMessage(new(I18n.Message("NotImplemented"), 2));
                        Game1.playSound("cancel");
                        break;
                    case FarmAnimal farmAnimal:
                        CopyFarmAnimal(farmAnimal, location, tile);
                        break;
                    case SObject sObject:
                        CopySObject(sObject, location, tile);
                        break;
                    case ResourceClump resourceClump:
                        CopyResourceClump(resourceClump, location, tile);
                        break;
                    case TerrainFeature terrainFeature:
                        CopyTerrainFeature(terrainFeature, location, tile, overwriteTile);
                        break;
                    case Crop crop:
                        CopyCrop(crop, location, tile);
                        break;
                    case Building:
                        Game1.addHUDMessage(new(I18n.Message("NotImplemented"), 2));
                        Game1.playSound("cancel");
                        break;
                    default:
                        Monitor.Log($"Unbekannter Typ: {TargetObject.GetType()}", StardewModdingAPI.LogLevel.Warn);
                        break;
                }
            }
            catch (System.Exception ex)
            {
                Monitor.Log($"Fehler beim Kopieren: {ex}", StardewModdingAPI.LogLevel.Error);
                Game1.playSound("dwop");
            }
        }

        private void CopyFarmAnimal(FarmAnimal farmAnimal, GameLocation location, Vector2 tile)
        {
            AnimalHouse animalHouse;
            if (location is AnimalHouse currentHouse && location.Map.Id.Remove(4) == farmAnimal.buildingTypeILiveIn.Value)
            {
                animalHouse = currentHouse;
            }
            else if (location is Farm && farmAnimal.home?.GetIndoors() is AnimalHouse thisAnimal)
            {
                animalHouse = thisAnimal;
            }
            else
            {
                Game1.playSound("cancel");
                return;
            }
            if (animalHouse.isFull())
            {
                Game1.addHUDMessage(new(I18n.Message("BuildingIsFull"), 3));
                Game1.playSound("cancel");
                return;
            }
            FarmAnimal farmAnimalCopy = new(farmAnimal.type.Value, Game1.Multiplayer.getNewID(), Game1.player.UniqueMultiplayerID);
            if (farmAnimal.isAdult())
                farmAnimalCopy.growFully();
            farmAnimalCopy.Name = Dialogue.randomName();
            farmAnimalCopy.displayName = farmAnimalCopy.Name;
            animalHouse.adoptAnimal(farmAnimalCopy);
            location.animals.TryAdd(farmAnimalCopy.myID.Value, farmAnimalCopy);
            farmAnimalCopy.Position = (Game1.getMousePosition() + new Point(Game1.viewport.Location.X - 32, Game1.viewport.Location.Y - 32)).ToVector2();
            farmAnimalCopy.makeSound();
        }

        private void CopySObject(SObject sObject, GameLocation location, Vector2 tile)
        {
            if (location.objects.ContainsKey(tile))
            {
                location.objects.Remove(tile);
            }
            if (sObject is BreakableContainer)
            {
                BreakableContainer breakableContainerCopy;
                if (sObject.ItemId == "174")
                {
                    breakableContainerCopy = BreakableContainer.GetBarrelForVolcanoDungeon(tile);
                }
                else
                {
                    breakableContainerCopy = new(tile, sObject.ItemId);
                    breakableContainerCopy.showNextIndex.Value = Game1.random.NextBool();
                }
                location.objects.Add(tile, breakableContainerCopy);
                location.playSound("axe", tile);
            }
            else
            {
                SObject? sObjectCopy = sObject.getOne() as SObject;
                if (sObjectCopy is not null)
                {
                    if (sObject.isPlaceable())
                    {
                        sObjectCopy.placementAction(location, (int)tile.X * 64, (int)tile.Y * 64, Game1.player);
                    }
                    else
                    {
                        sObjectCopy.MinutesUntilReady = sObject.MinutesUntilReady;
                        sObjectCopy.IsSpawnedObject = sObject.IsSpawnedObject;
                        location.objects.Add(tile, sObjectCopy);
                        location.playSound("woodyStep", tile);
                    }
                    if (sObject.heldObject.Value is not null)
                    {
                        if (location.objects.TryGetValue(tile, out var obj))
                        {
                            if (obj is Fence fence)
                            {
                                fence.performObjectDropInAction(sObject.heldObject.Value.getOne(), false, Game1.player);
                            }
                            else
                            {
                                obj.performObjectDropInAction(sObject.heldObject.Value.getOne(), false, Game1.player);
                            }
                        }
                    }
                }
            }
        }

        private void CopyResourceClump(ResourceClump resourceClump, GameLocation location, Vector2 tile)
        {
            if (resourceClump is GiantCrop giantCrop)
            {
                GiantCrop giantCropCopy = new(giantCrop.Id, tile);
                location.resourceClumps.Add(giantCropCopy);
                location.playSound("axe", tile);
            }
            else
            {
                ResourceClump resourceClumpCopy = new(resourceClump.parentSheetIndex.Value, resourceClump.width.Value, resourceClump.height.Value, tile, null, resourceClump.textureName.Value);
                location.resourceClumps.Add(resourceClumpCopy);
                location.playSound("axe", tile);
            }
        }

        private void CopyTerrainFeature(TerrainFeature terrainFeature, GameLocation location, Vector2 tile, bool overwriteTile)
        {
            if (terrainFeature is Bush bush && bush.size.Value == 3)
            {
                if (location.objects.TryGetValue(tile, out var obj) && obj is IndoorPot pot)
                {
                    if (pot.bush.Value is not null || pot.hoeDirt.Value.crop is not null)
                    {
                        Game1.playSound("cancel");
                        return;
                    }
                }
                if (location.objects.TryGetValue(tile, out var obj2) && obj2 is IndoorPot pot2)
                {
                    Bush bushCopy = new(tile, bush.size.Value, location, bush.datePlanted.Value);
                    bushCopy.modData.CopyFrom(bush.modData);
                    bushCopy.dayUpdate();
                    bushCopy.inPot.Value = true;
                    pot2.bush.Value = bushCopy;
                    location.playSound("leafrustle", tile);
                }
                else
                {
                    if (location.terrainFeatures.ContainsKey(tile))
                    {
                        location.terrainFeatures.Remove(tile);
                    }
                    Bush bushCopy = new(tile, bush.size.Value, location, bush.datePlanted.Value);
                    bushCopy.modData.CopyFrom(bush.modData);
                    bushCopy.dayUpdate();
                    location.terrainFeatures.Add(tile, bushCopy);
                    location.playSound("leafrustle", tile);
                }
            }
            else if (terrainFeature is LargeTerrainFeature largeTerrainFeature && terrainFeature is Bush bush1)
            {
                Bush bushCopy = new(tile, bush1.size.Value, location, bush1.datePlanted.Value);
                bushCopy.modData.CopyFrom(bush1.modData);
                bushCopy.dayUpdate();
                bushCopy.townBush.Value = bush1.townBush.Value;
                bushCopy.tileSheetOffset.Value = bush1.tileSheetOffset.Value;
                bushCopy.setUpSourceRect();
                location.largeTerrainFeatures.Add(bushCopy);
                location.playSound("leafrustle", tile);
            }
            else
            {
                if (location.terrainFeatures.ContainsKey(tile))
                {
                    location.terrainFeatures.Remove(tile);
                }
                if (terrainFeature is Flooring flooring)
                {
                    Flooring flooringCopy = new(flooring.whichFloor.Value);
                    location.terrainFeatures.Add(tile, flooringCopy);
                    location.playSound(flooring.GetData().PlacementSound, tile);
                }
                else if (terrainFeature is HoeDirt hoeDirt)
                {
                    HoeDirt hoeDirtCopy = new(hoeDirt.state.Value);
                    hoeDirtCopy.fertilizer.Value = hoeDirt.fertilizer.Value;
                    location.terrainFeatures.Add(tile, hoeDirtCopy);
                    if (hoeDirt.crop is not null)
                    {
                        Target c = new(hoeDirt.crop.currentLocation, hoeDirt.crop.tilePosition, hoeDirt.crop);
                        c?.CopyTo(location, tile, overwriteTile);
                    }
                    location.playSound("hoeHit", tile);
                }
                else if (terrainFeature is Grass grass)
                {
                    Grass grassCopy = new(grass.grassType.Value, grass.numberOfWeeds.Value);
                    location.terrainFeatures.Add(tile, grassCopy);
                    location.playSound("grassyStep", tile);
                }
                else if (terrainFeature is FruitTree fruitTree)
                {
                    FruitTree fruitTreeCopy = new(fruitTree.treeId.Value, fruitTree.growthStage.Value);
                    location.terrainFeatures.Add(tile, fruitTreeCopy);
                    location.playSound("leafrustle", tile);
                }
                else if (terrainFeature is Tree tree)
                {
                    Tree treeCopy = new(tree.treeType.Value, tree.growthStage.Value, tree.isTemporaryGreenRainTree.Value);
                    location.terrainFeatures.Add(tile, treeCopy);
                    location.playSound("leafrustle", tile);
                }
            }
        }

        private void CopyCrop(Crop crop, GameLocation location, Vector2 tile)
        {
            if (location.isCropAtTile((int)tile.X, (int)tile.Y) || !location.isTileHoeDirt(tile))
            {
                Game1.playSound("cancel");
                return;
            }
            if (location.objects.TryGetValue(tile, out var isPot) && isPot is IndoorPot pot && pot.hoeDirt.Value.crop is not null)
            {
                Game1.playSound("cancel");
                return;
            }
            Crop cropCopy;
            if (crop.forageCrop.Value)
            {
                cropCopy = new(crop.forageCrop.Value, crop.whichForageCrop.Value, (int)tile.X, (int)tile.Y, location);
            }
            else
            {
                cropCopy = new(crop.netSeedIndex.Value, (int)tile.X, (int)tile.Y, location);
            }
            cropCopy.currentPhase.Value = crop.currentPhase.Value;
            cropCopy.dayOfCurrentPhase.Value = crop.dayOfCurrentPhase.Value;
            cropCopy.phaseToShow.Value = crop.phaseToShow.Value;
            cropCopy.fullyGrown.Value = crop.fullyGrown.Value;
            cropCopy.phaseDays.Set(crop.phaseDays);
            if (location.objects.TryGetValue(tile, out var newPot) && newPot is IndoorPot pot2)
            {
                pot2.hoeDirt.Value.crop = cropCopy;
                pot2.hoeDirt.Value.applySpeedIncreases(Game1.player);
                pot2.hoeDirt.Value.crop.updateDrawMath(tile);
            }
            else if (location.terrainFeatures.TryGetValue(tile, out var newHoeDirt) && newHoeDirt is HoeDirt hoeDirt)
            {
                hoeDirt.crop = cropCopy;
                hoeDirt.applySpeedIncreases(Game1.player);
                hoeDirt.crop.updateDrawMath(tile);
            }
            location.playSound("dirtyHit", tile);
        }
    }
}
