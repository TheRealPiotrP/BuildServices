using System;
using System.IO.Packaging;
using System.Threading.Tasks;
using SigningService.Repositories;

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

            var signedPackage = Sign(package);

            var signedPackageUri = _packageAgent.StorePackage(signedPackage);

            _pushTriggerAgent.FirePackageSignedTrigger(signedPackageUri);
        }

        private Package Sign(Package package)
        {
            foreach (var packagePart in package.GetParts())
            {
                SignPackagePart(packagePart);
            }

            return package;
        }

        private void SignPackagePart(PackagePart packagePart)
        {
            foreach (var signer in _signerRepository)
            {
                signer.TrySign(packagePart);
            }
        }
    }
}
