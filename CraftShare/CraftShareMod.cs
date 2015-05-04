using UnityEngine;

namespace CraftShare
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class CraftShareMod
        : MonoBehaviour
    {
        public static Texture2D TransparentTexture;

        public void Awake()
        {
            // create a 1x1 transparent texture
            var transparentTexture = new Texture2D(1, 1);
            transparentTexture.SetPixel(0, 0, new Color(0, 0, 0, 0));
            transparentTexture.Apply();
            // add a button to the application launcher in the top right corner
            var buttonTexture = GameDatabase.Instance.GetTexture("CraftShare/Data/ModButton", false);
            var window = new MainWindow();
            var modApp = ApplicationLauncher.Instance.AddModApplication(window.Show, window.Hide, null, null, null, null, ApplicationLauncher.AppScenes.ALWAYS, buttonTexture);
        }
    }
}
