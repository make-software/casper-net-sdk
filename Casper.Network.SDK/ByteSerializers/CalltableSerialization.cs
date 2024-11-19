using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Casper.Network.SDK.Types;

namespace Casper.Network.SDK.ByteSerializers
{
    public class Field
    {
        public readonly ushort Index;
        public readonly uint Offset;
        public readonly byte[] Value;

        public Field(ushort index, uint offset, byte[] value)
        {
            this.Index = index;
            this.Offset = offset;
            this.Value = value;
        }
        
        public static int SerializedVecSize(int numberOfFields)
        {
            return sizeof(uint) + sizeof(uint) * sizeof(ushort);
        }
    }
    
    public class CalltableSerialization : BaseByteSerializer
    {
        private readonly List<Field> _fields = new List<Field>();
        private int _currentOffset;
        
        public CalltableSerialization()
        {
            _fields = new List<Field>();
        }

        public CalltableSerialization AddField(int index, CLValue value)
        {
            // var serializer = new CLValueByteSerializer();
            // var bytes = serializer.ToBytes(value);
            return AddField(index, value.Bytes);
        }
        
        public CalltableSerialization AddField(int index, byte[] value)
        {
            if (_fields.Count != index) {
                throw new Exception("Add fields in correct index order.");
            }

            _fields.Add(new Field((ushort)index, (uint)_currentOffset, value));
            
            _currentOffset += value.Length;

            return this;
        }

        public byte[] GetBytes()
        {
            var calltable_bytes = new MemoryStream();
            var payload_bytes = new MemoryStream();
            WriteUInteger(calltable_bytes, (uint)_fields.Count);
            foreach (var field in _fields)
            {
                WriteUShort(calltable_bytes, field.Index);
                WriteUInteger(calltable_bytes, field.Offset);
                payload_bytes.Write(field.Value, 0, field.Value.Length);
            }
            WriteUInteger(calltable_bytes, (uint)payload_bytes.Length);
            payload_bytes.Seek(0, SeekOrigin.Begin);
            payload_bytes.CopyTo(calltable_bytes);

            return calltable_bytes.ToArray();
        }
    }
}
