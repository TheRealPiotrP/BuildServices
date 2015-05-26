using System;
using System.Collections.Generic;
using System.IO;

namespace SigningService.Tests.Utils
{
    public struct TestAssembly
    {
        public TestAssembly(string resourceName, byte[] strongNameSignatureHash = null)
        {
            ResourceName = resourceName;
            StrongNameSignatureHash = strongNameSignatureHash;
        }
        public string ResourceName;
        public byte[] StrongNameSignatureHash;
        public Stream GetWritablePEImage()
        {
            MemoryStream writablePEImage = new MemoryStream();
            using (Stream peImage = this.GetType().Assembly.GetManifestResourceStream(ResourceName))
            {
                peImage.CopyTo(writablePEImage);
                writablePEImage.Seek(0, SeekOrigin.Begin);
            }

            return writablePEImage;
        }

        public bool HasKnownHash { get { return StrongNameSignatureHash != null; } }

        public static IEnumerable<TestAssembly> GetTestAssemblies()
        {
            // Hashes are extracted using:
            // sn -dg assembly.dll assembly.dig
            // sn -dh assembly.dig assembly.hash
            // assembly.hash contains base64 encoded hash
            yield return new TestAssembly(
                resourceName: @"TestLib.delay.dll",
                strongNameSignatureHash : Convert.FromBase64String("oxU1tTdsx+TPFhAls92mowTsj4BD00e48WTXL51CbS4=")
                
            );

            yield return new TestAssembly(
                resourceName: @"TestLib.sha256.dll",
                strongNameSignatureHash: Convert.FromBase64String("pVO1WsR42BR2Kohh7oAzJ1jBvxjPsuxK5ma3UgyThTE=")
            );

            yield return new TestAssembly(
                resourceName: @"TestLib.sha384.dll",
                strongNameSignatureHash : Convert.FromBase64String("63If3dULhFir3QmITjN4+rElTzghRgn6CenZLhZ1HsVVtOiN18lBwOfSgubDKbkp")
            );

            yield return new TestAssembly(
                resourceName: @"Microsoft.JScript.dll",
                strongNameSignatureHash : Convert.FromBase64String("uh6+pyQ7H9cFs/gQWiKMdaIpFniWLyTiV2718xRTocs=")
            );
        }
    }
}
