// <copyright file="DBQueryProvider.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using AutoMapper;
using AutoMapper.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using OnlineSales.DataAnnotations;
using OnlineSales.Entities;

namespace OnlineSales.Infrastructure
{
    public class DBQueryProvider<T> : IQueryProvider<T>
        where T : BaseEntityWithId
    {
        private readonly QueryParseData<T> parseData;
        private IQueryable<T> query;

        public DBQueryProvider(IQueryable<T> query, QueryParseData<T> parseData)
        {
            this.query = query;
            this.parseData = parseData;
        }

        public async Task<QueryResult<T>> GetResult()
        {
            AddWhereCommands();
            AddSearchCommands();

            var totalCount = query.Count();
            IList<T>? records;

            AddOrderCommands();
            AddSkipCommand();
            AddLimitCommand();
            if (parseData.SelectData.IsSelect)
            {
                records = await GetSelectResult();
            }
            else
            {
                records = await query.ToListAsync();
            }

            return new QueryResult<T>(records, totalCount);
        }

        private void AddOrderCommands()
        {
            if (parseData.OrderData.Count == 0)
            {
                query = query.OrderBy(t => t.Id);
            }
            else
            {
                bool moreThanOne = false;
                foreach (var orderCmd in parseData.OrderData)
                {
                    var expressionParameter = Expression.Parameter(typeof(T));
                    var orderPropertyType = orderCmd.Property.PropertyType;
                    var orderPropertyExpression = Expression.Property(expressionParameter, orderCmd.Property.Name);
                    var orderDelegateType = typeof(Func<,>).MakeGenericType(typeof(T), orderPropertyType);
                    var orderLambda = Expression.Lambda(orderDelegateType, orderPropertyExpression, expressionParameter);
                    var methodName = string.Empty;
                    
                    if (orderCmd.Ascending)
                    {
                        methodName = moreThanOne ? "ThenBy" : "OrderBy";
                    }
                    else
                    {
                        methodName = moreThanOne ? "ThenByDescending" : "OrderByDescending";
                    }

                    moreThanOne = true;

                    var orderMethod = typeof(Queryable).GetMethods().First(
                                                                        m => m.Name == methodName &&
                                                                        m.GetGenericArguments().Length == 2 &&
                                                                        m.GetParameters().Length == 2).MakeGenericMethod(typeof(T), orderPropertyType);
                    query = (IOrderedQueryable<T>)orderMethod.Invoke(null, new object?[] { query, orderLambda }) !;
                }
            }
        }

        private void AddSkipCommand()
        {
            if (parseData.Skip > 0)
            {
                query = query.Skip(parseData.Skip);
            }
        }

        private void AddLimitCommand()
        {
            if (parseData.Limit > 0)
            {
                query = query.Take(parseData.Limit);
            }
        }

        private void AddSearchCommands()
        {
            foreach (string cmdValue in parseData.SearchData)
            {
                var props = typeof(T).GetProperties().Where(p => p.IsDefined(typeof(SearchableAttribute), false));

                Expression orExpression = Expression.Constant(false);
                var paramExpr = Expression.Parameter(typeof(T), "entity");
                var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });

                foreach (var prop in props)
                {
                    if (prop != null)
                    {
                        var n = prop.Name;
                        var me = Expression.Property(paramExpr, n);
                        Expression containsExpression;
                        if (prop.PropertyType == typeof(string))
                        {
                            containsExpression = Expression.Call(me, containsMethod!, Expression.Constant(cmdValue));
                        }
                        else
                        {
                            var pt = prop.PropertyType;
                            Console.WriteLine(pt);
                            var toStringMethod = prop.PropertyType.GetMethod("ToString", new Type[0]);
                            var ce = Expression.Call(me, toStringMethod!);
                            containsExpression = Expression.Call(ce, containsMethod!, Expression.Constant(cmdValue));
                        }

                        orExpression = Expression.Or(orExpression, containsExpression);
                    }
                }

                if (!ExpressionEqualityComparer.Instance.Equals(orExpression, Expression.Constant(false)))
                {
                    var predicate = Expression.Lambda<Func<T, bool>>(orExpression, paramExpr);
                    query = query.Where(predicate);
                }
            }
        }

        private void AddWhereCommands()
        {
            var commands = parseData.WhereData;
            if (commands.Count > 0)
            {
                var expressionParameter = Expression.Parameter(typeof(T));
                Expression andExpression = Expression.Constant(true);
                bool andExpressionExist = false;
                Expression orExpression = Expression.Constant(false);
                var errorList = new List<QueryException>();

                foreach (var cmds in commands)
                {
                    try
                    {
                        if (cmds.OrOperation)
                        {
                            foreach (var cmd in cmds.Data)
                            {
                                var expression = ParseWhereCommand(expressionParameter, cmd);
                                orExpression = Expression.Or(orExpression, expression);
                            }
                        }
                        else
                        {
                            foreach (var cmd in cmds.Data)
                            {
                                var expression = ParseWhereCommand(expressionParameter, cmd);
                                andExpression = Expression.And(andExpression, expression);
                                andExpressionExist = true;
                            }
                        }
                    }
                    catch (QueryException e)
                    {
                        errorList.Add(e);
                    }
                }

                if (errorList.Any())
                {
                    throw new QueryException(errorList);
                }

                if (!andExpressionExist)
                {
                    andExpression = Expression.Constant(false);
                }

                var resExpression = Expression.Or(andExpression, orExpression);
                query = query.Where(Expression.Lambda<Func<T, bool>>(resExpression, expressionParameter));
            }
        }

        private Expression ParseWhereCommand(ParameterExpression expressionParameter, QueryParseData<T>.WhereUnitData cmd)
        {
            object parsedValue = cmd.ParseValue();
            var valueParameterExpression = Expression.Constant(parsedValue, cmd.Property.PropertyType);
            var parameterPropertyExpression = Expression.Property(expressionParameter, cmd.Property.Name);

            Expression outputExpression;

            Expression? CreateCompareExpression(QueryParseData<T>.WhereUnitData cmd, Expression parameter, Expression value)
            {
                Expression? res = null;

                Expression pEx = parameter;
                Expression vEx = value;

                if (cmd.Property.PropertyType == typeof(string))
                {
                    var compareMethod = cmd.Property.PropertyType.GetMethod("CompareTo", new[] { typeof(string) });
                    pEx = Expression.Call(parameter, compareMethod!, value);
                    vEx = Expression.Constant(0);
                }

                if (cmd.Operation == WOperand.GreaterThan)
                {
                    res = Expression.GreaterThan(pEx, vEx);
                }
                else if (cmd.Operation == WOperand.GreaterThanOrEqualTo)
                {
                    res = Expression.GreaterThanOrEqual(pEx, vEx);
                }
                else if (cmd.Operation == WOperand.LessThan)
                {
                    res = Expression.LessThan(pEx, vEx);
                }
                else if (cmd.Operation == WOperand.LessThanOrEqualTo)
                {
                    res = Expression.LessThanOrEqual(pEx, vEx);
                }

                return res;
            }

            Expression? CreateLikeExpression(QueryParseData<T>.WhereUnitData cmd, Expression parameter, Expression value)
            {
                Expression? res = null;
                
                var matchOperation = typeof(Regex).GetMethod("IsMatch", BindingFlags.Static | BindingFlags.Public, new[] { typeof(string), typeof(string), typeof(RegexOptions) });
                var trueConstant = Expression.Constant(true);
                var falseConstant = Expression.Constant(false);
                var regexOptionExpression = Expression.Constant(RegexOptions.Compiled);

                if (cmd.Operation == WOperand.Like)
                {
                    res = Expression.Equal(Expression.Call(matchOperation!, parameter, value, regexOptionExpression), trueConstant);
                }
                else if (cmd.Operation == WOperand.NLike)
                {
                    res = Expression.Equal(Expression.Call(matchOperation!, parameter, value, regexOptionExpression), falseConstant);
                }

                return res;
            }

            try
            {
                switch (cmd.Operation)
                {
                    case WOperand.Equal:
                        outputExpression = Expression.Equal(parameterPropertyExpression, valueParameterExpression);
                        break;
                    case WOperand.NotEqual:
                        outputExpression = Expression.NotEqual(parameterPropertyExpression, valueParameterExpression);
                        break;
                    case WOperand.GreaterThan:
                    case WOperand.GreaterThanOrEqualTo:
                    case WOperand.LessThan:
                    case WOperand.LessThanOrEqualTo:
                        outputExpression = CreateCompareExpression(cmd, parameterPropertyExpression, valueParameterExpression) !;
                        break;
                    case WOperand.Like:
                    case WOperand.NLike:
                        outputExpression = CreateLikeExpression(cmd, parameterPropertyExpression, valueParameterExpression) !;
                        break;
                    default:
                        throw new QueryException(cmd.Cmd.Source, $"No such operand '{cmd.Operation}'");
                }                
            }
            catch (Exception ex)
            {
                throw new QueryException(cmd.Cmd.Source, ex.Message);
            }

            return outputExpression;
        }
        
        private async Task<IList<T>?> GetSelectResult()
        {
            if (parseData.SelectData.SelectedProperties.Any())
            {
                var expressionParameter = Expression.Parameter(typeof(T));
                var outputType = TypeHelper.CompileTypeForSelectStatement(parseData.SelectData.SelectedProperties.ToArray());
                var delegateType = typeof(Func<,>).MakeGenericType(typeof(T), outputType);
                var createOutputTypeExpression = Expression.New(outputType);

                var expressionSelectedProperties = parseData.SelectData.SelectedProperties.Select(p =>
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
                return taskResult as IList<T>;
            }
            else
            {
                return null;
            }
        }
    }
}
