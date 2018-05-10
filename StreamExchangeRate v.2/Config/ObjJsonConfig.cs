using Newtonsoft.Json;
using System.Collections.Generic;

namespace StreamExchangeRate_v._2.Config
{
    public class ObjJsonConfig
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("providers")]
        public IList<Provider> Providers { get; set; }
    }
    public class Provider
    {
        [JsonProperty("providerId")]
        public string ProviderId { get; set; }

        [JsonProperty("mappedTo")]
        public string mappedTo { get; set; }
    }
}
