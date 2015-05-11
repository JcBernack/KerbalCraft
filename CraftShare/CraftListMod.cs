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
            // do absolutely nothing if the current scene is not an editor
            if (!HighLogic.LoadedSceneIsEditor) return;
            // make sure global ui elements are initialized
            ModGlobals.InitializeGUI();
            // handle asynchronous responses
            RestApi.HandleResponses();
        }

        private void AddLauncherButton()
        {
            GameEvents.onGUIApplicationLauncherReady.Remove(AddLauncherButton);
            AppLauncherButton = ApplicationLauncher.Instance.AddModApplication(OnTrue, OnFalse, null, null, null, OnDisable, VisibleInScenes, ModGlobals.IconSmall);
            ModGlobals.MainWindow.Hide += OnHide;
            ModGlobals.SettingsWindow.Hide += OnHide;
        }

        /// <summary>
        /// Make sure the application launcher button is in its false state when both windows are closed.
        /// </summary>
        private void OnHide()
        {
            if (!ModGlobals.MainWindow.Visible && !ModGlobals.SettingsWindow.Visible)
            {
                OnDisable();
            }
        }

        /// <summary>
        /// Open window when the application launcher button is pressed. If no settings were found the settings window is opened, otherwise the main window.
        /// </summary>
        private void OnTrue()
        {
            if (ModGlobals.SettingsLoaded)
            {
                ModGlobals.MainWindow.Open();
            }
            else
            {
                ModGlobals.SettingsWindow.Open();
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