using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace prakt8_leshukov.Models;

public partial class LeshukovDbContext : DbContext
{
    public LeshukovDbContext()
    {
    }

    public LeshukovDbContext(DbContextOptions<LeshukovDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<RevokedToken> RevokedTokens { get; set; }

    public virtual DbSet<Task> Tasks { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=sql.ects;Database=LeshukovDB;User Id=student_00;Password=student_00;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.Property(e => e.TokenHash)
                .HasMaxLength(64)
                .IsUnicode(false);
        });

        modelBuilder.Entity<RevokedToken>(entity =>
        {
            entity.Property(e => e.Jti).HasMaxLength(64);
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.ToTable("Task");
            entity.HasKey( t=>t.Id);
            
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
