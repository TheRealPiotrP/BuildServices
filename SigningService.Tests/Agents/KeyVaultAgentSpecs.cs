using Its.Configuration;
using SigningService.Agents;
using SigningService.Models;
using SigningService.Signers.StrongName;
using SigningService.Tests.Utils;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using FluentAssertions;
using SigningService.Services.Configuration;

namespace SigningService.Tests
{
    public class KeyVaultAgentSpecs
    {
        private readonly ITestOutputHelper output;

        public KeyVaultAgentSpecs(ITestOutputHelper output)
        {
            this.output = output;
        }

        public async Task<string> GetKeyVaultKeyId(Stream peImage)
        {
            string keyId = null;
            using (peImage)
            {
                StrongNameSignerHelper sns = new StrongNameSignerHelper(peImage);

                var keyVaultAgent = new KeyVaultAgent();
                PublicKey publicKey = sns.SignaturePublicKeyBlob.PublicKey;
                keyId = await keyVaultAgent.GetRsaKeyIdAsync(publicKey.Exponent, publicKey.Modulus);
                output.WriteLine("KeyVault KeyId = {0}", keyId ?? "<None>");
                output.WriteLine(sns.ToString());
            }
            return keyId;
        }

        [Fact]
        public async void Test()
        {
            TestAssembly sha256 = new TestAssembly("TestLib.sha256.dll", null);
            TestAssembly sha384 = new TestAssembly("TestLib.sha384.dll", null);
            TestAssembly ppsha256delay = new TestAssembly("TestLib.delay.dll", null);
            TestAssembly jscript = new TestAssembly("Microsoft.JScript.dll", null);

            Settings.Precedence = new string [] { "test" };

            (await GetKeyVaultKeyId(sha256.GetWritablePEImage())).Should().BeNull();
            (await GetKeyVaultKeyId(sha384.GetWritablePEImage())).Should().BeNull();
            (await GetKeyVaultKeyId(ppsha256delay.GetWritablePEImage())).Should().NotBeNull();
            (await GetKeyVaultKeyId(jscript.GetWritablePEImage())).Should().BeNull();
        }
    }
}
