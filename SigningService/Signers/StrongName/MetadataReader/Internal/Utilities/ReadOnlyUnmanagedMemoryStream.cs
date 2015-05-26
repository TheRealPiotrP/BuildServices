// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Runtime.InteropServices;

namespace System.Reflection.Internal
{
    internal unsafe sealed class ReadOnlyUnmanagedMemoryStream : Stream
    {
        private readonly byte* _data;
        private readonly int _length;
        private int _position;

        public ReadOnlyUnmanagedMemoryStream(byte* data, int length)
        {
            _data = data;
            _length = length;
        }

        public unsafe override int ReadByte()
        {
            if (_position == _length)
            {
                return -1;
            }

            return _data[_position++];
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = Math.Min(count, _length - _position);
            Marshal.Copy((IntPtr)(_data + _position), buffer, offset, bytesRead);
            _position += bytesRead;
            return bytesRead;
        }

        public override void Flush()
        {
        }

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public override long Length
        {
            get
            {
                return _length;
            }
        }

        public override long Position
        {
            get
            {
                return _position;
            }

            set
            {
                Seek(value, SeekOrigin.Begin);
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long target;
            try
            {
                switch (origin)
                {
                    case SeekOrigin.Begin:
                        target = offset;
                        break;

                    case SeekOrigin.Current:
                        target = checked(offset + _position);
                        break;

                    case SeekOrigin.End:
                        target = checked(offset + _length);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException("origin");
                }
            }
            catch (OverflowException)
            {
                throw new ArgumentOutOfRangeException("offset");
            }

            if (target < 0 || target >= _length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }

            _position = (int)target;
            return target;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}