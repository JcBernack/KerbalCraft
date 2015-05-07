using UnityEngine;

namespace CraftShare
{
    public class SettingsWindow
        : Window
    {
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
        }

        protected override void DrawMenu(int id)
        {
            GUILayout.BeginVertical(GUILayout.Width(200));
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
            GUILayout.Label("Please refresh the list after changing the host address.");
            GUILayout.EndVertical();
            GUI.DragWindow();
        }
    }
}