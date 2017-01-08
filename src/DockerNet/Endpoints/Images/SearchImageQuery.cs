using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace DockerNet.Endpoints.Images
{
    public class SearchImageQuery
    {
        public string Term { get; set; }
        public int? Limit { get; set; }
        public int? Stars { get; set; }
        public bool? IsAutomated { get; set; }
        public bool? IsOfficial { get; set; }

        internal string ToQueryString()
        {
            var queryBuilder = new StringBuilder();

            queryBuilder.Append($"term={Term.ToLower().Trim()}");

            if (Limit.HasValue)
                queryBuilder.Append($"&limit={Limit.Value}");

            var filters = new Dictionary<string, string[]>();

            if (Stars.HasValue)
                filters.Add("stars", new[] { Stars.Value.ToString() });

            if (IsAutomated.HasValue)
                filters.Add("is-automated", new[] { IsAutomated.Value.ToString() });

            if (IsOfficial.HasValue)
                filters.Add("is-official", new[] { IsOfficial.Value.ToString() });

            if (filters.Values.Count > 0)
                queryBuilder.Append($"&filters={JsonConvert.SerializeObject(filters)}");

            return queryBuilder.ToString();
        }
    }
}
