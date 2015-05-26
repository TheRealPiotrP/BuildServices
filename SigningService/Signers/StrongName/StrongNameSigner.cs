using SigningService.Agents;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace SigningService.Signers.StrongName
{
    public class StrongNameSigner : IPackagePartSigner
    {
        private IKeyVaultAgent _keyVaultAgent;

        public StrongNameSigner(IKeyVaultAgent keyVaultAgent)
        {
            _keyVaultAgent = keyVaultAgent;
        }

        public async Task<bool> TrySignAsync(Stream peStream)
        {
            StrongNameSignerHelper strongNameSigner = new StrongNameSignerHelper(peStream);
            if (strongNameSigner.CanSign)
            {
                if (!SupportsHashAlgorithm(strongNameSigner.SignaturePublicKeyBlob.HashAlgorithm))
                {
                    return false;
                }

                byte[] hash = strongNameSigner.PrepareForSigningAndComputeHash();

                string keyId = await GetKeyVaultId(strongNameSigner);
                if (keyId == null)
                {
                    return false;
                }

                byte[] signature = await _keyVaultAgent.SignAsync(keyId, hash);
                if (strongNameSigner.StrongNameSignatureSize != signature.Length)
                {
                    return false;
                }

                strongNameSigner.StrongNameSignature = signature;
                return true;
            }

            return false;
        }

        public async Task<bool> CanSignAsync(Stream peStream)
        {
            StrongNameSignerHelper strongNameSigner = new StrongNameSignerHelper(peStream);
            if (!strongNameSigner.CanSign)
            {
                return false;
            }
            
            string keyId = await GetKeyVaultId(strongNameSigner);
            return keyId != null;
        }

        private static bool SupportsHashAlgorithm(AssemblyHashAlgorithm hashAlgorithm)
        {
            switch (hashAlgorithm)
            {
                case AssemblyHashAlgorithm.Sha256:
                case AssemblyHashAlgorithm.Sha384:
                case AssemblyHashAlgorithm.Sha512:
                    return true;
                default: return false;
            }
        }

        /// <summary>
        /// Gets KeyVault KeyId related to signature public key
        /// </summary>
        internal async Task<string> GetKeyVaultId(StrongNameSignerHelper strongNameSigner)
        {
            return await _keyVaultAgent.GetRsaKeyIdAsync(strongNameSigner.SignaturePublicKeyBlob.PublicKey.Exponent, strongNameSigner.SignaturePublicKeyBlob.PublicKey.Modulus);
        }
    }
}