// <copyright file="ESQueryProvider.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Linq;
using DnsClient;
using Microsoft.AspNetCore.OData.Formatter.Wrapper;
using Nest;
using Newtonsoft.Json.Linq;
using OnlineSales.DataAnnotations;
using OnlineSales.Entities;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

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
        private readonly PropertyInfo[] searchableTextProperties;
        private readonly PropertyInfo[] searchableNonTextProperties;

        public ESQueryProvider(ElasticClient elasticClient, QueryParseData<T> parseData, string indexPrefix)
        {
            indexName = indexPrefix + "-" + typeof(T).Name.ToLower();
            this.elasticClient = elasticClient;
            this.parseData = parseData;
            searchableTextProperties = typeof(T).GetProperties().Where(p => p.IsDefined(typeof(SearchableAttribute), false) && p.PropertyType == typeof(string)).ToArray();
            searchableNonTextProperties = typeof(T).GetProperties().Where(p => p.IsDefined(typeof(SearchableAttribute), false) && p.PropertyType != typeof(string)).ToArray();
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

            var sr = new SearchRequest<T>(indexName);

            sr.Query = (orQueries.Count > 0) ? new BoolQuery { Should = orQueries.ToArray(), } : new MatchAllQuery();
                        
            if (parseData.OrderData.Count > 0)
            {
                var sortedConditions = new List<ISort>();

                foreach (var orderCmd in parseData.OrderData)
                {
                    var sortOrder = orderCmd.Ascending ? Nest.SortOrder.Ascending : Nest.SortOrder.Descending;

                    var field = orderCmd.Property.PropertyType == typeof(string) ? new Field(GetElasticKeywordName(orderCmd.Property)) : new Field(orderCmd.Property);
                    sortedConditions.Add(new FieldSort { Field = field, Order = sortOrder, UnmappedType = FieldType.Long });
                }

                sr.Sort = sortedConditions;
            }

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

            var res = await elasticClient.SearchAsync<T>(sr);
            if (res.IsValid)
            {
                return new QueryResult<T>(res.Documents.ToList(), res.Total);
            }
            else
            {
                throw res.OriginalException;
            }
        }

        private static string GetElasticKeywordName(PropertyInfo pi)
        {
            return char.ToLower(pi.Name[0]) + pi.Name.Substring(1) + ".keyword";
        }

        private void AddWhereCommands()
        {
            QueryContainer CreateQueryComparison(QueryParseData<T>.WhereUnitData cmd)
            {
                object value = cmd.ParseValue();

                TermRangeQuery CreateTermRangeQuery(QueryParseData<T>.WhereUnitData cmd)
                {
                    TermRangeQuery res;
                    if (cmd.Property.PropertyType == typeof(string))
                    {
                        res = new TermRangeQuery { Field = new Field(GetElasticKeywordName(cmd.Property)), };
                    }
                    else
                    {
                        res = new TermRangeQuery { Field = new Field(cmd.Property), };
                    }

                    res.GetType().GetProperty(cmd.Operation.ToString()) !.SetValue(res, value.ToString());
                    return res;
                }

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
                    return CreateTermRangeQuery(cmd);
                }
            }

            QueryContainer CreateQuery(QueryParseData<T>.WhereUnitData cmd)
            {
                TermQuery CreateTermQuery(QueryParseData<T>.WhereUnitData cmd)
                {
                    if (cmd.Property.PropertyType == typeof(string))
                    {
                        return new TermQuery { Field = new Field(GetElasticKeywordName(cmd.Property)), Value = cmd.StringValue };
                    }
                    else
                    {
                        return new TermQuery { Field = new Field(cmd.Property), Value = cmd.StringValue };
                    }
                }

                RegexpQuery CreateRegExpQuery(QueryParseData<T>.WhereUnitData cmd)
                {
                    return new RegexpQuery { Field = new Field(cmd.Property), Value = cmd.StringValue };
                }

                try
                {
                    switch (cmd.Operation)
                    {
                        case WOperand.Equal:
                            return CreateTermQuery(cmd);
                        case WOperand.NotEqual:
                            var tq = CreateTermQuery(cmd);
                            return new BoolQuery { MustNot = new QueryContainer[] { tq } };
                        case WOperand.GreaterThan:
                        case WOperand.GreaterThanOrEqualTo:
                        case WOperand.LessThan:
                        case WOperand.LessThanOrEqualTo:
                            return CreateQueryComparison(cmd);
                        case WOperand.Like:
                            return CreateRegExpQuery(cmd);
                        case WOperand.NLike:
                            var req = CreateRegExpQuery(cmd);
                            return new BoolQuery { MustNot = new QueryContainer[] { req } };
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

        private void AddSearchCommands()
        {
            List<QueryContainer> sq = new List<QueryContainer>();            

            if (parseData.SearchData.Count > 0)
            {
                var tQ = new MultiMatchQuery
                {
                    Query = string.Join(" ", parseData.SearchData),
                    Fields = searchableTextProperties,
                    Lenient = true,
                    Fuzziness = Fuzziness.Auto,
                    Operator = Operator.Or,
                };

                sq.Add(tQ);

                var ntQ = new MultiMatchQuery
                {
                    Query = string.Join(" ", parseData.SearchData),
                    Fields = searchableNonTextProperties,
                    Lenient = true,
                    Operator = Operator.Or,
                };

                sq.Add(ntQ);
            }

            andQueries.Add(new BoolQuery { Should = sq.ToArray() });
        }
    }
}
