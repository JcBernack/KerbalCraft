using System.IO;
using UnityEngine;

namespace CraftShare
{
    public static class ModGlobals
    {
        public static string PluginDataPath = "GameData/CraftShare/PluginData/";
        public static string ConfigPath = Path.Combine(PluginDataPath, "config.cfg");

        public static Texture2D TransparentTexture;
        public static Texture2D TrollfaceTexture;
        public static GUIStyle HeadStyle;
        public static GUIStyle RowStyle;

        static ModGlobals()
        {
            // create a 1x1 transparent texture
            TransparentTexture = new Texture2D(1, 1);
            TransparentTexture.SetPixel(0, 0, new Color(0, 0, 0, 0));
            TransparentTexture.Apply();
            // load trollface
            TrollfaceTexture = GameDatabase.Instance.GetTexture("CraftShare/Data/ModButton", false);
            // define gui styles for table headers and rows
            HeadStyle = new GUIStyle(HighLogic.Skin.label)
            {
                fontStyle = FontStyle.Bold,
                wordWrap = false
            };
            RowStyle = new GUIStyle(HighLogic.Skin.label)
            {
                wordWrap = false,
                hover =
                {
                    background = TransparentTexture,
                    textColor = Color.white
                }
            };
        }
    }
}