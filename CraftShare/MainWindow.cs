using System.Collections.Generic;
using UnityEngine;

namespace CraftShare
{
    public class MainWindow
        : Window
    {
        private GUITable _table;
        private IList<SharedCraft> _craftList;
        private SharedCraft _selectedCraft;

        public MainWindow()
            : base(50, 50)
        {
        }

        protected override void Initialize()
        {
            _table = new GUITable();
            _table.AddColumns("Name", "Author", "Date");
        }

        protected override void DrawMenu(int id)
        {
            GUILayout.BeginHorizontal();
            DrawCraftList();
            DrawCraftDetails();
            GUILayout.EndHorizontal();
            GUI.DragWindow();
        }

        private void DrawCraftList()
        {
            GUILayout.BeginVertical(GUILayout.Width(500));
            GUILayout.Label("Craft list:");
            if (GUILayout.Button("Refresh list") || _craftList == null)
            {
                _craftList = RestApi.GetCraftList();
                Debug.Log(string.Format("CraftShare: Received {0} entries.", _craftList.Count));
            }
            if (_craftList != null)
            {
                _table.ClearRows();
                foreach (var craft in _craftList)
                {
                    _table.AddRow(craft.Name, craft.Author, craft.Date);
                }
                int clickedRow;
                if (_table.Render(out clickedRow))
                {
                    _selectedCraft = _craftList[clickedRow];
                    _selectedCraft.ThumbnailString = RestApi.GetThumbnail(_selectedCraft.Id);
                    if (!string.IsNullOrEmpty(_selectedCraft.ThumbnailString))
                    {
                        _selectedCraft.DeserializeThumbnail();
                    }
                }
            }
            else
            {
                GUILayout.Label("List not loaded.");
            }
            GUILayout.EndVertical();
        }

        private void DrawCraftDetails()
        {
            GUILayout.BeginVertical(GUILayout.Width(250));
            GUILayout.Label("Craft details:");
            if (_selectedCraft == null)
            {
                GUILayout.Label("Select a shared craft from the list.");
            }
            else
            {
                GUILayout.Label(string.Format("(ID): {0}", _selectedCraft.Id));
                GUILayout.Label(string.Format("Name: {0}", _selectedCraft.Name));
                GUILayout.Label(string.Format("Author: {0}", _selectedCraft.Author));
                GUILayout.Label(string.Format("Date: {0}", _selectedCraft.Date));
            }
            GUILayout.EndVertical();
        }
    }
}