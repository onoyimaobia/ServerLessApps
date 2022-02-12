using AzureFunc.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunc.Dbcontext
{
    public class AzureFuncDbContext: DbContext
    {
        public AzureFuncDbContext(DbContextOptions<AzureFuncDbContext> dbContextOptions): base(dbContextOptions)
        {

        }
        public DbSet<SalesRequest> SalesRequests { get; set; }
        public DbSet<GroceryItem> GroceryItems { get; set; }
        public DbSet<MainRtIssue> MainRtIssues { get; set; }
        public DbSet<Scan> Scans { get; set; }
        protected  override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<SalesRequest>(entity =>
            {
                entity.HasKey(c => c.Id);
            });
            modelBuilder.Entity<GroceryItem>(entity =>
            {
                entity.HasKey(c => c.Id);
            });
            modelBuilder.Entity<MainRtIssue>(entity =>
            {
                entity.HasOne<Scan>()
                .WithMany()
                .HasPrincipalKey(e => e.ScanId)
                .HasForeignKey(e => e.ScanId);
            });
        }
    }
}
