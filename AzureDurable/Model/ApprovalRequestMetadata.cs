using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDurable.Model
{
    public class ApprovalRequestMetadata
    {
        public string ApprovalType { get; set; }
        public string InstanceId { get; set; }
        public string ReferenceUrl { get; set; }
        public string Requestor { get; set; }
        public string Subject { get; set; }

    }
    public class ApprovalResponseMetadata
    {
        public string ReferenceUrl { get; set; }
        public string DestinationContainer { get; set; }
    }
}
