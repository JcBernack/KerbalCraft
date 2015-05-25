using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using KerbalCraft.Models;
using RestSharp;
using UnityEngine;

namespace KerbalCraft
{
    public class MainWindow
        : Window
    {
        public event Action SettingsClicked;

        private const int LeftWidth = 400;
        private const int RightWidth = 250;

        private List<Craft> _craftList;
        private Craft _selectedCraft;

        private readonly RestApi _api;
        private readonly Texture2D _thumbnail;
        private string _tableMessage;

        private int _pageSkip;
        private int _pageLimit = 20;
        private int _pageLength;

        public MainWindow()
            : base(Screen.width / 2f - (LeftWidth + RightWidth) / 2f, 250, "KerbalCraft v" + Assembly.GetExecutingAssembly().GetName().Version.ToString(2))
        {
            _api = new RestApi();
            _thumbnail = new Texture2D(ModGlobals.ThumbnailResolution, ModGlobals.ThumbnailResolution, TextureFormat.ARGB32, false);
            // reset state and update configuration
            OnConfigSaved();
            // hook up events
            Show += OnShow;
            ModSettings.ConfigSaved += OnConfigSaved;
        }

        private void ResetState()
        {
            _craftList = null;
            _selectedCraft = null;
            _tableMessage = "List not loaded.";
            _pageSkip = 0;
            _pageLength = 0;
        }

        protected void OnShow()
        {
            // update the list when the window is opened
            UpdateCraftList();
        }

        private void OnConfigSaved()
        {
            // remove any previous content and state
            ResetState();
            // update configuration
            _api.SetConfig(ModSettings.HostAddress, ModSettings.Username, ModSettings.Password);
            // update the list when the settings are changed and the window is already open
            if (Visible) UpdateCraftList();
        }

        protected override void DrawMenu(int id)
        {
            // handle asynchronous responses
            _api.HandleResponses();
            // prevent clicking through the window into the editor
            PreventEditorClickthrough();
            // draw window content
            GUILayout.BeginHorizontal();
            DrawLeftSide();
            DrawRightSide();
            GUILayout.EndHorizontal();
            // draw buttons
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
            if (GUILayout.Button("Refresh page"))
            {
                UpdateCraftList();
            }
            if (HighLogic.LoadedSceneIsEditor && GUILayout.Button("Share current craft"))
            {
                ShareCurrentCraft();
            }
            if (GUILayout.Button("Settings"))
            {
                if (SettingsClicked != null) SettingsClicked();
            }
            if (GUILayout.Button("Close"))
            {
                Close();
            }
            GUILayout.EndHorizontal();
            // make the window draggable
            GUI.DragWindow();
        }

        private void UpdateCraftList()
        {
            _tableMessage = "loading..";
            _api.GetCraft(_pageSkip, _pageLimit, delegate(IRestResponse response)
            {
                if (HandleResponseError(response, "get craft list"))
                {
                    _craftList = null;
                    _pageLength = 0;
                    _tableMessage = "Failed to load list, maybe check the settings.";
                    return;
                }
                var data = _api.Deserialize<List<Craft>>(response);
                if (data == null || data.Count == 0)
                {
                    _pageLength = 0;
                    _tableMessage = "There is nothing here.";
                }
                else
                {
                    _pageLength = data.Count;
                }
                _craftList = data;
                Debug.Log(string.Format("[KerbalCraft] Received {0} entries.", _pageLength));
                ResetWindowSize();
            });
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
            var cells = Craft.GetInfoNames().ToList();
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
                    cells.AddRange(craft.GetInfoValues());
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

        private void SelectCraft(Craft craft)
        {
            // change the selected craft
            _selectedCraft = craft;
            // skip the request if the thumbnail was previously loaded
            if (_selectedCraft.ThumbnailCache != null)
            {
                UpdateThumbnail(craft);
                return;
            }
            _api.GetCraftThumbnail(craft._id, delegate(IRestResponse response)
            {
                if (HandleResponseError(response, "get craft thumbnail")) return;
                craft.ThumbnailCache = response.RawBytes;
                UpdateThumbnail(craft);
            });
        }

        private void UpdateThumbnail(Craft craft)
        {
            // prevent race condition
            if (_selectedCraft != craft) return;
            _thumbnail.LoadImage(craft.ThumbnailCache);
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
            GUILayout.BeginHorizontal();
            // draw thumbnail or icon
            GUILayout.Box(_selectedCraft.ThumbnailCache == null ? ModGlobals.IconLarge : _thumbnail);
            // draw details table
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            foreach (var key in _selectedCraft.GetExtendedInfoNames())
            {
                GUILayout.Label(key, ModGlobals.HeadStyle);
            }
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            foreach (var value in _selectedCraft.GetExtendedInfoValues())
            {
                GUILayout.Label(value, ModGlobals.RowStyle);
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            // draw delete button if the user owns the craft
            // just comparing the username is not enough of course, but the server will make sure
            // the user actually owns it, otherwise the delete will fail
            if (_selectedCraft.author.username == ModSettings.Username && GUILayout.Button("Delete"))
            {
                Debug.Log("[KerbalCraft] deleting craft craft: " + _selectedCraft._id);
                _api.DeleteCraft(_selectedCraft._id, delegate(IRestResponse response)
                {
                    if (HandleResponseError(response, "delete craft")) return;
                    Debug.Log("[KerbalCraft] delete successful");
                    // update list after deletion
                    UpdateCraftList();
                });
                // deselect to reduce chance of double-delete
                _selectedCraft = null;
            }
            // skip the options to load the craft if not in an editor
            if (HighLogic.LoadedSceneIsEditor)
            {
                // merge is only available when there something in the editor to merge into
                var merge = false;
                if (EditorLogic.fetch.ship.Count > 0) merge = GUILayout.Button("Merge");
                // load is always available
                var load = GUILayout.Button("Load");
                if (merge || load)
                {
                    RequestCraftData(_selectedCraft, merge);
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            if (!string.IsNullOrEmpty(_selectedCraft.Description))
            {
                GUILayout.Label(Craft.DescriptionKey, ModGlobals.HeadStyle);
                GUILayout.Label(_selectedCraft.Description, ModGlobals.LabelStyle);
            }
        }

        private void RequestCraftData(Craft craft, bool merge)
        {
            if (craft.CraftCache != null)
            {
                LoadCraft(craft, merge);
            }
            else
            {
                Debug.Log("[KerbalCraft] loading craft: " + craft._id);
                _api.GetCraftData(craft._id, delegate(IRestResponse response)
                {
                    if (HandleResponseError(response, "get craft data")) return;
                    craft.CraftCache = CLZF2.Decompress(response.RawBytes);
                    Debug.Log("[KerbalCraft] craft bytes loaded: " + craft.CraftCache.Length);
                    LoadCraft(craft, merge);
                });
            }
        }

        private void LoadCraft(Craft craft, bool merge)
        {
            //TODO: find a way to load the ConfigNode directly from a string and skip writing it to a file
            var craftPath = Path.Combine(ModGlobals.PluginDataPath, "download.craft");
            File.WriteAllBytes(craftPath, craft.CraftCache);
            Debug.Log("[KerbalCraft] craft written to: " + craftPath);
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
            if (ship.Count == 0) return;
            if (ship.shipName.Length == 0) return;
            // save current craft to file
            var craftPath = Path.Combine(ModGlobals.PluginDataPath, "upload.craft");
            ship.SaveShip().Save(craftPath);
            // update the thumbnail
            ThumbnailHelper.CaptureThumbnail(ship, ModGlobals.ThumbnailResolution, ModGlobals.PluginDataPath, "thumbnail");
            // prepare binary data of the craft and thumbnail
            var craftData = CLZF2.Compress(File.ReadAllBytes(craftPath));
            //var craftData = File.ReadAllBytes(craftPath);
            var thumbnail = File.ReadAllBytes(Path.Combine(ModGlobals.PluginDataPath, "thumbnail.png"));
            //TODO: find a way to get thumbnail and craft data without file access
            // upload the craft
            Debug.Log("[KerbalCraft] uploading craft");
            _api.PostCraft(craftData, thumbnail, delegate(IRestResponse response)
            {
                if (HandleResponseError(response, "post craft")) return;
                var craft = _api.Deserialize<Craft>(response);
                Debug.Log("[KerbalCraft] new shared craft ID: " + craft._id);
                SelectCraft(craft);
                // refresh list to reflect the new entry
                UpdateCraftList();
            });
        }

        private static bool HandleResponseError(IRestResponse response, string action)
        {
            if (response.ErrorException != null)
            {
                Debug.LogError(string.Format("[KerbalCraft] {0} (transport error)", action));
                Debug.LogException(response.ErrorException);
                return true;
            }
            switch (response.StatusCode)
            {
                default:
                    Debug.LogError(string.Format("[KerbalCraft] {0} (HTTP code {1})", action, response.StatusCode));
                    return true;
                case HttpStatusCode.NoContent:
                case HttpStatusCode.OK:
                    return false;
            }
        }
    }
}