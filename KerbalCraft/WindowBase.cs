using System;

namespace KerbalCraft
{
    /// <summary>
    /// Base class for UI elements.
    /// </summary>
    public abstract class WindowBase
    {
        /// <summary>
        /// Specifies whether Initialize has been called.
        /// </summary>
        protected bool Initialized { get; private set; }

        /// <summary>
        /// Specifies whether the window is currently visible.
        /// </summary>
        public bool Visible { get; private set; }

        /// <summary>
        /// Occurs when the window is opened.
        /// </summary>
        public event Action Show;

        /// <summary>
        /// Occurs when the window is closed.
        /// </summary>
        public event Action Hide;

        /// <summary>
        /// Opens the window.
        /// </summary>
        public void Open()
        {
            if (Visible) return;
            Visible = true;
            if (Show != null) Show();
        }

        /// <summary>
        /// Closes the window.
        /// </summary>
        public void Close()
        {
            if (!Visible) return;
            Visible = false;
            if (Hide != null) Hide();
        }

        public void OnGUI()
        {
            if (!Visible) return;
            if (!Initialized)
            {
                Initialized = true;
                Initialize();
            }
            DrawGUI();
        }

        /// <summary>
        /// Is called once on the first OnGUI call. Can be overridden to handle GUI initialization.
        /// </summary>
        protected virtual void Initialize() { }

        /// <summary>
        /// Is called every time unity calls the OnGUI method.
        /// </summary>
        protected abstract void DrawGUI();
    }
}