using System;
using System.Collections.Generic;

namespace Casper.Network.SDK.Types
{
    public class CLListTypeInfo : CLTypeInfo
    {
        public CLTypeInfo ListType { get; }

        public CLListTypeInfo(CLTypeInfo type)
            : base(CLType.List)
        {
            ListType = type;
        }
        
        public override bool Equals(object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || ! this.GetType().Equals(obj.GetType()))
            {
                return false;
            }

            return base.Equals(obj) && this.ListType.Equals(((CLListTypeInfo) obj).ListType);
        }
        
        public override int GetHashCode()
        {
            return (int)Type^(int)ListType.Type;
        }
        
        public override string ToString()
        {
            return $"List({ListType.ToString()})";
        }

        public override Type GetFrameworkType()
        {
            Type valueType = this.ListType.GetFrameworkType();
            var listType = typeof(List<>).MakeGenericType(new[] {valueType});
            
            return listType;
        }
    }
}