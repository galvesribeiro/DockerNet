using DockerNet.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DockerNet.Endpoints.Images
{
    public class ListImagesQuery
    {
        public bool All { get; internal set; }
        public bool Dangling { get; internal set; }
        public string Before { get; internal set; }
        public string Since { get; internal set; }
        public IList<string> Labels { get; internal set; } = new List<string>();
        public string Name { get; internal set; }

        internal string ToQueryString()
        {
            var queryBuilder = new StringBuilder();

            if (All)
                queryBuilder.Append("all=1&");

            if (!string.IsNullOrWhiteSpace(Name))
            {
                queryBuilder.Append($"filter={Name}&");
            }
            else
            {
                var filters = new Dictionary<string, string[]>();
                if (Dangling)
                    filters.Add("dangling", new[] { "true" });

                if (!string.IsNullOrWhiteSpace(Since))
                    filters.Add("since", new[] { Since });

                if (!string.IsNullOrWhiteSpace(Before))
                    filters.Add("before", new[] { Before });

                if (Labels.Count > 0)
                    filters.Add("label", Labels.ToArray());

                if (filters.Values.Count > 0)
                    queryBuilder.Append($"filters={JsonConvert.SerializeObject(filters)}");
            }

            var query = queryBuilder.ToString();

            if (query.EndsWith("&"))
                query = query.Remove(query.Length - 1);

            return query;
        }
    }

    public static class ListImageQueryExtensions
    {
        public static ListImagesQuery GetAll(this ListImagesQuery query, bool all = true)
        {
            query.All = all;
            return query;
        }

        public static ListImagesQuery WithDangling(this ListImagesQuery query, bool dangling = true)
        {
            query.Dangling = dangling;
            return query;
        }

        public static ListImagesQuery WithName(this ListImagesQuery query, string name)
        {
            query.Name = name;
            return query;
        }

        public static ListImagesQuery Since(this ListImagesQuery query, string since)
        {
            query.Since = since;
            return query;
        }

        public static ListImagesQuery Before(this ListImagesQuery query, string before)
        {
            query.Before = before;
            return query;
        }

        public static ListImagesQuery WithLabel(this ListImagesQuery query,
            string label, string op = LabelOperators.Applied, string value = "")
        {
            if (string.IsNullOrWhiteSpace(label)) throw new ArgumentNullException(nameof(label));

            var exp = label;

            if (op != LabelOperators.Applied)
                exp = $"{exp}{op}{value}";

            if (!query.Labels.Contains(exp))
                query.Labels.Add(exp);

            return query;
        }
    }
}
