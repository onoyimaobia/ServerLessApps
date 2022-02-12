using AzureFunc;
using AzureFunc.Dbcontext;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: WebJobsStartup(typeof(Startup))]
namespace AzureFunc
{

    public class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            string connectionString = Environment.GetEnvironmentVariable("AzureSqlDatabase");
            builder.Services.AddDbContext<AzureFuncDbContext>(option =>
            
                option.UseSqlServer(connectionString)
            );
            builder.Services.BuildServiceProvider();
        }
    }
}
