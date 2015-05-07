﻿using System.IO;
using UnityEngine;

namespace CraftShare
{
    /// <summary>
    /// Holds all the static data and configuration of the mod.
    /// </summary>
    public static class ModGlobals
    {
        public const int ThumbnailResolution = 256;

        public static MainWindow MainWindow { get; private set; }
        public static SettingsWindow SettingsWindow { get; private set; }

        public static string PluginDataPath { get; private set; }
        public static string ConfigPath { get; private set; }
        
        public static string HostAddress { get; private set; }
        public static string AuthorName { get; private set; }

        public static Texture2D IconSmall { get; private set; }
        public static Texture2D IconLarge { get; private set; }
        public static Texture2D TransparentTexture { get; private set; }
        public static GUIStyle HeadStyle { get; private set; }
        public static GUIStyle RowStyle { get; private set; }

        private static bool _initialized;
        private static bool _initializedGUI;

        /// <summary>
        /// Initialize static data.
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;
            PluginDataPath = "GameData/CraftShare/PluginData/";
            // make sure the PluginData folder exists
            Directory.CreateDirectory(PluginDataPath);
            ConfigPath = Path.Combine(PluginDataPath, "config.cfg");
            IconSmall = GameDatabase.Instance.GetTexture("CraftShare/Data/IconSmall", false);
            IconLarge = GameDatabase.Instance.GetTexture("CraftShare/Data/IconLarge", false);
            LoadConfig();
            // create windows
            MainWindow = new MainWindow();
            SettingsWindow = new SettingsWindow();
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
        }

        /// <summary>
        /// Builds the path to the auto-generated thumbnail for the given ship.
        /// </summary>
        public static string GetCraftThumbnailPath(ShipConstruct ship)
        {
            return string.Format("thumbs/{0}_{1}_{2}.png", HighLogic.SaveFolder, ShipConstruction.GetShipsSubfolderFor(ship.shipFacility), ship.shipName);
        }

        /// <summary>
        /// Loads the configuration file and applies the values.
        /// </summary>
        public static void LoadConfig()
        {
            // set default values
            HostAddress = "localhost:8000";
            AuthorName = HighLogic.SaveFolder;
            // try to parse values from config
            var config = ConfigNode.Load(ConfigPath);
            if (config != null)
            {
                HostAddress = config.GetValue("HostAddress");
                AuthorName = config.GetValue("AuthorName");
                Debug.Log("CraftShare: configuration loaded");
                SetHostAddress(HostAddress);
            }
            else
            {
                Debug.LogWarning(string.Format("CraftShare: failed to load configuration from {0}", ConfigPath));
            }
        }

        /// <summary>
        /// Applies the given values and saves them to the configuration file.
        /// </summary>
        /// <param name="hostAddress"></param>
        /// <param name="authorName"></param>
        public static void SaveConfig(string hostAddress, string authorName)
        {
            var config = new ConfigNode();
            config.AddValue("HostAddress", hostAddress);
            config.AddValue("AuthorName", authorName);
            if (config.Save(ConfigPath))
            {
                HostAddress = hostAddress;
                AuthorName = authorName;
                Debug.Log("CraftShare: configuration saved");
                SetHostAddress(HostAddress);
            }
            else
            {
                Debug.LogWarning(string.Format("CraftShare: failed to save configuration to {0}", ConfigPath));
            }
        }

        private static void SetHostAddress(string hostAddress)
        {
            RestApi.SetHostAddress(HostAddress);
            Debug.Log("CraftShare: changed host to " + hostAddress);
        }
    }
}