using System.Reflection;
using UnityEngine;

namespace CraftShare
{
    /// <summary>
    /// Base class for UI windows.
    /// </summary>
    public abstract class Window
        : WindowBase
    {
        /// <summary>
        /// The current window rect.
        /// </summary>
        public Rect Rect = new Rect(50, 50, 0, 0);

        /// <summary>
        /// The window title.
        /// </summary>
        public string Title;

        private readonly int _windowID;
        private readonly string _lockID;
        private bool _inputsLocked;

        protected Window(float left, float top, string title)
        {
            Rect.x = left;
            Rect.y = top;
            Title = title;
            _windowID = Random.Range(1000, 2000000) + Assembly.GetExecutingAssembly().FullName.GetHashCode();
            _lockID = "CraftShare_noclick" + _windowID;
            Hide += OnClose;
        }

        protected override void DrawGUI()
        {
            Rect = GUILayout.Window(_windowID, Rect, DrawMenu, Title);
        }

        protected abstract void DrawMenu(int id);

        /// <summary>
        /// Resets the window size. Causes the window to be resized to its smallest size on next render.
        /// </summary>
        protected void ResetWindowSize()
        {
            Rect.width = 0;
            Rect.height = 0;
        }

        /// <summary>
        /// Prevents clicking through the window by locking the editor UI while the mouse is over the window.
        /// </summary>
        protected void PreventEditorClickthrough()
        {
            var mouseOverWindow = Rect.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y));
            if (!_inputsLocked && mouseOverWindow)
            {
                EditorLogic.fetch.Lock(true, true, true, _lockID);
                _inputsLocked = true;
            }
            if (_inputsLocked && !mouseOverWindow)
            {
                EditorLogic.fetch.Unlock(_lockID);
                _inputsLocked = false;
            }
        }

        private void OnClose()
        {
            // prevent the UI from being locked forever when the window is closed while the mouse is over the window
            // e.g. when the user clicks a "close" button on the window itself
            if (_inputsLocked) EditorLogic.fetch.Unlock(_lockID);
        }
    }
}