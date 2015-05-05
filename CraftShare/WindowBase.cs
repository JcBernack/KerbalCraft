using UnityEngine;

namespace CraftShare
{
    public abstract class WindowBase
    {
        protected bool Initialized { get; private set; }

        public void Show()
        {
            RenderingManager.AddToPostDrawQueue(0, OnGUI);
        }

        public void Hide()
        {
            RenderingManager.RemoveFromPostDrawQueue(0, OnGUI);
        }

        private void OnGUI()
        {
            GUI.skin = HighLogic.Skin;
            if (!Initialized)
            {
                Initialized = true;
                Initialize();
            }
            DrawGUI();
        }

        protected virtual void Initialize() { }

        protected abstract void DrawGUI();
    }
}