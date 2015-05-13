using UnityEngine;

namespace KerbalCraft
{
#if DEBUG
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
#else
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
#endif
    public class KerbalCraftMod
        : MonoBehaviour
    {
        private const ApplicationLauncher.AppScenes VisibleInScenes = ApplicationLauncher.AppScenes.ALWAYS;

        public ApplicationLauncherButton AppLauncherButton;

        public void Awake()
        {
            // initialize globally used objects
            ModGlobals.Initialize();
            // hook up events
            ModGlobals.MainWindow.Hide += OnHide;
            ModGlobals.SettingsWindow.Hide += OnHide;
            // add a button to the application launcher
            if (ApplicationLauncher.Ready) AddLauncherButton();
            else GameEvents.onGUIApplicationLauncherReady.Add(AddLauncherButton);
        }

        public void Start()
        {
            DontDestroyOnLoad(this);
#if DEBUG
            OnTrue();
#endif
        }

        public void OnGUI()
        {
            // make sure global ui elements are initialized
            ModGlobals.InitializeGUI();
            // handle asynchronous responses
            RestApi.HandleResponses();
            // render windows
            ModGlobals.MainWindow.OnGUI();
            ModGlobals.SettingsWindow.OnGUI();
        }

        private void AddLauncherButton()
        {
            GameEvents.onGUIApplicationLauncherReady.Remove(AddLauncherButton);
            Debug.Log("[KerbalCraft] adding button to ApplicationLauncher");
            AppLauncherButton = ApplicationLauncher.Instance.AddModApplication(OnTrue, OnFalse, null, null, null, OnDisable, VisibleInScenes, ModGlobals.IconSmall);
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