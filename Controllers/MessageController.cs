using Microsoft.AspNetCore.Mvc;
using Penguin.Cms.InternalMessaging;
using Penguin.Cms.InternalMessaging.Repositories;
using Penguin.Cms.Security;
using Penguin.Cms.Security.Repositories;
using Penguin.Cms.Web.Extensions;
using Penguin.Persistence.Abstractions.Interfaces;
using Penguin.Security.Abstractions;
using Penguin.Security.Abstractions.Constants;
using Penguin.Security.Abstractions.Extensions;
using Penguin.Security.Abstractions.Interfaces;
using Penguin.Shared.Objects.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Penguin.Cms.Modules.InternalMessaging.Controllers
{
    public class MessageController : Controller
    {
        protected EntityPermissionsRepository EntityPermissionsRepository { get; set; }
        protected MessageRepository MessageRepository { get; set; }

        protected IRepository<SecurityGroup> SecurityGroupRepository { get; set; }

        protected IServiceProvider ServiceProvider { get; set; }

        protected IUserSession UserSession { get; set; }

        public MessageController(EntityPermissionsRepository entityPermissionsRepository, IRepository<SecurityGroup> securityGroupRepository, MessageRepository messageRepository, IUserSession userSession, IServiceProvider serviceProvider)
        {
            this.EntityPermissionsRepository = entityPermissionsRepository;
            this.UserSession = userSession;
            this.MessageRepository = messageRepository;
            this.ServiceProvider = serviceProvider;
            this.SecurityGroupRepository = securityGroupRepository;
        }

        [HttpGet]
        public virtual ActionResult Compose(string Recipient, string? Origin = null, int ParentId = 0)
        {
            return this.View(this.MessageRepository.Draft(Recipient, Origin, ParentId));
        }

        [HttpPost]
        public virtual ActionResult Compose(InternalMessage model)
        {
            if (model is null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if ((model.Parent?._Id ?? 0) != 0)
            {
                if (this.MessageRepository.Find(model.Parent!._Id) is null)
                {
                    throw new UnauthorizedAccessException();
                }
            }

            bool HasPermission = true;

            //Logged in user is a member of sending security group
            HasPermission = HasPermission && this.UserSession.LoggedInUser.SecurityGroupGuids().Any(s => s == model.Origin);

            SecurityGroup target = this.SecurityGroupRepository.Find(model.Recipient);

            if (this.UserSession.LoggedInUser.HasRole(RoleNames.SYS_ADMIN) || target is IUser)
            {
                //We allow
            }
            else
            {
                //If its not a user, we need to be sure we have write access to the object since we dont know how it might be used
                HasPermission = this.EntityPermissionsRepository.AllowsAccessType(model.Recipient, this.UserSession.LoggedInUser, PermissionTypes.Write);
            }

            if (HasPermission)
            {
                using IWriteContext context = this.MessageRepository.WriteContext();
                model = this.MessageRepository.SendMessage(model.Body, model.Subject, model.Recipient, model.Parent?._Id ?? 0, model.Origin);
            }
            else
            {
                throw new UnauthorizedAccessException();
            }

            if (!string.IsNullOrWhiteSpace(this.Request?.Headers["Referer"]))
            {
                this.AddMessage("Message sent successfully");
                return this.Redirect(this.Request.Headers["Referer"]);
            }
            else
            {
                return this.View("Success");
            }
        }

        public InternalMessage GetTreeFromTop(Guid Id)
        {
            InternalMessage thisMessage = this.MessageRepository.GetMessageChain(Id);

            while (thisMessage.Parent != null)
            {
                thisMessage = thisMessage.Parent;
            }

            return thisMessage;
        }

        public ActionResult Inbox(string? SecurityGroupGuid = null)
        {
            if (!Guid.TryParse(SecurityGroupGuid, out Guid SecurityGroup))
            {
                SecurityGroup = this.UserSession.LoggedInUser.Guid;
            }

            List<InternalMessage> messages = this.MessageRepository.GetByRecipient(SecurityGroup);

            return this.View(messages);
        }

        public ActionResult Sent(string? SecurityGroupGuid = null)
        {
            if (!Guid.TryParse(SecurityGroupGuid, out Guid SecurityGroup))
            {
                SecurityGroup = this.UserSession.LoggedInUser.Guid;
            }

            List<InternalMessage> messages = this.MessageRepository.GetBySender(SecurityGroup);

            return this.View(messages);
        }

        public ActionResult ViewFlatMessageTree(string Id)
        {
            List<InternalMessage> model = this.GetTreeFromTop(Guid.Parse(Id)).Flatten().ToList();

            return this.View(model);
        }

        public virtual ActionResult ViewMessage(string Id)
        {
            return this.View(Guid.Parse(Id));
        }

        public ActionResult ViewTree(string Id)
        {
            InternalMessage model = this.MessageRepository.RecursiveFill(this.MessageRepository.Find(Guid.Parse(Id)));

            return this.View(model);
        }
    }
}