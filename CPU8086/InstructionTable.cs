using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using IL = Final.CPU8086.InstructionList;
using IE = Final.CPU8086.InstructionEntry;
using DW = Final.CPU8086.DataWidthType;
using DF = Final.CPU8086.DataFlags;
using System;

namespace Final.CPU8086
{
    public class InstructionEntryTable : IReadOnlyCollection<InstructionList>
    {
        private readonly IL[] _opToList = new IL[256];

        public ref readonly IL this[int index] => ref _opToList[index];

        public InstructionEntryTable()
        {
            Array.Clear(_opToList, 0, _opToList.Length);
        }

        public void Load()
        {
            _opToList[0] = new IL(0b00000000,
                new IE(0x00, "ADD", "B", DF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmb", "rb" })
            );
            _opToList[1] = new IL(0b00000001,
                new IE(0x01, "ADD", "W", DF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmw", "rw" })
            );
            _opToList[2] = new IL(0b00000010,
                new IE(0x02, "ADD", "B", DF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rb", "rmb" })
            );
            _opToList[3] = new IL(0b00000011,
                new IE(0x03, "ADD", "W", DF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rw", "rmw" })
            );
            _opToList[4] = new IL(0b00000100,
                new IE(0x04, "ADD", "B", DF.None, "o---szap", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "al", "ib" })
            );
            _opToList[5] = new IL(0b00000101,
                new IE(0x05, "ADD", "W", DF.None, "o---szap", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "ax", "iw" })
            );
            _opToList[6] = new IL(0b00000110,
                new IE(0x06, "PUSH", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "es" })
            );
            _opToList[7] = new IL(0b00000111,
                new IE(0x07, "POP", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "es" })
            );
            _opToList[8] = new IL(0b00001000,
                new IE(0x08, "OR", "B", DF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmb", "rb" })
            );
            _opToList[9] = new IL(0b00001001,
                new IE(0x09, "OR", "W", DF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmw", "rw" })
            );
            _opToList[10] = new IL(0b00001010,
                new IE(0x0A, "OR", "B", DF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rb", "rmb" })
            );
            _opToList[11] = new IL(0b00001011,
                new IE(0x0B, "OR", "W", DF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rw", "rmw" })
            );
            _opToList[12] = new IL(0b00001100,
                new IE(0x0C, "OR", "B", DF.None, "o---szap", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "al", "ib" })
            );
            _opToList[13] = new IL(0b00001101,
                new IE(0x0D, "OR", "W", DF.None, "o---szap", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "ax", "iw" })
            );
            _opToList[14] = new IL(0b00001110,
                new IE(0x0E, "PUSH", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "cs" })
            );
            _opToList[16] = new IL(0b00010000,
                new IE(0x10, "ADC", "B", DF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmb", "rb" })
            );
            _opToList[17] = new IL(0b00010001,
                new IE(0x11, "ADC", "W", DF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmw", "rw" })
            );
            _opToList[18] = new IL(0b00010010,
                new IE(0x12, "ADC", "B", DF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rb", "rmb" })
            );
            _opToList[19] = new IL(0b00010011,
                new IE(0x13, "ADC", "W", DF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rw", "rmw" })
            );
            _opToList[20] = new IL(0b00010100,
                new IE(0x14, "ADC", "B", DF.None, "o---szap", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "al", "ib" })
            );
            _opToList[21] = new IL(0b00010101,
                new IE(0x15, "ADC", "W", DF.None, "o---szap", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "ax", "iw" })
            );
            _opToList[22] = new IL(0b00010110,
                new IE(0x16, "PUSH", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ss" })
            );
            _opToList[23] = new IL(0b00010111,
                new IE(0x17, "POP", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ss" })
            );
            _opToList[24] = new IL(0b00011000,
                new IE(0x18, "SBB", "B", DF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmb", "rb" })
            );
            _opToList[25] = new IL(0b00011001,
                new IE(0x19, "SBB", "W", DF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmw", "rw" })
            );
            _opToList[26] = new IL(0b00011010,
                new IE(0x1A, "SBB", "B", DF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rb", "rmb" })
            );
            _opToList[27] = new IL(0b00011011,
                new IE(0x1B, "SBB", "W", DF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rw", "rmw" })
            );
            _opToList[28] = new IL(0b00011100,
                new IE(0x1C, "SBB", "B", DF.None, "o---szap", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "al", "ib" })
            );
            _opToList[29] = new IL(0b00011101,
                new IE(0x1D, "SBB", "W", DF.None, "o---szap", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "ax", "iw" })
            );
            _opToList[30] = new IL(0b00011110,
                new IE(0x1E, "PUSH", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ds" })
            );
            _opToList[31] = new IL(0b00011111,
                new IE(0x1F, "POP", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ds" })
            );
            _opToList[32] = new IL(0b00100000,
                new IE(0x20, "AND", "B", DF.None, "0---sz-p", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmb", "rb" })
            );
            _opToList[33] = new IL(0b00100001,
                new IE(0x21, "AND", "W", DF.None, "0---sz-p", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmw", "rw" })
            );
            _opToList[34] = new IL(0b00100010,
                new IE(0x22, "AND", "B", DF.None, "0---sz-p", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rb", "rmb" })
            );
            _opToList[35] = new IL(0b00100011,
                new IE(0x23, "AND", "W", DF.None, "0---sz-p", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rw", "rmw" })
            );
            _opToList[36] = new IL(0b00100100,
                new IE(0x24, "AND", "B", DF.None, "0---sz-p", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "al", "ib" })
            );
            _opToList[37] = new IL(0b00100101,
                new IE(0x25, "AND", "W", DF.None, "0---sz-p", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "ax", "iw" })
            );
            _opToList[39] = new IL(0b00100111,
                new IE(0x27, "DAA", "", DF.None, "----szap", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[40] = new IL(0b00101000,
                new IE(0x28, "SUB", "B", DF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmb", "rb" })
            );
            _opToList[41] = new IL(0b00101001,
                new IE(0x29, "SUB", "W", DF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmw", "rw" })
            );
            _opToList[42] = new IL(0b00101010,
                new IE(0x2A, "SUB", "B", DF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rb", "rmb" })
            );
            _opToList[43] = new IL(0b00101011,
                new IE(0x2B, "SUB", "W", DF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rw", "rmw" })
            );
            _opToList[44] = new IL(0b00101100,
                new IE(0x2C, "SUB", "B", DF.None, "o---szap", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "al", "ib" })
            );
            _opToList[45] = new IL(0b00101101,
                new IE(0x2D, "SUB", "W", DF.None, "o---szap", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "ax", "iw" })
            );
            _opToList[47] = new IL(0b00101111,
                new IE(0x2F, "DAS", "", DF.None, "----szap", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[48] = new IL(0b00110000,
                new IE(0x30, "XOR", "B", DF.None, "0---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmb", "rb" })
            );
            _opToList[49] = new IL(0b00110001,
                new IE(0x31, "XOR", "W", DF.None, "0---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmw", "rw" })
            );
            _opToList[50] = new IL(0b00110010,
                new IE(0x32, "XOR", "B", DF.None, "0---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rb", "rmb" })
            );
            _opToList[51] = new IL(0b00110011,
                new IE(0x33, "XOR", "W", DF.None, "0---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rw", "rmw" })
            );
            _opToList[52] = new IL(0b00110100,
                new IE(0x34, "XOR", "B", DF.None, "0---szap", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "al", "ib" })
            );
            _opToList[53] = new IL(0b00110101,
                new IE(0x35, "XOR", "W", DF.None, "0---szap", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "ax", "iw" })
            );
            _opToList[55] = new IL(0b00110111,
                new IE(0x37, "AAA", "", DF.None, "------a-", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[56] = new IL(0b00111000,
                new IE(0x38, "CMP", "B", DF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmb", "rb" })
            );
            _opToList[57] = new IL(0b00111001,
                new IE(0x39, "CMP", "W", DF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmw", "rw" })
            );
            _opToList[58] = new IL(0b00111010,
                new IE(0x3A, "CMP", "B", DF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rb", "rmb" })
            );
            _opToList[59] = new IL(0b00111011,
                new IE(0x3B, "CMP", "W", DF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rw", "rmw" })
            );
            _opToList[60] = new IL(0b00111100,
                new IE(0x3C, "CMP", "B", DF.None, "o---szap", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "al", "ib" })
            );
            _opToList[61] = new IL(0b00111101,
                new IE(0x3D, "CMP", "W", DF.None, "o---szap", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "ax", "iw" })
            );
            _opToList[63] = new IL(0b00111111,
                new IE(0x3F, "AAS", "", DF.None, "------a-", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[64] = new IL(0b01000000,
                new IE(0x40, "INC", "", DF.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ax" })
            );
            _opToList[65] = new IL(0b01000001,
                new IE(0x41, "INC", "", DF.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "cx" })
            );
            _opToList[66] = new IL(0b01000010,
                new IE(0x42, "INC", "", DF.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "dx" })
            );
            _opToList[67] = new IL(0b01000011,
                new IE(0x43, "INC", "", DF.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "bx" })
            );
            _opToList[68] = new IL(0b01000100,
                new IE(0x44, "INC", "", DF.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "sp" })
            );
            _opToList[69] = new IL(0b01000101,
                new IE(0x45, "INC", "", DF.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "bp" })
            );
            _opToList[70] = new IL(0b01000110,
                new IE(0x46, "INC", "", DF.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "si" })
            );
            _opToList[71] = new IL(0b01000111,
                new IE(0x47, "INC", "", DF.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "di" })
            );
            _opToList[72] = new IL(0b01001000,
                new IE(0x48, "DEC", "", DF.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ax" })
            );
            _opToList[73] = new IL(0b01001001,
                new IE(0x49, "DEC", "", DF.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "cx" }),
                new IE(0x49, "DEC", "", DF.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "dx" })
            );
            _opToList[74] = new IL(0b01001010,
                new IE(0x4A, "DEC", "", DF.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "bx" })
            );
            _opToList[75] = new IL(0b01001011,
                new IE(0x4B, "DEC", "", DF.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "sp" })
            );
            _opToList[76] = new IL(0b01001100,
                new IE(0x4C, "DEC", "", DF.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "bp" })
            );
            _opToList[77] = new IL(0b01001101,
                new IE(0x4D, "DEC", "", DF.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "si" })
            );
            _opToList[79] = new IL(0b01001111,
                new IE(0x4F, "DEC", "", DF.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "di" })
            );
            _opToList[80] = new IL(0b01010000,
                new IE(0x50, "PUSH", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ax" })
            );
            _opToList[81] = new IL(0b01010001,
                new IE(0x51, "PUSH", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "cx" })
            );
            _opToList[82] = new IL(0b01010010,
                new IE(0x52, "PUSH", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "dx" })
            );
            _opToList[83] = new IL(0b01010011,
                new IE(0x53, "PUSH", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "bx" })
            );
            _opToList[84] = new IL(0b01010100,
                new IE(0x54, "PUSH", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "sp" })
            );
            _opToList[85] = new IL(0b01010101,
                new IE(0x55, "PUSH", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "bp" })
            );
            _opToList[86] = new IL(0b01010110,
                new IE(0x56, "PUSH", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "si" })
            );
            _opToList[87] = new IL(0b01010111,
                new IE(0x57, "PUSH", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "di" })
            );
            _opToList[88] = new IL(0b01011000,
                new IE(0x58, "POP", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ax" })
            );
            _opToList[89] = new IL(0b01011001,
                new IE(0x59, "POP", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "cx" })
            );
            _opToList[90] = new IL(0b01011010,
                new IE(0x5A, "POP", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "dx" })
            );
            _opToList[91] = new IL(0b01011011,
                new IE(0x5B, "POP", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "bx" })
            );
            _opToList[92] = new IL(0b01011100,
                new IE(0x5C, "POP", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "sp" })
            );
            _opToList[93] = new IL(0b01011101,
                new IE(0x5D, "POP", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "bp" })
            );
            _opToList[94] = new IL(0b01011110,
                new IE(0x5E, "POP", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "si" })
            );
            _opToList[95] = new IL(0b01011111,
                new IE(0x5F, "POP", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "di" })
            );
            _opToList[105] = new IL(0b01101001,
                new IE(0x69, "IMUL", "W", DF.None, "o---szap", "8086", 6, 6, new Field[] { "mr", "i0", "i1", "i2", "i3" }, new Operand[] { "rd", "id" }),
                new IE(0x69, "IMUL", "W", DF.None, "o---szap", "8086", 6, 6, new Field[] { "mr", "d0", "d1", "i0~i3" }, new Operand[] { "rd", "rmd", "id" }),
                new IE(0x69, "IMUL", "B", DF.None, "o---szap", "8086", 4, 4, new Field[] { "mr", "i0", "i1" }, new Operand[] { "rw", "iw" }),
                new IE(0x69, "IMUL", "B", DF.None, "o---szap", "8086", 4, 6, new Field[] { "mr", "d0", "d1", "i0", "i1" }, new Operand[] { "rw", "rmw", "iw" })
            );
            _opToList[107] = new IL(0b01101011,
                new IE(0x6B, "IMUL", "W", DF.None, "o---szap", "8086", 3, 3, new Field[] { "mr", "i0" }, new Operand[] { "rd", "ib" }),
                new IE(0x6B, "IMUL", "W", DF.None, "o---szap", "8086", 3, 5, new Field[] { "mr", "d0", "d1", "i0" }, new Operand[] { "rd", "rmd", "ib" }),
                new IE(0x6B, "IMUL", "B", DF.None, "o---szap", "8086", 3, 3, new Field[] { "mr", "i0" }, new Operand[] { "rw", "ib" }),
                new IE(0x6B, "IMUL", "B", DF.None, "o---szap", "8086", 3, 5, new Field[] { "mr", "d0", "d1", "i0" }, new Operand[] { "rw", "rmw", "ib" })
            );
            _opToList[112] = new IL(0b01110000,
                new IE(0x70, "JO", "", DF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[113] = new IL(0b01110001,
                new IE(0x71, "JNO", "", DF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[114] = new IL(0b01110010,
                new IE(0x72, "JB", "", DF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" }),
                new IE(0x72, "JC", "", DF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[115] = new IL(0b01110011,
                new IE(0x73, "JAE", "", DF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" }),
                new IE(0x73, "JNC", "", DF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[116] = new IL(0b01110100,
                new IE(0x74, "JE", "", DF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[117] = new IL(0b01110101,
                new IE(0x75, "JNE", "", DF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[118] = new IL(0b01110110,
                new IE(0x76, "JBE", "", DF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[119] = new IL(0b01110111,
                new IE(0x77, "JA", "", DF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[120] = new IL(0b01111000,
                new IE(0x78, "JS", "", DF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[121] = new IL(0b01111001,
                new IE(0x79, "JNS", "", DF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[122] = new IL(0b01111010,
                new IE(0x7A, "JP", "", DF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[123] = new IL(0b01111011,
                new IE(0x7B, "JNP", "", DF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[124] = new IL(0b01111100,
                new IE(0x7C, "JL", "", DF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[125] = new IL(0b01111101,
                new IE(0x7D, "JGE", "", DF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[126] = new IL(0b01111110,
                new IE(0x7E, "JLE", "", DF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[127] = new IL(0b01111111,
                new IE(0x7F, "JG", "", DF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[128] = new IL(0b10000000,
                new IE(0x80, "ADC", "B", DF.None, "o---szap", "8086", 3, 5, new Field[] { "/2", "d0", "d1", "i0" }, new Operand[] { "rmb", "ib" }),
                new IE(0x80, "ADD", "B", DF.None, "o---szap", "8086", 3, 5, new Field[] { "/0", "d0", "d1", "i0" }, new Operand[] { "rmb", "ib" }),
                new IE(0x80, "AND", "B", DF.None, "0---sz-p", "8086", 3, 5, new Field[] { "/4", "d0", "d1", "i0" }, new Operand[] { "rmb", "ib" }),
                new IE(0x80, "CMP", "B", DF.None, "o---szap", "8086", 3, 5, new Field[] { "/7", "d0", "d1", "i0" }, new Operand[] { "rmb", "ib" }),
                new IE(0x80, "OR", "B", DF.None, "o---szap", "8086", 3, 5, new Field[] { "/1", "d0", "d1", "i0" }, new Operand[] { "rmb", "ib" }),
                new IE(0x80, "SBB", "B", DF.None, "o---szap", "8086", 3, 5, new Field[] { "/3", "d0", "d1", "i0" }, new Operand[] { "rmb", "ib" }),
                new IE(0x80, "SUB", "B", DF.None, "o---szap", "8086", 3, 5, new Field[] { "/5", "d0", "d1", "i0" }, new Operand[] { "rmb", "ib" }),
                new IE(0x80, "XOR", "B", DF.None, "0---szap", "8086", 3, 5, new Field[] { "/6", "d0", "d1", "i0" }, new Operand[] { "rmb", "ib" })
            );
            _opToList[129] = new IL(0b10000001,
                new IE(0x81, "ADC", "W", DF.None, "o---szap", "8086", 4, 6, new Field[] { "/2", "d0", "d1", "i0", "i1" }, new Operand[] { "rmw", "iw" }),
                new IE(0x81, "ADD", "W", DF.None, "o---szap", "8086", 4, 6, new Field[] { "/0", "d0", "d1", "i0", "i1" }, new Operand[] { "rmw", "iw" }),
                new IE(0x81, "AND", "W", DF.None, "0---sz-p", "8086", 4, 6, new Field[] { "/4", "d0", "d1", "i0", "i1" }, new Operand[] { "rmw", "iw" }),
                new IE(0x81, "CMP", "W", DF.None, "o---szap", "8086", 4, 6, new Field[] { "/7", "d0", "d1", "i0", "i1" }, new Operand[] { "rmw", "iw" }),
                new IE(0x81, "OR", "W", DF.None, "o---szap", "8086", 4, 6, new Field[] { "/1", "d0", "d1", "i0", "i1" }, new Operand[] { "rmw", "iw" }),
                new IE(0x81, "SBB", "W", DF.None, "o---szap", "8086", 4, 6, new Field[] { "/3", "d0", "d1", "i0", "i1" }, new Operand[] { "rmw", "iw" }),
                new IE(0x81, "SUB", "W", DF.None, "o---szap", "8086", 4, 6, new Field[] { "/5", "d0", "d1", "i0", "i1" }, new Operand[] { "rmw", "iw" }),
                new IE(0x81, "XOR", "W", DF.None, "0---szap", "8086", 4, 6, new Field[] { "/6", "d0", "d1", "i0", "i1" }, new Operand[] { "rmw", "iw" })
            );
            _opToList[131] = new IL(0b10000011,
                new IE(0x83, "ADC", "W", DF.SignExtendedImm8, "o---szap", "8086", 3, 5, new Field[] { "/2", "d0", "d1", "i0" }, new Operand[] { "rmw", "ib" }),
                new IE(0x83, "ADD", "W", DF.SignExtendedImm8, "o---szap", "8086", 3, 5, new Field[] { "/0", "d0", "d1", "i0" }, new Operand[] { "rmw", "ib" }),
                new IE(0x83, "AND", "W", DF.SignExtendedImm8, "0---sz-p", "8086", 3, 5, new Field[] { "/4", "d0", "d1", "i0" }, new Operand[] { "rmw", "ib" }),
                new IE(0x83, "CMP", "W", DF.SignExtendedImm8, "o---szap", "8086", 3, 5, new Field[] { "/7", "d0", "d1", "i0" }, new Operand[] { "rmw", "ib" }),
                new IE(0x83, "OR", "W", DF.SignExtendedImm8, "o---szap", "8086", 3, 5, new Field[] { "/1", "d0", "d1", "i0" }, new Operand[] { "rmw", "ib" }),
                new IE(0x83, "SBB", "W", DF.SignExtendedImm8, "o---szap", "8086", 3, 5, new Field[] { "/3", "d0", "d1", "i0" }, new Operand[] { "rmw", "ib" }),
                new IE(0x83, "SUB", "W", DF.SignExtendedImm8, "o---szap", "8086", 3, 5, new Field[] { "/5", "d0", "d1", "i0" }, new Operand[] { "rmw", "ib" }),
                new IE(0x83, "XOR", "W", DF.SignExtendedImm8, "0---szap", "8086", 3, 5, new Field[] { "/6", "d0", "d1", "i0" }, new Operand[] { "rmw", "ib" })
            );
            _opToList[132] = new IL(0b10000100,
                new IE(0x84, "TEST", "B", DF.None, "0---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmb", "rmb" })
            );
            _opToList[133] = new IL(0b10000101,
                new IE(0x85, "TEST", "W", DF.None, "0---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmw", "rmw" })
            );
            _opToList[134] = new IL(0b10000110,
                new IE(0x86, "XCHG", "B", DF.None, "--------", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rb", "rmb" }),
                new IE(0x86, "XCHG", "B", DF.None, "--------", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmb", "rb" })
            );
            _opToList[135] = new IL(0b10000111,
                new IE(0x87, "XCHG", "W", DF.None, "--------", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmw", "rw" }),
                new IE(0x87, "XCHG", "W", DF.None, "--------", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rw", "rmw" })
            );
            _opToList[136] = new IL(0b10001000,
                new IE(0x88, "MOV", "B", DF.None, "--------", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmb", "rb" })
            );
            _opToList[137] = new IL(0b10001001,
                new IE(0x89, "MOV", "W", DF.None, "--------", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmw", "rw" })
            );
            _opToList[138] = new IL(0b10001010,
                new IE(0x8A, "MOV", "B", DF.None, "--------", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rb", "rmb" })
            );
            _opToList[139] = new IL(0b10001011,
                new IE(0x8B, "MOV", "W", DF.None, "--------", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rw", "rmw" })
            );
            _opToList[140] = new IL(0b10001100,
                new IE(0x8C, "MOV", "", DF.None, "--------", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmw", "sr" })
            );
            _opToList[141] = new IL(0b10001101,
                new IE(0x8D, "LEA", "", DF.None, "--------", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rw", "mw" })
            );
            _opToList[142] = new IL(0b10001110,
                new IE(0x8E, "MOV", "", DF.None, "--------", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "sr", "rmw" })
            );
            _opToList[143] = new IL(0b10001111,
                new IE(0x8F, "POP", "", DF.None, "--------", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmw" })
            );
            _opToList[144] = new IL(0b10010000,
                new IE(0x90, "NOP", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[145] = new IL(0b10010001,
                new IE(0x91, "XCHG", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ax", "cx" })
            );
            _opToList[146] = new IL(0b10010010,
                new IE(0x92, "XCHG", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ax", "dx" })
            );
            _opToList[147] = new IL(0b10010011,
                new IE(0x93, "XCHG", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ax", "bx" })
            );
            _opToList[148] = new IL(0b10010100,
                new IE(0x94, "XCHG", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ax", "sp" })
            );
            _opToList[149] = new IL(0b10010101,
                new IE(0x95, "XCHG", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ax", "bp" })
            );
            _opToList[150] = new IL(0b10010110,
                new IE(0x96, "XCHG", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ax", "si" })
            );
            _opToList[151] = new IL(0b10010111,
                new IE(0x97, "XCHG", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ax", "di" })
            );
            _opToList[152] = new IL(0b10011000,
                new IE(0x98, "CBW", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[153] = new IL(0b10011001,
                new IE(0x99, "CWD", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[154] = new IL(0b10011010,
                new IE(0x9A, "CALL", "", DF.None, "--------", "8086", 5, 5, new Field[] { "o0", "o1", "sl", "sh" }, new Operand[] { "(far ptr)fp" })
            );
            _opToList[155] = new IL(0b10011011,
                new IE(0x9B, "WAIT", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[156] = new IL(0b10011100,
                new IE(0x9C, "PUSHF", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[157] = new IL(0b10011101,
                new IE(0x9D, "POPF", "", DF.None, "oditszap", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[158] = new IL(0b10011110,
                new IE(0x9E, "SAHF", "", DF.None, "----szap", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[159] = new IL(0b10011111,
                new IE(0x9F, "LAHF", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[160] = new IL(0b10100000,
                new IE(0xA0, "MOV", "B", DF.None, "--------", "8086", 3, 3, new Field[] { "d0", "d1" }, new Operand[] { "al", "rmb" })
            );
            _opToList[161] = new IL(0b10100001,
                new IE(0xA1, "MOV", "W", DF.None, "--------", "8086", 3, 3, new Field[] { "d0", "d1" }, new Operand[] { "ax", "rmw" })
            );
            _opToList[162] = new IL(0b10100010,
                new IE(0xA2, "MOV", "B", DF.None, "--------", "8086", 3, 3, new Field[] { "d0", "d1" }, new Operand[] { "rmb", "al" })
            );
            _opToList[163] = new IL(0b10100011,
                new IE(0xA3, "MOV", "W", DF.None, "--------", "8086", 3, 3, new Field[] { "d0", "d1" }, new Operand[] { "rmw", "ax" })
            );
            _opToList[164] = new IL(0b10100100,
                new IE(0xA4, "MOVSB", "B", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[165] = new IL(0b10100101,
                new IE(0xA5, "MOVSW", "W", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[166] = new IL(0b10100110,
                new IE(0xA6, "CMPSB", "B", DF.None, "od--szap", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[167] = new IL(0b10100111,
                new IE(0xA7, "CMPSW", "W", DF.None, "od--szap", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[168] = new IL(0b10101000,
                new IE(0xA8, "TEST", "B", DF.None, "0---szap", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "al", "ib" })
            );
            _opToList[169] = new IL(0b10101001,
                new IE(0xA9, "TEST", "W", DF.None, "0---szap", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "ax", "iw" })
            );
            _opToList[170] = new IL(0b10101010,
                new IE(0xAA, "STOSB", "B", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[171] = new IL(0b10101011,
                new IE(0xAB, "STOSW", "W", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[172] = new IL(0b10101100,
                new IE(0xAC, "LODSB", "B", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[173] = new IL(0b10101101,
                new IE(0xAD, "LODSW", "W", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[174] = new IL(0b10101110,
                new IE(0xAE, "SCASB", "B", DF.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[175] = new IL(0b10101111,
                new IE(0xAF, "SCASW", "W", DF.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[176] = new IL(0b10110000,
                new IE(0xB0, "MOV", "B", DF.None, "--------", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "al", "ib" })
            );
            _opToList[177] = new IL(0b10110001,
                new IE(0xB1, "MOV", "B", DF.None, "--------", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "cl", "ib" })
            );
            _opToList[178] = new IL(0b10110010,
                new IE(0xB2, "MOV", "B", DF.None, "--------", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "dl", "ib" })
            );
            _opToList[179] = new IL(0b10110011,
                new IE(0xB3, "MOV", "B", DF.None, "--------", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "bl", "ib" })
            );
            _opToList[180] = new IL(0b10110100,
                new IE(0xB4, "MOV", "B", DF.None, "--------", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "ah", "ib" })
            );
            _opToList[181] = new IL(0b10110101,
                new IE(0xB5, "MOV", "B", DF.None, "--------", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "ch", "ib" })
            );
            _opToList[182] = new IL(0b10110110,
                new IE(0xB6, "MOV", "B", DF.None, "--------", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "dh", "ib" })
            );
            _opToList[183] = new IL(0b10110111,
                new IE(0xB7, "MOV", "B", DF.None, "--------", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "bh", "ib" })
            );
            _opToList[184] = new IL(0b10111000,
                new IE(0xB8, "MOV", "W", DF.None, "--------", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "ax", "iw" })
            );
            _opToList[185] = new IL(0b10111001,
                new IE(0xB9, "MOV", "W", DF.None, "--------", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "cx", "iw" })
            );
            _opToList[186] = new IL(0b10111010,
                new IE(0xBA, "MOV", "W", DF.None, "--------", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "dx", "iw" })
            );
            _opToList[187] = new IL(0b10111011,
                new IE(0xBB, "MOV", "W", DF.None, "--------", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "bx", "iw" })
            );
            _opToList[188] = new IL(0b10111100,
                new IE(0xBC, "MOV", "W", DF.None, "--------", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "sp", "iw" })
            );
            _opToList[189] = new IL(0b10111101,
                new IE(0xBD, "MOV", "W", DF.None, "--------", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "bp", "iw" })
            );
            _opToList[190] = new IL(0b10111110,
                new IE(0xBE, "MOV", "W", DF.None, "--------", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "si", "iw" })
            );
            _opToList[191] = new IL(0b10111111,
                new IE(0xBF, "MOV", "W", DF.None, "--------", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "di", "iw" })
            );
            _opToList[194] = new IL(0b11000010,
                new IE(0xC2, "RET", "", DF.None, "--------", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "iw" })
            );
            _opToList[195] = new IL(0b11000011,
                new IE(0xC3, "RET", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[196] = new IL(0b11000100,
                new IE(0xC4, "LES", "", DF.None, "--------", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rw", "md" })
            );
            _opToList[197] = new IL(0b11000101,
                new IE(0xC5, "LDS", "", DF.None, "--------", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rw", "md" })
            );
            _opToList[198] = new IL(0b11000110,
                new IE(0xC6, "MOV", "B", DF.None, "--------", "8086", 3, 5, new Field[] { "mr", "d0", "d1", "i0" }, new Operand[] { "rmb", "ib" })
            );
            _opToList[199] = new IL(0b11000111,
                new IE(0xC7, "MOV", "W", DF.None, "--------", "8086", 4, 6, new Field[] { "mr", "d0", "d1", "i0", "i1" }, new Operand[] { "rmw", "iw" })
            );
            _opToList[202] = new IL(0b11001010,
                new IE(0xCA, "RETF", "", DF.None, "--------", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "iw" })
            );
            _opToList[203] = new IL(0b11001011,
                new IE(0xCB, "RETF", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[204] = new IL(0b11001100,
                new IE(0xCC, "INT", "", DF.None, "--00----", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "3" })
            );
            _opToList[205] = new IL(0b11001101,
                new IE(0xCD, "INT", "", DF.None, "--00----", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "ib" })
            );
            _opToList[206] = new IL(0b11001110,
                new IE(0xCE, "INTO", "", DF.None, "--00----", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[207] = new IL(0b11001111,
                new IE(0xCF, "IRET", "", DF.None, "oditszap", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[208] = new IL(0b11010000,
                new IE(0xD0, "RCL", "B", DF.None, "o-------", "8086", 2, 4, new Field[] { "/2", "d0", "d1" }, new Operand[] { "rmb", "1" }),
                new IE(0xD0, "RCR", "B", DF.None, "o-------", "8086", 2, 4, new Field[] { "/3", "d0", "d1" }, new Operand[] { "rmb", "1" }),
                new IE(0xD0, "ROL", "B", DF.None, "o-------", "8086", 2, 4, new Field[] { "/0", "d0", "d1" }, new Operand[] { "rmb", "1" }),
                new IE(0xD0, "ROR", "B", DF.None, "o-------", "8086", 2, 4, new Field[] { "/1", "d0", "d1" }, new Operand[] { "rmb", "1" }),
                new IE(0xD0, "SAL", "B", DF.None, "o-------", "8086", 2, 4, new Field[] { "/4", "d0", "d1" }, new Operand[] { "rmb", "1" }),
                new IE(0xD0, "SHL", "B", DF.None, "o-------", "8086", 2, 4, new Field[] { "/4", "d0", "d1" }, new Operand[] { "rmb", "1" }),
                new IE(0xD0, "SHR", "B", DF.None, "o-------", "8086", 2, 4, new Field[] { "/5", "d0", "d1" }, new Operand[] { "rmb", "1" })
            );
            _opToList[209] = new IL(0b11010001,
                new IE(0xD1, "RCL", "W", DF.None, "o-------", "8086", 2, 4, new Field[] { "/2", "d0", "d1" }, new Operand[] { "rmw", "1" }),
                new IE(0xD1, "RCR", "W", DF.None, "o-------", "8086", 2, 4, new Field[] { "/3", "d0", "d1" }, new Operand[] { "rmw", "1" }),
                new IE(0xD1, "ROL", "W", DF.None, "o-------", "8086", 2, 4, new Field[] { "/0", "d0", "d1" }, new Operand[] { "rmw", "1" }),
                new IE(0xD1, "ROR", "W", DF.None, "o-------", "8086", 2, 4, new Field[] { "/1", "d0", "d1" }, new Operand[] { "rmw", "1" }),
                new IE(0xD1, "SAL", "W", DF.None, "o-------", "8086", 2, 4, new Field[] { "/4", "d0", "d1" }, new Operand[] { "rmw", "1" }),
                new IE(0xD1, "SHL", "W", DF.None, "o-------", "8086", 2, 4, new Field[] { "/4", "d0", "d1" }, new Operand[] { "rmw", "1" }),
                new IE(0xD1, "SAR", "W", DF.None, "o-------", "8086", 2, 4, new Field[] { "/7", "d0", "d1" }, new Operand[] { "rmw", "1" }),
                new IE(0xD1, "SHR", "W", DF.None, "o-------", "8086", 2, 4, new Field[] { "/5", "d0", "d1" }, new Operand[] { "rmw", "1" })
            );
            _opToList[210] = new IL(0b11010010,
                new IE(0xD2, "RCL", "B", DF.None, "o-------", "8086", 2, 4, new Field[] { "/2", "d0", "d1" }, new Operand[] { "rmb", "cl" }),
                new IE(0xD2, "RCR", "B", DF.None, "o-------", "8086", 2, 4, new Field[] { "/3", "d0", "d1" }, new Operand[] { "rmb", "cl" }),
                new IE(0xD2, "ROL", "B", DF.None, "o-------", "8086", 2, 4, new Field[] { "/0", "d0", "d1" }, new Operand[] { "rmb", "cl" }),
                new IE(0xD2, "ROR", "B", DF.None, "o-------", "8086", 2, 4, new Field[] { "/1", "d0", "d1" }, new Operand[] { "rmb", "cl" }),
                new IE(0xD2, "SAL", "B", DF.None, "o-------", "8086", 2, 4, new Field[] { "/4", "d0", "d1" }, new Operand[] { "rmb", "cl" }),
                new IE(0xD2, "SHL", "B", DF.None, "o-------", "8086", 2, 4, new Field[] { "/4", "d0", "d1" }, new Operand[] { "rmb", "cl" }),
                new IE(0xD2, "SAR", "B", DF.None, "o-------", "8086", 2, 4, new Field[] { "/7", "d0", "d1" }, new Operand[] { "rmb", "cl" }),
                new IE(0xD2, "SHR", "B", DF.None, "o-------", "8086", 2, 4, new Field[] { "/5", "d0", "d1" }, new Operand[] { "rmb", "cl" })
            );
            _opToList[211] = new IL(0b11010011,
                new IE(0xD3, "RCL", "W", DF.None, "o-------", "8086", 2, 4, new Field[] { "/2", "d0", "d1" }, new Operand[] { "rmw", "cl" }),
                new IE(0xD3, "RCR", "W", DF.None, "o-------", "8086", 2, 4, new Field[] { "/3", "d0", "d1" }, new Operand[] { "rmw", "cl" }),
                new IE(0xD3, "ROL", "W", DF.None, "o-------", "8086", 2, 4, new Field[] { "/0", "d0", "d1" }, new Operand[] { "rmw", "cl" }),
                new IE(0xD3, "ROR", "W", DF.None, "o-------", "8086", 2, 4, new Field[] { "/1", "d0", "d1" }, new Operand[] { "rmw", "cl" }),
                new IE(0xD3, "SAL", "W", DF.None, "o-------", "8086", 2, 4, new Field[] { "/4", "d0", "d1" }, new Operand[] { "rmw", "cl" }),
                new IE(0xD3, "SHL", "W", DF.None, "o-------", "8086", 2, 4, new Field[] { "/4", "d0", "d1" }, new Operand[] { "rmw", "cl" }),
                new IE(0xD3, "SAR", "W", DF.None, "o-------", "8086", 2, 4, new Field[] { "/7", "d0", "d1" }, new Operand[] { "rmw", "cl" }),
                new IE(0xD3, "SHR", "W", DF.None, "o-------", "8086", 2, 4, new Field[] { "/5", "d0", "d1" }, new Operand[] { "rmw", "cl" })
            );
            _opToList[212] = new IL(0b11010100,
                new IE(0xD4, "AAM", "", DF.None, "----sz-p", "8086", 2, 2, new Field[] { "0A" }, Array.Empty<Operand>())
            );
            _opToList[213] = new IL(0b11010101,
                new IE(0xD5, "AAD", "", DF.None, "----sz-p", "8086", 2, 2, new Field[] { "0A" }, Array.Empty<Operand>())
            );
            _opToList[215] = new IL(0b11010111,
                new IE(0xD7, "XLAT", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[224] = new IL(0b11100000,
                new IE(0xE0, "LOOPNZ", "", DF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[225] = new IL(0b11100001,
                new IE(0xE1, "LOOPE", "", DF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[226] = new IL(0b11100010,
                new IE(0xE2, "LOOP", "", DF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[227] = new IL(0b11100011,
                new IE(0xE3, "JCXZ", "", DF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[228] = new IL(0b11100100,
                new IE(0xE4, "IN", "B", DF.None, "--------", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "al", "ib" })
            );
            _opToList[229] = new IL(0b11100101,
                new IE(0xE5, "IN", "W", DF.None, "--------", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "ax", "ib" })
            );
            _opToList[230] = new IL(0b11100110,
                new IE(0xE6, "OUT", "B", DF.None, "--------", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "ib", "al" })
            );
            _opToList[231] = new IL(0b11100111,
                new IE(0xE7, "OUT", "W", DF.None, "--------", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "ib", "ax" })
            );
            _opToList[232] = new IL(0b11101000,
                new IE(0xE8, "CALL", "", DF.None, "--------", "8086", 3, 3, new Field[] { "o0", "o1" }, new Operand[] { "np" })
            );
            _opToList[233] = new IL(0b11101001,
                new IE(0xE9, "JMP", "", DF.None, "--------", "8086", 3, 3, new Field[] { "o0", "o1" }, new Operand[] { "np" })
            );
            _opToList[234] = new IL(0b11101010,
                new IE(0xEA, "JMP", "", DF.None, "--------", "8086", 5, 5, new Field[] { "o0", "o1", "s0", "s1" }, new Operand[] { "(far ptr)fp" })
            );
            _opToList[235] = new IL(0b11101011,
                new IE(0xEB, "JMP", "", DF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "(short)sl" })
            );
            _opToList[236] = new IL(0b11101100,
                new IE(0xEC, "IN", "B", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "al", "dx" })
            );
            _opToList[237] = new IL(0b11101101,
                new IE(0xED, "IN", "W", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ax", "dx" })
            );
            _opToList[238] = new IL(0b11101110,
                new IE(0xEE, "OUT", "B", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "dx", "al" })
            );
            _opToList[239] = new IL(0b11101111,
                new IE(0xEF, "OUT", "W", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "dx", "ax" })
            );
            _opToList[240] = new IL(0b11110000,
                new IE(0xF0, "LOCK", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[242] = new IL(0b11110010,
                new IE(0xF2, "REPNE", "", DF.None, "-----z--", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[243] = new IL(0b11110011,
                new IE(0xF3, "REP", "", DF.None, "-----z--", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>()),
                new IE(0xF3, "REPE", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[244] = new IL(0b11110100,
                new IE(0xF4, "HLT", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[245] = new IL(0b11110101,
                new IE(0xF5, "CMC", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[246] = new IL(0b11110110,
                new IE(0xF6, "DIV", "B", DF.None, "o---szap", "8086", 2, 4, new Field[] { "/6", "d0", "d1" }, new Operand[] { "rmb" }),
                new IE(0xF6, "IDIV", "B", DF.None, "o---szap", "8086", 2, 4, new Field[] { "/7", "d0", "d1" }, new Operand[] { "rmb" }),
                new IE(0xF6, "IMUL", "B", DF.None, "o---szap", "8086", 2, 4, new Field[] { "/5", "d0", "d1" }, new Operand[] { "rmb" }),
                new IE(0xF6, "MUL", "B", DF.None, "o---szap", "8086", 2, 4, new Field[] { "/4", "d0", "d1" }, new Operand[] { "rmb" }),
                new IE(0xF6, "NEG", "B", DF.None, "o---szap", "8086", 2, 4, new Field[] { "/3", "d0", "d1" }, new Operand[] { "rmb" }),
                new IE(0xF6, "NOT", "B", DF.None, "--------", "8086", 2, 4, new Field[] { "/2", "d0", "d1" }, new Operand[] { "rmb" }),
                new IE(0xF6, "TEST", "B", DF.None, "0---szap", "8086", 3, 5, new Field[] { "/0", "d0", "d1", "i0" }, new Operand[] { "rmb", "ib" })
            );
            _opToList[247] = new IL(0b11110111,
                new IE(0xF7, "DIV", "W", DF.None, "o---szap", "8086", 2, 4, new Field[] { "/6", "d0", "d1" }, new Operand[] { "rmw" }),
                new IE(0xF7, "IDIV", "W", DF.None, "o---szap", "8086", 2, 4, new Field[] { "/7", "d0", "d1" }, new Operand[] { "rmw" }),
                new IE(0xF7, "IMUL", "W", DF.None, "o---szap", "8086", 2, 4, new Field[] { "/5", "d0", "d1" }, new Operand[] { "rmw" }),
                new IE(0xF7, "MUL", "W", DF.None, "o---szap", "8086", 2, 4, new Field[] { "/4", "d0", "d1" }, new Operand[] { "rmw" }),
                new IE(0xF7, "NEG", "W", DF.None, "o---szap", "8086", 2, 4, new Field[] { "/3", "d0", "d1" }, new Operand[] { "rmw" }),
                new IE(0xF7, "NOT", "W", DF.None, "--------", "8086", 2, 4, new Field[] { "/2", "d0", "d1" }, new Operand[] { "rmw" }),
                new IE(0xF7, "TEST", "W", DF.None, "0---szap", "8086", 4, 6, new Field[] { "/0", "d0", "d1", "i0", "i1" }, new Operand[] { "rmw", "iw" })
            );
            _opToList[248] = new IL(0b11111000,
                new IE(0xF8, "CLC", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[249] = new IL(0b11111001,
                new IE(0xF9, "STC", "", DF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[250] = new IL(0b11111010,
                new IE(0xFA, "CLI", "", DF.None, "--0-----", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[251] = new IL(0b11111011,
                new IE(0xFB, "STI", "", DF.None, "--1-----", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[252] = new IL(0b11111100,
                new IE(0xFC, "CLD", "", DF.None, "-0------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[253] = new IL(0b11111101,
                new IE(0xFD, "STD", "", DF.None, "-1------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[254] = new IL(0b11111110,
                new IE(0xFE, "DEC", "", DF.None, "o---szap", "8086", 2, 4, new Field[] { "/1", "d0", "d1" }, new Operand[] { "rmb" }),
                new IE(0xFE, "INC", "", DF.None, "o---szap", "8086", 2, 4, new Field[] { "/0", "d0", "d1" }, new Operand[] { "rmb" })
            );
            _opToList[255] = new IL(0b11111111,
                new IE(0xFF, "CALL", "W", DF.None, "--------", "8086", 2, 4, new Field[] { "/2", "d0", "d1" }, new Operand[] { "rw" }),
                new IE(0xFF, "CALL", "W", DF.None, "--------", "8086", 2, 4, new Field[] { "/3", "d0", "d1" }, new Operand[] { "(dword ptr)rw" }),
                new IE(0xFF, "DEC", "", DF.None, "o---szap", "8086", 2, 4, new Field[] { "/1", "d0", "d1" }, new Operand[] { "rmw" }),
                new IE(0xFF, "INC", "", DF.None, "o---szap", "8086", 2, 4, new Field[] { "/0", "d0", "d1" }, new Operand[] { "rmw" }),
                new IE(0xFF, "JMP", "", DF.None, "--------", "8086", 2, 4, new Field[] { "/4", "d0", "d1" }, new Operand[] { "rmw" }),
                new IE(0xFF, "JMP", "", DF.None, "--------", "8086", 2, 4, new Field[] { "/5", "d0", "d1" }, new Operand[] { "(dword ptr)rmw" }),
                new IE(0xFF, "PUSH", "", DF.None, "--------", "8086", 2, 4, new Field[] { "/6", "d0", "d1" }, new Operand[] { "rmw" })
            );
        }

        public int Count => _opToList.Length;

        public InstructionList GetOrCreate(byte op)
        {
            Debug.Assert(op < _opToList.Length);
            if (_opToList[op] == null)
                _opToList[op] = new InstructionList(op);
            return _opToList[op];
        }

        public IEnumerator<InstructionList> GetEnumerator() => _opToList.Cast<InstructionList>().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class InstructionTable
    {
        private readonly InstructionDefinition[] _table;

        public ref readonly InstructionDefinition this[int index] => ref _table[index];

        public InstructionTable()
        {
            _table = new InstructionDefinition[256];

            // Instruction Encoding (8086 Family Users Manual v1)
            // We dont want to test for invidiual bits, we rather want to map the full opcode directly

            //
            // Register Field Encoding (Table 4-9, Page: 162)
            //
            // |  REG  | W = 0 | W = 1 |
            // | 0 0 0 |  AL   | AX    |
            // | 0 0 1 |  CL   | CX    |
            // | 0 1 0 |  DL   | DX    |
            // | 0 1 1 |  BL   | BX    |
            // | 1 0 0 |  AH   | SP    |
            // | 1 0 1 |  CH   | BP    |
            // | 1 1 0 |  DH   | SI    |
            // | 1 1 1 |  BH   | DI    |

            //
            // Move instructions (Table 4-12, Page: 164 or 174-175)
            //
            // Bug(Page 174) $A3: MOV16, AL is wrong, use MOV16, AX instead
            //

            // 0000 0000 to 0000 1111 (ADD/OR, PUSH/POP ES/CS)
            _table[0x00 /* 0000 0000 */] = new InstructionDefinition(OpCode.ADD_dREG8_dMEM8_sREG8, OpFamily.Add8_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.ArithmeticAdd, "Adds 8-bit Register to 8-bit Register/Memory");
            _table[0x01 /* 0000 0001 */] = new InstructionDefinition(OpCode.ADD_dREG16_dMEM16_sREG16, OpFamily.Add16_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.ArithmeticAdd, "Adds 16-bit Register to 16-bit Register/Memory");
            _table[0x02 /* 0000 0010 */] = new InstructionDefinition(OpCode.ADD_dREG8_sREG8_sMEM8, OpFamily.Add8_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.ArithmeticAdd, "Adds 8-bit Register/Memory to 8-bit Register");
            _table[0x03 /* 0000 0011 */] = new InstructionDefinition(OpCode.ADD_dREG16_sREG16_sMEM16, OpFamily.Add16_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.ArithmeticAdd, "Adds 16-bit Register/Memory to 16-bit Register");
            _table[0x04 /* 0000 0100 */] = new InstructionDefinition(OpCode.ADD_dAL_sIMM8, OpFamily.Add8_FixedReg_Imm, FieldEncoding.None, RegisterType.AL, 2, Mnemonics.ArithmeticAdd, "Adds 8-bit Immediate to 8-bit " + RegisterType.AL + " Register");
            _table[0x05 /* 0000 0101 */] = new InstructionDefinition(OpCode.ADD_dAX_sIMM16, OpFamily.Add16_FixedReg_Imm, FieldEncoding.None, RegisterType.AX, 3, Mnemonics.ArithmeticAdd, "Adds 16-bit Immediate to 16-bit " + RegisterType.AX + " Register");
            _table[0x06 /* 0000 0110 */] = new InstructionDefinition(OpCode.PUSH_ES, OpFamily.Push_FixedReg, FieldEncoding.None, RegisterType.ES, 1, Mnemonics.Push, "Decrements the 16-bit " + RegisterType.SP + " by the amount of the " + RegisterType.ES + " Register");
            _table[0x07 /* 0000 0111 */] = new InstructionDefinition(OpCode.POP_ES, OpFamily.Pop_FixedReg, FieldEncoding.None, RegisterType.ES, 1, Mnemonics.Pop, "Increments the 16-bit " + RegisterType.SP + " by the amount of the " + RegisterType.ES + " Register");
            _table[0x08 /* 0000 1000 */] = new InstructionDefinition(OpCode.OR_dREG8_dMEM8_sREG8, OpFamily.Or8_RegOrMem_RegOrMem, FieldEncoding.None, 2, 4, Mnemonics.LogicalOr, "Logical OR 8-bit Register with 8-bit Register/Memory");
            _table[0x09 /* 0000 1001 */] = new InstructionDefinition(OpCode.OR_dREG16_dMEM16_sREG16, OpFamily.Or8_RegOrMem_RegOrMem, FieldEncoding.None, 2, 4, Mnemonics.LogicalOr, "Logical OR 16-bit Register with 16-bit Register/Memory");
            _table[0x0A /* 0000 1010 */] = new InstructionDefinition(OpCode.OR_dREG8_sREG8_sMEM8, OpFamily.Or8_RegOrMem_RegOrMem, FieldEncoding.None, 2, 4, Mnemonics.LogicalOr, "Logical OR 8-bit Register/Memory with 8-bit Register");
            _table[0x0B /* 0000 1011 */] = new InstructionDefinition(OpCode.OR_dREG16_sREG16_sMEM16, OpFamily.Or16_RegOrMem_RegOrMem, FieldEncoding.None, 2, 4, Mnemonics.LogicalOr, "Logical OR 16-bit Register/Memory with 16-bit Register");
            _table[0x0C /* 0000 1100 */] = new InstructionDefinition(OpCode.OR_dAL_sIMM8, OpFamily.Or8_FixedReg_Imm, FieldEncoding.None, RegisterType.AL, 2, Mnemonics.LogicalOr, "Logical OR 8-bit Immediate with 8-bit " + RegisterType.AL + " Register");
            _table[0x0D /* 0000 1101 */] = new InstructionDefinition(OpCode.OR_dAX_sIMM16, OpFamily.Or16_FixedReg_Imm, FieldEncoding.None, RegisterType.AX, 3, Mnemonics.LogicalOr, "Logical OR 16-bit Immediate with 16-bit " + RegisterType.AX + " Register");
            _table[0x0E /* 0000 1110 */] = new InstructionDefinition(OpCode.PUSH_CS, OpFamily.Push_FixedReg, FieldEncoding.None, RegisterType.CS, 1, Mnemonics.LogicalOr, "Decrements the 16-bit " + RegisterType.SP + " by the amount of the " + RegisterType.CS + " Register");
            _table[0x0F /* 0000 1111 */] = null; // Unused

            // 0001 0000 to 0001 1111 (ADC/SBB, PUSH/POP SS/DS)
            _table[0x10 /* 0001 0000 */] = new InstructionDefinition(OpCode.ADC_dREG8_dMEM8_sREG8, OpFamily.Adc8_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.ArithmeticAddWithCarry, "Add with Carry 8-bit Register to 8-bit Register/Memory");
            _table[0x11 /* 0001 0001 */] = new InstructionDefinition(OpCode.ADC_dREG16_dMEM16_sREG16, OpFamily.Adc16_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.ArithmeticAddWithCarry, "Add with Carry 16-bit Register to 16-bit Register/Memory");
            _table[0x12 /* 0001 0010 */] = new InstructionDefinition(OpCode.ADC_dREG8_sREG8_MEM8, OpFamily.Adc8_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.ArithmeticAddWithCarry, "Add with Carry 8-bit Register/Memory to 8-bit Register");
            _table[0x13 /* 0001 0011 */] = new InstructionDefinition(OpCode.ADC_dREG16_sREG16_MEM16, OpFamily.Adc16_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.ArithmeticAddWithCarry, "Add with Carry 16-bit Register/Memory to 16-bit Register");
            _table[0x14 /* 0001 0100 */] = new InstructionDefinition(OpCode.ADC_dAL_sIMM8, OpFamily.Adc8_FixedReg_Imm, FieldEncoding.None, 2, Mnemonics.ArithmeticAddWithCarry, "Add with Carry 8-bit Immediate to 8-bit " + RegisterType.AL + " Register");
            _table[0x15 /* 0001 0101 */] = new InstructionDefinition(OpCode.ADC_dAX_sIMM16, OpFamily.Adc16_FixedReg_Imm, FieldEncoding.None, 3, Mnemonics.ArithmeticAddWithCarry, "Add with Carry 16-bit Immediate to 16-bit " + RegisterType.AX + " Register");
            _table[0x16 /* 0001 0110 */] = new InstructionDefinition(OpCode.PUSH_SS, OpFamily.Push_FixedReg, FieldEncoding.None, RegisterType.SS, 1, Mnemonics.Push, "Decrements the 16-bit " + RegisterType.SP + " by the amount of the " + RegisterType.SS + " Register");
            _table[0x17 /* 0001 0111 */] = new InstructionDefinition(OpCode.POP_SS, OpFamily.Pop_FixedReg, FieldEncoding.None, RegisterType.SS, 1, Mnemonics.Push, "Increments the 16-bit " + RegisterType.SP + " by the amount of the " + RegisterType.SS + " Register");
            _table[0x18 /* 0001 1000 */] = new InstructionDefinition(OpCode.SBB_dREG8_dMEM8_sREG8, OpFamily.Sbb8_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.ArithmeticSubWithBorrow, "Sub with Borrow 8-bit Register from 8-bit Register/Memory");
            _table[0x19 /* 0001 1001 */] = new InstructionDefinition(OpCode.SBB_dREG16_dMEM16_sREG16, OpFamily.Sbb16_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.ArithmeticSubWithBorrow, "Sub with Borrow 16-bit Register from 16-bit Register/Memory");
            _table[0x1A /* 0001 1010 */] = new InstructionDefinition(OpCode.SBB_dREG8_sREG8_MEM8, OpFamily.Sbb8_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.ArithmeticSubWithBorrow, "Sub with Borrow 8-bit Register/Memory from 8-bit Register");
            _table[0x1B /* 0001 1011 */] = new InstructionDefinition(OpCode.SBB_dREG16_sREG16_MEM16, OpFamily.Sbb16_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.ArithmeticSubWithBorrow, "Sub with Borrow 16-bit Register/Memory from 16-bit Register");
            _table[0x1C /* 0001 1100 */] = new InstructionDefinition(OpCode.SBB_dAL_sIMM8, OpFamily.Sbb8_FixedReg_Imm, FieldEncoding.None, 2, Mnemonics.ArithmeticSubWithBorrow, "Sub with Borrow 8-bit Immediate from 8-bit " + RegisterType.AL + " Register");
            _table[0x1D /* 0001 1101 */] = new InstructionDefinition(OpCode.SBB_dAX_sIMM16, OpFamily.Sbb16_FixedReg_Imm, FieldEncoding.None, 3, Mnemonics.ArithmeticSubWithBorrow, "Sub with Borrow 16-bit Immediate from 16-bit " + RegisterType.AX + " Register");
            _table[0x1E /* 0001 1110 */] = new InstructionDefinition(OpCode.PUSH_DS, OpFamily.Push_FixedReg, FieldEncoding.None, RegisterType.DS, 1, Mnemonics.Push, "Decrements the 16-bit " + RegisterType.SP + " by the amount of the " + RegisterType.DS + " Register");
            _table[0x1F /* 0001 1111 */] = new InstructionDefinition(OpCode.POP_DS, OpFamily.Pop_FixedReg, FieldEncoding.None, RegisterType.DS, 1, Mnemonics.Push, "Increments the 16-bit " + RegisterType.SP + " by the amount of the " + RegisterType.DS + " Register");

            // 0010 0000 to 0010 1111 (AND, SUB)
            _table[0x20 /* 0010 0000 */] = new InstructionDefinition(OpCode.AND_dREG8_dMEM8_sREG8, OpFamily.And8_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.LogicalAnd, "Logical AND 8-bit Register with 8-bit Register/Memory");
            _table[0x21 /* 0010 0001 */] = new InstructionDefinition(OpCode.AND_dREG16_dMEM16_sREG16, OpFamily.And16_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.LogicalAnd, "Logical AND 16-bit Register with 16-bit Register/Memory");
            _table[0x22 /* 0010 0010 */] = new InstructionDefinition(OpCode.AND_dREG8_sREG8_MEM8, OpFamily.And8_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.LogicalAnd, "Logical AND 8-bit Register/Memory with 8-bit Register");
            _table[0x23 /* 0010 0011 */] = new InstructionDefinition(OpCode.AND_dREG16_sREG16_MEM16, OpFamily.And16_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.LogicalAnd, "Logical AND 16-bit Register/Memory with 16-bit Register");
            _table[0x24 /* 0010 0100 */] = new InstructionDefinition(OpCode.AND_dAL_sIMM8, OpFamily.And8_FixedReg_Imm, FieldEncoding.None, RegisterType.AL, 2, Mnemonics.LogicalAnd, "Logical AND 8-bit Immediate with 8-bit " + RegisterType.AL + " Register");
            _table[0x25 /* 0010 0101 */] = new InstructionDefinition(OpCode.AND_dAX_sIMM16, OpFamily.And16_FixedReg_Imm, FieldEncoding.None, RegisterType.AX, 3, Mnemonics.LogicalAnd, "Logical AND 16-bit Immediate with 16-bit " + RegisterType.AX + " Register");
            // @TODO(final): 0x26 ES: segment override prefix
            // @TODO(final): 0x27 DAA
            _table[0x28 /* 0010 1000 */] = new InstructionDefinition(OpCode.SUB_dREG8_dMEM8_sREG8, OpFamily.Sub8_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.ArithmeticSub, "Sub 8-bit Register from 8-bit Register/Memory");
            _table[0x29 /* 0010 1001 */] = new InstructionDefinition(OpCode.SUB_dREG16_dMEM16_sREG16, OpFamily.Sub16_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.ArithmeticSub, "Sub 16-bit Register from 16-bit Register/Memory");
            _table[0x2A /* 0010 1010 */] = new InstructionDefinition(OpCode.SUB_dREG8_sREG8_MEM8, OpFamily.Sub8_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.ArithmeticSub, "Sub 8-bit Register/Memory from 8-bit Register");
            _table[0x2B /* 0010 1011 */] = new InstructionDefinition(OpCode.SUB_dREG16_sREG16_MEM16, OpFamily.Sub16_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.ArithmeticSub, "Sub 16-bit Register/Memory from 16-bit Register");
            _table[0x2C /* 0010 1100 */] = new InstructionDefinition(OpCode.SUB_dAL_sIMM8, OpFamily.Sub8_FixedReg_Imm, FieldEncoding.None, RegisterType.AL, 2, Mnemonics.ArithmeticSub, "Sub 8-bit Immediate from 8-bit " + RegisterType.AL + " Register");
            _table[0x2D /* 0010 1101 */] = new InstructionDefinition(OpCode.SUB_dAX_sIMM16, OpFamily.Sub16_FixedReg_Imm, FieldEncoding.None, RegisterType.AX, 3, Mnemonics.ArithmeticSub, "Sub 16-bit Immediate from 16-bit " + RegisterType.AX + " Register");
            // @TODO(final): 0x2E CS: segment override prefix
            // @TODO(final): 0x2F DAS

            // 0011 0000 to 0011 1111 (XOR, CMP)
            _table[0x30 /* 0011 0000 */] = new InstructionDefinition(OpCode.XOR_dREG8_dMEM8_sREG8, OpFamily.Xor8_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.LogicalXor, "Logical XOR 8-bit Register with 8-bit Register/Memory");
            _table[0x31 /* 0011 0001 */] = new InstructionDefinition(OpCode.XOR_dREG16_dMEM16_sREG16, OpFamily.Xor16_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.LogicalXor, "Logical XOR 16-bit Register with 16-bit Register/Memory");
            _table[0x32 /* 0011 0010 */] = new InstructionDefinition(OpCode.XOR_dREG8_sREG8_MEM8, OpFamily.Xor8_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.LogicalXor, "Logical XOR 8-bit Register/Memory with 8-bit Register");
            _table[0x33 /* 0011 0011 */] = new InstructionDefinition(OpCode.XOR_dREG16_sREG16_MEM16, OpFamily.Xor16_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.LogicalXor, "Logical XOR 16-bit Register/Memory with 16-bit Register");
            _table[0x34 /* 0011 0100 */] = new InstructionDefinition(OpCode.XOR_dAL_sIMM8, OpFamily.Xor8_FixedReg_Imm, FieldEncoding.None, RegisterType.AL, 2, Mnemonics.LogicalXor, "Logical XOR 8-bit Immediate with 8-bit " + RegisterType.AL + " Register");
            _table[0x35 /* 0011 0101 */] = new InstructionDefinition(OpCode.XOR_dAX_sIMM16, OpFamily.Xor16_FixedReg_Imm, FieldEncoding.None, RegisterType.AX, 3, Mnemonics.LogicalXor, "Logical XOR 16-bit Immediate with 16-bit " + RegisterType.AX + " Register");
            // @TODO(final): 0x36 SS: segment override prefix
            // @TODO(final): 0x37 AAA
            _table[0x38 /* 0011 1000 */] = new InstructionDefinition(OpCode.CMP_dREG8_dMEM8_sREG8, OpFamily.Cmp8_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.ArithmeticCompare, "Compare 8-bit Register with 8-bit Register/Memory");
            _table[0x39 /* 0011 1001 */] = new InstructionDefinition(OpCode.CMP_dREG16_dMEM16_sREG16, OpFamily.Cmp16_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.ArithmeticCompare, "Compare 16-bit Register with 16-bit Register/Memory");
            _table[0x3A /* 0011 1010 */] = new InstructionDefinition(OpCode.CMP_dREG8_sREG8_MEM8, OpFamily.Cmp8_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.ArithmeticCompare, "Compare 8-bit Register/Memory with 8-bit Register");
            _table[0x3B /* 0011 1011 */] = new InstructionDefinition(OpCode.CMP_dREG16_sREG16_MEM16, OpFamily.Cmp16_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.ArithmeticCompare, "Compare 16-bit Register/Memory with 16-bit Register");
            _table[0x3C /* 0011 1100 */] = new InstructionDefinition(OpCode.CMP_dAL_sIMM8, OpFamily.Cmp8_FixedReg_Imm, FieldEncoding.None, RegisterType.AL, 2, Mnemonics.ArithmeticCompare, "Compare 8-bit Immediate with 8-bit " + RegisterType.AL + " Register");
            _table[0x3D /* 0011 1101 */] = new InstructionDefinition(OpCode.CMP_dAX_sIMM16, OpFamily.Cmp16_FixedReg_Imm, FieldEncoding.None, RegisterType.AX, 3, Mnemonics.ArithmeticCompare, "Compare 16-bit Immediate with 16-bit " + RegisterType.AX + " Register");
            // @TODO(final): 0x3E DS: segment override prefix
            // @TODO(final): 0x3F AAS

            // 0111 0000 to 0111 1111 (Jumps)
            _table[0x75 /* 0111 0000 */] = new InstructionDefinition(OpCode.JNE_JNZ, OpFamily.Jump8, FieldEncoding.None, 2, Mnemonics.JumpNotEqual, "Jump to 8-bit offset when not Equal");

            // 1000 0000 (ADD/ADC/SUB,etc.)
            _table[0x80 /* 1000 0000 */] = new InstructionDefinition(OpCode.ARITHMETIC_dREG8_dMEM8_sIMM8, OpFamily.Arithmetic8_RegOrMem_Imm, FieldEncoding.ModRemRM, 3, 5, Mnemonics.Dynamic, "Arithmetic 8-bit Immediate to 8-bit Register/Memory");

            // 1000 0011 (ADD/ADC/SUB,etc.)
            _table[0x83 /* 1000 0011 */] = new InstructionDefinition(OpCode.ARITHMETIC_dREG16_dMEM16_sIMM16, OpFamily.Arithmetic16_RegOrMem_Imm, FieldEncoding.ModRemRM, 3, 5, Mnemonics.Dynamic, "Arithmetic 16-bit Immediate to 16-bit Register/Memory");

            // 1 0 0 0 1 0 d w (MOV)
            _table[0x88 /* 100010 00 */] = new InstructionDefinition(OpCode.MOV_dREG8_dMEM8_sREG8, OpFamily.Move8_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.Move, "Copy 8-bit Register to 8-bit Register/Memory");
            _table[0x89 /* 100010 01 */] = new InstructionDefinition(OpCode.MOV_dREG16_dMEM16_sREG16, OpFamily.Move16_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.Move, "Copy 16-bit Register to 16-bit Register/Memory");
            _table[0x8A /* 100010 10 */] = new InstructionDefinition(OpCode.MOV_dREG8_sMEM8_sREG8, OpFamily.Move8_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.Move, "Copy 8-bit Register/Memory to 8-bit Register");
            _table[0x8B /* 100010 11 */] = new InstructionDefinition(OpCode.MOV_dREG16_sMEM16_sREG16, OpFamily.Move16_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.Move, "Copy 8-bit Register/Memory to 8-bit Register");

            // 1010 0000 to 1010 0011 (MOV)
            _table[0xA0 /* 1010000 */] = new InstructionDefinition(OpCode.MOV_dAL_sMEM8, OpFamily.Move8_FixedReg_Mem, FieldEncoding.None, RegisterType.AL, 3, Mnemonics.Move, "Copy 8-bit Memory to 8-bit " + RegisterType.AL + " Register");
            _table[0xA1 /* 1010001 */] = new InstructionDefinition(OpCode.MOV_dAX_sMEM16, OpFamily.Move16_FixedReg_Mem, FieldEncoding.None, RegisterType.AX, 3, Mnemonics.Move, "Copy 16-bit Memory to 16-bit " + RegisterType.AX + " Register");
            _table[0xA2 /* 1010010 */] = new InstructionDefinition(OpCode.MOV_dMEM8_sAL, OpFamily.Move8_Mem_FixedReg, FieldEncoding.None, RegisterType.AL, 3, Mnemonics.Move, "Copy 8-bit " + RegisterType.AL + " Register to 8-bit Memory");
            _table[0xA3 /* 1010011 */] = new InstructionDefinition(OpCode.MOV_dMEM16_sAX, OpFamily.Move16_Mem_FixedReg, FieldEncoding.None, RegisterType.AX, 3, Mnemonics.Move, "Copy 16-bit " + RegisterType.AX + " Register to 16-bit Memory"); // BUG(final): Page 174, instruction $A3: MOV16, AL is wrong, correct is MOV16, AX

            // 1 0 1 1 w reg (MOV)
            _table[0xB0 /* 1011 0 000 */] = new InstructionDefinition(OpCode.MOV_dAL_sIMM8, OpFamily.Move8_Reg_Imm, FieldEncoding.None, RegisterType.AL, 2, Mnemonics.Move, "Copy 8-bit Immediate to 8-bit " + RegisterType.AL + " Register");
            _table[0xB1 /* 1011 0 001 */] = new InstructionDefinition(OpCode.MOV_dCL_sIMM8, OpFamily.Move8_Reg_Imm, FieldEncoding.None, RegisterType.CL, 2, Mnemonics.Move, "Copy 8-bit Immediate to 8-bit " + RegisterType.CL + " Register");
            _table[0xB2 /* 1011 0 010 */] = new InstructionDefinition(OpCode.MOV_dDL_sIMM8, OpFamily.Move8_Reg_Imm, FieldEncoding.None, RegisterType.DL, 2, Mnemonics.Move, "Copy 8-bit Immediate to 8-bit " + RegisterType.DL + " Register");
            _table[0xB3 /* 1011 0 011 */] = new InstructionDefinition(OpCode.MOV_dBL_sIMM8, OpFamily.Move8_Reg_Imm, FieldEncoding.None, RegisterType.BL, 2, Mnemonics.Move, "Copy 8-bit Immediate to 8-bit " + RegisterType.BL + " Register");
            _table[0xB4 /* 1011 0 100 */] = new InstructionDefinition(OpCode.MOV_dAH_sIMM8, OpFamily.Move8_Reg_Imm, FieldEncoding.None, RegisterType.AH, 2, Mnemonics.Move, "Copy 8-bit Immediate to 8-bit " + RegisterType.AH + " Register");
            _table[0xB5 /* 1011 0 101 */] = new InstructionDefinition(OpCode.MOV_dCH_sIMM8, OpFamily.Move8_Reg_Imm, FieldEncoding.None, RegisterType.CH, 2, Mnemonics.Move, "Copy 8-bit Immediate to 8-bit " + RegisterType.CH + " Register");
            _table[0xB6 /* 1011 0 110 */] = new InstructionDefinition(OpCode.MOV_dDH_sIMM8, OpFamily.Move8_Reg_Imm, FieldEncoding.None, RegisterType.DH, 2, Mnemonics.Move, "Copy 8-bit Immediate to 8-bit " + RegisterType.DH + " Register");
            _table[0xB7 /* 1011 0 111 */] = new InstructionDefinition(OpCode.MOV_dDH_sIMM8, OpFamily.Move8_Reg_Imm, FieldEncoding.None, RegisterType.BH, 2, Mnemonics.Move, "Copy 8-bit Immediate to 8-bit " + RegisterType.BH + " Register");
            _table[0xB8 /* 1011 1 000 */] = new InstructionDefinition(OpCode.MOV_dAX_sIMM16, OpFamily.Move16_Reg_Imm, FieldEncoding.None, RegisterType.AX, 3, Mnemonics.Move, "Copy 16-bit Immediate to 16-bit " + RegisterType.AX + " Register");
            _table[0xB9 /* 1011 1 001 */] = new InstructionDefinition(OpCode.MOV_dCX_sIMM16, OpFamily.Move16_Reg_Imm, FieldEncoding.None, RegisterType.CX, 3, Mnemonics.Move, "Copy 16-bit Immediate to 16-bit " + RegisterType.CX + " Register");
            _table[0xBA /* 1011 1 010 */] = new InstructionDefinition(OpCode.MOV_dDX_sIMM16, OpFamily.Move16_Reg_Imm, FieldEncoding.None, RegisterType.DX, 3, Mnemonics.Move, "Copy 16-bit Immediate to 16-bit " + RegisterType.DX + " Register");
            _table[0xBB /* 1011 1 011 */] = new InstructionDefinition(OpCode.MOV_dBX_sIMM16, OpFamily.Move16_Reg_Imm, FieldEncoding.None, RegisterType.BX, 3, Mnemonics.Move, "Copy 16-bit Immediate to 16-bit " + RegisterType.BX + " Register");
            _table[0xBC /* 1011 1 100 */] = new InstructionDefinition(OpCode.MOV_dSP_sIMM16, OpFamily.Move16_Reg_Imm, FieldEncoding.None, RegisterType.SP, 3, Mnemonics.Move, "Copy 16-bit Immediate to 16-bit " + RegisterType.SP + " Register");
            _table[0xBD /* 1011 1 101 */] = new InstructionDefinition(OpCode.MOV_dBP_sIMM16, OpFamily.Move16_Reg_Imm, FieldEncoding.None, RegisterType.BP, 3, Mnemonics.Move, "Copy 16-bit Immediate to 16-bit " + RegisterType.BP + " Register");
            _table[0xBE /* 1011 1 110 */] = new InstructionDefinition(OpCode.MOV_dSI_sIMM16, OpFamily.Move16_Reg_Imm, FieldEncoding.None, RegisterType.SI, 3, Mnemonics.Move, "Copy 16-bit Immediate to 16-bit " + RegisterType.SI + " Register");
            _table[0xBF /* 1011 1 111 */] = new InstructionDefinition(OpCode.MOV_dDI_sIMM16, OpFamily.Move16_Reg_Imm, FieldEncoding.None, RegisterType.DI, 3, Mnemonics.Move, "Copy 16-bit Immediate to 16-bit " + RegisterType.DI + " Register");

            // 1100 0110 to 1100 0111 (MOV)
            _table[0xC6 /* 1100 0110 */] = new InstructionDefinition(OpCode.MOV_dMEM8_sIMM8, OpFamily.Move8_Mem_Imm, FieldEncoding.ModRemRM, 3, 5, Mnemonics.Move, "Copy 8-bit Immediate to 8-bit Memory");
            _table[0xC7 /* 1100 0111 */] = new InstructionDefinition(OpCode.MOV_dMEM16_sIMM16, OpFamily.Move16_Mem_Imm, FieldEncoding.ModRemRM, 4, 6, Mnemonics.Move, "Copy 16-bit Immediate to 16-bit Memory");
        }
    }
}
