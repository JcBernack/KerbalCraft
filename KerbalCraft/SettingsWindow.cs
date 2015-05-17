using System;
using System.Net;
using RestSharp;
using UnityEngine;

namespace KerbalCraft
{
    public class SettingsWindow
        : Window
    {
        private const int Width = 250;
        private string _editHostAddress;
        private string _editUsername;
        private string _editPassword;

        private bool _working;
        private string _message;

        public SettingsWindow()
            : base(0, 0, ModGlobals.ModName + " - Settings")
        {
            Show += OnShow;
        }

        protected void OnShow()
        {
            // fetch current values
            _editHostAddress = ModSettings.HostAddress;
            _editUsername = ModSettings.Username;
            _editPassword = ModSettings.Password;
            // move the window to the screen center
            Rect.x = Screen.width/2 - Width/2;
            Rect.y = 80;
            // reset the working state in case it got stuck
            _working = false;
        }

        protected override void DrawMenu(int id)
        {
            PreventEditorClickthrough();
            GUILayout.BeginVertical(GUILayout.Width(Width));
            if (_working)
            {
                GUILayout.Label("Working...");
            }
            else
            {
                DrawInputs();
                if (!string.IsNullOrEmpty(_message))
                {
                    GUILayout.Label(_message, ModGlobals.MessageStyle);
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
            GUILayout.Label("Username:", ModGlobals.HeadStyle);
            _editUsername = GUILayout.TextField(_editUsername, 30);
            GUILayout.Label("Password:", ModGlobals.HeadStyle);
            _editPassword = GUILayout.PasswordField(_editPassword, '#', 50);
            GUILayout.Label("Login will either verify your credentials or create a new user if it is not existing. If the username is already taken but the password does not match login will fail. The settings are saved on success.");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Login"))
            {
                _working = true;
                ModSettings.SetConfig(_editHostAddress, _editUsername, _editPassword);
                try
                {
                    RestApi.PostUser(_editUsername, _editPassword, delegate(IRestResponse response)
                    {
                        _working = false;
                        if (response.ErrorException != null)
                        {
                            Debug.LogWarning("[KerbalCraft] login (transport error)");
                            Debug.LogException(response.ErrorException);
                            _message = "Connection failed";
                            return;
                        }
                        switch (response.StatusCode)
                        {
                            case HttpStatusCode.NoContent:
                                _message = "Success!";
                                ModSettings.SaveConfig();
                                break;
                            case HttpStatusCode.Unauthorized:
                                _message = "Login failed";
                                break;
                            default:
                                _message = string.Format("Error ({0}: {1})", (int)response.StatusCode, response.StatusDescription);
                                break;
                        }
                    });
                }
                catch (Exception ex)
                {
                    _working = false;
                    _message = ex.Message;
                    Debug.LogException(ex);
                }
            }
            if (GUILayout.Button("Close"))
            {
                Close();
            }
            GUILayout.EndHorizontal();
        }
    }
}