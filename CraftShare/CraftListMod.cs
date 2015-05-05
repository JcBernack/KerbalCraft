using System.IO;
using UnityEngine;

namespace CraftShare
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class CraftListMod
        : MonoBehaviour
    {
        private const ApplicationLauncher.AppScenes VisibleInScenes = ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.VAB;
        
        private MainWindow _window;

        public void Awake()
        {
            // make sure the PluginData folder exists
            Directory.CreateDirectory(ModGlobals.PluginDataPath);
            // instantiate the main window of this mod
            _window = new MainWindow();
            // add a button to the application launcher
            if (ApplicationLauncher.Ready) AddLauncherButton();
            else GameEvents.onGUIApplicationLauncherReady.Add(AddLauncherButton);
        }

        private void AddLauncherButton()
        {
            ApplicationLauncher.Instance.AddModApplication(_window.Show, _window.Hide, null, null, null, null, VisibleInScenes, ModGlobals.TrollfaceTexture);
            GameEvents.onGUIApplicationLauncherReady.Remove(AddLauncherButton);
        }

        public void Start()
        {
            DontDestroyOnLoad(this);
        }
    }
}