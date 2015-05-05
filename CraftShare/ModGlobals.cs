using System.IO;
using UnityEngine;

namespace CraftShare
{
    public static class ModGlobals
    {
        public static string PluginDataPath = "GameData/CraftShare/PluginData/";
        public static string ConfigPath = Path.Combine(PluginDataPath, "config.cfg");
        public const int ThumbnailResolution = 256;

        public static Texture2D IconSmall;
        public static Texture2D IconLarge;

        public static Texture2D TransparentTexture;
        public static GUIStyle HeadStyle;
        public static GUIStyle RowStyle;

        public static void Initialize()
        {
            IconSmall = GameDatabase.Instance.GetTexture("CraftShare/Data/IconSmall", false);
            IconLarge = GameDatabase.Instance.GetTexture("CraftShare/Data/IconLarge", false);
        }

        public static void InitializeGUI()
        {
            // create a 1x1 transparent texture
            TransparentTexture = new Texture2D(1, 1);
            TransparentTexture.SetPixel(0, 0, new Color(0, 0, 0, 0));
            TransparentTexture.Apply();
            // define gui styles for table headers and rows
            HeadStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                wordWrap = false
            };
            RowStyle = new GUIStyle(GUI.skin.label)
            {
                wordWrap = false,
                hover =
                {
                    background = TransparentTexture,
                    textColor = Color.white
                }
            };
        }
        
        public static string GetCraftThumbnailPath(ShipConstruct ship)
        {
            return string.Format("thumbs/{0}_{1}_{2}.png", HighLogic.SaveFolder, ShipConstruction.GetShipsSubfolderFor(ship.shipFacility), ship.shipName);
        }
    }
}