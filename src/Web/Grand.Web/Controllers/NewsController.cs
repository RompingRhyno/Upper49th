﻿using Grand.Business.Core.Interfaces.Cms;
using Grand.Business.Core.Interfaces.Common.Directory;
using Grand.Business.Core.Interfaces.Common.Localization;
using Grand.Business.Core.Interfaces.Common.Security;
using Grand.Domain.Permissions;
using Grand.Domain.Customers;
using Grand.Domain.News;
using Grand.Infrastructure;
using Grand.Web.Commands.Models.News;
using Grand.Web.Common.Controllers;
using Grand.Web.Common.Filters;
using Grand.Web.Events;
using Grand.Web.Features.Models.News;
using Grand.Web.Models.News;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Grand.SharedKernel.Attributes;

namespace Grand.Web.Controllers;

[ApiGroup(SharedKernel.Extensions.ApiConstants.ApiGroupNameV2)]
public class NewsController : BasePublicController
{
    #region Constructors

    public NewsController(INewsService newsService,
        IContextAccessor contextAccessor,
        ITranslationService translationService,
        IAclService aclService,
        IPermissionService permissionService,
        IMediator mediator,
        NewsSettings newsSettings)
    {
        _newsService = newsService;
        _contextAccessor = contextAccessor;
        _translationService = translationService;
        _aclService = aclService;
        _permissionService = permissionService;
        _mediator = mediator;
        _newsSettings = newsSettings;
    }

    #endregion

    #region Fields

    private readonly INewsService _newsService;
    private readonly IContextAccessor _contextAccessor;
    private readonly ITranslationService _translationService;
    private readonly IAclService _aclService;
    private readonly IPermissionService _permissionService;
    private readonly IMediator _mediator;
    private readonly NewsSettings _newsSettings;

    #endregion

    #region Methods

    [HttpGet]
    public virtual async Task<ActionResult<NewsItemListModel>> List(NewsPagingFilteringModel command)
    {
        if (!_newsSettings.Enabled)
            return RedirectToRoute("HomePage");

        var model = await _mediator.Send(new GetNewsItemList { Command = command });
        return View(model);
    }

    [HttpGet]
    public virtual async Task<ActionResult<NewsItemModel>> NewsItem(string newsItemId)
    {
        if (!_newsSettings.Enabled)
            return RedirectToRoute("HomePage");

        var newsItem = await _newsService.GetNewsById(newsItemId);
        if (newsItem is not { Published: true } ||
            (newsItem.StartDateUtc.HasValue && newsItem.StartDateUtc.Value >= DateTime.UtcNow) ||
            (newsItem.EndDateUtc.HasValue && newsItem.EndDateUtc.Value <= DateTime.UtcNow) ||
            //Store acl
            !_aclService.Authorize(newsItem, _contextAccessor.StoreContext.CurrentStore.Id))
            return RedirectToRoute("HomePage");

        var model = await _mediator.Send(new GetNewsItem { NewsItem = newsItem });

        //display "edit" (manage) link
        if (await _permissionService.Authorize(StandardPermission.ManageAccessAdminPanel) &&
            await _permissionService.Authorize(StandardPermission.ManageNews))
            DisplayEditLink(Url.Action("Edit", "News", new { id = newsItem.Id, area = "Admin" }));

        return View(model);
    }

    [HttpPost]
    [AutoValidateAntiforgeryToken]
    [DenySystemAccount]
    public virtual async Task<IActionResult> NewsCommentAdd(AddNewsCommentModel model)
    {
        if (!_newsSettings.Enabled)
            return Json(new {
                success = false
            });

        var newsItem = await _newsService.GetNewsById(model.Id);
        if (newsItem is not { Published: true } || !newsItem.AllowComments)
            return Json(new {
                success = false
            });

        if (ModelState.IsValid)
        {
            var newsComment = await _mediator.Send(new InsertNewsCommentCommand { NewsItem = newsItem, Model = model });

            //notification
            await _mediator.Publish(new NewsCommentEvent(newsItem, model));

            return Json(new {
                success = true,
                message = _translationService.GetResource("News.Comments.SuccessfullyAdded"),
                model = new {
                    newsComment.CommentText,
                    newsComment.CommentTitle,
                    CreatedOn = HttpContext.RequestServices.GetService<IDateTimeService>()
                        .ConvertToUserTime(newsComment.CreatedOnUtc, DateTimeKind.Utc),
                    CustomerName = _contextAccessor.WorkContext.CurrentCustomer.FormatUserName(HttpContext.RequestServices
                        .GetService<CustomerSettings>().CustomerNameFormat)
                }
            });
        }

        return Json(new {
            success = false,
            message = string.Join(',', ModelState.Values.SelectMany(x => x.Errors.Select(x => x.ErrorMessage)))
        });
    }

    #endregion
}