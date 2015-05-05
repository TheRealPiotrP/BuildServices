using System;
using System.IO;
using System.IO.Packaging;
using System.Net.Http;
using System.Threading.Tasks;

namespace SigningService.Agents
{
    public class PackageAgent : IPackageAgent
    {
        public async Task<Package> GetPackage(Uri packageUri)
        {
            if (packageUri == null) throw new ArgumentNullException(nameof(packageUri));

            var httpClient = new HttpClient();

            var packageContent = await httpClient.GetStreamAsync(packageUri);

            var seekablePackageContent = new MemoryStream();

            packageContent.CopyTo(seekablePackageContent);

            var package = Package.Open(seekablePackageContent);

            return package;
        }

        public object StorePackage(Package signedPackage)
        {
            throw new NotImplementedException();
        }
    }
}