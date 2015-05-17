using System;
using System.Net;
using KerbalCraft.Models;
using RestSharp;
using UnityEngine;

namespace KerbalCraft
{
    public class SettingsWindow
        : Window
    {
        private const int Width = 250;

        private readonly RestApi _api;

        private string _editHostAddress;
        private string _editUsername;
        private string _editPassword;
        private string _editNewPassword;

        private bool _working;
        private string _status;

        public SettingsWindow()
            : base(0, 0, ModGlobals.ModName + " - Settings")
        {
            _api = new RestApi();
            Show += OnShow;
        }

        private void Reset()
        {
            // fetch current values
            _editHostAddress = ModSettings.HostAddress;
            _editUsername = ModSettings.Username;
            _editPassword = ModSettings.Password;
            _editNewPassword = "";
            // reset message
            _status = null;
            // reset window size
            ResetWindowSize();
        }

        protected void OnShow()
        {
            Reset();
            // move the window to the screen center
            Rect.x = Screen.width/2 - Width/2;
            Rect.y = 80;
            // reset the working state in case it got stuck
            _working = false;
        }

        protected override void DrawMenu(int id)
        {
            // handle asynchronous responses
            _api.HandleResponses();
            // prevent clicking through the window into the editor
            PreventEditorClickthrough();
            // draw window content
            GUILayout.BeginVertical(GUILayout.Width(Width));
            if (_working)
            {
                GUILayout.Label("Working...");
            }
            else
            {
                DrawInputs();
                if (!string.IsNullOrEmpty(_status))
                {
                    GUILayout.Label(string.Format("Status: {0}", _status), ModGlobals.MessageStyle);
                }
            }
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private void DrawInputs()
        {
            GUILayout.Label("Host address:", ModGlobals.HeadStyle);
            GUILayout.BeginHorizontal();
            GUILayout.Label("https://");
            _editHostAddress = GUILayout.TextField(_editHostAddress);
            GUILayout.Label("/api/");
            GUILayout.EndHorizontal();
            GUILayout.Label("Username", ModGlobals.HeadStyle);
            _editUsername = GUILayout.TextField(_editUsername, 30);
            GUILayout.Label("Password", ModGlobals.HeadStyle);
            _editPassword = GUILayout.PasswordField(_editPassword, '#', 50);
            GUILayout.Label("New password (optional)", ModGlobals.HeadStyle);
            _editNewPassword = GUILayout.PasswordField(_editNewPassword, '#', 50);
            GUILayout.Label("Submit will either verify your credentials or create a new user if it is not existing. The settings are saved on success.");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Submit"))
            {
                try
                {
                    _working = true;
                    _api.SetConfig(_editHostAddress, _editUsername, _editPassword);
                    _api.PostUser(_editUsername, _editPassword, _editNewPassword, delegate(IRestResponse response)
                    {
                        _working = false;
                        if (response.ErrorException != null)
                        {
                            Debug.LogWarning("[KerbalCraft] login (transport error)");
                            Debug.LogException(response.ErrorException);
                            _status = "Connection failed";
                            return;
                        }
                        _status = null;
                        if (response.ContentLength > 0)
                        {
                            _status = _api.Deserialize<CraftMessage>(response).message;
                        }
                        switch (response.StatusCode)
                        {
                            case HttpStatusCode.OK:
                            case HttpStatusCode.NoContent:
                                if (!string.IsNullOrEmpty(_editNewPassword))
                                {
                                    _editPassword = _editNewPassword;
                                    _editNewPassword = "";
                                }
                                ModSettings.SetConfig(_editHostAddress, _editUsername, _editPassword);
                                ModSettings.SaveConfig();
                                break;
                            default:
                                if (string.IsNullOrEmpty(_status))
                                {
                                    _status = string.Format("Error ({0} {1})", (int)response.StatusCode, response.StatusDescription);
                                }
                                break;
                        }
                        // reset window size to adjust to the new message
                        ResetWindowSize();
                    });
                }
                catch (Exception ex)
                {
                    _working = false;
                    _status = string.Format("Error ({0})", ex.Message);
                    Debug.LogException(ex);
                }
            }
            if (GUILayout.Button("Reset"))
            {
                Reset();
            }
            if (GUILayout.Button("Close"))
            {
                Close();
            }
            GUILayout.EndHorizontal();
        }
    }
}