//using System;
//using System.IO;
//using System.IO.Compression;
//using System.Text;
//using UnityEngine;

//namespace CraftShare
//{
//    public class AblageP
//    {
//        public void Stuff()
//        {
//            if (GUILayout.Button("Do debug stuff"))
//            {
//                // request craft
//                //var craft = RestApi.GetCraft(_selectedCraft.Id);
//                //File.WriteAllText<CraftShareMod>("trolololo", "test.craft");
//                var ship = ShipConstruction.LoadShip(Path.Combine(ModGlobals.PluginDataPath, "GigoTest.craft"));
//                CraftThumbnail.TakeSnaphot(ship, 64, ModGlobals.PluginDataPath, "GigoTestCraft64");
//                CraftThumbnail.TakeSnaphot(ship, 128, ModGlobals.PluginDataPath, "GigoTestCraft128");
//                CraftThumbnail.TakeSnaphot(ship, 256, ModGlobals.PluginDataPath, "GigoTestCraft256");
//                CraftThumbnail.TakeSnaphot(ship, 512, ModGlobals.PluginDataPath, "GigoTestCraft512");
//                CraftThumbnail.TakeSnaphot(ship, 1024, ModGlobals.PluginDataPath, "GigoTestCraft1024");

//                //TODO: maybe get proper thumbnails from this? ShipConstruction.CaptureThumbnail();

//                var craftString = File.ReadAllText(Path.Combine(ModGlobals.PluginDataPath, "GigoTest.craft"));
//                File.WriteAllText("compressed.txt", StringCompressor.CompressString(craftString));
//            }
//            if (GUILayout.Button("Save"))
//            {
//                var ship = ShipConstruction.LoadShip(Path.Combine(ModGlobals.PluginDataPath, "GigoTest.craft"));
//                ship.shipName = "Halt einfach dein Maul";
//                ship.SaveShip().Save(string.Format("saves/{0}/Ships/{1}/{2}.craft", HighLogic.SaveFolder, ship.shipFacility, ship.shipName));
//            }
//        }

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

//        public static class StringCompressor
//        {
//            /// <summary>
//            /// Compresses the string given string using gzip.
//            /// </summary>
//            /// <param name="text">The string to compress.</param>
//            /// <returns>The compressed string.</returns>
//            public static string CompressString(string text)
//            {
//                var buffer = Encoding.UTF8.GetBytes(text);
//                var memoryStream = new MemoryStream();
//                using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
//                {
//                    gZipStream.Write(buffer, 0, buffer.Length);
//                }
//                memoryStream.Position = 0;

//                var compressedData = new byte[memoryStream.Length];
//                memoryStream.Read(compressedData, 0, compressedData.Length);

//                var gZipBuffer = new byte[compressedData.Length + 4];
//                Buffer.BlockCopy(compressedData, 0, gZipBuffer, 4, compressedData.Length);
//                Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gZipBuffer, 0, 4);
//                return Convert.ToBase64String(gZipBuffer);
//            }

//            /// <summary>
//            /// Decompresses the given string using gzip.
//            /// </summary>
//            /// <param name="compressedText">The compressed string.</param>
//            /// <returns>The uncompressed string.</returns>
//            public static string DecompressString(string compressedText)
//            {
//                var gZipBuffer = Convert.FromBase64String(compressedText);
//                using (var memoryStream = new MemoryStream())
//                {
//                    var dataLength = BitConverter.ToInt32(gZipBuffer, 0);
//                    memoryStream.Write(gZipBuffer, 4, gZipBuffer.Length - 4);

//                    var buffer = new byte[dataLength];

//                    memoryStream.Position = 0;
//                    using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
//                    {
//                        gZipStream.Read(buffer, 0, buffer.Length);
//                    }

//                    return Encoding.UTF8.GetString(buffer);
//                }
//            }
//        }
//    }
//}