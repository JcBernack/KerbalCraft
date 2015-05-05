using UnityEngine;

namespace CraftShare
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class CraftListMod
        : MonoBehaviour
    {
        private const ApplicationLauncher.AppScenes VisibleInScenes = ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.VAB;
        
        private MainWindow _window;

        private ApplicationLauncherButton _launcherButton;

        public void Awake()
        {
            _window = new MainWindow();
            // add a button to the application launcher
            if (ApplicationLauncher.Ready) AddLauncherButton();
            else GameEvents.onGUIApplicationLauncherReady.Add(AddLauncherButton);
        }

        private void AddLauncherButton()
        {
            _launcherButton = ApplicationLauncher.Instance.AddModApplication(_window.Show, _window.Hide, null, null, null, null, VisibleInScenes, ModGlobals.TrollfaceTexture);
            GameEvents.onGUIApplicationLauncherReady.Remove(AddLauncherButton);
        }

        public void Start()
        {
            DontDestroyOnLoad(this);
        }
    }
}