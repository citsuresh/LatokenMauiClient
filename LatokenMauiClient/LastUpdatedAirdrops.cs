using Newtonsoft.Json;

namespace LatokenMauiClient
{
    public class LastUpdatedAirdrops
    {
        public List<string> LastUpdatedAirdropData { get; set; } = new List<string>();

        public string Serialize()
        {
            return JsonConvert.SerializeObject(LastUpdatedAirdropData, Formatting.Indented);
        }

        public void DeSerialize(string serializedString)
        {
            var deserializedObj = JsonConvert.DeserializeObject<List<string>>(serializedString);
            if (deserializedObj is List<string>)
            {
                LastUpdatedAirdropData = deserializedObj;
            }
        }
    }

}
