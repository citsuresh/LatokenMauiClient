using Newtonsoft.Json;

namespace LatokenMauiClient
{
    public class LastUpdatedTimestampData
    {
        public Dictionary<string, LastUpdatedTransfersTimestampData> TransfersTimestamps { get; set; } = new Dictionary<string, LastUpdatedTransfersTimestampData>();

        public string Serialize()
        {
            return JsonConvert.SerializeObject(TransfersTimestamps, Formatting.Indented);
        }

        public void DeSerialize(string serializedString)
        {
            var deserializedObj = JsonConvert.DeserializeObject<Dictionary<string, LastUpdatedTransfersTimestampData>>(serializedString);
            if (deserializedObj is Dictionary<string, LastUpdatedTransfersTimestampData>)
            {
                TransfersTimestamps = deserializedObj;
            }
        }
    }

    public class LastUpdatedTransfersTimestampData
    {
        public DateTime? LastUpdatedAllTransfersTimeStamp { get; set; }
        public DateTime? LastUpdatedRewardsAirdropsTimeStamp { get; set; }
    }
}
