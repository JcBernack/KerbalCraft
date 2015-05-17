using System.IO;
using UnityEngine;

namespace KerbalCraft
{
    /// <summary>
    /// Holds all the static data and configuration of the mod.
    /// </summary>
    public static class ModGlobals
    {
        public const string ModName = "KerbalCraft";
        public const int ThumbnailResolution = 256;

        public static readonly string PluginDataPath;

        public static Texture2D IconSmall { get; private set; }
        public static Texture2D IconLarge { get; private set; }
        public static Texture2D TransparentTexture { get; private set; }
        public static GUIStyle HeadStyle { get; private set; }
        public static GUIStyle RowStyle { get; private set; }
        public static GUIStyle MessageStyle { get; private set; }

        private static bool _initializedGUI;

        static ModGlobals()
        {
            PluginDataPath = "GameData/" + ModName + "/PluginData/";
            // make sure the PluginData folder exists
            Directory.CreateDirectory(PluginDataPath);
            // load icons, small for the application launcher, large as a placeholder for missing or loading thumbnails
            IconSmall = GameDatabase.Instance.GetTexture(ModName + "/Data/IconSmall", false);
            IconLarge = GameDatabase.Instance.GetTexture(ModName + "/Data/IconLarge", false);
        }

        /// <summary>
        /// Initialize GUI related data. Must be called within the OnGUI() call.
        /// </summary>
        public static void InitializeGUI()
        {
            if (_initializedGUI) return;
            _initializedGUI = true;
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
            MessageStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold
            };
        }
    }
}