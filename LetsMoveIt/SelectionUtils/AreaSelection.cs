using Microsoft.Xna.Framework;
using StardewValley;

namespace LetsMoveIt.SelectionUtils
{
    internal class AreaSelection
    {
        public GameLocation Location { get; protected set; }
        public Rectangle Area { get; protected set; }
        public bool IsMultiSelect { get; protected set; } = false;
        public AreaSelection(GameLocation location, Rectangle area, bool isMultiSelect = false)
        {
            Area = area;
            Location = location;
            IsMultiSelect = isMultiSelect;
        }
        public AreaSelection(AreaSelection selection) : this(selection.Location, selection.Area, selection.IsMultiSelect) { }
    }
}
