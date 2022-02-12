using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using AzureFunc.Dbcontext;
using AzureFunc.Extension;
using AzureFunc.Model;
using CsvHelper;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureFunc
{
    public class DownloadZipAndUploadToDbFunc
    {
        private List<MainRtIssue> rtIssues = new();
        private List<AllFile> allFiles = new();
        private readonly AzureFuncDbContext _db;
        public DownloadZipAndUploadToDbFunc(AzureFuncDbContext db)
        {
            _db = db;
        }
        [FunctionName("DownloadZipAndUploadToDbFunc")]
        public async Task RunAsync([BlobTrigger("runtime-issue/{name}", Connection = "AzureWebJobsStorage")]Stream myBlob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            try
            {
                if (name.Split('.').Last().ToLower() == "zip")
                {
                    var fileFullName = Path.GetFileNameWithoutExtension(name);
                   // var texts = fileFullName.Split(new[] { '_' }, 2);
                    //var scanId = texts[0].ToString();
                    //string SystemDrive = Environment.GetEnvironmentVariable(@"SystemDrive");
                    //var dir = SystemDrive + @"\Rt-Issues";
                    //CreateDirectories(dir);
                    //string[] folders = Directory.GetDirectories(dir, "*", SearchOption.AllDirectories);
                    myBlob.Position = 0;
                    var zip = new ZipArchive(myBlob);
                    foreach (var file in zip.Entries)
                    {
                        string fileName = file.Name;
                        string fileExtension = Path.GetExtension(fileName);
                        switch (fileExtension)
                        {
                            case ".csv":
                                GetRecordsFromFile(file, Guid.Parse(fileFullName));
                                break;

                            case ".txt":
                                ReadTextFile(file);
                                //file.ExtractToFile(dir + "\\" + "txtfiles\\" + file.FullName);
                                // ReadFiletToFolder(file, dir + "\\" + "txtfiles\\" + file.FullName);
                                break;

                            default:
                                break;
                        }
                    }
                   await _db.MainRtIssues.AddRangeAsync(rtIssues);
                   await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                log.LogInformation($"Error! Something went wrong: {ex.Message}");

            }
        }
        private void ReadFiletToFolder(ZipArchiveEntry file, string filePath)
        {
            using Stream fileStream = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            using Stream entryStream = file.Open();
            entryStream.CopyTo(fileStream);
        }
        private  void CreateDirectories(string dir)
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
            Directory.CreateDirectory(dir);
            string[] subDirectories =
              {
                        @"csvfiles",
                        @"txtfiles",
                        @"pdgfiles",
                        @"images",
                        ""
                    };
            foreach (string workdir in subDirectories)
            {
                if (!String.IsNullOrEmpty(workdir))
                {
                    string newdir = dir + @"\" + workdir;
                    Directory.CreateDirectory(newdir);
                }
            }
        }
        private void GetRecordsFromFile(ZipArchiveEntry file, Guid scanId)
        {
            using Stream entryStream = file.Open();
            using var reader = new StreamReader(entryStream);
            reader.ReadLine();
            reader.ReadLine();
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Context.RegisterClassMap<RtIssueMap>();
            var records = csv.GetRecords<RtIssue>().ToList();
            foreach (var record in records) rtIssues.Add(record.ToRtIssue(scanId));
        }
        private void ReadTextFile(ZipArchiveEntry file)
        {

            using Stream entryStream = file.Open();
            using var sr = new StreamReader(entryStream);
            var resultString = sr.ReadToEnd();
            foreach (var myString in resultString.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                allFiles.Add(myString.ToAllFiles());
            }  
            sr.Close();

        }
    }
}
