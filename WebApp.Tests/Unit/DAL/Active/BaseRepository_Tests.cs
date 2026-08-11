using Base.Contracts;
using Base.DAL.EF;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Tests.Unit.DAL.Active;

public class BaseRepository_Tests
{
    [Fact]
    public async Task Add_Find_All_Remove_Works()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var ctx = new TestDbContext(options);
        await ctx.Database.EnsureCreatedAsync();

        var repo = new BaseRepository<TestEntity>(ctx);
        var id = Guid.NewGuid();

        repo.Add(new TestEntity { Id = id, Name = "n1" });
        await ctx.SaveChangesAsync();

        var found = await repo.FindAsync(id);
        Assert.NotNull(found);
        Assert.Equal("n1", found!.Name);

        var all = await repo.AllAsync();
        Assert.Contains(all, e => e.Id == id);

        await repo.RemoveAsync(id);
        await ctx.SaveChangesAsync();

        Assert.Null(await repo.FindAsync(id));
    }

    private sealed class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }

        public DbSet<TestEntity> Entities => Set<TestEntity>();
    }

    private sealed class TestEntity : IBaseEntity
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
    }
}
