// <copyright file="ESQueryProvider.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;
using Nest;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using OnlineSales.DataAnnotations;
using OnlineSales.Entities;

namespace OnlineSales.Infrastructure
{    
    public class ESQueryProvider<T> : IQueryProvider<T>
        where T : BaseEntityWithId
    {
        private readonly ElasticClient elasticClient;
        private readonly List<QueryContainer> andQueries = new List<QueryContainer>();
        private readonly List<QueryContainer> orQueries = new List<QueryContainer>();
        private readonly QueryParseData<T> parseData;
        private readonly string indexName;
        private readonly PropertyInfo[] searchableProperties;
        private readonly int maxResultWindow = 10000;

        public ESQueryProvider(ElasticClient elasticClient, QueryParseData<T> parseData, string indexPrefix)
        {
            indexName = indexPrefix + "-" + typeof(T).Name.ToLower();
            this.elasticClient = elasticClient;
            this.parseData = parseData;
            searchableProperties = CreateSearchableFields();
            elasticClient.Indices.UpdateSettings(indexName, s => s.IndexSettings(i => i.Setting(UpdatableIndexSettings.MaxResultWindow, maxResultWindow)));
        }

        public async Task<QueryResult<T>> GetResult()
        {
            AddWhereCommands();
            AddSearchCommands();

            if (andQueries.Count > 0)
            {
                orQueries.Add(new BoolQuery
                {
                    Must = andQueries.ToArray(),
                });
            }

            var count = Count();

            var sr = new SearchRequest<T>(indexName);

            sr.Query = (orQueries.Count > 0) ? new BoolQuery { Should = orQueries.ToArray(), } : new MatchAllQuery();

            AddSortConditions(sr);

            if (parseData.Skip >= 0)
            {
                sr.From = parseData.Skip;
            }

            if (parseData.Limit >= 0)
            {
                sr.Size = parseData.Limit;
            }

            if (parseData.SelectData.IsSelect)
            {
                var fields = new List<Field>();
                foreach (var sp in parseData.SelectData.SelectedProperties)
                {
                    fields.Add(sp);
                }

                sr.Source = new SourceFilter { Includes = fields.ToArray(), };
            }

            return await Query(sr, count);      
        }

        private void AddSortConditions(SearchRequest<T> sr)
        {
            var sortedConditions = new List<ISort>();

            if (parseData.OrderData.Count > 0)
            {
                foreach (var orderCmd in parseData.OrderData)
                {
                    var sortOrder = orderCmd.Ascending ? Nest.SortOrder.Ascending : Nest.SortOrder.Descending;

                    if (orderCmd.Property.PropertyType == typeof(string))
                    {
                        var paramExpr = Expression.Parameter(typeof(T), "t");
                        var propExpr = Expression.Property(paramExpr, orderCmd.Property.Name);
                        var f = Expression.Lambda<Func<T, object>>(propExpr, paramExpr);

                        if (orderCmd.Property.PropertyType == typeof(string))
                        {
                            f = f.AppendSuffix("keyword");
                        }

                        sortedConditions.Add(new FieldSort { Field = Infer.Field<T>(f), Order = sortOrder, });
                    }
                    else
                    {
                        sortedConditions.Add(new FieldSort { Field = new Field(orderCmd.Property), Order = sortOrder, });
                    }
                }
            }
            else
            {
                throw new QueryException(string.Empty, "Sorted properties list must be notempty");
            }

            sr.Sort = sortedConditions;
        }

        private async Task<QueryResult<T>> Query(SearchRequest<T> sr, long count)
        {            
            List<object> CreateSearchAfterObjects(T lastObject)
            {
                var res = new List<object>();

                foreach (var p in parseData.OrderData)
                {
                    res.Add(p.Property.GetValue(lastObject) !);
                }

                return res;
            }

            if ((parseData.Skip >= 0 || parseData.Limit >= 0) && (parseData.Skip + parseData.Limit <= maxResultWindow))
            {
                var res = await elasticClient.SearchAsync<T>(sr);
                return new QueryResult<T>(res.Documents.ToList(), count);
            }
            else
            {
                var result = new List<T>();

                sr.From = null;
                sr.Size = maxResultWindow;
                var pit = elasticClient.OpenPointInTime(new OpenPointInTimeRequest(indexName) { KeepAlive = "1m" });
                sr.PointInTime = new PointInTime(pit.Id, "2m");
                int total = 0;
                try
                {
                    while (total <= parseData.Skip + parseData.Limit)
                    {
                        var res = await elasticClient.SearchAsync<T>(sr);
                        var ds = res.Documents.ToList();
                        var newTotal = total + ds.Count;
                        if (ds.Count == 0)
                        {
                            break;
                        }

                        if (newTotal >= parseData.Skip)
                        {
                            if (total <= parseData.Skip)
                            {
                                if (newTotal <= parseData.Skip + parseData.Limit)
                                {
                                    result.AddRange(res.Documents.Take(new Range(parseData.Skip - total, ds.Count)));
                                }
                                else
                                {
                                    result.AddRange(res.Documents.Take(new Range(parseData.Skip - total, parseData.Skip + parseData.Limit - total)));
                                }
                            }
                            else
                            {
                                if (newTotal <= parseData.Skip + parseData.Limit)
                                {
                                    result.AddRange(res.Documents.Take(new Range(0, ds.Count)));
                                }
                                else
                                {
                                    result.AddRange(res.Documents.Take(new Range(0, parseData.Skip + parseData.Limit - total)));
                                }
                            }
                        }

                        sr.SearchAfter = CreateSearchAfterObjects(ds[ds.Count - 1]);
                        total = newTotal;
                    }
                }
                finally
                {
                    elasticClient.ClosePointInTime(new ClosePointInTimeRequest() { Id = pit.Id });
                }

                return new QueryResult<T>(result, count);
            }
        }

        private long Count()
        {
            var countDescriptor = new CountDescriptor<T>();
            countDescriptor.Index(indexName);

            if (orQueries.Count > 0)
            {
                countDescriptor = countDescriptor.Query(q => q.Bool(b => b.Should(orQueries.ToArray())));
            }
            else
            {
                countDescriptor = countDescriptor.Query(q => q.MatchAll());
            }            

            return elasticClient.Count(countDescriptor).Count;
        }

        private void AddWhereCommands()
        {
            QueryContainer CreateQueryComparison(QueryParseData<T>.WhereUnitData cmd)
            {
                object value = cmd.ParseValue();

                if (double.TryParse(cmd.StringValue, out _))
                {
                    var res = new NumericRangeQuery { Field = cmd.Property, };
                    res.GetType().GetProperty(cmd.Operation.ToString()) !.SetValue(res, Convert.ChangeType(value, typeof(double)));
                    return res;
                }
                else if (cmd.Property.PropertyType == typeof(DateTime))
                {
                    var res = new DateRangeQuery { Field = cmd.Property, };
                    res.GetType().GetProperty(cmd.Operation.ToString()) !.SetValue(res, value);
                    return res;
                }
                else
                {
                    var res = new TermRangeQuery { Field = cmd.Property, };
                    res.GetType().GetProperty(cmd.Operation.ToString()) !.SetValue(res, value.ToString());
                    return res;
                }
            }

            QueryContainer CreateQuery(QueryParseData<T>.WhereUnitData cmd)
            {
                try
                {
                    switch (cmd.Operation)
                    {
                        case WOperand.Equal:
                            return new MatchQuery() { Field = cmd.Property, Query = cmd.StringValue, };
                        case WOperand.NotEqual:
                            var mq = new MatchQuery() { Field = cmd.Property, Query = cmd.StringValue, };
                            return new BoolQuery { MustNot = new QueryContainer[] { mq } };
                        case WOperand.GreaterThan:
                        case WOperand.GreaterThanOrEqualTo:
                        case WOperand.LessThan:
                        case WOperand.LessThanOrEqualTo:
                            return CreateQueryComparison(cmd);
                        default:
                            throw new QueryException(cmd.Cmd.Source, $"No such operand '{cmd.Operation}'");
                    }
                }
                catch (Exception ex)
                {
                    throw new QueryException(cmd.Cmd.Source, ex.Message);
                }
            }

            foreach (var cmds in parseData.WhereData)
            {
                if (cmds.OrOperation)
                {
                    foreach (var cmd in cmds.Data)
                    {
                        var mq = CreateQuery(cmd);
                        orQueries.Add(mq);
                    }
                }
                else
                {
                    foreach (var cmd in cmds.Data)
                    {
                        var mq = CreateQuery(cmd);
                        andQueries.Add(mq);
                    }
                }
            }
        }

        private PropertyInfo[] CreateSearchableFields()
        {
            return typeof(T).GetProperties().Where(p => p.IsDefined(typeof(SearchableAttribute), false)).ToArray();
        }

        private void AddSearchCommands()
        {
            if (parseData.SearchData.Count > 0)
            {
                var cQ = new MultiMatchQuery
                {
                    Query = string.Join(" ", parseData.SearchData),
                    Fields = searchableProperties,
                    Lenient = true,
                    Operator = Operator.Or,
                };

                andQueries.Add(cQ);
            }
        }
    }
}
