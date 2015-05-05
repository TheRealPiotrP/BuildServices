using System;
using System.IO.Packaging;
using System.Threading.Tasks;

namespace SigningService.Agents
{
    internal interface IPackageAgent
    {
        Task<Package> GetPackage(Uri packageUri);
        object StorePackage(Package signedPackage);
    }
}