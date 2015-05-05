using System.IO.Packaging;

namespace SigningService
{
    internal interface IPackagePartSigner
    {
        bool TrySign(PackagePart packagePart);
    }
}