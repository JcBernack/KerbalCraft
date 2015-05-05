using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CraftShare
{
    public class MainWindow
        : Window
    {
        private GUITable _table;
        private List<SharedCraft> _craftList;
        private SharedCraft _selectedCraft;

        public MainWindow()
            : base(50, 50, "CraftShare")
        {
        }

        protected override void Initialize()
        {
            _table = new GUITable();
            _table.AddColumns("Name", "Facility", "Author", "Date");
            UpdateCraftList();
        }

        protected override void DrawMenu(int id)
        {
            GUILayout.BeginHorizontal();
            DrawCraftList();
            DrawCraftDetails();
            GUILayout.EndHorizontal();
            GUI.DragWindow();
        }

        private void UpdateCraftList()
        {
            try
            {
                _craftList = RestApi.GetCraftList();
                Debug.Log(string.Format("CraftShare: Received {0} entries.", _craftList.Count));
            }
            catch (Exception ex)
            {
                Debug.LogError("CraftShare: failed to load craft list.");
                Debug.LogException(ex);
            }
        }

        private void DrawCraftList()
        {
            GUILayout.BeginVertical(GUILayout.Width(600));
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh list")) UpdateCraftList();
            if (GUILayout.Button("Share current craft")) ShareCurrentCraft();
            GUILayout.EndHorizontal();
            DrawTable();
            GUILayout.EndVertical();
        }

        private void DrawTable()
        {
            if (_craftList == null)
            {
                GUILayout.Label("List not loaded.");
                return;
            }
            if (_craftList.Count == 0)
            {
                GUILayout.Label("No has shared anything :(");
                return;
            }
            _table.ClearRows();
            foreach (var craft in _craftList)
            {
                _table.AddRow(craft.name, craft.facility, craft.author, craft.date);
            }
            int clickedRow;
            if (_table.Render(out clickedRow))
            {
                _selectedCraft = _craftList[clickedRow];
            }
        }

        private void DrawCraftDetails()
        {
            GUILayout.BeginVertical(GUILayout.Width(250));
            GUILayout.Label("Craft details:", ModGlobals.HeadStyle);
            if (_selectedCraft == null)
            {
                GUILayout.Label("Nothing selected.");
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                //GUILayout.Label("(ID):");
                GUILayout.Label("Name:");
                GUILayout.Label("Facility:");
                GUILayout.Label("Author:");
                GUILayout.Label("Date:");
                GUILayout.Label("Description:");
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                //GUILayout.Label(_selectedCraft._id);
                GUILayout.Label(_selectedCraft.name);
                GUILayout.Label(_selectedCraft.facility);
                GUILayout.Label(_selectedCraft.author);
                GUILayout.Label(_selectedCraft.date);
                GUILayout.Label("todo");
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                if (GUILayout.Button("Delete"))
                {
                    RestApi.DeleteCraft(_selectedCraft._id);
                    UpdateCraftList();
                }
                if (GUILayout.Button("Load"))
                {
                    if (string.IsNullOrEmpty(_selectedCraft.craft))
                    {
                        Debug.Log("CraftShare: loading craft: " + _selectedCraft._id);
                        _selectedCraft.craft = RestApi.GetCraft(_selectedCraft._id);
                        _selectedCraft.craft = StringCompressor.Decompress(_selectedCraft.craft);
                        Debug.Log("CraftShare: craft characters loaded: " + _selectedCraft.craft.Length);
                    }
                    //TODO: find a way to load the ConfigNode directly from a string and skip writing it to a file
                    var craftPath = Path.Combine(ModGlobals.PluginDataPath, "download.craft");
                    File.WriteAllText(craftPath, _selectedCraft.craft);
                    Debug.Log("CraftShare: craft written to: " + craftPath);
                    EditorLogic.LoadShipFromFile(craftPath);
                }
            }
            GUILayout.EndVertical();
        }

        private void ShareCurrentCraft()
        {
            var ship = EditorLogic.fetch.ship;
            var craftPath = Path.Combine(ModGlobals.PluginDataPath, "shared.craft");
            ship.SaveShip().Save(craftPath);
            var shared = new SharedCraft
            {
                name = ship.shipName,
                facility = ship.shipFacility.ToString(),
                author = "Fettsack",
                craft = StringCompressor.Compress(File.ReadAllText(craftPath))
            };
            try
            {
                // upload
                shared = RestApi.CreateCraft(shared);
                Debug.Log("CraftShare: new shared craft ID: " + shared._id);
            }
            catch(Exception ex)
            {
                Debug.LogError("CraftShare: sharing craft failed");
                Debug.LogException(ex);
            }
            // refresh list to show the new entry
            UpdateCraftList();
        }
    }
}