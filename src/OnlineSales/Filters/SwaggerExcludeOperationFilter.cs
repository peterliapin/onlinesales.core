// <copyright file="SwaggerExcludeOperationFilter.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SwaggerFilters
{
    public class SwaggerExcludeOperationFilter : IDocumentFilter
    {
        private static List<string> includedOperations = new List<string>();
        private static List<string> excludedOperations = new List<string>();

        public static void Configure(IConfiguration config)
        {
            var incNames = config.GetSection("Entities:Include").Get<string[]>();
            var excNames = config.GetSection("Entities:Exclude").Get<string[]>();
            includedOperations = (incNames != null && incNames.Length > 0) ? incNames.ToList() : new List<string>();
            excludedOperations = (excNames != null && excNames.Length > 0) ? excNames.ToList() : new List<string>();
        }

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var excludePaths = swaggerDoc.Paths.Where(p => OperationNeedsToBeExcluded(p.Key.ToString()));
            foreach (var path in excludePaths)
            {
                swaggerDoc.Paths.Remove(path.Key);
            }
        }

        public bool OperationNeedsToBeExcluded(string path)
        {
            var included = includedOperations.Count == 0 || includedOperations.Exists(op => path.Contains('/' + op));
            var excluded = includedOperations.Count == 0 && excludedOperations.Exists(op => path.Contains('/' + op));
            return !included || excluded;
        }
    }
}
