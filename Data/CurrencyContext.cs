using Microsoft.EntityFrameworkCore;
using BlazorTestApp.Models;

namespace BlazorTestApp.Data;

/// <summary>
/// Контекст базы данных для нашего приложения валют
/// Всё довольно просто — по сути, только одна таблица
/// </summary>
public class CurrencyContext : DbContext
{
    public CurrencyContext(DbContextOptions<CurrencyContext> options)
        : base(options)
    {
    }

    // используем новую синтаксическую форму, она чище, чем старая
    public DbSet<Currency> Currencies => Set<Currency>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Currency>(entity =>
        {
            // нужна высокая точность для курсов валют, 6 знаков после запятой должно хватить
            entity.Property(c => c.Rate)
                .HasPrecision(18, 6);

            // убеждаемся, что коды валют не дублируются
            entity.HasIndex(c => c.Code)
                .IsUnique();
        });
    }
}