using Microsoft.EntityFrameworkCore;

namespace Haskoli.Domain.Interfaces.Base
{
    public interface IDbFactory<TContext> : IDisposable where TContext : DbContext, new()
    {
        TContext Init();
    }
}
