using Microsoft.AspNetCore.Mvc;
using Penguin.Cms.InternalMessaging;
using Penguin.Cms.Modules.InternalMessaging.Repositories;
using System;

namespace Penguin.Cms.Modules.InternalMessaging.ViewComponents
{
    public class ViewMessage : ViewComponent
    {
        protected MessageRepository MessageRepository { get; set; }

        public ViewMessage(MessageRepository messageRepository)
        {
            this.MessageRepository = messageRepository;
        }

        public IViewComponentResult Invoke(Guid id)
        {
            InternalMessage model = this.MessageRepository.GetMessageChain(id);

            if (model is null)
            {
                return this.Content("");
            }
            else
            {
                return this.View(model);
            }
        }
    }
}