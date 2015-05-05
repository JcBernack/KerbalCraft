using System.Runtime.Serialization;

namespace CraftShare
{
    [DataContract]
    public class SharedCraft
    {
        [DataMember(Name = "_id", EmitDefaultValue = false)]
        public string Id;

        [DataMember(Name = "date", EmitDefaultValue = false)]
        public string Date;

        [DataMember(Name = "name")]
        public string Name;

        [DataMember(Name = "facility")]
        public string Facility;

        [DataMember(Name = "author")]
        public string Author;

        [DataMember(Name = "craft")]
        public string Craft;
    }
}