using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.GameData.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace LetsMoveIt.TargetData
{
    internal partial class Target
    {
        private static readonly Dictionary<string, Texture2D> TextureCache = [];

        private static Texture2D LoadTextureCached(string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
                return Game1.objectSpriteSheet;

            if (TextureCache.TryGetValue(assetName, out var texture))
                return texture;

            try
            {
                texture = Game1.content.Load<Texture2D>(assetName);
                TextureCache[assetName] = texture;
                return texture;
            }
            catch
            {
                // Fallback auf default SpriteSheet, Logging bei Bedarf
                Monitor?.Log($"Unable to load texture '{assetName}', falling back to object sprite sheet.", LogLevel.Warn);
                return Game1.objectSpriteSheet;
            }
        }

        public void Render(SpriteBatch spriteBatch, GameLocation location, Vector2 tile)
        {
            try
            {
                switch (TargetObject)
                {
                    case Character character:
                        RenderCharacter(spriteBatch, character, location, tile);
                        break;
                    case SObject sObject:
                        RenderSObject(spriteBatch, sObject, location, tile);
                        break;
                    case ResourceClump resourceClump:
                        RenderResourceClump(spriteBatch, resourceClump, location, tile);
                        break;
                    case TerrainFeature terrainFeature:
                        RenderTerrainFeature(spriteBatch, terrainFeature, location, tile);
                        break;
                    case Crop crop:
                        RenderCrop(spriteBatch, crop, location, tile);
                        break;
                    case Building building:
                        RenderBuilding(spriteBatch, building, location, tile);
                        break;
                    default:
                        break;
                }
            }
            catch (System.Exception ex)
            {
                Monitor?.Log($"Fehler beim Rendern: {ex.Message}\n{ex.StackTrace}", LogLevel.Error);
            }
        }

        private void RenderCharacter(SpriteBatch spriteBatch, Character character, GameLocation location, Vector2 tile)
        {
            Rectangle box = character.GetBoundingBox();
            if (TargetObject is Farmer farmer)
            {
                farmer.FarmerRenderer.draw(spriteBatch, farmer, farmer.FarmerSprite.CurrentFrame, new Vector2(Game1.getMouseX() - 32, Game1.getMouseY() - 128), box.Center.Y / 10000f, farmer.FacingDirection == 3);
            }
            else
            {
                bool flip = character.flip;
                if (TargetObject is FarmAnimal)
                    flip = character.FacingDirection == 3;
                character.Sprite.draw(spriteBatch, new Vector2(Game1.getMouseX() - 32, Game1.getMouseY()) + new Vector2(character.GetSpriteWidthForPositioning() * 4 / 2, box.Height / 2), box.Center.Y / 10000f, 0, character.ySourceRectOffset, Color.White, flip, 4f, 0f, true);
            }
        }

        private void RenderSObject(SpriteBatch spriteBatch, SObject sObject, GameLocation location, Vector2 tile)
        {
            //if (sObject is Furniture furniture)
            //{
            //    BoundingBoxTile.Clear();
            //    var f = furniture.GetBoundingBox();
            //    for (int x_offset = 0; x_offset < f.Width / 64; x_offset++)
            //    {
            //        for (int y_offset = 0; y_offset < f.Height / 64; y_offset++)
            //        {
            //            BoundingBoxTile.Add(tile + new Vector2(x_offset, y_offset));
            //        }
            //    }
            //}
            sObject.draw(spriteBatch, (int)tile.X, (int)tile.Y, 0.6f);
        }

        private void RenderResourceClump(SpriteBatch spriteBatch, ResourceClump resourceClump, GameLocation location, Vector2 tile)
        {
            BoundingBoxTile.Clear();
            var rc = resourceClump.getBoundingBox();
            for (int x_offset = 0; x_offset < rc.Width / 64; x_offset++)
            {
                for (int y_offset = 0; y_offset < rc.Height / 64; y_offset++)
                {
                    BoundingBoxTile.Add(tile + new Vector2(x_offset, y_offset));
                }
            }
            if (TargetObject is GiantCrop giantCrop)
            {
                var data = giantCrop.GetData();
                for (int x_offset = 0; x_offset < data.TileSize.X; x_offset++)
                {
                    for (int y_offset = 0; y_offset < data.TileSize.Y; y_offset++)
                    {
                        spriteBatch.Draw(Game1.mouseCursors, tile.ToLocal(x_offset * 64, y_offset * 64), new Rectangle(194, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1);
                    }
                }
                Texture2D texture = LoadTextureCached(data.Texture);
                spriteBatch.Draw(texture, tile.ToLocal(y: -64), new Rectangle(data.TexturePosition.X, data.TexturePosition.Y, 16 * data.TileSize.X, 16 * (data.TileSize.Y + 1)), Color.White * 0.6f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1);
            }
            else
            {
                string textureName = resourceClump.textureName.Value;
                Texture2D texture = (textureName != null) ? LoadTextureCached(textureName) : Game1.objectSpriteSheet;
                Rectangle sourceRect = Game1.getSourceRectForStandardTileSheet(texture, resourceClump.parentSheetIndex.Value, 16, 16);
                sourceRect.Width = resourceClump.width.Value * 16;
                sourceRect.Height = resourceClump.height.Value * 16;
                spriteBatch.Draw(texture, tile.ToLocal(), sourceRect, Color.White * 0.6f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1);
            }
        }

        private void RenderTerrainFeature(SpriteBatch spriteBatch, TerrainFeature terrainFeature, GameLocation location, Vector2 tile)
        {
            BoundingBoxTile.Clear();
            var tf = terrainFeature.getBoundingBox();
            for (int x_offset = 0; x_offset < tf.Width / 64; x_offset++)
            {
                BoundingBoxTile.Add(tile + new Vector2(x_offset, 0));
                spriteBatch.Draw(Game1.mouseCursors, tile.ToLocal(x_offset * 64), new Rectangle(194, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1);
            }
            if (TargetObject is Bush bush)
            {
                if (!bush.modData.Any())
                {
                    Texture2D texture = LoadTextureCached("TileSheets\\bushes");
                    SpriteEffects flipped = bush.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                    int tileOffset = (bush.sourceRect.Height / 16 - 1) * -64;
                    spriteBatch.Draw(texture, tile.ToLocal(y: tileOffset), bush.sourceRect.Value, Color.White * 0.6f, 0f, Vector2.Zero, 4f, flipped, 1);
                }
            }
            else if (TargetObject is Flooring flooring)
            {
                Texture2D texture = flooring.GetTexture();
                Point textureCorner = flooring.GetTextureCorner();
                spriteBatch.Draw(texture, tile.ToLocal(), new Rectangle(textureCorner.X, textureCorner.Y, 16, 16), Color.White * 0.5f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1);
            }
            else if (TargetObject is HoeDirt hoeDirt)
            {
                string texKey = (location.Name.Equals("Mountain") || location.Name.Equals("Mine") || (location is MineShaft ms && ms.shouldShowDarkHoeDirt()) || location is VolcanoDungeon) ? "TerrainFeatures\\hoeDirtDark" : "TerrainFeatures\\hoeDirt";
                if ((location.GetSeason() == Season.Winter && !location.SeedsIgnoreSeasonsHere() && location is not MineShaft) || (location is MineShaft mineShaft2 && mineShaft2.shouldUseSnowTextureHoeDirt()))
                {
                    texKey = "TerrainFeatures\\hoeDirtSnow";
                }
                Texture2D texture = LoadTextureCached(texKey);
                spriteBatch.Draw(texture, tile.ToLocal(), new Rectangle(0, 0, 16, 16), Color.White * 0.5f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1);
                if (hoeDirt?.crop is not null)
                {
                    RenderCrop(spriteBatch, hoeDirt.crop, location, tile);
                }
            }
            else if (TargetObject is Grass grass)
            {
                Texture2D texture = grass.texture.Value;
                int grassSourceOffset = grass.grassSourceOffset.Value;
                spriteBatch.Draw(texture, tile.ToLocal(y: -16), new Rectangle(0, grassSourceOffset, 15, 20), Color.White * 0.5f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1);
            }
            else if (TargetObject is FruitTree fruitTree)
            {
                Texture2D texture = fruitTree.texture;
                SpriteEffects flipped = fruitTree.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                int growthStage = fruitTree.growthStage.Value;
                int spriteRowNumber = fruitTree.GetSpriteRowNumber();
                int seasonIndexForLocation = Game1.GetSeasonIndexForLocation(location);
                bool flag = fruitTree.IgnoresSeasonsHere();
                if (fruitTree.stump.Value)
                {
                    spriteBatch.Draw(texture, tile.ToLocal(-64, -64), new Rectangle(8 * 48, spriteRowNumber * 5 * 16 + 48, 48, 32), Color.White * 0.5f, 0f, Vector2.Zero, 4f, flipped, 1);
                }
                else
                {
                    spriteBatch.Draw(texture, tile.ToLocal(-64, -256), new Rectangle(((flag ? 1 : seasonIndexForLocation) + System.Math.Min(growthStage, 4)) * 48, spriteRowNumber * 5 * 16, 48, 80), Color.White * 0.5f, 0f, Vector2.Zero, 4f, flipped, 1);
                }
            }
            else if (TargetObject is Tree tree)
            {
                Texture2D texture = tree.texture.Value;
                Rectangle treeTopSourceRect = new(0, 0, 48, 96);
                Rectangle stumpSourceRect = new(32, 96, 16, 32);
                SpriteEffects flipped = tree.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                int growthStage = tree.growthStage.Value;
                int seasonIndexForLocation = Game1.GetSeasonIndexForLocation(location);
                if (tree.hasMoss.Value)
                {
                    treeTopSourceRect.X += 96;
                    stumpSourceRect.X += 96;
                }
                if (tree.stump.Value)
                {
                    spriteBatch.Draw(texture, tile.ToLocal(y: -64), stumpSourceRect, Color.White * 0.5f, 0f, Vector2.Zero, 4f, flipped, 1);
                }
                else if (growthStage < 5)
                {
                    Rectangle value = growthStage switch
                    {
                        0 => new Rectangle(32, 128, 16, 16),
                        1 => new Rectangle(0, 128, 16, 16),
                        2 => new Rectangle(16, 128, 16, 16),
                        _ => new Rectangle(0, 96, 16, 32),
                    };
                    spriteBatch.Draw(texture, tile.ToLocal(y: growthStage >= 3 ? -64 : 0), value, Color.White * 0.5f, 0f, Vector2.Zero, 4f, flipped, 1);
                }
                else
                {
                    spriteBatch.Draw(texture, tile.ToLocal(y: -64), stumpSourceRect, Color.White * 0.5f, 0f, Vector2.Zero, 4f, flipped, 1);
                    spriteBatch.Draw(texture, tile.ToLocal(-64, -320), treeTopSourceRect, Color.White * 0.5f, 0f, Vector2.Zero, 4f, flipped, 1);
                }
            }
        }

        private void RenderCrop(SpriteBatch spriteBatch, Crop crop, GameLocation location, Vector2 tile)
        {
            if (crop.whichForageCrop.Value == "2")
            {
                crop.draw(spriteBatch, tile, Color.White * 0.6f, 0f);
            }
            else
            {
                crop.drawWithOffset(spriteBatch, tile, Color.White * 0.6f, 0f, new Vector2(32));
            }
        }
        
        private void RenderBuilding(SpriteBatch spriteBatch, Building building, GameLocation location, Vector2 tile)
        {
            for (int y = 0; y < building.tilesHigh.Value; y++)
            {
                for (int x = 0; x < building.tilesWide.Value; x++)
                {
                    int redTileIndex = building.getTileSheetIndexForStructurePlacementTile(x, y);
                    Vector2 buildigTiles = tile + new Vector2(x, y) - TileOffset;
                    if (!Game1.currentLocation.isBuildable(buildigTiles))
                    {
                        redTileIndex++;
                    }

                    spriteBatch.Draw(Game1.mouseCursors, buildigTiles.ToLocal(), new Rectangle(194 + redTileIndex * 16, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
                }
            }

            foreach (BuildingPlacementTile additionalPlacementTile in building.GetAdditionalPlacementTiles())
            {
                bool onlyNeedsToBePassable2 = additionalPlacementTile.OnlyNeedsToBePassable;
                foreach (Point point in additionalPlacementTile.TileArea.GetPoints())
                {
                    int x2 = point.X;
                    int y2 = point.Y;
                    int redTileIndex = building.getTileSheetIndexForStructurePlacementTile(x2, y2);
                    Vector2 additionalTiles = tile + new Vector2(x2, y2) - TileOffset;
                    if (!Game1.currentLocation.isBuildable(additionalTiles, onlyNeedsToBePassable2))
                    {
                        redTileIndex++;
                    }

                    spriteBatch.Draw(Game1.mouseCursors, additionalTiles.ToLocal(), new Rectangle(194 + redTileIndex * 16, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
                }
            }
        }
    }
}
