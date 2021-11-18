using System;

namespace Casper.Network.SDK.Types
{
    public enum AccessRights : byte
    {
        // No permissions
        NONE = 0b00,
        // Permission to read the value under the associated [[URef]].
        READ = 0b01,
        // Permission to write a value under the associated [[URef]].
        WRITE = 0b10,
        // Permission to add to the value under the associated [[URef]].
        ADD = 0b100,
        // Permission to read or write the value under the associated [[URef]].
        READ_WRITE = 0b11,
        // Permission to read or add to the value under the associated [[URef]].
        READ_ADD = 0b101,
        // Permission to add to, or write the value under the associated [[URef]].
        ADD_WRITE = 0b110,
        // Permission to read, add to, or write the value under the associated [[URef]].
        READ_ADD_WRITE = 0b111
    }
}