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
    public class StrongNameSignerHelperSpecs : TestData
    {
        private readonly ITestOutputHelper output;

        public StrongNameSignerHelperSpecs(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory, MemberData("TestAssembliesWithKnownHash")]
        public void Hash_test(TestAssembly testAssembly)
        {
            output.WriteLine("Assembly: {0}", testAssembly.ResourceName);

            using (Stream outputPeImage = testAssembly.GetWritablePEImage())
            {
                StrongNameSignerHelper strongNameSigner = new StrongNameSignerHelper(outputPeImage);
                output.WriteLine("Expected hash size: {0}", testAssembly.StrongNameSignatureHash.Length);
                output.WriteLine("Expected hash: {0}", testAssembly.StrongNameSignatureHash.ToHex());
                output.WriteLine(strongNameSigner.ToString());
                strongNameSigner.ComputeHash().Should().BeEquivalentTo(testAssembly.StrongNameSignatureHash);
            }
        }

        // Use this test method if sn.exe doesn't let you get the digest file from signed dll file
        // After this test is finished you should see .nonsigned.dll files for each signed assembly in your output directory
        [Theory(Skip = "Helper test method"), MemberData("AllTestAssemblies")]
        public void Remove_signature_from_signed_assemblies_and_save_to_file(TestAssembly testAssembly)
        {
            using (Stream outputPeImage = testAssembly.GetWritablePEImage())
            {
                StrongNameSignerHelper strongNameSigner = new StrongNameSignerHelper(outputPeImage);
                if (strongNameSigner.HasStrongNameSignature)
                {
                    strongNameSigner.RemoveStrongNameSignature();
                    using (FileStream fs = new FileStream(testAssembly.ResourceName + ".nonsigned.dll", FileMode.Create, FileAccess.Write))
                    {
                        outputPeImage.Seek(0, SeekOrigin.Begin);
                        outputPeImage.CopyTo(fs);
                    }
                }
            }
        }
    }
}
