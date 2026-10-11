using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Product.Domain.Interfaces
{
    public interface IDatabaseTransaction
    {
        public Task ExecuteAsync(Func<Task> action);
    }
}
