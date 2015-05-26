using SigningService.Extensions;
using System;

namespace SigningService.Signers.StrongName
{
    internal struct HashingBlock : IComparable<HashingBlock>
    {
        public HashingBlock(HashingBlockHashing hashing, string name, int offset, int size) : this()
        {
            Hashing = hashing;
            Name = name;
            Offset = offset;
            Size = size;
        }

        public HashingBlockHashing Hashing { get; set; }
        public string Name { get; set; }
        public int Offset { get; set; }
        public int Size { get; set; }

        public int CompareTo(HashingBlock other)
        {
            return Offset.CompareTo(other.Offset);
        }

        public override string ToString()
        {
            string blockInfo = string.Format(
                "BLOCK(type: {0}, start: {1}, size: {2}, name: {3})",
                Hashing, Offset, Size, Name.RemoveSpecialCharacters());
            return blockInfo;
        }
    }
}
