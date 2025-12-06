using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Monsters;
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
        public void MoveTo(GameLocation location, Vector2 tile, bool overwriteTile)
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
                    case Farmer farmer:
                        MoveFarmer(farmer);
                        break;
                    case NPC character:
                        MoveNPC(character, location);
                        break;
                    case FarmAnimal farmAnimal:
                        MoveFarmAnimal(farmAnimal, location, tile);
                        break;
                    case SObject sObject:
                        MoveSObject(sObject, location, tile);
                        break;
                    case ResourceClump resourceClump:
                        MoveResourceClump(resourceClump, location, tile);
                        break;
                    case TerrainFeature terrainFeature:
                        MoveTerrainFeature(terrainFeature, location, tile);
                        break;
                    case Crop crop:
                        MoveCrop(crop, location, tile);
                        break;
                    case Building building:
                        MoveBuilding(building, location, tile, overwriteTile);
                        break;
                    default:
                        Monitor.Log($"Unbekannter Typ: {TargetObject.GetType()}", LogLevel.Warn);
                        TargetObject = null;
                        break;
                }
            }
            catch (System.Exception ex)
            {
                Monitor.Log($"Fehler beim Verschieben: {ex.Message}\n{ex.StackTrace}", LogLevel.Error);
                Game1.playSound("dwop");
                TargetObject = null;
            }
        }

        private void MoveFarmer(Farmer farmer)
        {
            farmer.Position = (Game1.getMousePosition() + new Point(Game1.viewport.Location.X - 32, Game1.viewport.Location.Y - 32)).ToVector2();
            Game1.player.forceCanMove();
            TargetObject = null;
        }

        private void MoveNPC(NPC character, GameLocation location)
        {
            if (location == TargetLocation)
            {
                character.Position = (Game1.getMousePosition() + new Point(Game1.viewport.Location.X - 32, Game1.viewport.Location.Y - 32)).ToVector2();
            }
            else
            {
                Game1.warpCharacter(character, location, (Game1.getMousePosition() + new Point(Game1.viewport.Location.X - 32, Game1.viewport.Location.Y - 32)).ToVector2() / 64);
            }
            if (character is not Monster)
                character.Halt();
            TargetObject = null;
        }

        private void MoveFarmAnimal(FarmAnimal farmAnimal, GameLocation location, Vector2 tile)
        {
            if (location != TargetLocation)
            {
                if (MarniesLivestock && TargetLocation is Forest forest)
                {
                    forest.marniesLivestock.Remove(farmAnimal);
                }
                TargetLocation.animals.Remove(farmAnimal.myID.Value);
                location.animals.TryAdd(farmAnimal.myID.Value, farmAnimal);
                if (location is AnimalHouse currentHouse && !currentHouse.isFull() && location.Map.Id.Remove(4) == farmAnimal.buildingTypeILiveIn.Value && location.NameOrUniqueName != farmAnimal.home?.GetIndoors().NameOrUniqueName)
                {
                    if (farmAnimal.home?.GetIndoors() is AnimalHouse animalHouse)
                    {
                        animalHouse.animalsThatLiveHere.Remove(farmAnimal.myID.Value);
                    }
                    currentHouse.animalsThatLiveHere.Add(farmAnimal.myID.Value);
                }
            }
            farmAnimal.Position = (Game1.getMousePosition() + new Point(Game1.viewport.Location.X - 32, Game1.viewport.Location.Y - 32)).ToVector2();
            TargetObject = null;
        }

        private void MoveSObject(SObject sObject, GameLocation location, Vector2 tile)
        {
            if (TargetLocation.objects.ContainsKey(TilePosition))
            {
                TargetLocation.objects.Remove(TilePosition);
                if (location.objects.ContainsKey(tile))
                {
                    location.objects.Remove(tile);
                }
                location.objects.Add(tile, sObject);
                if (sObject.lightSource is not null)
                {
                    TargetLocation.removeLightSource(sObject.lightSource.Id);
                    sObject.reloadSprite();
                }
                if (sObject is Fence fence && fence.heldObject.Value is Torch)
                {
                    location.objects.TryGetValue(tile, out var obj);
                    Torch? torch = obj?.heldObject.Value as Torch;
                    if (torch is not null)
                    {
                        TargetLocation.removeLightSource(torch.lightSource.Id);
                        torch.Location = location;
                        torch.initializeLightSource(tile);
                    }
                }
                TargetObject = null;
            }
            else if (sObject is Furniture furniture)
            {
                if(TargetLocation.furniture.Contains(furniture))
                {
                    TargetLocation.furniture.Remove(furniture);
                    location.furniture.Add(furniture);
                    int y = furniture.GetModifiedWallTilePosition(location, (int)tile.X, (int)tile.Y);
                    Vector2 newTile = new(tile.X, y);
                    furniture.InitializeAtTile(newTile);
                    furniture.updateDrawPosition();
                    furniture.RemoveLightGlow();
                    furniture.removeLights();
                    if (Game1.isDarkOut(location))
                    {
                        furniture.addLights();
                    }
                    TargetObject = null;
                }
            }
            else
            {
                TargetObject = null;
                Game1.playSound("dwop");
            }
        }

        private void MoveTerrainFeature(TerrainFeature terrainFeature, GameLocation location, Vector2 tile)
        {
            if (terrainFeature is Bush { size.Value: 3 } bush)
            {
                if (location.objects.TryGetValue(tile, out var obj) && obj is IndoorPot pot)
                {
                    if (pot.bush.Value is not null || pot.hoeDirt.Value.crop is not null)
                    {
                        Game1.playSound("cancel");
                        return;
                    }
                }
                if (TargetLocation.objects.TryGetValue(TilePosition, out var obj1) && obj1 is IndoorPot pot1 && pot1.bush.Value is not null)
                {
                    pot1.bush.Value = null;
                }
                else if (TargetLocation.terrainFeatures.ContainsKey(TilePosition))
                {
                    TargetLocation.terrainFeatures.Remove(TilePosition);
                }
                else
                {
                    TargetObject = null;
                    Game1.playSound("dwop");
                    return;
                }
                if (location.objects.TryGetValue(tile, out var obj2) && obj2 is IndoorPot pot2)
                {
                    bush.inPot.Value = true;
                    bush.netTilePosition.Value = tile;
                    pot2.bush.Value = bush;
                    TargetObject = null;
                }
                else
                {
                    if (bush.inPot.Value)
                    {
                        bush.inPot.Value = false;
                        bush.GetType().GetField("yDrawOffset", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(bush, 0f);
                    }
                    if (location.terrainFeatures.ContainsKey(tile))
                    {
                        location.terrainFeatures.Remove(tile);
                    }
                    location.terrainFeatures.Add(tile, terrainFeature);
                    TargetObject = null;
                }
            }
            else if (terrainFeature is LargeTerrainFeature largeTerrainFeature && TargetLocation.largeTerrainFeatures.Contains(largeTerrainFeature))
            {
                int index = TargetLocation.largeTerrainFeatures.IndexOf(largeTerrainFeature);
                if (index >= 0)
                {
                    if (location == TargetLocation)
                    {
                        location.largeTerrainFeatures[index].netTilePosition.Value = tile;
                        TargetObject = null;
                    }
                    else
                    {
                        TargetLocation.largeTerrainFeatures.Remove(largeTerrainFeature);
                        location.largeTerrainFeatures.Add(largeTerrainFeature);
                        int newIndex = location.largeTerrainFeatures.IndexOf(largeTerrainFeature);
                        location.largeTerrainFeatures[newIndex].netTilePosition.Value = tile;
                        TargetObject = null;
                    }
                }
                else
                {
                    TargetObject = null;
                    Game1.playSound("dwop");
                }
            }
            else if (TargetLocation.terrainFeatures.ContainsKey(TilePosition))
            {
                TargetLocation.terrainFeatures.Remove(TilePosition);
                if (location.terrainFeatures.ContainsKey(tile))
                {
                    location.terrainFeatures.Remove(tile);
                }
                location.terrainFeatures.Add(tile, terrainFeature);

                Vector2[] neighbors = new[]
                {
                    tile + new Vector2(0, 1),
                    tile + new Vector2(1, 0),
                    tile + new Vector2(0, -1),
                    tile + new Vector2(-1, 0)
                };
                foreach (Vector2 ct in neighbors)
                {
                    if (location.terrainFeatures.TryGetValue(ct, out var neighbor) && neighbor is HoeDirt hoeDirtNeighbors)
                    {
                        hoeDirtNeighbors.updateNeighbors();
                    }
                }
                if (terrainFeature is HoeDirt hoeDirt)
                {
                    hoeDirt.paddyWaterCheck(true);
                    hoeDirt.updateNeighbors();
                    hoeDirt.crop?.updateDrawMath(tile);
                }
                TargetObject = null;
            }
        }

        private void MoveCrop(Crop crop, GameLocation location, Vector2 tile)
        {
            if (location.isCropAtTile((int)tile.X, (int)tile.Y) || !location.isTileHoeDirt(tile))
            {
                Game1.playSound("cancel");
                return;
            }
            if (location.objects.TryGetValue(tile, out var isPot) && isPot is IndoorPot pot)
            {
                if (pot.bush.Value is not null || pot.hoeDirt.Value.crop is not null)
                {
                    Game1.playSound("cancel");
                    return;
                }
            }
            if (TargetLocation.objects.TryGetValue(TilePosition, out var oldPot))
            {
                if (oldPot is IndoorPot pot1 && pot1.hoeDirt.Value.crop is not null)
                {
                    pot1.hoeDirt.Value.crop = null;
                }
                else
                {
                    TargetObject = null;
                    Game1.playSound("dwop");
                    return;
                }
            }
            else if (TargetLocation.terrainFeatures.TryGetValue(TilePosition, out var oldHoeDirt))
            {
                if (oldHoeDirt is HoeDirt hoeDirt && hoeDirt.crop is not null)
                {
                    hoeDirt.crop = null;
                }
                else
                {
                    TargetObject = null;
                    Game1.playSound("dwop");
                    return;
                }
            }
            else
            {
                TargetObject = null;
                Game1.playSound("dwop");
                return;
            }
            if (location.objects.TryGetValue(tile, out var newPot) && newPot is IndoorPot pot2)
            {
                pot2.hoeDirt.Value.crop = crop;
                pot2.hoeDirt.Value.applySpeedIncreases(Game1.player);
                pot2.hoeDirt.Value.crop.updateDrawMath(tile);
                TargetObject = null;
            }
            else if (location.terrainFeatures.TryGetValue(tile, out var newHoeDirt) && newHoeDirt is HoeDirt hoeDirt2)
            {
                hoeDirt2.crop = crop;
                hoeDirt2.applySpeedIncreases(Game1.player);
                hoeDirt2.crop.updateDrawMath(tile);
                TargetObject = null;
            }
        }

        private void MoveBuilding(Building building, GameLocation location, Vector2 tile, bool overwriteTile)
        {
            if (building.daysOfConstructionLeft.Value > 0 || building.daysUntilUpgrade.Value > 0)
            {
                Game1.addHUDMessage(new(I18n.Message("UnderConstruction"), 3));
                Game1.playSound("cancel");
                TargetObject = null;
                return;
            }
            if (location.IsBuildableLocation())
            {
                if (location.buildStructure(building, tile - TileOffset, Game1.player, overwriteTile))
                {
                    if (building is ShippingBin shippingBin)
                    {
                        shippingBin.initLid();
                    }
                    if (building is GreenhouseBuilding)
                    {
                        Game1.getFarm().greenhouseMoved.Value = true;
                    }
                    building.performActionOnBuildingPlacement();
                    TargetObject = null;
                }
                else
                {
                    Game1.playSound("cancel");
                }
            }
        }

        private void MoveResourceClump(ResourceClump resourceClump, GameLocation location, Vector2 tile)
        {
            int index = TargetLocation.resourceClumps.IndexOf(resourceClump);
            if (index >= 0)
            {
                if (location == TargetLocation)
                {
                    location.resourceClumps[index].netTile.Value = tile;
                }
                else
                {
                    TargetLocation.resourceClumps.Remove(resourceClump);
                    location.resourceClumps.Add(resourceClump);
                    int newIndex = location.resourceClumps.IndexOf(resourceClump);
                    location.resourceClumps[newIndex].netTile.Value = tile;
                }
            }
            else
            {
                Game1.playSound("dwop");
            }
            TargetObject = null;
        }
    }
}
