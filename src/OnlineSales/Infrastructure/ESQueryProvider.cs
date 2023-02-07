// <copyright file="ESQueryProvider.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Linq.Expressions;
using System.Reflection;
using Nest;
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

        public ESQueryProvider(ElasticClient elasticClient, QueryParseData<T> parseData)
        {
            indexName = "onlinesales-" + typeof(T).Name.ToLower();
            this.elasticClient = elasticClient;
            this.parseData = parseData;
            searchableProperties = CreateSearchableFields();
        }

        public async Task<(IList<T>?, long)> GetResult()
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
                       
            if (parseData.OrderData.Count > 0)
            {
                var sortedConditions = new List<ISort>();

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
            return (res.Documents.ToList(), count);
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
                    Operator = Operator.Or,
                };

                andQueries.Add(cQ);
            }
        }
    }
}
