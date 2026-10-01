using System;
using LetsMoveIt.SelectionUtils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.TokenizableStrings;

namespace LetsMoveIt.Menus
{
    internal class SelectionElement : AreaSelection
    {
        // Action to perform when the element is clicked
        private readonly Action<AreaSelection> ClickAction;

        // Element bounds for click detection
        public Rectangle Bounds;

        public SelectionElement(AreaSelection selection, Rectangle bounds, Action<AreaSelection> clickAction) : base(selection)
        {
            Bounds = bounds;
            ClickAction = clickAction;
        }

        public void receiveLeftClick(int x, int y)
        {
            if (Bounds.Contains(x, y) && ClickAction != null)
            {
                ClickAction(this);
            }
        }

        public void draw(SpriteBatch b, int slotX, int slotY)
        {
            Vector2 namesize = Game1.smallFont.MeasureString(Location.Name);
            if (Location.parentLocationName.Value is not null)
            {
                var building = Location.GetParentLocation()?.getBuildingByName(Location.NameOrUniqueName);
                string buildingName = building is not null ? (TokenParser.ParseText(building.GetData().Name) ?? "-") : "-";
                b.DrawString(Game1.smallFont, Location.DisplayName, new Vector2(slotX + 16, slotY + 4), Game1.textColor);
                b.DrawString(Game1.smallFont, buildingName, new Vector2(slotX + 16, slotY + namesize.Y - 4), Game1.textColor);
            }
            else
            {
                b.DrawString(Game1.smallFont, Location.DisplayName, new Vector2(slotX + 16, slotY + namesize.Y / 2), Game1.textColor);
            }            

            if (IsMultiSelect)
            {
                b.DrawString(Game1.smallFont, "From:", new Vector2(slotX + 320 + 16, slotY + 4), Game1.textColor);
                b.DrawString(Game1.smallFont, $"X:{Area.X} Y:{Area.Y}", new Vector2(slotX + 320 + 96, slotY + 4), Game1.textColor);
                b.DrawString(Game1.smallFont, "To:", new Vector2(slotX + 320 + 16, slotY + namesize.Y - 4), Game1.textColor);
                b.DrawString(Game1.smallFont, $"X:{Area.X + Area.Width - 1} Y:{Area.Y + Area.Height - 1}", new Vector2(slotX + 320 + 96, slotY + namesize.Y - 4), Game1.textColor);
            }
            else
            {
                b.DrawString(Game1.smallFont, "Pos:", new Vector2(slotX + 320 + 16, slotY + namesize.Y / 2), Game1.textColor);
                b.DrawString(Game1.smallFont, $"X:{Area.X} Y:{Area.Y}", new Vector2(slotX + 320 + 96, slotY + namesize.Y / 2), Game1.textColor);
            }
        }
    }
}
