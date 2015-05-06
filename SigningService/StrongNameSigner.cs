using System;
using System.IO.Packaging;
using System.Threading.Tasks;
using SigningService.Agents;

namespace SigningService
{
    public class StrongNameSigner : IPackagePartSigner
    {
        private readonly IKeyVaultAgent _keyVaultAgent;

        public StrongNameSigner(IKeyVaultAgent keyVaultAgent)
        {
            _keyVaultAgent = keyVaultAgent;
        }

        public bool TrySign(PackagePart packagePart)
        {
            if (!CanSign(packagePart))
                return false;

            var digest = GetDigest(packagePart);

            var signedDigest = _keyVaultAgent.Sign(digest);

            InsertSignedDigest(packagePart, signedDigest);

            return true;
        }

        private static bool CanSign(PackagePart packagePart)
        {
            if (!IsAssembly(packagePart))
            {
                return false;
            }

            if (IsSigned(packagePart))
            {
                return false;
            }

            return true;
        }

        private static bool IsSigned(PackagePart packagePart)
        {
            throw new NotImplementedException();
        }

        private static bool IsAssembly(PackagePart packagePart)
        {
            throw new NotImplementedException();
        }

        private void InsertSignedDigest(PackagePart packagePart, Task<byte[]> signedDigest)
        {
            throw new NotImplementedException();
        }

        private byte[] GetDigest(PackagePart packagePart)
        {
            throw new NotImplementedException();
        }
    }
}