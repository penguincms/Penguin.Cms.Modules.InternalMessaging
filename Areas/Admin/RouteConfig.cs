using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Penguin.Web.Abstractions.Interfaces;

namespace Penguin.Cms.Modules.InternalMessaging.Areas.Admin
{
    public class RouteConfig : IRouteConfig
    {
        public void RegisterRoutes(IRouteBuilder routes)
        {
            routes.MapRoute(
                name: "ComposeMessage",
                template: "Message/Compose/{Recipient}/{Origin?}",
                defaults: new { controller = "Message", action = "Compose" }
            );

            routes.MapRoute(
                name: "MessageInbox",
                template: "Message/Inbox/{SecurityGroupGuid?}",
                defaults: new { controller = "Message", action = "Inbox" }
            );

            routes.MapRoute(
                name: "MessageSent",
                template: "Message/Sent/{SecurityGroupGuid?}",
                defaults: new { controller = "Message", action = "Sent" }
            );
        }
    }
}