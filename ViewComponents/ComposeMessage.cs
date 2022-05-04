using Microsoft.AspNetCore.Mvc;
using Penguin.Cms.InternalMessaging.Repositories;

namespace Penguin.Cms.Modules.InternalMessaging.ViewComponents
{
    public class ComposeMessage : ViewComponent
    {
        protected MessageRepository MessageRepository { get; set; }

        public ComposeMessage(MessageRepository messageRepository)
        {
            this.MessageRepository = messageRepository;
        }

        public IViewComponentResult Invoke(string recipient, string? origin = null, int parentId = 0)
        {
            return this.View(this.MessageRepository.Draft(recipient, origin, parentId));
        }
    }
}