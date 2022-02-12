using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunc.Model
{
    public class RtIssue
    {
        public string FullName { get; set; }
        public string FileChecksum { get; set; }
        public string DetailText { get; set; }
        [Optional]
        public string CopyrightInfo { get; set; }
        [Optional]
        public string CalledFunction { get; set; }
        [Optional]
        public string SourceLocations { get; set; }
    }

    public  class MainRtIssue: RtIssue
    {
        [Key]
        public long MainRtIssueId { get; set; }
        public Guid ScanId { get; set; }
    }

    public sealed class RtIssueMap : ClassMap<RtIssue>
    {
        public RtIssueMap()
        {
            Map(m => m.FullName).Name("Full Name");
            Map(m => m.FileChecksum).Name("File Checksum");
            Map(m => m.DetailText).Name("Detail Text");
            Map(m => m.CopyrightInfo).Name("Copyright Info");
            Map(m => m.CalledFunction).Name("Called Function");
            Map(m => m.SourceLocations).Name("Source Location(s)");
        }
    }
    public class Scan
    {
        [Key]
        public Guid ScanId { get; set; }
        public string ScanName { get; set; }
    }
    public class AllFile
    {
        [Key]
        public long AllFileId { get; set; }
        public string Name { get; set; }

        public string FileChecksum { get; set; }
    }
}
