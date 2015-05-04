using System.Runtime.Serialization;
using UnityEngine;

namespace CraftShare
{
    [DataContract]
    public class SharedCraft
    {
        [DataMember(Name = "_id", EmitDefaultValue = false)]
        public string Id;

        [DataMember(Name = "name")]
        public string Name;

        [DataMember(Name = "author")]
        public string Author;
        
        [DataMember(Name = "date", EmitDefaultValue = false)]
        public string Date;

        [DataMember(Name = "craft")]
        public string Craft;

        [DataMember(Name = "thumbnail")]
        public string ThumbnailString;

        public Texture2D Thumbnail;

        public void SerializeThumbnail()
        {
            
        }

        public void DeserializeThumbnail()
        {
            
        }
    }
}