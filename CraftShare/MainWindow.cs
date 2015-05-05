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

        private int _pageSkip;
        private int _pageLimit = 20;
        private int _pageLength;
        
        public MainWindow()
            : base(Screen.width / 2f - (LeftWidth + RightWidth) / 2f, Screen.height / 4f, "CraftShare")
        {
            LoadConfig();
            RestApi.SetHostAddress(_hostAddress);
            ResetWindow();
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

        private void ResetWindow()
        {
            _tableMessage = "List not loaded.";
            _pageSkip = 0;
            _pageLength = 0;
            _craftList = null;
            _selectedCraft = null;
        }

        private void UpdateCraftList()
        {
            // make the window as small as possible again
            Rect.width = 0;
            Rect.height = 0;
            try
            {
                _craftList = RestApi.GetCraftList(_pageSkip, _pageLimit);
                if (_craftList == null)
                {
                    _pageLength = 0;
                    _tableMessage = "There is nothing here.";
                }
                else
                {
                    _pageLength = _craftList.Count;
                }
                Debug.Log(string.Format("CraftShare: Received {0} entries.", _pageLength));
            }
            catch (Exception ex)
            {
                Debug.LogError("CraftShare: failed to load craft list.");
                Debug.LogException(ex);
                _craftList = null;
                _pageLength = 0;
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
            // add table headers
            var cells = new List<string>
            {
                "Name", "Facility", "Author", "Date"
            };
            var dimension = cells.Count;
            if (_pageLength == 0)
            {
                cells.Add(_tableMessage);
            }
            else
            {
                // add table data
                foreach (var craft in _craftList)
                {
                    cells.AddRange(new[] {craft.name, craft.facility, craft.author, craft.date.ToLongDateString()});
                }
            }
            // draw table
            var clickedCell = GUIHelper.Grid(dimension, false, cells);
            // handle clicks on table cells
            if (_pageLength > 0 && clickedCell > -1)
            {
                _selectedCraft = _craftList[clickedCell / dimension - 1];
            }
            // draw paging controls at the bottom of the window
            GUILayout.FlexibleSpace();
            GUILayout.Label(string.Format("Showing #{0} to #{1}", _pageSkip, _pageSkip + _pageLimit));
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("<<"))
            {
                _pageSkip -= _pageLimit*5;
                if (_pageSkip < 0) _pageSkip = 0;
                UpdateCraftList();
            }
            if (GUILayout.Button("<"))
            {
                _pageSkip -= _pageLimit;
                if (_pageSkip < 0) _pageSkip = 0;
                UpdateCraftList();
            }
            if (GUILayout.Button(">"))
            {
                _pageSkip += _pageLimit;
                UpdateCraftList();
            }
            if (GUILayout.Button(">>"))
            {
                _pageSkip += _pageLimit*5;
                UpdateCraftList();
            }
            GUILayout.EndHorizontal();
        }

        private void DrawRightSide()
        {
            GUILayout.BeginVertical(GUILayout.Width(RightWidth));
            GUILayout.Label("Details", ModGlobals.HeadStyle);
            DrawCraftDetails();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Settings", ModGlobals.HeadStyle);
            _editHostAddress = GUIUtil.EditableStringField("Host", _editHostAddress, OnHostAddressSubmit);
            _editAuthorName = GUIUtil.EditableStringField("Author", _editAuthorName, OnAuthorNameSubmit);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh list"))
            {
                _pageSkip = 0;
                UpdateCraftList();
            }
            if (GUILayout.Button("Share current craft")) ShareCurrentCraft();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void OnHostAddressSubmit(string host)
        {
            Debug.Log("CraftShare: change host to " + host);
            _hostAddress = host;
            RestApi.SetHostAddress(host);
            SaveConfig();
            ResetWindow();
            UpdateCraftList();
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
                try
                {
                    RestApi.DeleteCraft(_selectedCraft._id);
                }
                catch (Exception)
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