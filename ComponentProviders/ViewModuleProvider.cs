using Penguin.Cms.Abstractions.Interfaces;
using Penguin.Cms.Entities;
using Penguin.Cms.InternalMessaging;
using Penguin.Cms.Modules.InternalMessaging.Areas.Admin.Models;
using Penguin.Cms.Modules.InternalMessaging.Repositories;
using Penguin.Cms.Web.Modules;
using System.Collections.Generic;

namespace Penguin.Cms.Modules.InternalMessaging.ComponentProviders
{
    public class ViewModuleProvider : IProvideComponents<ViewModule, Entity>
    {
        protected MessageRepository MessageRepository { get; set; }

        public ViewModuleProvider(MessageRepository messageRepository)
        {
            this.MessageRepository = messageRepository;
        }

        public IEnumerable<ViewModule> GetComponents(Entity Id)
        {
            if (Id is null)
            {
                throw new System.ArgumentNullException(nameof(Id));
            }

            List<InternalMessage> messages = this.MessageRepository.GetByRecipient(Id.Guid);

            AdminMessageChainModel model = new AdminMessageChainModel()
            {
                Messages = messages,
                Recipient = Id.Guid
            };

            yield return new ViewModule("~/Areas/Admin/Views/Message/AdminEditor.cshtml", model, "Messages");
        }
    }
}