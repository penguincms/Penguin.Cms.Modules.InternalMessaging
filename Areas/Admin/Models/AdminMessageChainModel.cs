using Penguin.Cms.InternalMessaging;
using System;
using System.Collections.Generic;

namespace Penguin.Cms.Modules.InternalMessaging.Areas.Admin.Models
{
    public class AdminMessageChainModel
    {
        public IEnumerable<InternalMessage> Messages { get; set; } = new List<InternalMessage>();
        public Guid Recipient { get; set; }
    }
}