// <copyright file="SwaggerEntitiesFilter.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.OpenApi.Models;
using Nest;
using OnlineSales.Configuration;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SwaggerFilters
{
    public class SwaggerEntitiesFilter : IDocumentFilter
    {
        private readonly List<string> includedEntities;
        private readonly List<string> excludedEntities;

        public SwaggerEntitiesFilter(EntitiesConfig config)
        {
            includedEntities = excludedEntities = new List<string>();
            if (config != null)
            {
                includedEntities = config.Include.ToList();
                excludedEntities = config.Exclude.ToList();
            }
        }

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var schemas = context.SchemaRepository.Schemas;
            var excludeSchemas = schemas.Where(s => SchemaNeedsToBeExcluded(s.Key.ToString().ToLower()));
            foreach (var schema in excludeSchemas)
            {
                schemas.Remove(schema.Key);
            }

            var excludePaths = swaggerDoc.Paths.Where(p => OperationNeedsToBeExcluded(p.Key.ToString()));
            foreach (var path in excludePaths)
            {
                swaggerDoc.Paths.Remove(path.Key);
            }
        }

        public bool SchemaNeedsToBeExcluded(string key)
        {
            var included = includedEntities.Count == 0 || includedEntities.Exists(s => key.Contains(s.ToLower()));
            var excluded = includedEntities.Count == 0 && excludedEntities.Exists(s => key.Contains(s.ToLower()));
            return !included || excluded;
        }

        public bool OperationNeedsToBeExcluded(string path)
        {
            var included = includedEntities.Count == 0 || includedEntities.Exists(op => path.Contains('/' + op));
            var excluded = includedEntities.Count == 0 && excludedEntities.Exists(op => path.Contains('/' + op));
            return !included || excluded;
        }
    }
}
