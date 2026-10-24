using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MovieStore.API.Data.Migrations;

[DbContext(typeof(AppDbContext))]
public class AppDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.12");

        modelBuilder.Entity("MovieStore.Domain.Entities.Genre", entity =>
        {
            entity.Property<int>("Id").ValueGeneratedOnAdd()
                .HasColumnType("INTEGER")
                .HasAnnotation("Sqlite:Autoincrement", true);
            entity.Property<string>("Name").IsRequired().HasMaxLength(100).HasColumnType("TEXT");
            entity.Property<string>("NormalizedName").IsRequired().HasMaxLength(100).HasColumnType("TEXT");
            entity.HasKey("Id");
            entity.HasIndex("NormalizedName").IsUnique();
            entity.ToTable("Genres");
        });

        modelBuilder.Entity("MovieStore.Domain.Entities.Movie", entity =>
        {
            entity.Property<int>("Id").ValueGeneratedOnAdd()
                .HasColumnType("INTEGER")
                .HasAnnotation("Sqlite:Autoincrement", true);
            entity.Property<string>("Description").IsRequired().HasMaxLength(500).HasColumnType("TEXT");
            entity.Property<int>("GenreId").HasColumnType("INTEGER");
            entity.Property<string>("Image").HasColumnType("TEXT");
            entity.Property<string>("MimeType").HasColumnType("TEXT");
            entity.Property<string>("Name").IsRequired().HasMaxLength(150).HasColumnType("TEXT");
            entity.Property<decimal>("Price").HasColumnType("TEXT");
            entity.HasKey("Id");
            entity.HasIndex("GenreId");
            entity.ToTable("Movies");
        });

        modelBuilder.Entity("MovieStore.Domain.Entities.Movie", entity =>
        {
            entity.HasOne("MovieStore.Domain.Entities.Genre", "Genre")
                .WithMany("Movies")
                .HasForeignKey("GenreId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
            entity.Navigation("Genre");
        });

        modelBuilder.Entity("MovieStore.Domain.Entities.Genre", entity =>
            entity.Navigation("Movies"));
    }
}
