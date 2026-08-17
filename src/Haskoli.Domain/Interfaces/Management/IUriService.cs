using Haskoli.Domain.Parameters;

namespace Haskoli.Domain.Interfaces.Management
{
    public interface IUriService
    {
        Uri GetPageUri(RequestParameter filter, string route);
    }
}
