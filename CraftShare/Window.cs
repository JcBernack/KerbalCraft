using UnityEngine;

namespace CraftShare
{
    public abstract class Window
    {
        private Rect _rect = new Rect(50, 50, 0, 0);

        protected bool Initialized { get; private set; }

        protected Window(float left, float top)
        {
            _rect.x = left;
            _rect.y = top;
        }

        public void Show()
        {
            RenderingManager.AddToPostDrawQueue(0, DrawWindow);
        }

        public void Hide()
        {
            RenderingManager.RemoveFromPostDrawQueue(0, DrawWindow);
        }
        
        private void DrawWindow()
        {
            GUI.skin = HighLogic.Skin;
            if (!Initialized)
            {
                Initialize();
                Initialized = true;
            }
            _rect = GUILayout.Window(0, _rect, DrawMenu, "CraftShare");
        }

        protected virtual void Initialize() { }
        protected abstract void DrawMenu(int id);
    }
}