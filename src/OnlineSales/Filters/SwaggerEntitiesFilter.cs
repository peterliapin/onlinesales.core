// <copyright file="SwaggerEntitiesFilter.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Reflection;
using Microsoft.OpenApi.Models;
using OnlineSales.Configuration;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OnlineSales.Filters
{
    public class SwaggerEntitiesFilter : IDocumentFilter
    {
        private readonly List<string> includedEntities;
        private readonly List<string> excludedEntities;
        private readonly List<Type> currentTypes;

        public SwaggerEntitiesFilter(EntitiesConfig config)
        {
            currentTypes = Assembly.GetExecutingAssembly().GetTypes().ToList();
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

            var excludePaths = swaggerDoc.Paths.Where(p => OperationNeedsToBeExcluded(context, p.Key));
            foreach (var path in excludePaths)
            {
                swaggerDoc.Paths.Remove(path.Key);
            }
        }

        private bool SchemaNeedsToBeExcluded(string key)
        {
            var included = includedEntities.Count == 0 || includedEntities.Exists(s => key.Contains(s.ToLower()));
            var excluded = includedEntities.Count == 0 && excludedEntities.Exists(s => key.Contains(s.ToLower()));
            var current = currentTypes.Exists(t => t.Name.ToLower() == key);
            return current && (!included || excluded);
        }

        private bool OperationNeedsToBeExcluded(DocumentFilterContext context, string path)
        {
            var included = includedEntities.Count == 0 || includedEntities.Exists(op => path.Contains('/' + op));
            var excluded = includedEntities.Count == 0 && excludedEntities.Exists(op => path.Contains('/' + op));
            var api = context.ApiDescriptions.FirstOrDefault(d => { return d != null && d.RelativePath != null && path == '/' + d.RelativePath; }, null);
            var ignored = api != null && api.ActionDescriptor.DisplayName != null && api.ActionDescriptor.DisplayName.Contains("Plugin");
            return !ignored && (!included || excluded);
        }
    }
}