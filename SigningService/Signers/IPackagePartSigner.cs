using System.IO;
using System.Threading.Tasks;

namespace SigningService.Signers
{
    internal interface IPackagePartSigner
    {
        Task<bool> TrySignAsync(Stream peStream);
        Task<bool> CanSignAsync(Stream peStream);
    }
}