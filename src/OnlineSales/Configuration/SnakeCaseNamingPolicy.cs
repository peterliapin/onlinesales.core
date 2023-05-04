// <copyright file="SnakeCaseNamingPolicy.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Text.Json;

namespace OnlineSales.Configuration
{
    public class SnakeCaseNamingPolicy : JsonNamingPolicy
    {
        public static SnakeCaseNamingPolicy Instance { get; } = new SnakeCaseNamingPolicy();

        public override string ConvertName(string name)
        {
            return this.ToSnakeCase(name);
        }

        public string ToSnakeCase(string str)
        {
            return string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
        }
    }
}
