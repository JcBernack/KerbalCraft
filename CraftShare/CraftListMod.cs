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

        public void OnGUI()
        {
            ModGlobals.InitializeGUI();
        }

        private void AddLauncherButton()
        {
            GameEvents.onGUIApplicationLauncherReady.Remove(AddLauncherButton);
            AppLauncherButton = ApplicationLauncher.Instance.AddModApplication(OnTrue, OnFalse, null, null, null, OnDisable, VisibleInScenes, ModGlobals.IconSmall);
            ModGlobals.MainWindow.Hide += OnHide;
            ModGlobals.SettingsWindow.Hide += OnHide;
        }

        private void OnHide()
        {
            if (!ModGlobals.MainWindow.Visible && !ModGlobals.SettingsWindow.Visible)
            {
                OnDisable();
            }
        }

        private void OnTrue()
        {
            ModGlobals.SettingsChange -= OnTrue;
            if (ModGlobals.SettingsLoaded)
            {
                ModGlobals.MainWindow.Open();
            }
            else
            {
                ModGlobals.SettingsWindow.Open();
                ModGlobals.SettingsChange += OnTrue;
            }
        }

        private void OnFalse()
        {
            ModGlobals.MainWindow.Close();
            ModGlobals.SettingsWindow.Close();
        }

        private void OnDisable()
        {
            // make sure the button is in its "false" state
            if (AppLauncherButton != null) AppLauncherButton.SetFalse();
        }
    }
}