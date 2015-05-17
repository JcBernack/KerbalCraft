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
        private ApplicationLauncherButton _appLauncherButton;
        private MainWindow _mainWindow;
        private SettingsWindow _settingsWindow;

        public void Awake()
        {
            // create windows
            _settingsWindow = new SettingsWindow();
            _mainWindow = new MainWindow();
            // hook up events
            _settingsWindow.Hide += OnHide;
            _mainWindow.Hide += OnHide;
            _mainWindow.SettingsClicked += _settingsWindow.Open;
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
            _mainWindow.OnGUI();
            _settingsWindow.OnGUI();
        }

        private void AddLauncherButton()
        {
            GameEvents.onGUIApplicationLauncherReady.Remove(AddLauncherButton);
            Debug.Log("[KerbalCraft] adding button to ApplicationLauncher");
            _appLauncherButton = ApplicationLauncher.Instance.AddModApplication(OnTrue, OnFalse, null, null, null, OnDisable,
                ApplicationLauncher.AppScenes.ALWAYS, ModGlobals.IconSmall);
        }

        /// <summary>
        /// Make sure the application launcher button is in its false state when both windows are closed.
        /// </summary>
        private void OnHide()
        {
            if (!_mainWindow.Visible && !_settingsWindow.Visible)
            {
                OnDisable();
            }
        }

        /// <summary>
        /// Open window when the application launcher button is pressed. If no settings were found the settings window is opened, otherwise the main window.
        /// </summary>
        private void OnTrue()
        {
            _mainWindow.Open();
        }

        private void OnFalse()
        {
            _mainWindow.Close();
            _settingsWindow.Close();
        }

        private void OnDisable()
        {
            // make sure the button is in its "false" state
            if (_appLauncherButton != null) _appLauncherButton.SetFalse();
        }
    }
}