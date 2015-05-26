using Its.Configuration;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.WebKey;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using SigningService.Extensions;
using SigningService.Models;
using SigningService.Services.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SigningService.Agents
{
    public class KeyVaultAgent : IKeyVaultAgent
    {
        // PublicKey to KeyId mapping
        private Dictionary<PublicKey, string> _supportedPublicKeysCache = new Dictionary<PublicKey, string>();
        public async Task<byte[]> SignAsync(string keyId, byte[] digest)
        {
            if (digest == null) throw new ArgumentNullException("digest");

            var client = new KeyVaultClient(GetAccessToken);

            var keyVaultSettings = Settings.Get<KeyVaultSettings>();

            var signResult =
                await client.SignAsync(
                        keyId,
                        keyVaultSettings.Algorithm,
                        digest);
            
            byte[] ret = signResult.Result;
            ret.ReverseInplace();
            return ret;
        }

        private async Task<IEnumerable<JsonWebKey>> GetKeysAsync()
        {
            var client = new KeyVaultClient(GetAccessToken);
            var keyVaultSettings = Settings.Get<KeyVaultSettings>();

            var keys = await client.GetKeysAsync(keyVaultSettings.Vault);

            List<JsonWebKey> ret = new List<JsonWebKey>();

            foreach (KeyItem k in keys.Value)
            {
                KeyBundle keybundle = await client.GetKeyAsync(k.Kid);
                ret.Add(keybundle.Key);
            }

            return ret;
        }

        // https://samlman.wordpress.com/2015/05/01/fun-with-azure-key-vault-services/
        private static async Task<string> GetAccessToken(string authority, string resource, string scope)
        {
            var identitySettings = Settings.Get<ServiceIdentitySettings>();

            var clientId = identitySettings.ClientId;

            var clientSecret = identitySettings.ClientSecret;

            var context = new AuthenticationContext(authority, null);

            var credential = new ClientCredential(clientId, clientSecret);
            
            var result = await context.AcquireTokenAsync(resource, credential);
            
            return result.AccessToken;
        }

        public async Task<string> GetRsaKeyIdAsync(byte[] exponent, byte[] modulus)
        {
            string kid;
            PublicKey publicKey = new PublicKey(exponent, modulus);
            if (_supportedPublicKeysCache.TryGetValue(publicKey, out kid))
            {
                return kid;
            }
            await UpdateCacheAsync();

            if (_supportedPublicKeysCache.TryGetValue(publicKey, out kid))
            {
                return kid;
            }

            return null;
        }

        private async Task UpdateCacheAsync()
        {
            Dictionary<PublicKey, string> supportedPublicKeysCache = new Dictionary<PublicKey, string>();

            IEnumerable<JsonWebKey> keys = await GetKeysAsync();
            foreach (var key in keys)
            {
                supportedPublicKeysCache.Add(new PublicKey(key.E, key.N), key.Kid);
            }

            _supportedPublicKeysCache = supportedPublicKeysCache;
        }
    }
}