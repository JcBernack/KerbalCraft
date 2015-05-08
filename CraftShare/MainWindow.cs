﻿using System;
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
                _craftList = RestApi.GetCraft(_pageSkip, _pageLimit);
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
            ResetWindowSize();
            _selectedCraft = craft;
            try
            {
                if (_selectedCraft.ThumbnailCache == null)
                {
                    _selectedCraft.ThumbnailCache = RestApi.GetThumbnail(_selectedCraft._id);
                }
                _thumbnail.LoadImage(_selectedCraft.ThumbnailCache);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("CraftShare: unable to load thumbnail for: " + _selectedCraft._id);
                Debug.LogException(ex);
            }
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
            GUILayout.Box(_selectedCraft.ThumbnailCache == null ? ModGlobals.IconLarge : _thumbnail);
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
                LoadSelectedCraft(merge);
            }
            GUILayout.EndHorizontal();
        }

        private void LoadSelectedCraft(bool merge)
        {
            if (_selectedCraft.CraftCache == null)
            {
                try
                {
                    Debug.Log("CraftShare: loading craft: " + _selectedCraft._id);
                    _selectedCraft.CraftCache = RestApi.GetCraft(_selectedCraft._id);
                    Debug.Log("CraftShare: craft characters loaded: " + _selectedCraft.CraftCache.Length);
                }
                catch (Exception ex)
                {
                    Debug.LogError("CraftShare: craft download failed");
                    Debug.LogException(ex);
                    return;
                }
            }
            //TODO: find a way to load the ConfigNode directly from a string and skip writing it to a file
            var craftPath = Path.Combine(ModGlobals.PluginDataPath, "download.craft");
            File.WriteAllBytes(craftPath, _selectedCraft.CraftCache);
            Debug.Log("CraftShare: craft written to: " + craftPath);
            if (merge)
            {
                var ship = ShipConstruction.LoadShip(craftPath);
                EditorLogic.fetch.SpawnConstruct(ship);
            }
            else
            {
                EditorLogic.LoadShipFromFile(craftPath);
            }
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
            // update the thumbnail
            ThumbnailHelper.CaptureThumbnail(ship, ModGlobals.ThumbnailResolution, ModGlobals.PluginDataPath, "thumbnail");
            // prepare binary data of the craft and thumbnail
            var craftData = File.ReadAllBytes(craftPath);
            var thumbnail = File.ReadAllBytes(Path.Combine(ModGlobals.PluginDataPath, "thumbnail.png"));
            //TODO: find a way to get thumbnail and craft data without file access
            // create transfer object
            var craft = new SharedCraft
            {
                name = ship.shipName,
                facility = ship.shipFacility.ToString(),
                author = ModGlobals.AuthorName
            };
            try
            {
                // upload the craft
                Debug.Log("CraftShare: uploading craft");
                craft = RestApi.PostCraft(craft, craftData, thumbnail);
                Debug.Log("CraftShare: new shared craft ID: " + craft._id);
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