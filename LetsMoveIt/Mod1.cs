using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;

namespace LetsMoveIt
{
    internal static class Mod1
    {
        /*********************
         * Drawing Utilities *
         *********************/

        /// <summary>Lazily initializes a 1x1 white pixel texture.</summary>
        private static readonly Lazy<Texture2D> LazyPixel = new(() =>
        {
            Texture2D pixel = new(Game1.graphics.GraphicsDevice, 1, 1);
            pixel.SetData([Color.White]);
            return pixel;
        });
        /// <summary>Get a 1x1 white pixel texture. This is useful for drawing rectangles, lines, and other shapes.</summary>
        public static Texture2D Pixel => LazyPixel.Value;
        /// <summary>Draw the Bounding Box</summary>
        /// <param name="spriteBatch">SpriteBatch</param>
        /// <param name="box">Bounding Box</param>
        /// <param name="color">Color</param>
        public static void DrawBoundingBox(SpriteBatch spriteBatch, Rectangle box, Color color)
        {
            Vector2 startPos = Game1.GlobalToLocal(new Vector2(box.X, box.Y));
            Rectangle vertical = new(0, 0, 1, box.Height);
            Rectangle horizontal = new(0, 0, box.Width, 1);
            spriteBatch.Draw(Pixel, startPos, horizontal, color, 0f, Vector2.Zero, 1, SpriteEffects.None, 1);
            spriteBatch.Draw(Pixel, startPos, vertical, color, 0f, Vector2.Zero, 1, SpriteEffects.None, 1);
            spriteBatch.Draw(Pixel, startPos + new Vector2(0, box.Height - 1), horizontal, color, 0f, Vector2.Zero, 1, SpriteEffects.None, 1);
            spriteBatch.Draw(Pixel, startPos + new Vector2(box.Width - 1, 0), vertical, color, 0f, Vector2.Zero, 1, SpriteEffects.None, 1);
        }

        /*****************************
         * Vector2 Extension Methods *
         *****************************/

        /// <summary>Get the local tile with local offset. Vector2 Extension</summary>
        /// <param name="tile">Tile</param>
        /// <param name="x">Offset X</param>
        /// <param name="y">Offset Y</param>
        public static Vector2 ToLocal(this Vector2 tile, float x = 0, float y = 0)
        {
            return Game1.GlobalToLocal(new Vector2(x, y) + tile * Game1.tileSize);
        }
        /// <summary>Get the local tile with local offset. Vector2 Extension</summary>
        /// <param name="tile">Tile</param>
        /// <param name="offset">Offset</param>
        public static Vector2 ToLocal(this Vector2 tile, Vector2 offset)
        {
            return Game1.GlobalToLocal(offset + tile * Game1.tileSize);
        }

        /*******************
         * Vector2 Methods *
         *******************/

        /// <summary>Get the local cursor tile with local offset.</summary>
        /// <param name="x">Offset X</param>
        /// <param name="y">Offset Y</param>
        public static Vector2 LocalCursorTile(float x = 0, float y = 0)
        {
            return Game1.GlobalToLocal(new Vector2(x, y) + Game1.currentCursorTile * Game1.tileSize);
        }

        /// <summary>Get the local cursor tile with local offset.</summary>
        /// <param name="offset">Offset</param>
        public static Vector2 LocalCursorTile(Vector2 offset)
        {
            return Game1.GlobalToLocal(offset + Game1.currentCursorTile * Game1.tileSize);
        }

        /*******************************
         * Rectangle Extension Methods *
         *******************************/

        /// <summary>One Tile to Rectangle Position with Size 1x1</summary>
        /// <param name="tile">Tile</param>
        public static Rectangle ToRectangle(this Vector2 tile)
        {
            return new Rectangle((int)tile.X, (int)tile.Y, 1, 1);
        }
        /// <summary>Get position of a Rectangle</summary>
        /// <param name="rect">Rectangle</param>
        public static Vector2 GetPosition(this Rectangle rect)
        {
            return new Vector2(rect.X, rect.Y);
        }
        /// <summary>Get size of a Rectangle</summary>
        /// <param name="rect">Rectangle</param>
        public static Vector2 GetSize(this Rectangle rect)
        {
            return new Vector2(rect.Width, rect.Height);
        }

        /*****************
         * Other Methods *
         *****************/

        /// <summary>Get the global Mouse Position.</summary>
        public static Point GetGlobalMousePosition()
        {
            return Game1.getMousePosition() + new Point(Game1.viewport.X, Game1.viewport.Y);
        }
        /// <summary>Get the Yes/No Responses for a QuestionDialogue.</summary>
        public static Response[] YesNoResponses()
        {
            return
            [
            new Response("Yes", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_Yes")).SetHotKey(Keys.Y).SetHotKey(Keys.Enter),
            new Response("No", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No")).SetHotKey(Keys.Escape)
            ];
        }
        /// <summary>Toggle a boolean value and play a sound effect.</summary>
        /// <param name="config"></param>
        public static bool Toggle(this bool config)
        {
            Game1.playSound("drumkit6", !config ? null : 200);
            return !config;
        }
        public static void PlaySound()
        {
            if (!string.IsNullOrEmpty(ConfigUtils.Config.Sound))
                Game1.playSound(ConfigUtils.Config.Sound);
        }
    }
}
