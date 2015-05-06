using System;
using System.Text;
using System.Threading.Tasks;
using Its.Configuration;
using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using SigningService.Services.Configuration;

namespace SigningService.Agents
{
    public class KeyVaultAgent : IKeyVaultAgent
    {
        public async Task<byte[]> Sign(byte[] digest)
        {
            if (digest == null) throw new ArgumentNullException("digest");

            if (digest.Length != 32) throw new ArgumentException("The value must have 32 bytes", "digest");

            var client = new KeyVaultClient(GetAccessToken);

            var keyVaultSettings = Settings.Get<KeyVaultSettings>();

            var signResult =
                await client.SignAsync(
                        keyVaultSettings.KeyId,
                        keyVaultSettings.Algorithm,
                        digest);

            return signResult.Result;
        }

        // https://samlman.wordpress.com/2015/05/01/fun-with-azure-key-vault-services/
        public static async Task<string> GetAccessToken(string authority, string resource, string scope)
        {
            var identitySettings = Settings.Get<ServiceIdentitySettings>();

            var clientId = identitySettings.ClientId;

            var clientSecret = identitySettings.ClientSecret;

            var context = new AuthenticationContext(authority, null);

            var credential = new ClientCredential(clientId, clientSecret);
            
            var result = await context.AcquireTokenAsync(resource, credential);
            
            return result.AccessToken;
        }
    }
}