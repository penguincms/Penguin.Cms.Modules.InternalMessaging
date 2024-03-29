﻿using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Penguin.Cms.Abstractions;
using Penguin.Cms.Abstractions.Interfaces;
using Penguin.Cms.Entities;
using Penguin.Cms.InternalMessaging;
using Penguin.Cms.InternalMessaging.Repositories;
using Penguin.Web.Mvc.Abstractions.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Penguin.Cms.Modules.InternalMessaging.Macros
{
    [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
    public class Messages : IMacroProvider
    {
        protected HttpContext HttpContext { get; set; }

        protected MessageRepository MessageRepository { get; set; }

        protected IViewRenderService ViewRenderService { get; set; }

        public Messages(HttpContext httpContext, MessageRepository messageRepository, IViewRenderService viewRenderService)
        {
            HttpContext = httpContext;
            MessageRepository = messageRepository;
            ViewRenderService = viewRenderService;
        }

        public HtmlString this[Guid Recipient]
        {
            get
            {
                List<InternalMessage> messages = MessageRepository.GetByRecipient(Recipient);

                string toReturn = string.Empty;

                foreach (InternalMessage thisMessage in messages.OrderByDescending(m => m.DateCreated))
                {
                    Task<string> task = ViewRenderService.RenderToStringAsync("/Views/Shared/Components/ViewMessage/Default.cshtml", "", thisMessage, true);

                    task.Wait();

                    toReturn += task.Result;
                }

                return new HtmlString(toReturn);
            }
        }

        public List<Macro> GetMacros(object requester)
        {
            return requester is Entity e
                ? new List<Macro>()
                {
                        new Macro
                        ( GetType().Name,
                             $"@Messages[\"{e.Guid}\"]"

                        )
                }
                : new List<Macro>();
        }
    }
}