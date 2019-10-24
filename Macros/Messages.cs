using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Penguin.Cms.Abstractions;
using Penguin.Cms.Abstractions.Interfaces;
using Penguin.Cms.Entities;
using Penguin.Cms.InternalMessaging;
using Penguin.Cms.Modules.InternalMessaging.Repositories;
using Penguin.Web.Mvc.Abstractions;
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
            this.HttpContext = httpContext;
            this.MessageRepository = messageRepository;
            this.ViewRenderService = viewRenderService;
        }

        [SuppressMessage("Design", "CA1054:Uri parameters should not be strings")]
        public HtmlString this[Guid Recipient]
        {
            get
            {
                List<InternalMessage> messages = this.MessageRepository.GetByRecipient(Recipient);

                string toReturn = string.Empty;

                foreach (InternalMessage thisMessage in messages.OrderByDescending(m => m.DateCreated))
                {
                    Task<string> task = this.ViewRenderService.RenderToStringAsync("/Views/Shared/Components/ViewMessage/Default.cshtml", "", thisMessage, true);

                    task.Wait();

                    toReturn += task.Result;
                }

                return new HtmlString(toReturn);
            }
        }

        public List<Macro> GetMacros(object requester)
        {
            if (requester is Entity e)
            {
                return new List<Macro>()
                {
                        new Macro
                        ( this.GetType().Name,
                             $"@Messages[\"{e.Guid}\"]"

                        )
                };
            }
            else
            {
                return new List<Macro>();
            }
        }
    }
}