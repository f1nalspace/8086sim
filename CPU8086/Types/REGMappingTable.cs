using System;
using System.ComponentModel.DataAnnotations;

namespace Final.CPU8086.Types
{
    public class REGMappingTable
    {
        // Contains a table mapping the bit-pattern for 8 bits, 16 bits and 32 bits register, each 8 entries long
        private readonly Register[] _table;

        public REGMappingTable()
        {
            _table = new Register[8 * 3];

            // 8-bits
            _table[0] = new Register(0b000, RegisterType.AL);
            _table[1] = new Register(0b001, RegisterType.CL);
            _table[2] = new Register(0b010, RegisterType.DL);
            _table[3] = new Register(0b011, RegisterType.BL);
            _table[4] = new Register(0b100, RegisterType.AH);
            _table[5] = new Register(0b101, RegisterType.CH);
            _table[6] = new Register(0b110, RegisterType.DH);
            _table[7] = new Register(0b111, RegisterType.BH);

            // 16-bits
            _table[8] = new Register(0b000, RegisterType.AX);
            _table[9] = new Register(0b001, RegisterType.CX);
            _table[10] = new Register(0b010, RegisterType.DX);
            _table[11] = new Register(0b011, RegisterType.BX);
            _table[12] = new Register(0b100, RegisterType.SP);
            _table[13] = new Register(0b101, RegisterType.BP);
            _table[14] = new Register(0b110, RegisterType.SI);
            _table[15] = new Register(0b111, RegisterType.DI);

            // 32-bits
            _table[16] = new Register(0b000, RegisterType.EAX);
            _table[17] = new Register(0b001, RegisterType.ECX);
            _table[18] = new Register(0b010, RegisterType.EDX);
            _table[19] = new Register(0b011, RegisterType.EBX);
            _table[20] = new Register(0b100, RegisterType.ESP);
            _table[21] = new Register(0b101, RegisterType.EBP);
            _table[22] = new Register(0b110, RegisterType.ESI);
            _table[23] = new Register(0b111, RegisterType.EDI);
        }

        public ref readonly Register Get(byte index, DataType type)
        {
            int baseIndex = type switch
            {
                DataType.Byte => 0,
                DataType.Word => 1,
                DataType.DoubleWord or
                DataType.Int => 2,
                _ => throw new NotSupportedException($"The data type '{type}' is not supported!")
            };
            int finalIndex = baseIndex * 8 + index;
            return ref _table[finalIndex];
        }

        public ref readonly Register GetByte(byte index) => ref Get(index, DataType.Byte);
        public ref readonly Register GetWord(byte index) => ref Get(index, DataType.Word);
    }
}
