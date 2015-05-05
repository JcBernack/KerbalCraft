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
            GUILayout.BeginVertical(GUILayout.Width(500));
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
                _table.AddRow(craft.Name, craft.Facility, craft.Author, craft.Date);
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
                GUILayout.Label("(ID):");
                GUILayout.Label("Name:");
                GUILayout.Label("Facility:");
                GUILayout.Label("Author:");
                GUILayout.Label("Date:");
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                GUILayout.Label(_selectedCraft.Id);
                GUILayout.Label(_selectedCraft.Name);
                GUILayout.Label(_selectedCraft.Facility);
                GUILayout.Label(_selectedCraft.Author);
                GUILayout.Label(_selectedCraft.Date);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                if (GUILayout.Button("Delete"))
                {
                    RestApi.DeleteCraft(_selectedCraft.Id);
                    UpdateCraftList();
                }
                if (GUILayout.Button("Load"))
                {
                    if (string.IsNullOrEmpty(_selectedCraft.Craft))
                    {
                        _selectedCraft.Craft = RestApi.GetCraft(_selectedCraft.Id);
                        _selectedCraft.Craft = StringCompressor.Decompress(_selectedCraft.Craft);
                        Debug.Log("CraftShare: craft characters loaded: " + _selectedCraft.Craft.Length);
                    }
                    //TODO: find a way to load the ConfigNode directly from a string and skip writing it to a file
                    var craftPath = Path.Combine(ModGlobals.PluginDataPath, "tmp.craft");
                    //var craftPath = GetCraftPath(_selectedCraft.Facility, _selectedCraft.Name);
                    File.WriteAllText(craftPath, _selectedCraft.Craft);
                    Debug.Log("CraftShare: craft written to: " + craftPath);
                    
                    //var configNode = ConfigNode.Load(tmpCraftFile);
                    //EditorLogic.fetch.ship.LoadShip(configNode);
                    //EditorLogic.fetch.ship = ShipConstruction.LoadShip(tmpCraftFile);
                    
                    EditorLogic.LoadShipFromFile(craftPath);

                    //var ship = ShipConstruction.LoadShip(Path.Combine(ModGlobals.PluginDataPath, "tmp.craft"));
                    //ship.SaveShip().Save(GetCraftPath(ship.shipFacility.ToString(), ship.shipName));
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
                Name = ship.shipName,
                Facility = ship.shipFacility.ToString(),
                Author = "Fettsack",
                Craft = StringCompressor.Compress(File.ReadAllText(craftPath))
            };
            try
            {
                // upload
                shared = RestApi.CreateCraft(shared);
                Debug.Log("CraftShare: new shared craft ID: " + shared.Id);
            }
            catch(Exception ex)
            {
                Debug.LogError("CraftShare: sharing craft failed");
                Debug.LogException(ex);
            }
            // refresh list to show the new entry
            UpdateCraftList();
        }

        public static string GetCraftPath(string facility, string name)
        {
            return string.Format("saves/{0}/Ships/{1}/{2}.craft", HighLogic.SaveFolder, facility, name);
        }

        public static string GetCraftThumbnailPath(string facility, string name)
        {
            return string.Format("thumbs/{0}_{1}_{2}.png", HighLogic.SaveFolder, facility, name);
        }
    }
}