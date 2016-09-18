using Microsoft.EntityFrameworkCore;
using GitHubImageTagger.Core.Models;

namespace GitHubImageTagger.CLI
{
    public class ApplicationDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(@"Filename=D:\Code\GitHubImageTagger\images.db");
        }

        public DbSet<Image> Images { get; set; }
        public DbSet<Tag> Tags { get; set; }
    }
}
