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
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
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
