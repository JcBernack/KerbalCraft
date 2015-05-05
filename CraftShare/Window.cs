using System.Reflection;
using UnityEngine;

namespace CraftShare
{
    public abstract class Window
        : WindowBase
    {
        private Rect _rect = new Rect(50, 50, 0, 0);
        
        private readonly string _title;
        
        private readonly int _windowID;

        protected Window(float left, float top, string title)
        {
            _rect.x = left;
            _rect.y = top;
            _title = title;
            _windowID = Random.Range(1000, 2000000) + Assembly.GetExecutingAssembly().FullName.GetHashCode();
        }

        protected override void DrawGUI()
        {
            _rect = GUILayout.Window(_windowID, _rect, DrawMenu, _title);
        }

        protected abstract void DrawMenu(int id);
    }
}