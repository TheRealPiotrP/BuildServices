using SigningService.Repositories;
using System;
using System.IO.Packaging;
using System.Threading.Tasks;

namespace SigningService.Agents
{
    class SignAgent : ISignAgent
    {
        private readonly IPackageAgent _packageAgent;
        private readonly IPushTriggerAgent _pushTriggerAgent;
        private readonly ISignerRepository _signerRepository;

        public SignAgent(IPushTriggerAgent pushTriggerAgent, IPackageAgent packageAgent, ISignerRepository signerRepository)
        {
            _pushTriggerAgent = pushTriggerAgent;
            _packageAgent = packageAgent;
            _signerRepository = signerRepository;
        }

        public async Task SignPackage(Uri packageUri)
        {
            if (packageUri == null) throw new ArgumentNullException("packageUri");

            var package = await _packageAgent.GetPackage(packageUri);

            await SignAsync(package);

            var signedPackageUri = _packageAgent.StorePackage(package);

            _pushTriggerAgent.FirePackageSignedTrigger(signedPackageUri);
        }

        private async Task SignAsync(Package package)
        {
            foreach (var packagePart in package.GetParts())
            {
                await SignPackagePartAsync(packagePart);
            }
        }

        private async Task SignPackagePartAsync(PackagePart packagePart)
        {
            foreach (var signer in _signerRepository)
            {
                await signer.TrySignAsync(packagePart.GetStream());
            }
        }
    }
}
