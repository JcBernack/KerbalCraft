using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CraftShare
{
    public class MainWindow
        : Window
    {
        private const int LeftWidth = 400;
        private const int RightWidth = 250;

        private List<SharedCraft> _craftList;
        private SharedCraft _selectedCraft;

        private readonly Texture2D _thumbnail;
        private string _tableMessage;

        private int _pageSkip;
        private int _pageLimit = 20;
        private int _pageLength;

        public MainWindow()
            : base(Screen.width / 2f - (LeftWidth + RightWidth) / 2f, 250, "CraftShare")
        {
            _thumbnail = new Texture2D(ModGlobals.ThumbnailResolution, ModGlobals.ThumbnailResolution, TextureFormat.ARGB32, false);
            ResetState();
            Show += OnShow;
            ModGlobals.SettingsChange += OnSettingsChanged;
        }

        private void ResetState()
        {
            _tableMessage = "List not loaded.";
            _pageSkip = 0;
            _pageLength = 0;
            _craftList = null;
            _selectedCraft = null;
        }

        protected void OnShow()
        {
            // update the list when the window is opened
            UpdateCraftList();
        }

        private void OnSettingsChanged()
        {
            // update the list when the settings are changed and the window is already open
            if (Visible) UpdateCraftList();
        }

        protected override void DrawMenu(int id)
        {
            PreventEditorClickthrough();
            GUILayout.BeginHorizontal();
            DrawLeftSide();
            DrawRightSide();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Previous page"))
            {
                _pageSkip -= _pageLimit;
                if (_pageSkip < 0) _pageSkip = 0;
                UpdateCraftList();
            }
            if (GUILayout.Button("Next page"))
            {
                _pageSkip += _pageLimit;
                UpdateCraftList();
            }
            if (GUILayout.Button("Refresh list"))
            {
                _pageSkip = 0;
                UpdateCraftList();
            }
            if (GUILayout.Button("Share current craft"))
            {
                ShareCurrentCraft();
            }
            if (GUILayout.Button("Settings"))
            {
                ModGlobals.SettingsWindow.Open();
            }
            if (GUILayout.Button("Close"))
            {
                Close();
            }
            GUILayout.EndHorizontal();
            GUI.DragWindow();
        }

        private void UpdateCraftList()
        {
            ResetWindowSize();
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
                _tableMessage = "Failed to load list, maybe check the settings.";
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
            var cells = new List<string> { "Name", "Facility", "Author" };
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
                    cells.AddRange(new[] { craft.name, craft.facility, craft.author });
                }
            }
            // draw table
            var clickedCell = GUIHelper.Grid(dimension, false, cells);
            // handle clicks on table cells
            if (_pageLength > 0 && clickedCell > -1)
            {
                SelectCraft(_craftList[clickedCell / dimension - 1]);
            }
            // draw paging controls at the bottom of the window
            GUILayout.FlexibleSpace();
            //TODO: improve page numbering
            GUILayout.Label(string.Format("Showing #{0} to #{1}", _pageSkip+1, _pageSkip + _pageLimit));
        }

        private void SelectCraft(SharedCraft craft)
        {
            _selectedCraft = craft;
            if (string.IsNullOrEmpty(_selectedCraft.thumbnail)) return;
            try
            {
                var bytes = BinaryCompressor.Decompress(_selectedCraft.thumbnail);
                _thumbnail.LoadImage(bytes);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("CraftShare: unable to parse thumbnail");
                Debug.LogException(ex);
            }
            ResetWindowSize();
        }

        private void DrawRightSide()
        {
            GUILayout.BeginVertical(GUILayout.Width(RightWidth));
            GUILayout.Label("Details", ModGlobals.HeadStyle);
            DrawCraftDetails();
            GUILayout.EndVertical();
        }
        
        private void DrawCraftDetails()
        {
            if (_selectedCraft == null)
            {
                GUILayout.Label("Nothing selected.");
                return;
            }
            // draw thumbnail or icon
            GUILayout.Box(string.IsNullOrEmpty(_selectedCraft.thumbnail) ? ModGlobals.IconLarge : _thumbnail);
            // draw details table
            var cells = new[]
            {
                "Name:", "Facility:", "Author:", "Date:",
                _selectedCraft.name, _selectedCraft.facility, _selectedCraft.author, _selectedCraft.date.ToLongDateString()
            };
            GUIHelper.Grid(4, true, cells);
            GUILayout.BeginHorizontal();
            // draw delete button
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
            // merge is only available when there something in the editor to merge into
            var merge = false;
            if (EditorLogic.fetch.ship.Count > 0) merge = GUILayout.Button("Merge");
            // load is always available
            var load = GUILayout.Button("Load");
            if (merge || load)
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
                if (merge)
                {
                    var ship = ShipConstruction.LoadShip(craftPath);
                    EditorLogic.fetch.SpawnConstruct(ship);
                }
                if (load)
                {
                    EditorLogic.LoadShipFromFile(craftPath);
                }
            }
            GUILayout.EndHorizontal();
        }

        private void ShareCurrentCraft()
        {
            var ship = EditorLogic.fetch.ship;
            //TODO: check if ship.Count also contains unattached parts
            if (ship.Count == 0) return;
            if (ship.shipName.Length == 0) return;
            if (ModGlobals.AuthorName.Length == 0) return;
            // save current craft to file
            var craftPath = Path.Combine(ModGlobals.PluginDataPath, "upload.craft");
            ship.SaveShip().Save(craftPath);
            // read the thumbnail
            //TODO: find a way to manually generate the thumbnail or force KSP to refresh it
            var bytes = File.ReadAllBytes(ModGlobals.GetCraftThumbnailPath(ship));
            // create transfer object including compressed content of the craft file
            var shared = new SharedCraft
            {
                name = ship.shipName,
                facility = ship.shipFacility.ToString(),
                author = ModGlobals.AuthorName,
                craft = StringCompressor.Compress(File.ReadAllText(craftPath)),
                thumbnail = BinaryCompressor.Compress(bytes)
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