// <copyright file="QueryData.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Collections;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using AutoMapper.Internal;
using OnlineSales.Data;
using OnlineSales.Entities;

namespace OnlineSales.Infrastructure
{
    public class QueryData<T>
        where T : BaseEntityWithId
    {
        private readonly PgDbContext dbContext;

        public QueryData(List<QueryCommand> commands, int maxLimitSize, PgDbContext dbContext)
        {
            this.dbContext = dbContext;
            this.IncludeData = ParseIncludeCommands(commands);
            this.Limit = ParseLimitCommands(commands, maxLimitSize);
            this.Skip = ParseSkipCommands(commands);
            this.SearchData = ParseSearchCommands(commands);
            this.WhereData = ParseWhereCommands(commands);
            this.OrderData = ParseOrderCommands(commands);
            this.SelectData = ParseSelectCommands(commands);
        }

        public int Limit { get; set; } = 0;

        public int Skip { get; set; } = 0;

        public List<string> SearchData { get; set; }

        public List<WhereCommandData> WhereData { get; set; }

        public List<OrderCommandData> OrderData { get; set; }

        public List<PropertyInfo> IncludeData { get; set; }

        public SelectCommandData SelectData { get; set; }

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

        private SelectCommandData ParseSelectCommands(List<QueryCommand> commands)
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

        private List<OrderCommandData> ParseOrderCommands(List<QueryCommand> commands)
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

            return result;
        }

        private List<string> ParseSearchCommands(List<QueryCommand> commands)
        {
            var result = new List<string>();
            foreach (var cmdValue in commands.Where(c => c.Type == FilterType.Search && c.Value.Length > 0).Select(cmd => cmd.Value))
            {
                result.Add(cmdValue);
            }

            return result;
        }

        private List<WhereCommandData> ParseWhereCommands(List<QueryCommand> commands)
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

            return result;
        }

        private int ParseLimitCommands(List<QueryCommand> commands, int maxLimitSize)
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

        private int ParseSkipCommands(List<QueryCommand> commands)
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

        private List<PropertyInfo> ParseIncludeCommands(List<QueryCommand> commands)
        {
            var result = new List<PropertyInfo>();
            foreach (var cmd in commands.Where(c => c.Type == FilterType.Include).ToArray())
            {
                var property = ParseProperty(cmd.Value, cmd);
                var isCollection = property.PropertyType.IsGenericType && typeof(ICollection<>).MakeGenericType(property.PropertyType.GetGenericArguments()).IsAssignableFrom(property.PropertyType);

                if (dbContext.Model.FindEntityType(property.PropertyType) != null
                    || (isCollection && dbContext.Model.FindEntityType(property.PropertyType.GetGenericArguments().Single()) != null))
                {
                    result.Add(property);
                }
                else
                {
                    throw new QueryException(cmd.Source, "Invalid property in include query");
                }
            }

            return result;
        }     

        public sealed class SelectCommandData
        {
            public List<PropertyInfo> SelectedProperties { get; set; } = new List<PropertyInfo>();

            public bool IsSelect { get; set; } = false;
        }

        public sealed class OrderCommandData
        {
            public OrderCommandData(string propertyName)
            {
                this.Property = this.InitProperty(propertyName);
                this.Ascending = true;
            }

            public OrderCommandData(QueryCommand cmd)
            {
                var valueProps = cmd.Value.Split(' ');

                var propertyName = string.Empty;
                this.Ascending = true;

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
                        this.Ascending = valueProps.ElementAt(1).ToLowerInvariant() != "desc";
                        break;
                    default:
                        throw new QueryException(cmd.Source, "Failed to parse. Check syntax.");
                }

                this.Property = this.InitProperty(propertyName);
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
                this.StringValue = cmd.Value;

                var fProp = cmd.Props.ElementAtOrDefault(0);

                if (fProp == null || string.IsNullOrWhiteSpace(fProp))
                {
                    throw new QueryException(cmd.Source, "Property field not found");
                }

                var indexOffset = 0;
                if (fProp == "or")
                {
                    if (cmd.Props.Length == 1)
                    {
                        throw new QueryException(cmd.Source, "Property fields not found");
                    }

                    indexOffset = 1;
                    this.OrOperation = true;
                }
                else
                {
                    this.OrOperation = false;
                }

                var propertyName = cmd.Props.ElementAtOrDefault(indexOffset);
                var rawOperation = cmd.Props.ElementAtOrDefault(indexOffset + 1);
                this.Property = ParseProperty(propertyName, cmd);
                this.Operation = this.ParseOperation(rawOperation, cmd);
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
                    throw new QueryException(this.Cmd.Source, "Empty contain query argument");
                }

                var result = new List<Tuple<ContainsType, string>>();
                var sb = new StringBuilder();

                for (var i = 0; i < value.Length; ++i)
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
                    if ((sv == "null" || sv == string.Empty) && this.GetUnderlyingPropertyType() != typeof(string))
                    {
                        result.Add(null);
                    }
                    else
                    {
                        if (this.GetUnderlyingPropertyType() == typeof(DateTime) && DateTime.TryParseExact(sv, "yyyy-MM-dd'T'HH:mm:ss.fffK", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var date))
                        {
                            result.Add(date);
                        }
                        else if (this.GetUnderlyingPropertyType() == typeof(decimal) && decimal.TryParse(sv, out var decimalValue))
                        {
                            result.Add(decimalValue);
                        }
                        else if (this.GetUnderlyingPropertyType() == typeof(double) && double.TryParse(sv, out var doubleValue))
                        {
                            result.Add(doubleValue);
                        }
                        else if (this.GetUnderlyingPropertyType() == typeof(int) && int.TryParse(sv, out var intValue))
                        {
                            result.Add(intValue);
                        }
                        else if (this.GetUnderlyingPropertyType() == typeof(bool) && bool.TryParse(sv, out var boolValue))
                        {
                            result.Add(boolValue);
                        }
                        else if (this.GetUnderlyingPropertyType().IsEnum && !int.TryParse(sv, out _) && Enum.TryParse(this.GetUnderlyingPropertyType(), sv, true, out var enumValue))
                        {
                            result.Add(enumValue);
                        }
                        else if (this.GetUnderlyingPropertyType() == typeof(string))
                        {
                            result.Add(sv);
                            if (sv == string.Empty)
                            {
                                result.Add(null);
                            }
                        }
                        else
                        {
                            throw new QueryException(this.Cmd.Source, "Property type and provided type value do not match");
                        }
                    }
                }

                return result;
            }

            public HashSet<string> ParseStringValues()
            {
                var result = new HashSet<string>();

                var sb = new StringBuilder();

                for (var i = 0; i < this.StringValue.Length; ++i)
                {
                    if (this.StringValue[i] == '|')
                    {
                        if (i == 0 || this.StringValue[i - 1] != '\\')
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
                        sb.Append(this.StringValue[i]);
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

            private Type GetUnderlyingPropertyType()
            {
                var nt = Nullable.GetUnderlyingType(this.Property.PropertyType);
                return nt == null ? this.Property.PropertyType : nt!;
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
