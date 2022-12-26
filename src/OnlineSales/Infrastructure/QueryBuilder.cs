// <copyright file="QueryBuilder.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using OnlineSales.Entities;

namespace OnlineSales.Infrastructure
{
    public static class QueryBuilder<T>
        where T : BaseEntity
    {
        public static IQueryable<T> ReadIntoQuery(IQueryable<T> query, string[] queryString, out bool selectStatementAvailable)
        {
            var cmds = Parse(queryString);
            query = AppendWhereExpression(query, cmds);
            query = AppendSkipExpression(query, cmds);
            query = AppendLimitExpression(query, cmds);
            query = AppendOrderExpression(query, cmds);

            selectStatementAvailable = IsSelectCommandExists(cmds);
            return query;
        }

        public static bool IsSelectCommandExists(QueryCommand[] commands)
        {
            var validFieldCommands = commands.Where(c => c.Type == FilterType.Fields && bool.TryParse(c.Value, out var _)).ToList();
            return validFieldCommands.Any();
        }

        public static async Task<object?> ExecuteSelectExpression(IQueryable<T> query, string[] queryString)
        {
            var commands = Parse(queryString);
            var typeProperties = typeof(T).GetProperties();
            var expressionParameter = Expression.Parameter(typeof(T));

            var validFieldCommands = commands.Where(c => c.Type == FilterType.Fields && bool.TryParse(c.Value, out var _)).ToList();
            if (validFieldCommands.Any())
            {
                var atLeastOneTrue = validFieldCommands.Select(c => c.Value).Any(bool.Parse);
                var selectedProperties = new List<PropertyInfo>(atLeastOneTrue ? Array.Empty<PropertyInfo>() : typeProperties);
                foreach (var cmd in validFieldCommands)
                {
                    var prop = cmd.Props.ElementAtOrDefault(0);
                    if (prop == null)
                    {
                        continue;
                    }

                    switch (bool.Parse(cmd.Value))
                    {
                        case true:
                            var typeProperty = typeProperties.FirstOrDefault(p => p.Name.ToLowerInvariant() == prop);
                            if (typeProperty == null)
                            {
                                continue;
                            }

                            selectedProperties.Add(typeProperty);
                            break;
                        case false:
                            var rProperty = selectedProperties.FirstOrDefault(p => p.Name.ToLowerInvariant() == prop);
                            if (rProperty == null)
                            {
                                continue;
                            }

                            selectedProperties.Remove(rProperty);
                            break;
                    }
                }

                var outputType = TypeHelper.CompileTypeForSelectStatement(selectedProperties.ToArray());
                var delegateType = typeof(Func<,>).MakeGenericType(typeof(T), outputType);
                var createOutputTypeExpression = Expression.New(outputType);

                var expressionSelectedProperties = selectedProperties.Select(p =>
                {
                    var bindProp = outputType.GetProperty(p.Name);
                    var exprProp = Expression.Property(expressionParameter, p);
                    return Expression.Bind(bindProp!, exprProp);
                }).ToArray();
                var expressionCreateArray = Expression.MemberInit(createOutputTypeExpression, expressionSelectedProperties);
                dynamic lambda = Expression.Lambda(delegateType, expressionCreateArray, expressionParameter);

                var queryMethod = typeof(Queryable).GetMethods().FirstOrDefault(m => m.Name == "Select" && m.GetParameters()[1].ParameterType.GetGenericArguments()[0].GetGenericArguments().Length == 2) !.MakeGenericMethod(typeof(T), outputType);

                var toArrayAsyncMethod = typeof(EntityFrameworkQueryableExtensions).GetMethod("ToArrayAsync") !.MakeGenericMethod(outputType);

                var selectQueryable = queryMethod!.Invoke(query, new object[] { query, lambda });

                var outputTypeTaskResultProp = typeof(Task<>).MakeGenericType(outputType.MakeArrayType()).GetProperty("Result");

                var selectResult = (Task)toArrayAsyncMethod.Invoke(selectQueryable, new object?[] { selectQueryable!, null }) !;
                await selectResult;
                var taskResult = outputTypeTaskResultProp!.GetValue(selectResult);
                return taskResult;
            }

            return null;
        }

        private static QueryCommand[] Parse(string[] query)
        {
            var processedCommands = new List<QueryCommand>();
            foreach (var cmd in query)
            {
                var match = Regex.Match(cmd, "filter(\\[(?'property'.*?)\\])+?=(?'value'.*)");
                if (!match.Success)
                {
                    continue;
                }

                var type = match.Groups["property"].Captures[0].Value.ToLowerInvariant();
                if (type == null || string.IsNullOrWhiteSpace(type) || !QueryCommand.FilterMappings.ContainsKey(type))
                {
                    continue; // broken command
                }

                var qcmd = new QueryCommand()
                {
                    Type = QueryCommand.FilterMappings.First(m => m.Key == type).Value,
                    Props = match.Groups["property"].Captures.Skip(1).Select(capture => capture.Value).ToArray(),
                    Value = match.Groups["value"].Captures[0].Value,
                };
                processedCommands.Add(qcmd);
            }

            return processedCommands.ToArray();
        }

        private static IQueryable<T> AppendWhereExpression(IQueryable<T> query, QueryCommand[] commands)
        {
            BinaryExpression? whereExpression = null;
            BinaryExpression? orExpression = null;
            var expressionParameter = Expression.Parameter(typeof(T));

            // Executing received commands
            foreach (var cmd in commands.Where(c => c.Type == FilterType.Where).ToArray())
            {
                if (TryProcessWhereOperand(expressionParameter, cmd, out var outExpr, out var oExpr))
                {
                    if (oExpr)
                    {
                        if (orExpression == null)
                        {
                            orExpression = outExpr!;
                            continue;
                        }

                        orExpression = Expression.Or(orExpression, outExpr!);
                        continue;
                    }

                    if (whereExpression == null)
                    {
                        whereExpression = outExpr;
                        continue;
                    }

                    whereExpression = Expression.And(whereExpression, outExpr!);
                }
            }

            if (orExpression != null)
            {
                whereExpression = whereExpression == null
                    ? orExpression
                    : Expression.Or(whereExpression, orExpression);
            }

            if (whereExpression != null)
            {
                query = query.Where(Expression.Lambda<Func<T, bool>>(whereExpression, expressionParameter));
            }

            return query;
        }

        private static bool TryProcessWhereOperand(ParameterExpression expressionParameter, QueryCommand cmd, out BinaryExpression? expression, out bool orExpression)
        {
            BinaryExpression? outputExpression = null;
            orExpression = false;
            var typeProperties = typeof(T).GetProperties();
            var orOperandShift = 0;
            string? propertyName = string.Empty;

            var fProp = cmd.Props.ElementAtOrDefault(0);

            if (fProp == null || string.IsNullOrWhiteSpace(fProp))
            {
                expression = null;
                return false; // Broken command
            }

            if (fProp == "or")
            {
                orOperandShift = 1;
                orExpression = true;
                propertyName = cmd.Props.ElementAtOrDefault(1);
            }
            else
            {
                propertyName = fProp;
            }

            if (propertyName == null || string.IsNullOrWhiteSpace(propertyName))
            {
                expression = null;
                return false; // Broken command
            }

            var propertyType = typeProperties.FirstOrDefault(p => p.Name.ToLowerInvariant() == propertyName.ToLowerInvariant());

            // Property check
            if (propertyType == null)
            {
                expression = null;
                return false; // Broken command
            }

            dynamic parsedValue;
            // Value cast
            if (DateTime.TryParseExact(cmd.Value, "yyyy-MM-dd'T'HH:mm:ss.fffK", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) && propertyType.PropertyType == typeof(DateTime))
            {
                parsedValue = date;
            }
            else if (decimal.TryParse(cmd.Value, out var decimalValue) && propertyType.PropertyType == typeof(decimal))
            {
                parsedValue = decimalValue;
            }
            else if (double.TryParse(cmd.Value, out var doubleValue) && propertyType.PropertyType == typeof(double))
            {
                parsedValue = doubleValue;
            }
            else if (int.TryParse(cmd.Value, out int intValue) && propertyType.PropertyType == typeof(int))
            {
                parsedValue = intValue;
            }
            else
            {
                parsedValue = cmd.Value;
            }

            var valueParameterExpression = Expression.Constant(parsedValue);
            var parameterPropertyExpression = Expression.Property(expressionParameter, propertyName);

            var operand = QueryCommand.OperandMappings.FirstOrDefault(m => m.Key == cmd.Props.ElementAtOrDefault(1 + orOperandShift)).Value;
            switch (operand)
            {
                case WOperand.Equal:
                    outputExpression = Expression.Equal(parameterPropertyExpression, valueParameterExpression);
                    break;
                case WOperand.GreaterThan:
                    outputExpression = Expression.GreaterThan(parameterPropertyExpression, valueParameterExpression);
                    break;
                case WOperand.GreaterThanOrEquals:
                    outputExpression = Expression.GreaterThanOrEqual(parameterPropertyExpression, valueParameterExpression);
                    break;
                case WOperand.LessThan:
                    outputExpression = Expression.LessThan(parameterPropertyExpression, valueParameterExpression);
                    break;
                case WOperand.LessThanOrEquals:
                    outputExpression = Expression.LessThanOrEqual(parameterPropertyExpression, valueParameterExpression);
                    break;
                case WOperand.NotEqual:
                    outputExpression = Expression.NotEqual(parameterPropertyExpression, valueParameterExpression);
                    break;
                default:
                    expression = null;
                    return false; // Broken command
            }

            if (outputExpression == null)
            {
                expression = null;
                return false; // Broken command
            }

            expression = outputExpression;
            return true;
        }

        private static IOrderedQueryable<T> AppendOrderExpression(IQueryable<T> query, QueryCommand[] commands)
        {
            var orderCommand = commands.FirstOrDefault(c => c.Type == FilterType.Order);
            if (orderCommand == null)
            {
                return query.OrderBy(t => t.Id);
            }

            var typeProperties = typeof(T).GetProperties();
            var expressionParameter = Expression.Parameter(typeof(T));

            var valueProps = orderCommand.Value.Split(' ');
            var propertyName = string.Empty;
            var methodName = "OrderBy";

            switch (valueProps.Length)
            {
                case 0:
                    return query.OrderBy(t => t.Id);
                case 1:
                    propertyName = valueProps.First().ToLowerInvariant();
                    break;
                case 2:
                    propertyName = valueProps.First().ToLowerInvariant();
                    methodName = valueProps.ElementAt(1).ToLowerInvariant() switch
                    {
                        "asc" => "OrderBy",
                        "desc" => "OrderByDescending",
                        _ => "OrderBy",
                    };
                    break;
                default:
                    return query.OrderBy(t => t.Id);
            }

            if (typeProperties.Any(p => p.Name.ToLowerInvariant() == propertyName))
            {
                var orderPropertyType = typeProperties.First(p => p.Name.ToLowerInvariant() == propertyName.ToLowerInvariant()).PropertyType;
                var orderPropertyExpression = Expression.Property(expressionParameter, propertyName);
                var orderDelegateType = typeof(Func<,>).MakeGenericType(typeof(T), orderPropertyType);
                dynamic orderLambda = Expression.Lambda(orderDelegateType, orderPropertyExpression, expressionParameter);
                var orderMethod = typeof(Queryable).GetMethods().First(
                                                                    m => m.Name == methodName &&
                                                                    m.GetGenericArguments().Length == 2 &&
                                                                    m.GetParameters().Length == 2).MakeGenericMethod(typeof(T), orderPropertyType);
                query = (IOrderedQueryable<T>)orderMethod.Invoke(query, new object?[] { query, orderLambda }) !;
                return (IOrderedQueryable<T>)query;
            }

            return query.OrderBy(t => t.Id);
        }

        private static IQueryable<T> AppendSkipExpression(IQueryable<T> query, QueryCommand[] commands)
        {
            var skipCommand = commands.FirstOrDefault(c => c.Type == FilterType.Skip);
            if (skipCommand != null && int.TryParse(skipCommand.Value, out var skipCount))
            {
                query = query.Skip(skipCount);
            }

            return query;
        }

        private static IQueryable<T> AppendLimitExpression(IQueryable<T> query, QueryCommand[] commands)
        {
            var limitCommand = commands.FirstOrDefault(c => c.Type == FilterType.Limit);
            if (limitCommand != null && int.TryParse(limitCommand.Value, out var limitCount))
            {
                query = query.Take(limitCount);
            }

            return query;
        }
    }
}
