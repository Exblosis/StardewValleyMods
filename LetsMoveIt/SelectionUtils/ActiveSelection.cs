using System.Collections.Generic;
using System.Linq;
using LetsMoveIt.TargetData;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Extensions;
using static LetsMoveIt.ConfigUtils;

namespace LetsMoveIt.SelectionUtils
{
    internal class ActiveSelection : AreaSelection
    {
        public Dictionary<Vector2, List<Target>> MultipleTargets { get; protected set; } = [];
        public Target? SingleTarget { get; protected set; }

        public ActiveSelection(GameLocation location, Rectangle area, bool isMultiSelect) : base(location, area, isMultiSelect) { }
        public ActiveSelection(AreaSelection selectedArea) : this(selectedArea.Location, selectedArea.Area, selectedArea.IsMultiSelect) { }

        public static bool TrySelect(GameLocation location, Rectangle area, bool isMultiSelect, out ActiveSelection selection, bool withEntity = false, bool recordHistory = true)
        {
            var active = new ActiveSelection(location, area, isMultiSelect);
            bool found = active.IsMultiSelect
                ? active.SelectMultipleTargets(recordHistory)
                : active.SelectSingleTarget(withEntity, recordHistory);

            if (!found)
            {
                selection = null!;
                return false;
            }
            selection = active;
            return true;
        }

        public static bool TrySelect(AreaSelection selectedArea, out ActiveSelection selection)
        {
            // Reaktivierung: nie erneut in die History schreiben
            return TrySelect(selectedArea.Location, selectedArea.Area, selectedArea.IsMultiSelect, out selection, withEntity: false, recordHistory: false);
        }

        private bool SelectSingleTarget(bool withEntity, bool recordHistory)
        {
            Point map = withEntity ? Mod1.GetGlobalMousePosition() : Point.Zero;
            SingleTarget = Target.Get(Location, Area.GetPosition(), map);
            if (SingleTarget?.TargetObject is null)
            {
                SingleTarget = null;
                return false;
            }
            Mod1.PlaySound();

            bool isEntityTarget = SingleTarget.TargetObject is Farmer or NPC or FarmAnimal;
            if (recordHistory && !isEntityTarget)
                Selection.AddSelectedArea(Location, Area, false);

            return true;
        }

        private bool SelectMultipleTargets(bool recordHistory)
        {
            HashSet<object> seenObjects = [];
            foreach (Vector2 tile in Area.GetVectors())
            {
                List<Target>? targetsOnTile = Target.GetAllTargets(Location, tile, Game1.GlobalToLocal(tile).ToPoint());
                if (targetsOnTile is not null && targetsOnTile.Count > 0)
                {
                    if (!MultipleTargets.TryGetValue(tile, out var list))
                    {
                        list = [];
                        MultipleTargets[tile] = list;
                    }
                    foreach (Target t in targetsOnTile)
                    {
                        if (t.TargetObject is not null && seenObjects.Add(t.TargetObject))
                            list.Add(t);
                    }
                }
            }

            if (MultipleTargets.Count == 0)
                return false;

            var allKeys = MultipleTargets.Keys;
            int minX = (int)allKeys.Min(k => k.X);
            int minY = (int)allKeys.Min(k => k.Y);
            int maxX = (int)allKeys.Max(k => k.X);
            int maxY = (int)allKeys.Max(k => k.Y);
            Area = new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
            Mod1.PlaySound();

            if (recordHistory)
                Selection.AddSelectedArea(Location, Area, true);

            return true;
        }

        public void TargetAction()
        {
            if (IsMultiSelect)
                MultipleTargetsAction();
            else
                SingleTargetAction();
                
        }
        private void SingleTargetAction()
        {
            bool overwriteTile = Config.OverwriteKey.IsDown();

            if (SingleTarget!.IsOccupied(Game1.currentLocation, Game1.currentCursorTile) && !overwriteTile)
            {
                Game1.playSound("cancel");
                return;
            }

            if (Config.CopyMode)
            {
                SingleTarget.CopyTo(Game1.currentLocation, Game1.currentCursorTile, overwriteTile);
            }
            else
            {
                SingleTarget.MoveTo(Game1.currentLocation, Game1.currentCursorTile, overwriteTile);
                Mod1.PlaySound();
            }
        }
        private void MultipleTargetsAction()
        {
            bool overwriteTile = Config.OverwriteKey.IsDown();

            foreach (var targetList in MultipleTargets.Values)
            {
                var toRemove = new List<Target>();

                foreach (var t in targetList)
                {
                    if (t.TargetObject is null)
                        continue;

                    Vector2 targetTile = Game1.currentCursorTile + (t.TilePosition - Area.GetPosition());

                    if (t.IsOccupied(Game1.currentLocation, targetTile) && !overwriteTile)
                        continue;

                    if (Config.CopyMode)
                    {
                        t.CopyTo(Game1.currentLocation, targetTile, overwriteTile);
                    }
                    else
                    {
                        t.MoveTo(Game1.currentLocation, targetTile, overwriteTile);
                    }

                    if (t.TargetObject is null)
                        toRemove.Add(t);
                }

                foreach (var t in toRemove)
                    targetList.Remove(t);
            }

            var emptyKeys = MultipleTargets
                .Where(pair => pair.Value.Count == 0)
                .Select(pair => pair.Key)
                .ToList();

            foreach (var key in emptyKeys)
                MultipleTargets.Remove(key);

            Mod1.PlaySound();
        }

        public void RemoveTargetAction()
        {
            SingleTarget?.Remove();
            foreach (var targetList in MultipleTargets.Values)
            {
                foreach (var t in targetList)
                {
                    if (t.TargetObject is not null)
                        t.Remove();
                }
            }
        }
    }
}
