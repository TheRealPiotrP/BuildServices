using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace SigningService.Signers.StrongName
{
    internal static class HashingHelpers
    {
        public static HashAlgorithm CreateHashAlgorithm(AssemblyHashAlgorithm hashAlgorithm)
        {
            switch (hashAlgorithm)
            {
                case AssemblyHashAlgorithm.MD5: return MD5.Create();
                case AssemblyHashAlgorithm.Sha1: return SHA1.Create();
                case AssemblyHashAlgorithm.Sha256: return SHA256.Create();
                case AssemblyHashAlgorithm.Sha384: return SHA384.Create();
                case AssemblyHashAlgorithm.Sha512: return SHA512.Create();
                default: return null;
            }
        }

        public static byte[] CalculateAssemblyHash(Stream s, HashAlgorithm hashAlgorithm, List<HashingBlock> hashingBlocks)
        {
            hashAlgorithm.Initialize();

            for (int i = 0; i < hashingBlocks.Count; i++)
            {
                switch (hashingBlocks[i].Hashing)
                {
                    case HashingBlockHashing.HashZeros:
                    {
                        CalculatePartialHashFromZeros(hashAlgorithm, hashingBlocks[i].Size);
                        break;
                    }
                    case HashingBlockHashing.Hash:
                    {
                        s.Seek(hashingBlocks[i].Offset, SeekOrigin.Begin);
                        CalculatePartialHashFromStream(s, hashAlgorithm,  hashingBlocks[i].Size);
                        break;
                    }
                    case HashingBlockHashing.Skip:
                    {
                        break;
                    }
                    default:
                    {
                        ExceptionsHelper.ThrowArgumentOutOfRange("hashingBlocks");
                        return null;
                    }
                }
            }

            // Finalize hashing
            byte[] buffer = new byte[1];
            hashAlgorithm.TransformFinalBlock(buffer, 0, 0);

            return hashAlgorithm.Hash;
        }

        // Sorts by offsets and joins adjacent blocks
        // THIS IS SIMPLE IMPLEMENTATION which assumes there are up to 2 overlapping blocks at a time.
        // This might not work properly for 3 or more overlapping blocks.
        // TODO: Use priority to determine what hashing action we should take.
        //       We should use PriorityQueue implementation and split blocks into (begin, offset, id) (end, offset, id)
        // Intersections of blocks may happen if i.e. StrongNameDirectory persists inside CertificateDirectory
        // We always assume that regular hashing has lower precedence.
        //
        // Example intersection:
        // HASHING
        // ,____________,
        //        ,________,
        //         SKIP
        //
        // Results in:
        // ,______,________,
        // HASH    SKIP
        //
        // HASHING
        // ,____________,
        //        ,___,
        //         SKIP
        //
        // Results in:
        // ,______,___,_,
        // HASH    SKIP HASH
        public static List<HashingBlock> SortAndJoinIntersectingHashingBlocks(List<HashingBlock> blocks)
        {
            blocks.Sort();

            List<HashingBlock> ret = new List<HashingBlock>(16);

            HashingBlock prev = new HashingBlock(HashingBlockHashing.Hash, "Fake block beginning", 0, 0);
            for (int i = 0; i < blocks.Count; i++)
            {
                // are intersecting? (adjacent is ok)
                if (blocks[i].Offset < prev.Offset + prev.Size)
                {
                    if (prev.Size == 0)
                    {
                        prev = blocks[i];
                    }
                    else
                    {
                        if (prev.Hashing == blocks[i].Hashing)
                        {
                            // If they have the same type of hashing situation is clear, we just add them
                            prev.Name += " + " + blocks[i].Name;
                            // prev offset is always lower or equal to blocks[i] offset as DataBlocks are sorted
                            prev.Size = Math.Max(prev.Offset + prev.Size, blocks[i].Offset + blocks[i].Size) - prev.Offset;
                        }
                        else
                        {
                            // Now it means we have a conflict
                            // We are gonna try resolve DataBlockHashing.Hash vs anything else
                            // In any other case we throw
                            //
                            // We are gonna output 1-3 block:
                            // ,______,_______,_______,
                            //  LEFT   MIDDLE  RIGHT
                            // where MIDDLE = blocks[i]
                            if (prev.Hashing ==  HashingBlockHashing.Hash)
                            {
                                int leftBlockSize = blocks[i].Offset - prev.Offset;
                                if (leftBlockSize > 0)
                                {
                                    // we add left block if size is non zero
                                    ret.Add(new HashingBlock(prev.Hashing, prev.Name, prev.Offset, leftBlockSize));
                                }

                                int rightBlockEndOffset = prev.Offset + prev.Size;
                                int middleBlockEndOffset = blocks[i].Offset + blocks[i].Size;

                                HashingBlock rightBlock = new HashingBlock(prev.Hashing, prev.Name, middleBlockEndOffset, rightBlockEndOffset - middleBlockEndOffset);
                                if (rightBlock.Size > 0)
                                {
                                    // we add middle block and right block if right is non-zero
                                    ret.Add(blocks[i]);
                                    prev = rightBlock;
                                }
                                else
                                {
                                    // if right block is zero then
                                    // we only add middle block
                                    prev = blocks[i];
                                }
                            }
                            else if (blocks[i].Hashing == HashingBlockHashing.Hash)
                            {
                                // Let's assume that hashing is always less important as it is default.
                                int leftBlockEndOffset = prev.Offset + prev.Size;
                                int middleBlockEndOffset = blocks[i].Offset + blocks[i].Size;
                                int middleBlockSize = middleBlockEndOffset - leftBlockEndOffset;
                                if (middleBlockSize > 0)
                                {
                                    ret.Add(prev);
                                    prev.Name = blocks[i].Name;
                                    prev.Offset = leftBlockEndOffset;
                                    prev.Size = middleBlockSize;
                                }
                                // else do nothing
                            }
                            else
                            {
                                // here we have to fight HashZeros vs Skip
                                throw new NotImplementedException(string.Format("Incompatible intersecting blocks. {0} and {1}.", prev.Hashing, blocks[i].Hashing));
                            }
                        }
                    }
                } // if (are intersecting?)
                else
                {
                    // are not intersecting
                    if (prev.Size > 0)
                    {
                        ret.Add(prev);
                    }
                    prev = blocks[i];
                }
            } // for

            // add the remaining element
            if (prev.Size > 0)
            {
                ret.Add(prev);
            }

            return ret;
        }

        private static void CalculatePartialHashFromStream(Stream s, HashAlgorithm hashAlgorithm, int bytesToRead)
        {
            byte[] buffer = new byte[bytesToRead];
            int totalBytesRead = 0;
            while (bytesToRead > 0)
            {
                long prevPosition = s.Position;
                int bytesRead = s.Read(buffer, totalBytesRead, bytesToRead);
                totalBytesRead += bytesRead;
                bytesToRead -= bytesRead;

                if (bytesRead <= 0)
                {
                    ExceptionsHelper.ThrowUnexpectedEndOfStream(s.Position);
                    return;
                }
                
                hashAlgorithm.TransformBlock(buffer, 0, bytesRead, buffer, 0);
            }
        }

        private static void CalculatePartialHashFromZeros(HashAlgorithm hashAlgorithm, int numberOfZeroedBytes)
        {
            // Create 0-initialized array
            byte[] buffer = new byte[numberOfZeroedBytes];
            hashAlgorithm.TransformBlock(buffer, 0, buffer.Length, buffer, 0);
        }
    }
}