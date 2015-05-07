using UnityEngine;

namespace CraftShare
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class CraftListMod
        : MonoBehaviour
    {
        private const ApplicationLauncher.AppScenes VisibleInScenes = ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.VAB;

        public void Awake()
        {
            // initialize globally used objects
            ModGlobals.Initialize();
            // add a button to the application launcher
            if (ApplicationLauncher.Ready) AddLauncherButton();
            else GameEvents.onGUIApplicationLauncherReady.Add(AddLauncherButton);
        }

        private void AddLauncherButton()
        {
            ApplicationLauncher.Instance.AddModApplication(ModGlobals.MainWindow.Open, ModGlobals.MainWindow.Close, null, null, null, null, VisibleInScenes, ModGlobals.IconSmall);
            GameEvents.onGUIApplicationLauncherReady.Remove(AddLauncherButton);
        }

        public void Start()
        {
            DontDestroyOnLoad(this);
        }
    }
}