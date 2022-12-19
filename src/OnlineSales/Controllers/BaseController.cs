// <copyright file="BaseController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using AutoMapper;
using AutoMapper.Internal;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Microsoft.OData.ModelBuilder;
using Newtonsoft.Json;
using OnlineSales.Data;
using OnlineSales.Entities;
using OnlineSales.Infrastructure;
using Quartz;

namespace OnlineSales.Controllers
{
    public class BaseController<T, TC, TU> : ControllerBaseEH
        where T : BaseEntity, new()
        where TC : class
        where TU : class
    {
        protected readonly DbSet<T> dbSet;
        protected readonly DbContext dbContext;
        protected readonly IMapper mapper;

        public BaseController(ApiDbContext dbContext, IMapper mapper)
        {
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<T>();
            this.mapper = mapper;
        }

        // GET api/{entity}s/5
        [HttpGet("{id}")]
        // [EnableQuery]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public virtual async Task<ActionResult<T>> GetOne(int id)
        {
            try
            {
                var result = await (from p in this.dbSet
                                    where p.Id == id
                                    select p).FirstOrDefaultAsync();

                if (result == null)
                {
                    return errorHandler.CreateNotFoundResponce(CreateNotFoundMessage<T>(id));
                }

                return Ok(result);
            }
            catch (Exception e)
            {
                return errorHandler.CreateInternalServerErrorResponce(e.Message);
            }
        }

        // POST api/{entity}s
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public virtual async Task<ActionResult<T>> Post([FromBody] TC value)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return errorHandler.CreateBadRequestResponce();
                }

                var newValue = mapper.Map<T>(value);
                var result = await dbSet.AddAsync(newValue);
                await dbContext.SaveChangesAsync();

                return CreatedAtAction(nameof(GetOne), new { id = result.Entity.Id }, value);
            }
            catch (Exception e)
            {
                return errorHandler.CreateInternalServerErrorResponce(e.Message);
            }
        }

        // PUT api/posts/5
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public virtual async Task<ActionResult<T>> Patch(int id, [FromBody] TU value)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return errorHandler.CreateBadRequestResponce();
                }

                var existingEntity = await (from p in this.dbSet
                                            where p.Id == id
                                            select p).FirstOrDefaultAsync();

                if (existingEntity == null)
                {
                    return errorHandler.CreateUnprocessableEntityResponce(CreateNotFoundMessage<T>(id));
                }

                mapper.Map(value, existingEntity);
                await dbContext.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {
                return errorHandler.CreateInternalServerErrorResponce(e.Message);
            }
        }

        // DELETE api/posts/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public virtual async Task<ActionResult> Delete(int id)
        {
            try
            {
                var existingEntity = await (from p in this.dbSet
                                            where p.Id == id
                                            select p).FirstOrDefaultAsync();

                if (existingEntity == null)
                {
                    return errorHandler.CreateUnprocessableEntityResponce(CreateNotFoundMessage<T>(id));
                }

                dbContext.Remove(existingEntity);

                await dbContext.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception e)
            {
                return errorHandler.CreateInternalServerErrorResponce(e.Message);
            }
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public virtual async Task<ActionResult> Get([FromQuery] IDictionary<string, string>? parameters)
        {
            var queryCommands = this.Request.QueryString.ToString().Substring(1).Split('&').Select(s => HttpUtility.UrlDecode(s)).ToArray(); // Removing '?' character, split by '&'
            var query = this.dbSet!.AsQueryable<T>();
            var processedCommands = new List<QueryCommand>();
            var typeProperties = typeof(T).GetProperties();
            // Processing received commands
            foreach (var cmd in queryCommands)
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

            BinaryExpression? whereExpression = null;
            var expressionParameter = Expression.Parameter(typeof(T));

            // Executing received commands
            foreach (var cmd in processedCommands.Where(c => c.Type == FilterType.Where).ToArray())
            {
                var propertyName = cmd.Props.ElementAtOrDefault(0);
                var operand = QueryCommand.OperandMappings.FirstOrDefault(m => m.Key == cmd.Props.ElementAtOrDefault(1)).Value;

                var propertyType = typeProperties.FirstOrDefault(p => p.Name.ToLowerInvariant() == propertyName);

                // Property check
                if (propertyName == null || string.IsNullOrWhiteSpace(propertyName) || propertyType == null)
                {
                    continue; // Broken command
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

                // Construct expression
                BinaryExpression? outputExpression = null;
                var valueParameterExpression = Expression.Constant(parsedValue);
                var parameterPropertyExpression = Expression.Property(expressionParameter, propertyName);
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
                        continue; // Broken command
                }

                if (outputExpression == null)
                {
                    continue;
                }

                if (whereExpression == null)
                {
                    whereExpression = outputExpression;
                    continue;
                }

                // Concatenate
                whereExpression = Expression.And(whereExpression, outputExpression);
            }

            if (whereExpression != null)
            {
                query = query.Where(Expression.Lambda<Func<T, bool>>(whereExpression, expressionParameter));
            }

            var skipCommand = processedCommands.FirstOrDefault(c => c.Type == FilterType.Skip);
            if (skipCommand != null && int.TryParse(skipCommand.Value, out var skipCount))
            {
                query = query.Skip(skipCount);
            }

            var limitCommand = processedCommands.FirstOrDefault(c => c.Type == FilterType.Limit);
            if (limitCommand != null && int.TryParse(limitCommand.Value, out var limitCount))
            {
                query = query.Take(limitCount);
            }

            var orderCommand = processedCommands.FirstOrDefault(c => c.Type == FilterType.Order);
            if (orderCommand != null && typeProperties.Any(p => p.Name.ToLowerInvariant() == orderCommand.Value))
            {
                var orderPropertyType = typeProperties.First(p => p.Name.ToLowerInvariant() == orderCommand.Value).PropertyType;
                var orderPropertyExpression = Expression.Property(expressionParameter, orderCommand.Value);
                var orderDelegateType = typeof(Func<,>).MakeGenericType(typeof(T), orderPropertyType);
                dynamic orderLambda = Expression.Lambda(orderDelegateType, orderPropertyExpression, expressionParameter);
                var orderMethod = query.GetType().GetMethods().First(
                                                                    m => m.Name == "OrderBy" &&
                                                                    m.GetGenericArguments().Length == 2 &&
                                                                    m.GetParameters().Length == 2).MakeGenericMethod(typeof(T), orderPropertyType);
                query = (IOrderedQueryable<T>)orderMethod.Invoke(query, orderLambda);
            }

            var validFieldCommands = processedCommands.Where(c => c.Type == FilterType.Fields && bool.TryParse(c.Value, out var _)).ToList();
            if (validFieldCommands.Any())
            {
                var atLeastOneTrue = validFieldCommands.Select(c => c.Value).Any(v => bool.Parse(v));
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
                        default:
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
                return Ok(taskResult);
            }

            var result = await query!.ToArrayAsync();
            return Ok(result);
        }

        protected string CreateNotFoundMessage<TEntity>(int id)
        {
            // return JsonConvert.SerializeObject(new { Id = new string[] { string.Format("The Id field with value = {0} is not found", id) } }, Formatting.Indented);
            return string.Format("The {0} with Id = {1} is not found", typeof(TEntity).FullName, id);
        }
    }
#pragma warning restore
}