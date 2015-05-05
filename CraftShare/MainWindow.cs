using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CraftShare
{
    public class MainWindow
        : Window
    {
        private const int LeftWidth = 600;
        private const int RightWidth = 250;
        private List<SharedCraft> _craftList;
        private SharedCraft _selectedCraft;

        private string _tableMessage;

        private string _hostAddress;
        private string _authorName;

        private string _editHostAddress;
        private string _editAuthorName;
        
        public MainWindow()
            : base(Screen.width / 2f - (LeftWidth + RightWidth) / 2f, Screen.height / 4f, "CraftShare")
        {
            _tableMessage = "List not loaded.";
            LoadConfig();
            RestApi.SetHostAddress(_hostAddress);
        }

        private void LoadConfig()
        {
            // set default values
            _hostAddress = "localhost:8000";
            _authorName = HighLogic.SaveFolder;
            // try to parse values from config
            var config = ConfigNode.Load(ModGlobals.ConfigPath);
            if (config != null)
            {
                _hostAddress = config.GetValue("HostAddress");
                _authorName = config.GetValue("AuthorName");
            }
            // copy values over to the editable fields
            _editHostAddress = _hostAddress;
            _editAuthorName = _authorName;
        }

        private void SaveConfig()
        {
            var config = new ConfigNode();
            config.AddValue("HostAddress", _hostAddress);
            config.AddValue("AuthorName", _authorName);
            config.Save(ModGlobals.ConfigPath);
        }

        protected override void OnShow()
        {
            // update the list when the windows is opened
            UpdateCraftList();
        }

        protected override void DrawMenu(int id)
        {
            GUILayout.BeginHorizontal();
            DrawLeftSide();
            DrawRightSide();
            GUILayout.EndHorizontal();
            GUI.DragWindow();
        }

        private void UpdateCraftList()
        {
            // make the window as small as possible again
            Rect.width = 0;
            Rect.height = 0;
            try
            {
                _craftList = RestApi.GetCraftList();
                Debug.Log(string.Format("CraftShare: Received {0} entries.", _craftList.Count));
            }
            catch (Exception ex)
            {
                Debug.LogError("CraftShare: failed to load craft list.");
                Debug.LogException(ex);
                _tableMessage = "Failed to load list.";
            }
        }

        private void DrawLeftSide()
        {
            GUILayout.BeginVertical(GUILayout.Width(LeftWidth));
            DrawTable();
            GUILayout.EndVertical();
        }

        private void DrawTable()
        {
            if (_craftList == null)
            {
                GUILayout.Label(_tableMessage);
                return;
            }
            if (_craftList.Count == 0)
            {
                GUILayout.Label("Nothing shared yet :(");
                return;
            }
            var cells = new List<string>
            {
                "Name", "Facility", "Author", "Date"
            };
            var dimension = cells.Count;
            foreach (var craft in _craftList)
            {
                cells.AddRange(new[] { craft.name, craft.facility, craft.author, craft.date.ToLongDateString() });
            }
            var clickedCell = GUIHelper.Grid(dimension, false, cells);
            if (clickedCell > -1)
            {
                _selectedCraft = _craftList[clickedCell / dimension - 1];
            }
        }

        private void DrawRightSide()
        {
            GUILayout.BeginVertical(GUILayout.Width(RightWidth));
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh list")) UpdateCraftList();
            if (GUILayout.Button("Share current craft")) ShareCurrentCraft();
            GUILayout.EndHorizontal();
            GUILayout.Label("Settings", ModGlobals.HeadStyle);
            _editHostAddress = GUIUtil.EditableStringField("Host", _editHostAddress, OnHostAddressSubmit);
            _editAuthorName = GUIUtil.EditableStringField("Author", _editAuthorName, OnAuthorNameSubmit);
            GUILayout.Label("Details", ModGlobals.HeadStyle);
            DrawCraftDetails();
            GUILayout.EndVertical();
        }

        private void OnHostAddressSubmit(string host)
        {
            Debug.Log("CraftShare: change host to " + host);
            _hostAddress = host;
            RestApi.SetHostAddress(host);
            SaveConfig();
        }

        private void OnAuthorNameSubmit(string name)
        {
            // limit author name to 30 characters
            if (name.Length > 30)
            {
                name = name.Substring(0, 30);
                _editAuthorName = name;
            }
            _authorName = name;
            SaveConfig();
        }

        private void DrawCraftDetails()
        {
            if (_selectedCraft == null)
            {
                GUILayout.Label("Nothing selected.");
                return;
            }
            var cells = new[]
            {
                "Name:", "Facility:", "Author:", "Date:",
                _selectedCraft.name, _selectedCraft.facility, _selectedCraft.author, _selectedCraft.date.ToLongDateString()
            };
            GUIHelper.Grid(4, true, cells);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Delete"))
            {
                Debug.Log("CraftShare: deleting craft craft: " + _selectedCraft._id);
                if (!RestApi.DeleteCraft(_selectedCraft._id))
                {
                    Debug.LogWarning("CraftShare: deletion failed");
                }
                _selectedCraft = null;
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
            GUILayout.EndHorizontal();
        }

        private void ShareCurrentCraft()
        {
            var ship = EditorLogic.fetch.ship;
            if (ship.Count == 0) return;
            if (ship.shipName.Length == 0) return;
            if (_authorName.Length == 0) return;
            // save current craft to file
            var craftPath = Path.Combine(ModGlobals.PluginDataPath, "upload.craft");
            ship.SaveShip().Save(craftPath);
            // create transfer object including compressed content of the craft file
            var shared = new SharedCraft
            {
                name = ship.shipName,
                facility = ship.shipFacility.ToString(),
                author = _authorName,
                craft = StringCompressor.Compress(File.ReadAllText(craftPath))
            };
            try
            {
                // upload the craft
                Debug.Log("CraftShare: uploading craft");
                shared = RestApi.CreateCraft(shared);
                Debug.Log("CraftShare: new shared craft ID: " + shared._id);
                // refresh list to reflect the new entry
                UpdateCraftList();
            }
            catch(Exception ex)
            {
                Debug.LogError("CraftShare: sharing craft failed");
                Debug.LogException(ex);
            }
        }
    }
}