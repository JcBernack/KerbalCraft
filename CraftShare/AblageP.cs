//using System;
//using System.IO;
//using System.IO.Compression;
//using System.Text;
//using UnityEngine;

//namespace CraftShare
//{
//    public class AblageP
//    {

//        public static string GetCraftPath(string facility, string name)
//        {
//            return string.Format("saves/{0}/Ships/{1}/{2}.craft", HighLogic.SaveFolder, facility, name);
//        }

//        public static string GetCraftThumbnailPath(string facility, string name)
//        {
//            return string.Format("thumbs/{0}_{1}_{2}.png", HighLogic.SaveFolder, facility, name);
//        }

//        public class ShareWindow
//        : WindowBase
//        {
//            private CraftBrowser _browser;

//            protected override void Initialize()
//            {
//                _browser = new CraftBrowser(new Rect(50, 50, 400, 500), EditorFacility.VAB, "default", "lolwut titel",
//                    OnFileSelected, OnCancel, HighLogic.Skin, ModGlobals.TrollfaceTexture, true, true);
//            }

//            private void OnCancel()
//            {
//                throw new NotImplementedException();
//            }

//            private void OnFileSelected(string fullPath, string flagUrl, CraftBrowser.LoadType loadType)
//            {
//                throw new NotImplementedException();
//            }

//            protected override void DrawGUI()
//            {
//                _browser.OnGUI();
//            }
//        }
//    }
//}