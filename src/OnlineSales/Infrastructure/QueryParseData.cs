// <copyright file="QueryParseData.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using OnlineSales.Entities;

namespace OnlineSales.Infrastructure
{
    public class QueryParseData<T>
        where T : BaseEntityWithId
    {
        public readonly int Limit = 0;
        public readonly int Skip = 0;
        public readonly ImmutableList<string> SearchData;
        public readonly ImmutableList<WhereCommandData> WhereData;
        public readonly ImmutableList<OrderCommandData> OrderData;
        public readonly SelectCommandData SelectData;

        public QueryParseData(string[] cmds, int maxLimitSize)
        {
            var commands = Parse(cmds);
            Limit = ParseLimitCommands(commands, maxLimitSize);
            Skip = ParseSkipCommands(commands);
            SearchData = ParseSearchCommands(commands);
            WhereData = ParseWhereCommands(commands);
            OrderData = ParseOrderCommands(commands);
            SelectData = ParseSelectCommands(commands);
        }

        private QueryCommand[] Parse(string[] query)
        {
            var processedCommands = new List<QueryCommand>();
            var errorList = new List<QueryException>();

            foreach (var cmd in query)
            {
                var match = Regex.Match(cmd, "query+?=(?'value'.*)");
                if (match.Success)
                {
                    var qcmd = new QueryCommand()
                    {
                        Type = FilterType.Search,
                        Props = new string[0],
                        Value = match.Groups["value"].Captures[0].Value,
                        Source = cmd,
                    };
                    processedCommands.Add(qcmd);
                }
                else
                {
                    match = Regex.Match(cmd, "filter(\\[(?'property'.*?)\\])+?=(?'value'.*)");
                    if (!match.Success)
                    {
                        errorList.Add(new QueryException(cmd, "Failed to parse command"));
                        continue;
                    }

                    var type = match.Groups["property"].Captures[0].Value.ToLowerInvariant();
                    if (type == null || string.IsNullOrWhiteSpace(type) || !QueryCommand.FilterMappings.ContainsKey(type))
                    {
                        errorList.Add(new QueryException(cmd, $"Failed to parse command. Operator '{type}' not found. Available operators: {QueryCommand.AvailableCommandString}"));
                        continue;
                    }

                    var qcmd = new QueryCommand()
                    {
                        Type = QueryCommand.FilterMappings.First(m => m.Key == type).Value,
                        Props = match.Groups["property"].Captures.Skip(1).Select(capture => capture.Value).ToArray(),
                        Value = match.Groups["value"].Captures[0].Value,
                        Source = cmd,
                    };
                    processedCommands.Add(qcmd);
                }
            }

            if (errorList.Any())
            {
                throw new QueryException(errorList);
            }

            return processedCommands.ToArray();
        }

        private SelectCommandData ParseSelectCommands(QueryCommand[] commands)
        {
            var trueFields = new List<PropertyInfo>();
            var falseFields = new List<PropertyInfo>();

            var validFieldCommands = commands.Where(c => c.Type == FilterType.Fields);
            foreach (var command in validFieldCommands)
            {
                var parseSuccess = bool.TryParse(command.Value, out var value);
                if (parseSuccess)
                {
                    var pi = command.ParseProperty<T>();
                    if (value)
                    {
                        trueFields.Add(pi);
                    }
                    else
                    {
                        falseFields.Add(pi);
                    }
                }
                else
                {
                    throw new QueryException(command.Source, "Incorrect argument in field command");
                }
            }

            if (trueFields.Count == 0 && falseFields.Count == 0)
            {
                return new SelectCommandData
                {
                    SelectedProperties = new List<PropertyInfo>(),
                    IsSelect = false,
                };
            }

            if (trueFields.Count == 0)
            {
                trueFields = typeof(T).GetProperties().ToList();
            }

            var selectedProps = trueFields.Where(p => !falseFields.Contains(p)).ToList();

            return new SelectCommandData
            {
                SelectedProperties = selectedProps,
                IsSelect = true,
            };
        }

        private ImmutableList<OrderCommandData> ParseOrderCommands(QueryCommand[] commands)
        {
            var result = new List<OrderCommandData>();

            var orderCommands = commands.Where(c => c.Type == FilterType.Order).ToArray();
            if (orderCommands.Length > 1)
            {
                Array.ForEach(orderCommands, c =>
                {
                    if (c.Props.ElementAtOrDefault(0) == null || string.IsNullOrEmpty(c.Props[0]))
                    {
                        throw new QueryException(c.Source, "Fa iled to parse. Check syntax.");
                    }
                });

                orderCommands = orderCommands.OrderBy(c => c.Props[0]).ToArray();
            }

            foreach (var oc in orderCommands)
            {
                result.Add(new OrderCommandData(oc));
            }

            if (result.Count == 0)
            {
                result.Add(new OrderCommandData("Id"));
            }

            return result.ToImmutableList();
        }

        private ImmutableList<string> ParseSearchCommands(QueryCommand[] commands)
        {
            var result = new List<string>();
            foreach (var cmdValue in commands.Where(c => c.Type == FilterType.Search && c.Value.Length > 0).Select(cmd => cmd.Value))
            {
                result.Add(cmdValue);
            }

            return result.ToImmutableList();
        }

        private ImmutableList<WhereCommandData> ParseWhereCommands(QueryCommand[] commands)
        {
            var result = new List<WhereCommandData>();
            var orResult = new List<WhereUnitData>();
            var errorList = new List<QueryException>();

            foreach (var cmd in commands.Where(c => c.Type == FilterType.Where).ToArray())
            {
                try
                {
                    var unitData = new WhereUnitData(cmd);

                    if (unitData.OrOperation)
                    {
                        orResult.Add(unitData);
                    }
                    else
                    {
                        var tempResult = new List<WhereUnitData>();
                        tempResult.Add(unitData);
                        result.Add(new WhereCommandData
                        {
                            Data = tempResult,
                            OrOperation = false,
                        });
                    }
                }
                catch (QueryException ex)
                {
                    errorList.Add(ex);
                }
            }

            if (errorList.Any())
            {
                throw new QueryException(errorList);
            }

            if (orResult.Count > 0)
            {
                result.Add(new WhereCommandData
                {
                    Data = orResult,
                    OrOperation = true,
                });
            }

            return result.ToImmutableList();
        }

        private int ParseLimitCommands(QueryCommand[] commands, int maxLimitSize)
        {
            var res = maxLimitSize;
            var limitCommand = commands.FirstOrDefault(c => c.Type == FilterType.Limit);
            if (limitCommand != null)
            {
                if (!int.TryParse(limitCommand.Value, out res))
                {
                    throw new QueryException(limitCommand.Source, $"Failed to parse number '{limitCommand.Value}'");
                }

                if (res <= 0)
                {
                    throw new QueryException(limitCommand.Source, $"Invalid limit size. (Maximum {maxLimitSize}");
                }

                if (res > maxLimitSize)
                {
                    throw new QueryException(limitCommand.Source, $"Max limit size exceeded. (Maximum {maxLimitSize}");
                }
            }

            return res;
        }

        private int ParseSkipCommands(QueryCommand[] commands)
        {
            var res = 0;
            var skipCommand = commands.FirstOrDefault(c => c.Type == FilterType.Skip);
            if (skipCommand != null)
            {
                if (!int.TryParse(skipCommand.Value, out res))
                {
                    throw new QueryException(skipCommand.Source, $"Failed to parse number '{skipCommand.Value}'");
                }

                if (res < 0)
                {
                    throw new QueryException(skipCommand.Source, $"Invalid skip size");
                }
            }

            return res;
        }

        public sealed class SelectCommandData
        {
            public IList<PropertyInfo> SelectedProperties { get; set; } = new List<PropertyInfo>();

            public bool IsSelect { get; set; } = false;
        }

        public sealed class OrderCommandData
        {
            public OrderCommandData(string propertyName)
            {
                Property = InitProperty(propertyName);
                Ascending = true;
            }

            public OrderCommandData(QueryCommand cmd)
            {
                var valueProps = cmd.Value.Split(' ');

                var propertyName = string.Empty;
                Ascending = true;

                switch (valueProps.Length)
                {
                    case 0:
                        propertyName = "Id";
                        break;
                    case 1:
                        propertyName = valueProps.First();
                        break;
                    case 2:
                        propertyName = valueProps.First();
                        Ascending = valueProps.ElementAt(1).ToLowerInvariant() != "desc";
                        break;
                    default:
                        throw new QueryException(cmd.Source, "Failed to parse. Check syntax.");
                }

                Property = InitProperty(propertyName);
            }

            public PropertyInfo Property { get; set; }

            public bool Ascending { get; set; }

            private PropertyInfo InitProperty(string propertyName)
            {
                var typeProperties = typeof(T).GetProperties();

                var property = typeProperties.FirstOrDefault(p => p.Name.ToLowerInvariant() == propertyName.ToLowerInvariant());

                if (property == null)
                {
                    throw new QueryException(string.Empty, $"No such property '{propertyName}'");
                }

                return property;
            }
        }

        public sealed class WhereCommandData
        {
            public IList<WhereUnitData> Data { get; set; } = new List<WhereUnitData>();

            public bool OrOperation { get; set; } = false;
        }

        public sealed class WhereUnitData
        {
            public readonly string StringValue;
            public readonly PropertyInfo Property;
            public readonly WOperand Operation;
            public readonly bool OrOperation;
            public readonly QueryCommand Cmd;

            public WhereUnitData(QueryCommand cmd)
            {
                this.Cmd = cmd;
                StringValue = cmd.Value;

                var fProp = cmd.Props.ElementAtOrDefault(0);

                if (fProp == null || string.IsNullOrWhiteSpace(fProp))
                {
                    throw new QueryException(cmd.Source, "Property field not found");
                }

                int indexOffset = 0;
                if (fProp == "or")
                {
                    if (cmd.Props.Length == 1)
                    {
                        throw new QueryException(cmd.Source, "Property fields not found");
                    }

                    indexOffset = 1;
                    OrOperation = true;
                }
                else
                {
                    OrOperation = false;
                }

                var propertyName = cmd.Props.ElementAtOrDefault(indexOffset);
                var rawOperation = cmd.Props.ElementAtOrDefault(indexOffset + 1);
                Property = ParseProperty(propertyName, cmd);
                Operation = ParseOperation(rawOperation, cmd);
            }

            public enum ContainsType
            {
                MatchAll,
                Substring,
            }

            public List<Tuple<ContainsType, string>> ParseContainValue(string value)
            {
                if (value.Length == 0)
                {
                    throw new QueryException(Cmd.Source, "Empty contain query argument");
                }

                var result = new List<Tuple<ContainsType, string>>();
                var sb = new StringBuilder();

                for (int i = 0; i < value.Length; ++i)
                {
                    if (value[i] == '*')
                    {
                        if (i == 0 || value[i - 1] != '\\')
                        {
                            if (sb.Length > 0)
                            {
                                result.Add(new Tuple<ContainsType, string>(ContainsType.Substring, sb.ToString()));
                            }

                            result.Add(new Tuple<ContainsType, string>(ContainsType.MatchAll, string.Empty));
                            sb.Clear();
                        }
                        else
                        {
                            sb.Remove(sb.Length - 1, 1);
                            sb.Append('*');
                        }
                    }
                    else
                    {
                        sb.Append(value[i]);
                    }
                }

                if (sb.Length > 0)
                {
                    result.Add(new Tuple<ContainsType, string>(ContainsType.Substring, sb.ToString()));
                }

                return result;
            }

            public List<object?> ParseValues(IEnumerable<string> input)
            {
                var result = new List<object?>();

                foreach (var sv in input)
                {
                    if ((sv == "null" || sv == string.Empty) && GetUnderlyingPropertyType() != typeof(string))
                    {
                        result.Add(null);
                    }
                    else
                    {
                        if (GetUnderlyingPropertyType() == typeof(DateTime) && DateTime.TryParseExact(sv, "yyyy-MM-dd'T'HH:mm:ss.fffK", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var date))
                        {
                            result.Add(date);
                        }
                        else if (GetUnderlyingPropertyType() == typeof(decimal) && decimal.TryParse(sv, out var decimalValue))
                        {
                            result.Add(decimalValue);
                        }
                        else if (GetUnderlyingPropertyType() == typeof(double) && double.TryParse(sv, out var doubleValue))
                        {
                            result.Add(doubleValue);
                        }
                        else if (GetUnderlyingPropertyType() == typeof(int) && int.TryParse(sv, out int intValue))
                        {
                            result.Add(intValue);
                        }
                        else if (GetUnderlyingPropertyType() == typeof(bool) && bool.TryParse(sv, out bool boolValue))
                        {
                            result.Add(boolValue);
                        }
                        else if (GetUnderlyingPropertyType().IsEnum && !int.TryParse(sv, out _) && Enum.TryParse(GetUnderlyingPropertyType(), sv, true, out var enumValue))
                        {
                            result.Add(enumValue);
                        }
                        else if (GetUnderlyingPropertyType() == typeof(string))
                        {
                            result.Add(sv);
                            if (sv == string.Empty)
                            {
                                result.Add(null);
                            }
                        }
                        else
                        {
                            throw new QueryException(Cmd.Source, "Property type and provided type value do not match");
                        }
                    }
                }

                return result;
            }

            public HashSet<string> ParseStringValues()
            {
                var result = new HashSet<string>();

                var sb = new StringBuilder();

                for (int i = 0; i < StringValue.Length; ++i)
                {
                    if (StringValue[i] == '|')
                    {
                        if (i == 0 || StringValue[i - 1] != '\\')
                        {
                            result.Add(sb.ToString());
                            sb.Clear();
                        }
                        else
                        {
                            sb.Remove(sb.Length - 1, 1);
                            sb.Append('|');
                        }
                    }
                    else
                    {
                        sb.Append(StringValue[i]);
                    }
                }

                result.Add(sb.ToString());

                return result;
            }

            public bool IsNullableProperty()
            {
                var context = new NullabilityInfoContext();
                var info = context.Create(Property);
                return info.WriteState == NullabilityState.Nullable;
            }

            private static PropertyInfo ParseProperty(string? propertyName, QueryCommand cmd)
            {
                if (propertyName == null || string.IsNullOrWhiteSpace(propertyName))
                {
                    throw new QueryException(cmd.Source, "Property field not found");
                }

                var typeProperties = typeof(T).GetProperties();
                var property = typeProperties.FirstOrDefault(p => p.Name.ToLowerInvariant() == propertyName.ToLowerInvariant());

                if (property == null)
                {
                    throw new QueryException(cmd.Source, $"No such property '{propertyName}'");
                }

                return property;
            }          

            private Type GetUnderlyingPropertyType()
            {
                var nt = Nullable.GetUnderlyingType(Property.PropertyType);
                return nt == null ? Property.PropertyType : nt!;
            }

            private WOperand ParseOperation(string? rawOperation, QueryCommand cmd)
            {
                if (rawOperation == null)
                {
                    rawOperation = "eq";
                }

                if (string.IsNullOrEmpty(rawOperation))
                {
                    throw new QueryException(cmd.Source, "Empty operand");
                }

                if (!QueryCommand.OperandMappings.ContainsKey(rawOperation))
                {
                    throw new QueryException(cmd.Source, $"No such operand '{rawOperation}'");
                }

                return QueryCommand.OperandMappings.FirstOrDefault(m => m.Key == rawOperation).Value;
            }
        }
    }
}
