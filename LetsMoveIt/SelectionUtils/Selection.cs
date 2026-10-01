using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace LetsMoveIt.SelectionUtils
{
    internal static class Selection
    {
        //private static IDataHelper Data = null!;
        //private static Dictionary<string, string> BlueprintInfo = [];
        public static List<AreaSelection> AreaSelectionList { get; private set; } = [];
        public static ActiveSelection? CurrentSelection { get; private set; }

        /// <summary>Use in ModEntry.Entry() | Only for set values.</summary>
        //public static void Init(IDataHelper data)
        //{
        //    Data = data ?? throw new ArgumentNullException(nameof(data));
        //    LoadBlueprintInfo();
        //}

        public static void ClearSelections()
        {
            AreaSelectionList.Clear();
        }
        public static void ClearActiveSelection()
        {
            CurrentSelection = null;
        }

        public static bool TrySelect(GameLocation location, Rectangle area, bool isMultiSelect, bool withEntity = false, bool recordHistory = true)
        {
            if (ActiveSelection.TrySelect(location, area, isMultiSelect, out var selection, withEntity, recordHistory))
            {
                CurrentSelection = selection;
                return true;
            }
            return false;
        }

        public static bool TrySelect(AreaSelection selectedArea)
        {
            if (ActiveSelection.TrySelect(selectedArea, out var selection))
            {
                CurrentSelection = selection;
                return true;
            }
            return false;
        }

        public static void AddSelectedArea(GameLocation location, Rectangle area, bool isMultiSelect = false)
        {
            AreaSelectionList.Add(new AreaSelection(location, area, isMultiSelect));
        }

        public static void Draw(SpriteBatch spriteBatch, GameLocation location, Vector2 tile)
        {
            if (CurrentSelection is null)
                return;

            if (CurrentSelection.SingleTarget is not null)
            {
                if (CurrentSelection.SingleTarget.TargetObject is null)
                {
                    ClearActiveSelection();
                    return;
                }
                CurrentSelection.SingleTarget.Render(spriteBatch, location, tile);
            }
            if (CurrentSelection.MultipleTargets.Count > 0)
            {
                foreach (var targetList in CurrentSelection.MultipleTargets.Values)
                {
                    foreach (var t in targetList)
                    {
                        if (t.TargetObject is null)
                            continue;
                        Vector2 renderTile = tile + (t.TilePosition - new Vector2(CurrentSelection.Area.X, CurrentSelection.Area.Y));
                        t.Render(spriteBatch, location, renderTile);
                    }
                }
            }
            else if (CurrentSelection.SingleTarget is null)
            {
                ClearActiveSelection();
            }
        }

        //private static void LoadBlueprintInfo()
        //{
        //    BlueprintInfo = Data.ReadJsonFile<Dictionary<string, string>>("./ModStorage/Blueprint/BlueprintInfo.json") ?? [];
        //}

        //public static BlueprintSelection? LoadBlueprint(string name)
        //{
        //    if (string.IsNullOrEmpty(name)) return null;
        //    if (BlueprintInfo.ContainsKey(name))
        //    {
        //        return Data.ReadJsonFile<BlueprintSelection>($"./ModStorage/Blueprint/{name}.json");
        //    }
        //    return null;
        //}

        //public static void SaveRemoveBlueprint(string name, ActiveSelection? selection, string info)
        //{
        //    if (string.IsNullOrEmpty(name)) return;
        // // Location.NameOrUniqueName - Speichern
        // // Game1.getLocationFromName(...) - Laden
        //    Data.WriteJsonFile<BlueprintSelection>($"./ModStorage/Blueprint/{name}.json", bpSelection);
        //    if (selection is null)
        //    {
        //        BlueprintInfo.Remove(name);
        //        Data.WriteJsonFile<Dictionary<string, string>>($"./ModStorage/Blueprint/BlueprintInfo.json", BlueprintInfo);
        //    }
        //    else
        //    {
        //        BlueprintInfo.Add(name, info);
        //        Data.WriteJsonFile<Dictionary<string, string>>($"./ModStorage/Blueprint/BlueprintInfo.json", BlueprintInfo);
        //    }
        //}
    }
}
