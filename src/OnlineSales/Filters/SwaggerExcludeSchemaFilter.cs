// <copyright file="SwaggerExcludeSchemaFilter.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.OpenApi.Models;
using Nest;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SwaggerFilters
{
    public class SwaggerExcludeSchemaFilter : IDocumentFilter
    {
        private static List<string> includedSchemas = new List<string>();
        private static List<string> excludedSchemas = new List<string>();

        public static void Configure(IConfiguration config)
        {
            var incNames = config.GetSection("Entities:Include").Get<string[]>();
            var excNames = config.GetSection("Entities:Exclude").Get<string[]>();
            includedSchemas = (incNames != null && incNames.Length > 0) ? incNames.ToList() : new List<string>();
            excludedSchemas = (excNames != null && excNames.Length > 0) ? excNames.ToList() : new List<string>();
        }

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var schemas = context.SchemaRepository.Schemas;
            var excludeSchemas = schemas.Where(s => SchemaNeedsToBeExcluded(s.Key.ToString().ToLower()));
            foreach (var schema in excludeSchemas)
            {
                schemas.Remove(schema.Key);
            }
        }

        public bool SchemaNeedsToBeExcluded(string key)
        {
            var included = includedSchemas.Count == 0 || includedSchemas.Exists(s => key.Contains(s.ToLower()));
            var excluded = includedSchemas.Count == 0 && excludedSchemas.Exists(s => key.Contains(s.ToLower()));
            return !included || excluded;
        }
    }
}
