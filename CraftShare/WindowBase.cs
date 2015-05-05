using UnityEngine;

namespace CraftShare
{
    public abstract class WindowBase
    {
        protected bool Initialized { get; private set; }

        public void Show()
        {
            RenderingManager.AddToPostDrawQueue(0, OnGUI);
            OnShow();
        }

        public void Hide()
        {
            RenderingManager.RemoveFromPostDrawQueue(0, OnGUI);
            OnHide();
        }

        private void OnGUI()
        {
            if (!Initialized)
            {
                Initialized = true;
                Initialize();
            }
            DrawGUI();
        }

        protected virtual void Initialize() { }
        protected virtual void OnShow() { }
        protected virtual void OnHide() { }

        protected abstract void DrawGUI();
    }
}