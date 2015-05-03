using UnityEngine;

namespace CraftShare
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class Manager
        : MonoBehaviour
    {
        private Rect _windowRect = new Rect(50, 50, 0, 0);
        private const float WindowWidth = 400;

        public void Awake()
        {
            var buttonTexture = GameDatabase.Instance.GetTexture("CraftShare/Data/ModButton", false);
            var modApp = ApplicationLauncher.Instance.AddModApplication(OnTrue, OnFalse, null, null, null, null, ApplicationLauncher.AppScenes.ALWAYS, buttonTexture);
        }

        private void OnTrue()
        {
            RenderingManager.AddToPostDrawQueue(0, DrawWindow);
        }

        private void OnFalse()
        {
            RenderingManager.RemoveFromPostDrawQueue(0, DrawWindow);
        }

        private void DrawWindow()
        {
            GUI.skin = HighLogic.Skin;
            _windowRect = GUILayout.Window(0, _windowRect, DrawMenu, "CraftShare");
        }

        private void DrawMenu(int id)
        {
            GUILayout.BeginVertical(GUILayout.Width(WindowWidth));
            GUILayout.Label("bla bla bla bla");
            GUILayout.Label("herpidy derpidy");
            GUILayout.EndVertical();
            GUI.DragWindow();
        }
    }
}
