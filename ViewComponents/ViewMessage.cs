using Microsoft.AspNetCore.Mvc;
using Penguin.Cms.InternalMessaging;
using Penguin.Cms.InternalMessaging.Repositories;
using System;

namespace Penguin.Cms.Modules.InternalMessaging.ViewComponents
{
    public class ViewMessage : ViewComponent
    {
        protected MessageRepository MessageRepository { get; set; }

        public ViewMessage(MessageRepository messageRepository)
        {
            MessageRepository = messageRepository;
        }

        public IViewComponentResult Invoke(Guid id)
        {
            InternalMessage model = MessageRepository.GetMessageChain(id);

            return model is null ? Content("") : View(model);
        }
    }
}