using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.NewStuff.Board.BoardSettings
{
    public class DemoBoardSettings : ScriptableObject
    {
        [Header("Demo Map Generation Settings")]
        [Tooltip("Total rows in the generated board")]
        public int Rows;
        [Tooltip("Total cols in the generated board")]
        public int Cols;
        [Tooltip("TEMPORARY floor sprite to use in map gen")]
        public Sprite FloorSprite;
    }

#if UNITY_EDITOR
    public static class DemoBoardSettingsMenuItem
    {
        [UnityEditor.MenuItem("Tools/Create/BoardSettings/DemoBoardSettings")]
        public static void CreateAsset()
        {
            string path = UnityEditor.AssetDatabase.GetAssetPath(UnityEditor.Selection.activeObject);
            string assetPath = path + "/BlankDungeonBoardSettings.asset";
            DemoBoardSettings item = ScriptableObject.CreateInstance<DemoBoardSettings>();
            UnityEditor.ProjectWindowUtil.CreateAsset(item, assetPath);
        }
    }
#endif
}
