using System.Linq;

namespace SigningService.Repositories
{
    internal interface ISignerRepository : IOrderedEnumerable<IPackagePartSigner>
    {
    }
}