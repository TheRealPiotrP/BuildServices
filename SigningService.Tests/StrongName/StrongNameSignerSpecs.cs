using FluentAssertions;
using Microsoft.Its.Recipes;
using Moq;
using SigningService.Agents;
using SigningService.Extensions;
using SigningService.Signers.StrongName;
using SigningService.Tests.Utils;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace SigningService.Tests
{
    public class StrongNameSignerSpecs : TestData
    {
        private readonly ITestOutputHelper output;

        public StrongNameSignerSpecs(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory, MemberData("AllTestAssemblies")]
        public async void Sign_test_using_mocked_KeyVault(TestAssembly testAssembly)
        {
            output.WriteLine("Assembly: {0}", testAssembly.ResourceName);
            var keyVaultAgentMock = new Mock<IKeyVaultAgent>(MockBehavior.Strict);

            string keyId = Any.String(4, 10, "uvwxyz");
            byte[] signature = Any.Sequence(i => Any.Byte(), 256).ToArray();

            keyVaultAgentMock
                .Setup(k => k.SignAsync(It.Is<string>(s => s.Equals(keyId)), It.IsAny<byte[]>()))
                .Returns(Task.FromResult(signature));
            keyVaultAgentMock
                .Setup(k => k.GetRsaKeyIdAsync(It.IsAny<byte[]>(), It.IsAny<byte[]>()))
                .Returns(Task.FromResult(keyId));

            using (Stream outputPeImage = testAssembly.GetWritablePEImage())
            {
                (new StrongNameSignerHelper(outputPeImage)).RemoveStrongNameSignature();
                StrongNameSigner signer = new StrongNameSigner(keyVaultAgentMock.Object);

                (await signer.CanSignAsync(outputPeImage)).Should().BeTrue("CanSign before signing");
                (await signer.TrySignAsync(outputPeImage)).Should().BeTrue("TrySign");
                (await signer.CanSignAsync(outputPeImage)).Should().BeFalse("CanSign after signing");
                (await signer.TrySignAsync(outputPeImage)).Should().BeFalse("TrySign after signing");

                outputPeImage.Seek(0, SeekOrigin.Begin);
                StrongNameSignerHelper helper = new StrongNameSignerHelper(outputPeImage);
                helper.HasStrongNameSignatureDirectory.Should().BeTrue("Should have strong name signature directory");
                helper.StrongNameSignedFlag.Should().BeTrue("Should have strong name signed flag set");
                helper.StrongNameSignature.Should().BeEquivalentTo(signature, "Signatures should be equivalent");
            }
        }
    }
}
