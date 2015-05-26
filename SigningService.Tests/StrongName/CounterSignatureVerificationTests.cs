using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using SigningService.Extensions;
using SigningService.Signers.StrongName;
using Xunit.Abstractions;
using System.Security.Cryptography;
using SigningService.Tests.Utils;
using System.IO;

namespace SigningService.Tests.StrongName
{
    public class CounterSignatureVerificationTests : TestData
    {
        private readonly ITestOutputHelper output;

        public CounterSignatureVerificationTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void JScriptRawCounterSignatureVerification()
        {
            PublicKeyBlob identityKey = new PublicKeyBlob("002400000480000094000000060200000024000052534131000400000100010007d1fa57c4aed9f0a32e84aa0faefd0de9e8fd6aec8f87fb03766c834c99921eb23be79ad9d5dcc1dd9ad236132102900b723cf980957fc4e177108fc607774f29e8320e92ea05ece4e821c0a5efe8f1645c4c0c93c1ab99285d622caa652c1dfad63d745d6f2de5f17e5eaf0fc4963d261c8a12436518206dc093344d5ad293");
            PublicKeyBlob signatureKey = new PublicKeyBlob("002400000c800000140100000602000000240000525341310008000001000100613399aff18ef1a2c2514a273a42d9042b72321f1757102df9ebada69923e2738406c21e5b801552ab8d200a65a235e001ac9adc25f2d811eb09496a4c6a59d4619589c69f5baf0c4179a47311d92555cd006acc8b5959f2bd6e10e360c34537a1d266da8085856583c85d81da7f3ec01ed9564c58d93d713cd0172c8e23a10f0239b80c96b07736f5d8b022542a4e74251a5f432824318b3539a5a087f8e53d2f135f9ca47f3bb2e10aff0af0849504fb7cea3ff192dc8de0edad64c68efde34c56d302ad55fd6e80f302d5efcdeae953658d3452561b5f36c542efdbdd9f888538d374cef106acf7d93a4445c3c73cd911f0571aaf3d54da12b11ddec375b3");
            byte[] counterSignature = "6d4e780c84d2a4a29a743c058b19877a469ea2eb06a567c4ba7f7d071dc6e2b036008946af58f4003c48d92365d8ff0e5349dc9022a8cb435cadf8fe903543db6bdb6a10a2004313ce86c4494ab40d402136750d42d51434eef2aa38696b872f7d5d03dd26b1ab43313a8017f1215ece6a23113b9f206876806f18eee166a8a5".FromHexToByteArray();

            identityKey.VerifyData(signatureKey.Blob, counterSignature).Should().BeTrue();
        }

        [Fact]
        public void JScriptCounterSignatureVerification()
        {
            TestAssembly jscript = TestData.GetJScript();
            using (Stream peImage = jscript.GetWritablePEImage())
            {
                StrongNameSignerHelper sns = new StrongNameSignerHelper(peImage);
                sns.HasUniqueSignatureAndIdentityPublicKeyBlobs.Should().BeTrue();
                sns.VerifyCounterSignature().Should().BeTrue();
                sns.IdentityPublicKeyBlob.VerifyData(sns.SignaturePublicKeyBlob.Blob, sns.CounterSignature);
            }
        }

        [Theory, MemberData("AllTestAssemblies")]
        public void VerifyCounterSignatureOfAllAssemblies(TestAssembly assembly)
        {
            using (Stream peImage = assembly.GetWritablePEImage())
            {
                StrongNameSignerHelper sns = new StrongNameSignerHelper(peImage);
                sns.VerifyCounterSignature().Should().BeTrue();
            }
        }
    }
}
