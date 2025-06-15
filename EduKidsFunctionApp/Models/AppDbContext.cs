using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace EduKidsFunctionApp.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CustomerContact> CustomerContacts { get; set; }

    public virtual DbSet<CustomerMessage> CustomerMessages { get; set; }

    public virtual DbSet<WordsBank> WordsBanks { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=tcp:saisiri.database.windows.net,1433;Database=edukids;User Id=saisiriadmin;Password=Puday!@Kiran");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CustomerContact>(entity =>
        {
            entity.HasKey(e => e.ContactId).HasName("PK__customer__5C66259B1E1F9719");

            entity.ToTable("customerContacts", "Master");

            entity.HasIndex(e => e.Email, "UQ__customer__A9D10534DAAD4D2C").IsUnique();

            entity.Property(e => e.City)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FatherName).HasMaxLength(100);
            entity.Property(e => e.MotherName).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(15);
            entity.Property(e => e.States)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Subscribed).HasColumnName("subscribed");
            entity.Property(e => e.UserName).HasMaxLength(100);
        });

        modelBuilder.Entity<CustomerMessage>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__customer__C87C0C9C43A34DF7");

            entity.ToTable("customerMessages", "Master");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Message)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Phone).HasMaxLength(15);
        });

        modelBuilder.Entity<WordsBank>(entity =>
        {
            entity.HasKey(e => e.WordId).HasName("PK__wordsBan__2C20F066279AC9B4");

            entity.ToTable("wordsBank", "Master");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ExampleUsage).HasMaxLength(256);
            entity.Property(e => e.Grammar).HasMaxLength(100);
            entity.Property(e => e.Meaning).HasMaxLength(256);
            entity.Property(e => e.Word).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
