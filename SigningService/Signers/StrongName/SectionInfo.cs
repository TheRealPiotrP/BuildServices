using SigningService.Extensions;

namespace SigningService.Signers.StrongName
{
    internal class SectionInfo
    {
        public string Name;
        public int Offset;
        public int Size;

        public override string ToString()
        {
            string name = Name.RemoveSpecialCharacters();
            return string.Format("SECTION(Name = {0}, Start = {1}, Size = {2})", name, Offset, Size);
        }
    }
}