// <copyright file="RouteToKebabCase.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Text.RegularExpressions;

namespace OnlineSales.Configuration
{
    public class RouteToKebabCase : IOutboundParameterTransformer
    {
        public string? TransformOutbound(object? value)
        {
            if (value == null)
            {
                return null;
            }

            return Regex.Replace(value.ToString() !, "([a-z])([A-Z])", "$1-$2").ToLower();
        }
    }
}