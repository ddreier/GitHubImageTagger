using Microsoft.EntityFrameworkCore;
using GitHubImageTagger.Core.Models;

namespace GitHubImageTagger.Core
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        { }

        public DbSet<Image> Images { get; set; }
        public DbSet<Tag> Tags { get; set; }
    }
}
