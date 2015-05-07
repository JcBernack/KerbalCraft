using UnityEngine;

namespace CraftShare
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class CraftListMod
        : MonoBehaviour
    {
        private const ApplicationLauncher.AppScenes VisibleInScenes = ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.VAB;

        public ApplicationLauncherButton AppLauncherButton;

        public void Awake()
        {
            // initialize globally used objects
            ModGlobals.Initialize();
            // add a button to the application launcher
            if (ApplicationLauncher.Ready) AddLauncherButton();
            else GameEvents.onGUIApplicationLauncherReady.Add(AddLauncherButton);
        }

        public void Start()
        {
            DontDestroyOnLoad(this);
        }

        private void AddLauncherButton()
        {
            GameEvents.onGUIApplicationLauncherReady.Remove(AddLauncherButton);
            AppLauncherButton = ApplicationLauncher.Instance.AddModApplication(ModGlobals.MainWindow.Open, ModGlobals.MainWindow.Close,
                null, null, null, OnDisable, VisibleInScenes, ModGlobals.IconSmall);
        }

        private void OnDisable()
        {
            // make sure the windows are properly closed
            ModGlobals.MainWindow.Close();
            ModGlobals.SettingsWindow.Close();
            // and the button is in its "false" state
            if (AppLauncherButton != null) AppLauncherButton.SetFalse();
        }
    }
}