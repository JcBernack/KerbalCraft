using System;
using System.IO;
using UnityEngine;

namespace KerbalCraft
{
    public static class ModSettings
    {
        public static string HostAddress { get; private set; }
        public static string Username { get; private set; }
        public static string Password { get; private set; }

        public static event Action ConfigSaved;

        private static readonly string ConfigPath;

        static ModSettings()
        {
            ConfigPath = Path.Combine(ModGlobals.PluginDataPath, "config.cfg");
            // set default config
            SetConfig(null, null, null);
            // try to load saved config
            LoadConfig();
        }

        public static void SetConfig(string hostAddress, string username, string password)
        {
            HostAddress = hostAddress ?? "localhost:10412";
            Username = username ?? "";
            Password = password ?? "";
            RestApi.SetConfig(HostAddress, Username, Password);
        }

        /// <summary>
        /// Loads the configuration file and applies the values.
        /// </summary>
        public static void LoadConfig()
        {
            // try to parse values from config
            var config = ConfigNode.Load(ConfigPath);
            if (config != null)
            {
                var hostAddress = config.GetValue("HostAddress");
                var username = config.GetValue("Username");
                var password = config.GetValue("Password");
                Debug.Log("[KerbalCraft] configuration loaded");
                SetConfig(hostAddress, username, password);
            }
            else
            {
                Debug.LogWarning(string.Format("[KerbalCraft] failed to load configuration from {0}", ConfigPath));
            }
        }

        /// <summary>
        /// Applies the given values and saves them to the configuration file.
        /// </summary>
        public static void SaveConfig()
        {
            try
            {
                var config = new ConfigNode();
                config.AddValue("HostAddress", HostAddress);
                config.AddValue("Username", Username);
                config.AddValue("Password", Password);
                config.Save(ConfigPath);
                Debug.Log("[KerbalCraft] configuration saved");
            }
            catch (Exception ex)
            {
                Debug.LogWarning(string.Format("[KerbalCraft] failed to save configuration to {0}", ConfigPath));
                Debug.LogException(ex);
            }
            if (ConfigSaved != null) ConfigSaved();
        }
    }
}
