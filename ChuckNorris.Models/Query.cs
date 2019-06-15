using Newtonsoft.Json;
using System.Collections.Generic;

namespace ChuckNorris.Models
{
    public class Query
    {
        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("result")]
        public IList<Result> Result { get; set; }
    }
}
