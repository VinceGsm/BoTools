using Newtonsoft.Json;
using System.Collections.Generic;

namespace BoTools.Model
{
    public class GeoLoc
    {
        public string Name { get; set; }
        [JsonIgnore]
        public Dictionary<string, string> LocalNames { get; set; }
        public double Lat { get; set; }           
        public double Lon { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
    }
}
