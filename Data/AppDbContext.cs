using Microsoft.EntityFrameworkCore;
using Fm.Api.Entities;

namespace Fm.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // 사용자
    public DbSet<User> Users => Set<User>();
    // 가계부
    public DbSet<Transaction> Transactions => Set<Transaction>();
    // 게시글
    public DbSet<Post> Posts => Set<Post>();
    // 가족그룹
    public DbSet<Family> Families => Set<Family>();
    // 가족 구성원
    public DbSet<FamilyMember> FamilyMembers => Set<FamilyMember>();
    // 가족 관계선
    public DbSet<FamilyRelationship> FamilyRelationships => Set<FamilyRelationship>();
    // 일정
    public DbSet<Schedule> Schedules => Set<Schedule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Family>()
            .Property(f => f.Name)
            .IsRequired();

        modelBuilder.Entity<FamilyMember>()
            .Property(fm => fm.Name)
            .IsRequired();

        modelBuilder.Entity<FamilyRelationship>()
            .Property(fr => fr.RelationType)
            .IsRequired();

        modelBuilder.Entity<Schedule>()
            .Property(s => s.Title)
            .IsRequired();
    }
}

