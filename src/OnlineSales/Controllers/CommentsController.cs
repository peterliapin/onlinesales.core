// <copyright file="CommentsController.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.Reflection;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineSales.Data;
using OnlineSales.DTOs;
using OnlineSales.Entities;
using OnlineSales.Helpers;
using OnlineSales.Infrastructure;
using OnlineSales.Interfaces;

namespace OnlineSales.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
public class CommentsController : BaseControllerWithImport<Comment, CommentCreateDto, CommentUpdateDto, CommentDetailsDto, CommentImportDto>
{
    private static readonly Dictionary<string, Type> CommentableTypes = FindCommentableTypes();

    private readonly ICommentService commentService;
    private readonly CommentableControllerExtension commentableControllerExtension;

    public CommentsController(PgDbContext dbContext, IMapper mapper, ICommentService commentService, EsDbContext esDbContext, QueryProviderFactory<Comment> queryProviderFactory, CommentableControllerExtension commentableControllerExtension)
        : base(dbContext, mapper, esDbContext, queryProviderFactory)
    {
        this.commentService = commentService;
        this.commentableControllerExtension = commentableControllerExtension;
        additionalImportChecker = new CommentsImportChecker(dbContext);
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public override async Task<ActionResult<List<CommentDetailsDto>>> Get([FromQuery] string? query)
    {
        var returnedItems = (await base.Get(query)).Result;

        var items = (List<CommentDetailsDto>)((ObjectResult)returnedItems!).Value!;

        return commentableControllerExtension.ReturnComments(items, this);
    }

    // GET api/{entity}s/5
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public override async Task<ActionResult<CommentDetailsDto>> GetOne(int id)
    {
        var result = (await base.GetOne(id)).Result;

        var commentDetails = (CommentDetailsDto)((ObjectResult)result!).Value!;

        commentDetails!.AvatarUrl = GravatarHelper.EmailToGravatarUrl(commentDetails.AuthorEmail);

        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            return Ok(commentDetails!);
        }
        else
        {
            var commentForAnonymous = mapper.Map<AnonymousCommentDetailsDto>(commentDetails);

            return Ok(commentForAnonymous!);
        }
    }

    // POST api/{entity}s
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public override async Task<ActionResult<CommentDetailsDto>> Post([FromBody] CommentCreateDto value)
    {
        var comment = mapper.Map<Comment>(value);

        if (!CheckCommentableEntity(comment.CommentableType, comment.CommentableId))
        {
            return UnprocessableEntity(value);
        }

        return await commentableControllerExtension.PostComment(comment, this);
    }

    protected override async Task SaveRangeAsync(List<Comment> comments)
    {
        await commentService.SaveRangeAsync(comments);
    }

    private static Dictionary<string, Type> FindCommentableTypes()
    {
        var assembly = Assembly.GetAssembly(typeof(ICommentable));
        return assembly!.GetTypes().Where(t => t.IsClass && typeof(ICommentable).IsAssignableFrom(t)).ToDictionary(t => ICommentable.GetCommentableType(t), t => t);
    }

    private bool CheckCommentableEntity(string ct, int id)
    {
        var contains = CommentableTypes.TryGetValue(ct, out var type);
        if (contains)
        {
            var data = dbContext.SetDbEntity(type!);
            return data.Any(d => ((BaseEntityWithId)d).Id == id);
        }

        return false;
    }

    private sealed class CommentsImportChecker : AdditionalImportChecker
    {
        private readonly PgDbContext dbContext;
        private readonly Dictionary<string, List<int>> existedCommentableIds = new Dictionary<string, List<int>>();
        private List<CommentImportDto> importRecords = new List<CommentImportDto>();

        public CommentsImportChecker(PgDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public override void SetData(List<CommentImportDto> importRecords)
        {
            this.importRecords = importRecords;
            foreach (var commentableType in CommentableTypes)
            {
                var importRecordIds = importRecords.Where(r => r.CommentableType == commentableType.Key).Select(ir => ir.CommentableId).ToHashSet();
                var existedIds = dbContext.SetDbEntity(commentableType.Value).Where(c => importRecordIds.Contains(((BaseEntityWithId)c).Id)).Select(c => ((BaseEntityWithId)c).Id).ToList();
                existedCommentableIds.Add(commentableType.Key, existedIds);
            }
        }

        public override bool Check(int index, ImportResult result)
        {
            if (index < 0 || index > importRecords.Count)
            {
                return false;
            }

            var importRecord = importRecords[index];
            if (!existedCommentableIds[importRecord.CommentableType].Contains(importRecord.CommentableId))
            {
                result.AddError(index, $"Commentable entity of type {importRecord.CommentableType} with id = {importRecord.CommentableId} cannot be found");
                return false;
            }

            return true;
        }
    }
}