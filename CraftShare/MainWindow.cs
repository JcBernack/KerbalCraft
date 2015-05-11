using System.Collections.Generic;
using System.IO;
using System.Reflection;
using RestSharp;
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
            : base(Screen.width / 2f - (LeftWidth + RightWidth) / 2f, 250, "CraftShare v" + Assembly.GetExecutingAssembly().GetName().Version.ToString(2))
        {
            _thumbnail = new Texture2D(ModGlobals.ThumbnailResolution, ModGlobals.ThumbnailResolution, TextureFormat.ARGB32, false);
            ResetState();
            Show += OnShow;
            ModGlobals.SettingsChange += OnSettingsChanged;
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

        private void OnSettingsChanged()
        {
            // remove any previous content and state
            ResetState();
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
            if (GUILayout.Button("Refresh page"))
            {
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
            _tableMessage = "loading..";
            RestApi.GetCraft(_pageSkip, _pageLimit, delegate(IRestResponse<List<SharedCraft>> response)
            {
                if (response.ErrorException != null)
                {
                    //TODO: somehow there is an ArgumentException related to a missing "content-type" when an empty list is returned
                    Debug.LogError("CraftShare: failed to load craft list.");
                    Debug.LogException(response.ErrorException);
                    _craftList = null;
                    _pageLength = 0;
                    _tableMessage = "Failed to load list, maybe check the settings.";
                    return;
                }
                if (response.Data == null || response.Data.Count == 0)
                {
                    _pageLength = 0;
                    _tableMessage = "There is nothing here.";
                }
                else
                {
                    _pageLength = response.Data.Count;
                }
                _craftList = response.Data;
                Debug.Log(string.Format("CraftShare: Received {0} entries.", _pageLength));
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
            // change the selected craft
            _selectedCraft = craft;
            // skip the request if the thumbnail was previously loaded
            if (_selectedCraft.ThumbnailCache != null)
            {
                UpdateThumbnail(craft);
                return;
            }
            RestApi.GetThumbnail(craft._id, delegate(IRestResponse response)
            {
                if (response.ErrorException != null)
                {
                    Debug.LogWarning("CraftShare: unable to load thumbnail for: " + craft._id);
                    Debug.LogException(response.ErrorException);
                    return;
                }
                craft.ThumbnailCache = response.RawBytes;
                UpdateThumbnail(craft);
            });
        }

        private void UpdateThumbnail(SharedCraft craft)
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
                RestApi.DeleteCraft(_selectedCraft._id, delegate(IRestResponse response)
                {
                    if (response.ErrorException != null)
                    {
                        Debug.LogWarning("CraftShare: deletion failed");
                        Debug.LogException(response.ErrorException);
                        return;
                    }
                    Debug.Log("CraftShare: delete successful");
                    // update list after deletion
                    UpdateCraftList();
                });
                // deselect to reduce chance of double-delete
                _selectedCraft = null;
            }
            // merge is only available when there something in the editor to merge into
            var merge = false;
            if (EditorLogic.fetch.ship.Count > 0) merge = GUILayout.Button("Merge");
            // load is always available
            var load = GUILayout.Button("Load");
            if (merge || load)
            {
                RequestCraftData(_selectedCraft, merge);
            }
            GUILayout.EndHorizontal();
        }

        private void RequestCraftData(SharedCraft craft, bool merge)
        {
            if (craft.CraftCache != null)
            {
                LoadCraft(craft, merge);
            }
            else
            {
                Debug.Log("CraftShare: loading craft: " + craft._id);
                RestApi.GetCraft(craft._id, delegate(IRestResponse response)
                {
                    if (response.ErrorException != null)
                    {
                        Debug.LogError("CraftShare: craft download failed");
                        Debug.LogException(response.ErrorException);
                        return;
                    }
                    craft.CraftCache = CLZF2.Decompress(response.RawBytes);
                    Debug.Log("CraftShare: craft bytes loaded: " + craft.CraftCache.Length);
                    LoadCraft(craft, merge);
                });
            }
        }

        private void LoadCraft(SharedCraft craft, bool merge)
        {
            //TODO: find a way to load the ConfigNode directly from a string and skip writing it to a file
            var craftPath = Path.Combine(ModGlobals.PluginDataPath, "download.craft");
            File.WriteAllBytes(craftPath, craft.CraftCache);
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
            var craftData = CLZF2.Compress(File.ReadAllBytes(craftPath));
            var thumbnail = File.ReadAllBytes(Path.Combine(ModGlobals.PluginDataPath, "thumbnail.png"));
            //TODO: find a way to get thumbnail and craft data without file access
            // create transfer object
            var craft = new SharedCraft
            {
                name = ship.shipName,
                facility = ship.shipFacility.ToString(),
                author = ModGlobals.AuthorName
            };
            // upload the craft
            Debug.Log("CraftShare: uploading craft");
            RestApi.PostCraft(craft, craftData, thumbnail, delegate(IRestResponse<SharedCraft> response)
            {
                if (response.ErrorException != null)
                {
                    Debug.LogError("CraftShare: sharing craft failed");
                    Debug.LogException(response.ErrorException);
                    return;
                }
                craft = response.Data;
                Debug.Log("CraftShare: new shared craft ID: " + craft._id);
                // refresh list to reflect the new entry
                UpdateCraftList();
            });
        }
    }
}