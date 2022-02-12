using AzureFunc.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunc.Extension
{
    public static class RtIssueExtension
    {
        public static MainRtIssue ToRtIssue(this RtIssue item, Guid scanId) =>
            new()
            {
                ScanId = scanId,
                CalledFunction = item.CalledFunction,
                FileChecksum = item.FileChecksum,
                FullName = item.FullName,
                DetailText = item.DetailText,
                CopyrightInfo = item.CopyrightInfo,
                SourceLocations = item.SourceLocations
            };
        public static AllFile ToAllFiles(this string item) =>
            new()
            {
                Name = item
            };

    }
}
