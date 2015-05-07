using UnityEngine;

namespace CraftShare
{
    public class SettingsWindow
        : Window
    {
        private const int Width = 200;
        private string _editHostAddress;
        private string _editAuthorName;

        public SettingsWindow()
            : base(0, 0, "CraftShare - Settings")
        {
            Show += OnShow;
        }

        protected void OnShow()
        {
            _editHostAddress = ModGlobals.HostAddress;
            _editAuthorName = ModGlobals.AuthorName;
            // move the window to the screen center
            Rect.x = Screen.width/2 - Width/2;
            Rect.y = 80;
        }

        protected override void DrawMenu(int id)
        {
            PreventEditorClickthrough();
            GUILayout.BeginVertical(GUILayout.Width(Width));
            GUILayout.Label("Host address:", ModGlobals.HeadStyle);
            _editHostAddress = GUILayout.TextField(_editHostAddress);
            GUILayout.Label("Author name:", ModGlobals.HeadStyle);
            _editAuthorName = GUILayout.TextField(_editAuthorName, 30);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Apply"))
            {
                ModGlobals.SaveConfig(_editHostAddress, _editAuthorName);
                Close();
            }
            if (GUILayout.Button("Cancel"))
            {
                Close();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUI.DragWindow();
        }
    }
}