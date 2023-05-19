// <copyright file="ESQueryProvider.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Reflection;
using System.Text;
using Microsoft.Extensions.Primitives;
using Nest;
using OnlineSales.DataAnnotations;
using OnlineSales.Entities;

namespace OnlineSales.Infrastructure
{
    public class ESQueryProvider<T> : IQueryProvider<T>
        where T : BaseEntityWithId
    {
        private readonly char[] regExSymbols = { '.', '?', '+', '*', '|', '{', '}', '[', ']', '(', ')', '"', '\\', '#', '@', '&', '<', '>', '~' };

        private readonly ElasticClient elasticClient;
        private readonly List<QueryContainer> andQueries = new List<QueryContainer>();
        private readonly List<QueryContainer> orQueries = new List<QueryContainer>();
        private readonly QueryData<T> parseData;
        private readonly string indexName;
        private readonly PropertyInfo[] searchableTextProperties;
        private readonly PropertyInfo[] searchableNonTextProperties;
        private readonly int maxResultWindow = 10000;

        public ESQueryProvider(ElasticClient elasticClient, QueryData<T> parseData, string indexPrefix)
        {
            indexName = indexPrefix + "-" + typeof(T).Name.ToLower();
            this.elasticClient = elasticClient;
            this.parseData = parseData;
            searchableTextProperties = typeof(T).GetProperties().Where(p => p.IsDefined(typeof(SearchableAttribute), false) && p.PropertyType == typeof(string)).ToArray();
            searchableNonTextProperties = typeof(T).GetProperties().Where(p => p.IsDefined(typeof(SearchableAttribute), false) && p.PropertyType != typeof(string)).ToArray();
            elasticClient.Indices.UpdateSettings(indexName, s => s.IndexSettings(i => i.Setting(UpdatableIndexSettings.MaxResultWindow, maxResultWindow)));
        }

        public async Task<QueryResult<T>> GetResult()
        {
            if (!elasticClient.Indices.Exists(indexName).Exists)
            {
                return new QueryResult<T>(new List<T>(), 0);
            }

            AddWhereCommands();
            AddSearchCommands();

            if (andQueries.Count > 0)
            {
                orQueries.Add(new BoolQuery
                {
                    Must = andQueries.ToArray(),
                });
            }

            var pit = elasticClient.OpenPointInTime(new OpenPointInTimeRequest(indexName) { KeepAlive = "2m" });
            try
            {
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

                return await Query(sr, count, pit.Id);
            }
            finally
            {
                elasticClient.ClosePointInTime(new ClosePointInTimeRequest() { Id = pit.Id });
            }
        }

        private void AddSortConditions(SearchRequest<T> sr)
        {
            var sortedConditions = new List<ISort>();

            if (parseData.OrderData.Count > 0)
            {
                foreach (var orderCmd in parseData.OrderData)
                {
                    var sortOrder = orderCmd.Ascending ? Nest.SortOrder.Ascending : Nest.SortOrder.Descending;

                    var field = orderCmd.Property.PropertyType == typeof(string) ? new Field(GetElasticKeywordName(orderCmd.Property)) : new Field(orderCmd.Property);
                    sortedConditions.Add(new FieldSort { Field = field, Order = sortOrder, UnmappedType = FieldType.Long });
                }
            }
            else
            {
                throw new QueryException(string.Empty, "Sorted properties list must be notempty");
            }

            sr.Sort = sortedConditions;
        }

        private async Task<QueryResult<T>> Query(SearchRequest<T> sr, long count, string pitId)
        {
            List<object> CreateSearchAfterObjects(T lastObject)
            {
                var res = new List<object>();

                foreach (var p in parseData.OrderData)
                {
                    res.Add(p.Property.GetValue(lastObject)!);
                }

                return res;
            }

            if ((parseData.Skip >= 0 || parseData.Limit >= 0) && (parseData.Skip + parseData.Limit <= maxResultWindow))
            {
                var res = await elasticClient.SearchAsync<T>(sr);
                CheckSearchRequestResult(res);
                return new QueryResult<T>(res.Documents.ToList(), count);
            }
            else
            {
                var result = new List<T>();

                sr.From = null;
                sr.Size = maxResultWindow;
                sr.PointInTime = new PointInTime(pitId, "2m");
                var total = 0;
                while (total <= parseData.Skip + parseData.Limit)
                {
                    var res = await elasticClient.SearchAsync<T>(sr);
                    CheckSearchRequestResult(res);
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

        private void CheckSearchRequestResult(ISearchResponse<T> sr)
        {
            if (!sr.IsValid)
            {
                if (sr.OriginalException != null)
                {
                    throw sr.OriginalException;
                }
                else
                {
                    throw new QueryException(string.Empty, "Invalid elastic search Responce. Reason: " + sr.DebugInformation);
                }
            }
        }

        private string GetElasticKeywordName(PropertyInfo pi)
        {
            return char.ToLower(pi.Name[0]) + pi.Name.Substring(1) + ".keyword";
        }

        private void AddWhereCommands()
        {
            QueryContainer CreateQueryComparison(QueryData<T>.WhereUnitData cmd)
            {
                var value = cmd.ParseValues(new string[] { cmd.StringValue }).FirstOrDefault()!;

                TermRangeQuery CreateTermRangeQuery(QueryData<T>.WhereUnitData cmd)
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

                    res.GetType().GetProperty(cmd.Operation.ToString())!.SetValue(res, value.ToString());
                    return res;
                }

                if (double.TryParse(cmd.StringValue, out _))
                {
                    var res = new NumericRangeQuery { Field = cmd.Property, };
                    res.GetType().GetProperty(cmd.Operation.ToString())!.SetValue(res, Convert.ChangeType(value, typeof(double)));
                    return res;
                }
                else if (cmd.Property.PropertyType == typeof(DateTime))
                {
                    var res = new DateRangeQuery { Field = cmd.Property, };
                    res.GetType().GetProperty(cmd.Operation.ToString())!.SetValue(res, DateMath.Anchored((DateTime)value));
                    return res;
                }
                else
                {
                    return CreateTermRangeQuery(cmd);
                }
            }

            QueryContainer CreateQuery(QueryData<T>.WhereUnitData cmd)
            {
                BoolQuery CreateTermQuery(QueryData<T>.WhereUnitData cmd)
                {
                    var parsedValues = cmd.ParseValues(cmd.ParseStringValues().ToList());

                    var resQueries = new List<QueryContainer>();

                    foreach (var parsedValue in parsedValues)
                    {
                        if (parsedValue != null)
                        {
                            if (cmd.Property.PropertyType == typeof(string))
                            {
                                resQueries.Add(new TermQuery { Field = new Field(GetElasticKeywordName(cmd.Property)), Value = parsedValue!.ToString() });
                            }
                            else
                            {
                                resQueries.Add(new TermQuery { Field = new Field(cmd.Property), Value = parsedValue!.ToString() });
                            }
                        }
                        else
                        {
                            resQueries.Add(new BoolQuery() { MustNot = new QueryContainer[] { new ExistsQuery { Field = new Field(cmd.Property) } } });
                        }
                    }

                    var res = new BoolQuery() { Should = resQueries.ToArray() };
                    return res;
                }

                RegexpQuery CreateRegExpQuery(QueryData<T>.WhereUnitData cmd)
                {
                    if (cmd.Operation == WOperand.Like)
                    {
                        return new RegexpQuery { Field = new Field(cmd.Property), Value = cmd.StringValue };
                    }
                    else if (cmd.Operation == WOperand.Contains)
                    {
                        var data = cmd.ParseContainValue(cmd.StringValue);
                        var sb = new StringBuilder();

                        foreach (var d in data)
                        {
                            if (d.Item1 == QueryData<T>.WhereUnitData.ContainsType.MatchAll)
                            {
                                sb.Append(".*");
                            }
                            else if (d.Item1 == QueryData<T>.WhereUnitData.ContainsType.Substring)
                            {
                                sb.Append(Escape(d.Item2));
                            }
                        }

                        return new RegexpQuery { Field = new Field(GetElasticKeywordName(cmd.Property)), Value = sb.ToString() };
                    }
                    else
                    {
                        throw new QueryException(cmd.StringValue, "Unexpected operand type");
                    }
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
                        case WOperand.Contains:
                            return CreateRegExpQuery(cmd);
                        case WOperand.NLike:
                        case WOperand.NContains:
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
            var sq = new List<QueryContainer>();

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

        private string Escape(string value)
        {
            var sb = new StringBuilder();

            foreach (var c in value)
            {
                if (regExSymbols.Contains(c))
                {
                    sb.Append('\\');
                }

                sb.Append(c);
            }

            return sb.ToString();
        }
    }
}