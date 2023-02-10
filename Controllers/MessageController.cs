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
using Penguin.Shared.Extensions;
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
            EntityPermissionsRepository = entityPermissionsRepository;
            UserSession = userSession;
            MessageRepository = messageRepository;
            ServiceProvider = serviceProvider;
            SecurityGroupRepository = securityGroupRepository;
        }

        [HttpGet]
        public virtual ActionResult Compose(string Recipient, string? Origin = null, int ParentId = 0)
        {
            return View(MessageRepository.Draft(Recipient, Origin, ParentId));
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
                if (MessageRepository.Find(model.Parent!._Id) is null)
                {
                    throw new UnauthorizedAccessException();
                }
            }

            bool HasPermission = true;

            //Logged in user is a member of sending security group
            HasPermission = HasPermission && UserSession.LoggedInUser.SecurityGroupGuids().Any(s => s == model.Origin);

            SecurityGroup target = SecurityGroupRepository.Find(model.Recipient);

            if (UserSession.LoggedInUser.HasRole(RoleNames.SYS_ADMIN) || target is IUser)
            {
                //We allow
            }
            else
            {
                //If its not a user, we need to be sure we have write access to the object since we dont know how it might be used
                HasPermission = EntityPermissionsRepository.AllowsAccessType(model.Recipient, UserSession.LoggedInUser, PermissionTypes.Write);
            }

            if (HasPermission)
            {
                using IWriteContext context = MessageRepository.WriteContext();
                model = MessageRepository.SendMessage(model.Body, model.Subject, model.Recipient, model.Parent?._Id ?? 0, model.Origin);
            }
            else
            {
                throw new UnauthorizedAccessException();
            }

            if (!string.IsNullOrWhiteSpace(Request?.Headers["Referer"]))
            {
                this.AddMessage("Message sent successfully");
                return Redirect(Request.Headers["Referer"]);
            }
            else
            {
                return View("Success");
            }
        }

        public InternalMessage GetTreeFromTop(Guid Id)
        {
            InternalMessage thisMessage = MessageRepository.GetMessageChain(Id);

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
                SecurityGroup = UserSession.LoggedInUser.Guid;
            }

            List<InternalMessage> messages = MessageRepository.GetByRecipient(SecurityGroup);

            return View(messages);
        }

        public ActionResult Sent(string? SecurityGroupGuid = null)
        {
            if (!Guid.TryParse(SecurityGroupGuid, out Guid SecurityGroup))
            {
                SecurityGroup = UserSession.LoggedInUser.Guid;
            }

            List<InternalMessage> messages = MessageRepository.GetBySender(SecurityGroup);

            return View(messages);
        }

        public ActionResult ViewFlatMessageTree(string Id)
        {
            List<InternalMessage> model = GetTreeFromTop(Guid.Parse(Id)).Flatten().ToList();

            return View(model);
        }

        public virtual ActionResult ViewMessage(string Id)
        {
            return View(Guid.Parse(Id));
        }

        public ActionResult ViewTree(string Id)
        {
            InternalMessage model = MessageRepository.RecursiveFill(MessageRepository.Find(Guid.Parse(Id)));

            return View(model);
        }
    }
}