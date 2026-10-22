using Microsoft.EntityFrameworkCore;
using Product.Domain.Entities;
using Product.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Product.Infrastructure
{
    public class DatabaseTransaction : IDatabaseTransaction
    {
        private readonly ProductDbContext _dbContext;
        public DatabaseTransaction(ProductDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task ExecuteAsync(Func<Task> action)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            {
                try
                {
                    await action();
                    foreach (var entry in _dbContext.ChangeTracker.Entries<Images>())
                    {
                        Console.WriteLine(
                            $"Id: {entry.Entity.Id}, " +
                            $"State: {entry.State}, " +
                            $"IsDeleted: {entry.Entity.IsDeleted}");
                    }

                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    foreach (var entry in ex.Entries)
                    {
                        Console.WriteLine(
                            $"Entity: {entry.Metadata.ClrType.Name}");
                        Console.WriteLine(
                            $"State: {entry.State}");
                    }

                    await transaction.RollbackAsync();
                    throw;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

        }
    }
}
