using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pcea.Helpers
{
    public class MailMessageV2
    {
        public List<MailboxAddress> To { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public MailMessageV2(Dictionary<string,string> to, string subject, string content)
        {
            To = new List<MailboxAddress>();
            To.AddRange(to.Select(x => new MailboxAddress(x.Value,x.Key)));
            Subject = subject;
            Content = content;
        }
    }
}
