// <copyright file="QueryCommand.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Entities
{
    public enum FilterType
    {
        Fields = 0,
        Limit = 1,
        Order = 2,
        Skip = 3,
        Where = 4,
        None = 5,
    }

    public enum WOperand
    {
        Equal = 0,
        And = 1,
        Or = 2,
        GreaterThan = 3,
        GreaterThanOrEquals = 4,
        LessThan = 5,
        LessThanOrEquals = 6,
        NotEqual = 7,
    }

    public class QueryCommand
    {
        public static Dictionary<string, FilterType> FilterMappings { get; } = new Dictionary<string, FilterType>()
        {
            { "field", FilterType.Fields },
            { "limit", FilterType.Limit },
            { "order", FilterType.Order },
            { "skip", FilterType.Skip },
            { "where", FilterType.Where },
        };

        public static Dictionary<string, WOperand> OperandMappings { get; } = new Dictionary<string, WOperand>()
        {
            { "or", WOperand.Or },
            { "gt", WOperand.GreaterThan },
            { "gte", WOperand.GreaterThanOrEquals },
            { "lt", WOperand.LessThan },
            { "lte", WOperand.LessThanOrEquals },
            { "neq", WOperand.NotEqual },
        };

        public FilterType Type { get; set; } = FilterType.None;

        public string[] Props { get; set; } = Array.Empty<string>();

        public string Value { get; set; } = string.Empty;
    }
}
