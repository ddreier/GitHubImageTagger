using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using GitHubImageTagger.CLI;

namespace GitHubImageTagger.CLI.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rtm-21431");

            modelBuilder.Entity("GitHubImageTagger.Core.Models.Image", b =>
                {
                    b.Property<int>("ImageId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("FileName");

                    b.Property<string>("Path");

                    b.Property<string>("Url");

                    b.HasKey("ImageId");

                    b.ToTable("Images");
                });

            modelBuilder.Entity("GitHubImageTagger.Core.Models.Tag", b =>
                {
                    b.Property<int>("TagId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Content");

                    b.Property<int?>("ImageId");

                    b.HasKey("TagId");

                    b.HasIndex("ImageId");

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("GitHubImageTagger.Core.Models.Tag", b =>
                {
                    b.HasOne("GitHubImageTagger.Core.Models.Image", "Image")
                        .WithMany("Tags")
                        .HasForeignKey("ImageId");
                });
        }
    }
}
