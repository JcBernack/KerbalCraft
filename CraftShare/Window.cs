using System.Reflection;
using UnityEngine;

namespace CraftShare
{
    public abstract class Window
        : WindowBase
    {
        public Rect Rect = new Rect(50, 50, 0, 0);
        public string Title;

        private readonly int _windowID;

        protected Window(float left, float top, string title)
        {
            Rect.x = left;
            Rect.y = top;
            Title = title;
            _windowID = Random.Range(1000, 2000000) + Assembly.GetExecutingAssembly().FullName.GetHashCode();
        }

        protected override void DrawGUI()
        {
            Rect = GUILayout.Window(_windowID, Rect, DrawMenu, Title);
        }

        protected abstract void DrawMenu(int id);
    }
}