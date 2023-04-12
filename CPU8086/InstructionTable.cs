using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using IL = Final.CPU8086.InstructionList;
using IE = Final.CPU8086.InstructionEntry;
using DW = Final.CPU8086.DataWidthType;
using IF = Final.CPU8086.InstructionFlags;
using System;

namespace Final.CPU8086
{
    public class InstructionTable : IReadOnlyCollection<IL>
    {
        private readonly IL[] _opToList = new IL[256];

        public ref readonly IL this[int index] => ref _opToList[index];

        public InstructionTable()
        {
            Array.Clear(_opToList, 0, _opToList.Length);
        }

        public void Load()
        {
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

            _opToList[0] = new IL(0b00000000,
            new IE(0x00, "ADD", "B", IF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmb", "rb" })
        );
            _opToList[1] = new IL(0b00000001,
                new IE(0x01, "ADD", "W", IF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmw", "rw" })
            );
            _opToList[2] = new IL(0b00000010,
                new IE(0x02, "ADD", "B", IF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rb", "rmb" })
            );
            _opToList[3] = new IL(0b00000011,
                new IE(0x03, "ADD", "W", IF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rw", "rmw" })
            );
            _opToList[4] = new IL(0b00000100,
                new IE(0x04, "ADD", "B", IF.None, "o---szap", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "al", "ib" })
            );
            _opToList[5] = new IL(0b00000101,
                new IE(0x05, "ADD", "W", IF.None, "o---szap", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "ax", "iw" })
            );
            _opToList[6] = new IL(0b00000110,
                new IE(0x06, "PUSH", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "es" })
            );
            _opToList[7] = new IL(0b00000111,
                new IE(0x07, "POP", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "es" })
            );
            _opToList[8] = new IL(0b00001000,
                new IE(0x08, "OR", "B", IF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmb", "rb" })
            );
            _opToList[9] = new IL(0b00001001,
                new IE(0x09, "OR", "W", IF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmw", "rw" })
            );
            _opToList[10] = new IL(0b00001010,
                new IE(0x0A, "OR", "B", IF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rb", "rmb" })
            );
            _opToList[11] = new IL(0b00001011,
                new IE(0x0B, "OR", "W", IF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rw", "rmw" })
            );
            _opToList[12] = new IL(0b00001100,
                new IE(0x0C, "OR", "B", IF.None, "o---szap", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "al", "ib" })
            );
            _opToList[13] = new IL(0b00001101,
                new IE(0x0D, "OR", "W", IF.None, "o---szap", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "ax", "iw" })
            );
            _opToList[14] = new IL(0b00001110,
                new IE(0x0E, "PUSH", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "cs" })
            );
            _opToList[16] = new IL(0b00010000,
                new IE(0x10, "ADC", "B", IF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmb", "rb" })
            );
            _opToList[17] = new IL(0b00010001,
                new IE(0x11, "ADC", "W", IF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmw", "rw" })
            );
            _opToList[18] = new IL(0b00010010,
                new IE(0x12, "ADC", "B", IF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rb", "rmb" })
            );
            _opToList[19] = new IL(0b00010011,
                new IE(0x13, "ADC", "W", IF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rw", "rmw" })
            );
            _opToList[20] = new IL(0b00010100,
                new IE(0x14, "ADC", "B", IF.None, "o---szap", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "al", "ib" })
            );
            _opToList[21] = new IL(0b00010101,
                new IE(0x15, "ADC", "W", IF.None, "o---szap", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "ax", "iw" })
            );
            _opToList[22] = new IL(0b00010110,
                new IE(0x16, "PUSH", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ss" })
            );
            _opToList[23] = new IL(0b00010111,
                new IE(0x17, "POP", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ss" })
            );
            _opToList[24] = new IL(0b00011000,
                new IE(0x18, "SBB", "B", IF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmb", "rb" })
            );
            _opToList[25] = new IL(0b00011001,
                new IE(0x19, "SBB", "W", IF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmw", "rw" })
            );
            _opToList[26] = new IL(0b00011010,
                new IE(0x1A, "SBB", "B", IF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rb", "rmb" })
            );
            _opToList[27] = new IL(0b00011011,
                new IE(0x1B, "SBB", "W", IF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rw", "rmw" })
            );
            _opToList[28] = new IL(0b00011100,
                new IE(0x1C, "SBB", "B", IF.None, "o---szap", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "al", "ib" })
            );
            _opToList[29] = new IL(0b00011101,
                new IE(0x1D, "SBB", "W", IF.None, "o---szap", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "ax", "iw" })
            );
            _opToList[30] = new IL(0b00011110,
                new IE(0x1E, "PUSH", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ds" })
            );
            _opToList[31] = new IL(0b00011111,
                new IE(0x1F, "POP", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ds" })
            );
            _opToList[32] = new IL(0b00100000,
                new IE(0x20, "AND", "B", IF.None, "0---sz-p", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmb", "rb" })
            );
            _opToList[33] = new IL(0b00100001,
                new IE(0x21, "AND", "W", IF.None, "0---sz-p", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmw", "rw" })
            );
            _opToList[34] = new IL(0b00100010,
                new IE(0x22, "AND", "B", IF.None, "0---sz-p", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rb", "rmb" })
            );
            _opToList[35] = new IL(0b00100011,
                new IE(0x23, "AND", "W", IF.None, "0---sz-p", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rw", "rmw" })
            );
            _opToList[36] = new IL(0b00100100,
                new IE(0x24, "AND", "B", IF.None, "0---sz-p", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "al", "ib" })
            );
            _opToList[37] = new IL(0b00100101,
                new IE(0x25, "AND", "W", IF.None, "0---sz-p", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "ax", "iw" })
            );
            _opToList[39] = new IL(0b00100111,
                new IE(0x27, "DAA", "", IF.None, "----szap", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[40] = new IL(0b00101000,
                new IE(0x28, "SUB", "B", IF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmb", "rb" })
            );
            _opToList[41] = new IL(0b00101001,
                new IE(0x29, "SUB", "W", IF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmw", "rw" })
            );
            _opToList[42] = new IL(0b00101010,
                new IE(0x2A, "SUB", "B", IF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rb", "rmb" })
            );
            _opToList[43] = new IL(0b00101011,
                new IE(0x2B, "SUB", "W", IF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rw", "rmw" })
            );
            _opToList[44] = new IL(0b00101100,
                new IE(0x2C, "SUB", "B", IF.None, "o---szap", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "al", "ib" })
            );
            _opToList[45] = new IL(0b00101101,
                new IE(0x2D, "SUB", "W", IF.None, "o---szap", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "ax", "iw" })
            );
            _opToList[47] = new IL(0b00101111,
                new IE(0x2F, "DAS", "", IF.None, "----szap", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[48] = new IL(0b00110000,
                new IE(0x30, "XOR", "B", IF.None, "0---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmb", "rb" })
            );
            _opToList[49] = new IL(0b00110001,
                new IE(0x31, "XOR", "W", IF.None, "0---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmw", "rw" })
            );
            _opToList[50] = new IL(0b00110010,
                new IE(0x32, "XOR", "B", IF.None, "0---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rb", "rmb" })
            );
            _opToList[51] = new IL(0b00110011,
                new IE(0x33, "XOR", "W", IF.None, "0---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rw", "rmw" })
            );
            _opToList[52] = new IL(0b00110100,
                new IE(0x34, "XOR", "B", IF.None, "0---szap", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "al", "ib" })
            );
            _opToList[53] = new IL(0b00110101,
                new IE(0x35, "XOR", "W", IF.None, "0---szap", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "ax", "iw" })
            );
            _opToList[55] = new IL(0b00110111,
                new IE(0x37, "AAA", "", IF.None, "------a-", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[56] = new IL(0b00111000,
                new IE(0x38, "CMP", "B", IF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmb", "rb" })
            );
            _opToList[57] = new IL(0b00111001,
                new IE(0x39, "CMP", "W", IF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmw", "rw" })
            );
            _opToList[58] = new IL(0b00111010,
                new IE(0x3A, "CMP", "B", IF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rb", "rmb" })
            );
            _opToList[59] = new IL(0b00111011,
                new IE(0x3B, "CMP", "W", IF.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rw", "rmw" })
            );
            _opToList[60] = new IL(0b00111100,
                new IE(0x3C, "CMP", "B", IF.None, "o---szap", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "al", "ib" })
            );
            _opToList[61] = new IL(0b00111101,
                new IE(0x3D, "CMP", "W", IF.None, "o---szap", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "ax", "iw" })
            );
            _opToList[63] = new IL(0b00111111,
                new IE(0x3F, "AAS", "", IF.None, "------a-", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[64] = new IL(0b01000000,
                new IE(0x40, "INC", "", IF.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ax" })
            );
            _opToList[65] = new IL(0b01000001,
                new IE(0x41, "INC", "", IF.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "cx" })
            );
            _opToList[66] = new IL(0b01000010,
                new IE(0x42, "INC", "", IF.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "dx" })
            );
            _opToList[67] = new IL(0b01000011,
                new IE(0x43, "INC", "", IF.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "bx" })
            );
            _opToList[68] = new IL(0b01000100,
                new IE(0x44, "INC", "", IF.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "sp" })
            );
            _opToList[69] = new IL(0b01000101,
                new IE(0x45, "INC", "", IF.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "bp" })
            );
            _opToList[70] = new IL(0b01000110,
                new IE(0x46, "INC", "", IF.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "si" })
            );
            _opToList[71] = new IL(0b01000111,
                new IE(0x47, "INC", "", IF.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "di" })
            );
            _opToList[72] = new IL(0b01001000,
                new IE(0x48, "DEC", "", IF.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ax" })
            );
            _opToList[73] = new IL(0b01001001,
                new IE(0x49, "DEC", "", IF.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "cx" }),
                new IE(0x49, "DEC", "", IF.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "dx" })
            );
            _opToList[74] = new IL(0b01001010,
                new IE(0x4A, "DEC", "", IF.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "bx" })
            );
            _opToList[75] = new IL(0b01001011,
                new IE(0x4B, "DEC", "", IF.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "sp" })
            );
            _opToList[76] = new IL(0b01001100,
                new IE(0x4C, "DEC", "", IF.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "bp" })
            );
            _opToList[77] = new IL(0b01001101,
                new IE(0x4D, "DEC", "", IF.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "si" })
            );
            _opToList[79] = new IL(0b01001111,
                new IE(0x4F, "DEC", "", IF.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "di" })
            );
            _opToList[80] = new IL(0b01010000,
                new IE(0x50, "PUSH", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ax" })
            );
            _opToList[81] = new IL(0b01010001,
                new IE(0x51, "PUSH", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "cx" })
            );
            _opToList[82] = new IL(0b01010010,
                new IE(0x52, "PUSH", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "dx" })
            );
            _opToList[83] = new IL(0b01010011,
                new IE(0x53, "PUSH", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "bx" })
            );
            _opToList[84] = new IL(0b01010100,
                new IE(0x54, "PUSH", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "sp" })
            );
            _opToList[85] = new IL(0b01010101,
                new IE(0x55, "PUSH", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "bp" })
            );
            _opToList[86] = new IL(0b01010110,
                new IE(0x56, "PUSH", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "si" })
            );
            _opToList[87] = new IL(0b01010111,
                new IE(0x57, "PUSH", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "di" })
            );
            _opToList[88] = new IL(0b01011000,
                new IE(0x58, "POP", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ax" })
            );
            _opToList[89] = new IL(0b01011001,
                new IE(0x59, "POP", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "cx" })
            );
            _opToList[90] = new IL(0b01011010,
                new IE(0x5A, "POP", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "dx" })
            );
            _opToList[91] = new IL(0b01011011,
                new IE(0x5B, "POP", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "bx" })
            );
            _opToList[92] = new IL(0b01011100,
                new IE(0x5C, "POP", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "sp" })
            );
            _opToList[93] = new IL(0b01011101,
                new IE(0x5D, "POP", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "bp" })
            );
            _opToList[94] = new IL(0b01011110,
                new IE(0x5E, "POP", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "si" })
            );
            _opToList[95] = new IL(0b01011111,
                new IE(0x5F, "POP", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "di" })
            );
            _opToList[105] = new IL(0b01101001,
                new IE(0x69, "IMUL", "W", IF.None, "o---szap", "8086", 6, 6, new Field[] { "mr", "i0", "i1", "i2", "i3" }, new Operand[] { "rd", "id" }),
                new IE(0x69, "IMUL", "W", IF.None, "o---szap", "8086", 6, 6, new Field[] { "mr", "d0", "d1", "i0~i3" }, new Operand[] { "rd", "rmd", "id" }),
                new IE(0x69, "IMUL", "B", IF.None, "o---szap", "8086", 4, 4, new Field[] { "mr", "i0", "i1" }, new Operand[] { "rw", "iw" }),
                new IE(0x69, "IMUL", "B", IF.None, "o---szap", "8086", 4, 6, new Field[] { "mr", "d0", "d1", "i0", "i1" }, new Operand[] { "rw", "rmw", "iw" })
            );
            _opToList[107] = new IL(0b01101011,
                new IE(0x6B, "IMUL", "W", IF.None, "o---szap", "8086", 3, 3, new Field[] { "mr", "i0" }, new Operand[] { "rd", "ib" }),
                new IE(0x6B, "IMUL", "W", IF.None, "o---szap", "8086", 3, 5, new Field[] { "mr", "d0", "d1", "i0" }, new Operand[] { "rd", "rmd", "ib" }),
                new IE(0x6B, "IMUL", "B", IF.None, "o---szap", "8086", 3, 3, new Field[] { "mr", "i0" }, new Operand[] { "rw", "ib" }),
                new IE(0x6B, "IMUL", "B", IF.None, "o---szap", "8086", 3, 5, new Field[] { "mr", "d0", "d1", "i0" }, new Operand[] { "rw", "rmw", "ib" })
            );
            _opToList[112] = new IL(0b01110000,
                new IE(0x70, "JO", "", IF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[113] = new IL(0b01110001,
                new IE(0x71, "JNO", "", IF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[114] = new IL(0b01110010,
                new IE(0x72, "JB", "", IF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" }),
                new IE(0x72, "JC", "", IF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[115] = new IL(0b01110011,
                new IE(0x73, "JAE", "", IF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" }),
                new IE(0x73, "JNC", "", IF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[116] = new IL(0b01110100,
                new IE(0x74, "JE", "", IF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[117] = new IL(0b01110101,
                new IE(0x75, "JNE", "", IF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[118] = new IL(0b01110110,
                new IE(0x76, "JBE", "", IF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[119] = new IL(0b01110111,
                new IE(0x77, "JA", "", IF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[120] = new IL(0b01111000,
                new IE(0x78, "JS", "", IF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[121] = new IL(0b01111001,
                new IE(0x79, "JNS", "", IF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[122] = new IL(0b01111010,
                new IE(0x7A, "JP", "", IF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[123] = new IL(0b01111011,
                new IE(0x7B, "JNP", "", IF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[124] = new IL(0b01111100,
                new IE(0x7C, "JL", "", IF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[125] = new IL(0b01111101,
                new IE(0x7D, "JGE", "", IF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[126] = new IL(0b01111110,
                new IE(0x7E, "JLE", "", IF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[127] = new IL(0b01111111,
                new IE(0x7F, "JG", "", IF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[128] = new IL(0b10000000,
                new IE(0x80, "ADC", "B", IF.None, "o---szap", "8086", 3, 5, new Field[] { "/2", "d0", "d1", "i0" }, new Operand[] { "rmb", "ib" }),
                new IE(0x80, "ADD", "B", IF.None, "o---szap", "8086", 3, 5, new Field[] { "/0", "d0", "d1", "i0" }, new Operand[] { "rmb", "ib" }),
                new IE(0x80, "AND", "B", IF.None, "0---sz-p", "8086", 3, 5, new Field[] { "/4", "d0", "d1", "i0" }, new Operand[] { "rmb", "ib" }),
                new IE(0x80, "CMP", "B", IF.None, "o---szap", "8086", 3, 5, new Field[] { "/7", "d0", "d1", "i0" }, new Operand[] { "rmb", "ib" }),
                new IE(0x80, "OR", "B", IF.None, "o---szap", "8086", 3, 5, new Field[] { "/1", "d0", "d1", "i0" }, new Operand[] { "rmb", "ib" }),
                new IE(0x80, "SBB", "B", IF.None, "o---szap", "8086", 3, 5, new Field[] { "/3", "d0", "d1", "i0" }, new Operand[] { "rmb", "ib" }),
                new IE(0x80, "SUB", "B", IF.None, "o---szap", "8086", 3, 5, new Field[] { "/5", "d0", "d1", "i0" }, new Operand[] { "rmb", "ib" }),
                new IE(0x80, "XOR", "B", IF.None, "0---szap", "8086", 3, 5, new Field[] { "/6", "d0", "d1", "i0" }, new Operand[] { "rmb", "ib" })
            );
            _opToList[129] = new IL(0b10000001,
                new IE(0x81, "ADC", "W", IF.None, "o---szap", "8086", 4, 6, new Field[] { "/2", "d0", "d1", "i0", "i1" }, new Operand[] { "rmw", "iw" }),
                new IE(0x81, "ADD", "W", IF.None, "o---szap", "8086", 4, 6, new Field[] { "/0", "d0", "d1", "i0", "i1" }, new Operand[] { "rmw", "iw" }),
                new IE(0x81, "AND", "W", IF.None, "0---sz-p", "8086", 4, 6, new Field[] { "/4", "d0", "d1", "i0", "i1" }, new Operand[] { "rmw", "iw" }),
                new IE(0x81, "CMP", "W", IF.None, "o---szap", "8086", 4, 6, new Field[] { "/7", "d0", "d1", "i0", "i1" }, new Operand[] { "rmw", "iw" }),
                new IE(0x81, "OR", "W", IF.None, "o---szap", "8086", 4, 6, new Field[] { "/1", "d0", "d1", "i0", "i1" }, new Operand[] { "rmw", "iw" }),
                new IE(0x81, "SBB", "W", IF.None, "o---szap", "8086", 4, 6, new Field[] { "/3", "d0", "d1", "i0", "i1" }, new Operand[] { "rmw", "iw" }),
                new IE(0x81, "SUB", "W", IF.None, "o---szap", "8086", 4, 6, new Field[] { "/5", "d0", "d1", "i0", "i1" }, new Operand[] { "rmw", "iw" }),
                new IE(0x81, "XOR", "W", IF.None, "0---szap", "8086", 4, 6, new Field[] { "/6", "d0", "d1", "i0", "i1" }, new Operand[] { "rmw", "iw" })
            );
            _opToList[131] = new IL(0b10000011,
                new IE(0x83, "ADC", "W", IF.SignExtendedImm8, "o---szap", "8086", 3, 5, new Field[] { "/2", "d0", "d1", "i0" }, new Operand[] { "rmw", "ib" }),
                new IE(0x83, "ADD", "W", IF.SignExtendedImm8, "o---szap", "8086", 3, 5, new Field[] { "/0", "d0", "d1", "i0" }, new Operand[] { "rmw", "ib" }),
                new IE(0x83, "AND", "W", IF.SignExtendedImm8, "0---sz-p", "8086", 3, 5, new Field[] { "/4", "d0", "d1", "i0" }, new Operand[] { "rmw", "ib" }),
                new IE(0x83, "CMP", "W", IF.SignExtendedImm8, "o---szap", "8086", 3, 5, new Field[] { "/7", "d0", "d1", "i0" }, new Operand[] { "rmw", "ib" }),
                new IE(0x83, "OR", "W", IF.SignExtendedImm8, "o---szap", "8086", 3, 5, new Field[] { "/1", "d0", "d1", "i0" }, new Operand[] { "rmw", "ib" }),
                new IE(0x83, "SBB", "W", IF.SignExtendedImm8, "o---szap", "8086", 3, 5, new Field[] { "/3", "d0", "d1", "i0" }, new Operand[] { "rmw", "ib" }),
                new IE(0x83, "SUB", "W", IF.SignExtendedImm8, "o---szap", "8086", 3, 5, new Field[] { "/5", "d0", "d1", "i0" }, new Operand[] { "rmw", "ib" }),
                new IE(0x83, "XOR", "W", IF.SignExtendedImm8, "0---szap", "8086", 3, 5, new Field[] { "/6", "d0", "d1", "i0" }, new Operand[] { "rmw", "ib" })
            );
            _opToList[132] = new IL(0b10000100,
                new IE(0x84, "TEST", "B", IF.None, "0---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmb", "rmb" })
            );
            _opToList[133] = new IL(0b10000101,
                new IE(0x85, "TEST", "W", IF.None, "0---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmw", "rmw" })
            );
            _opToList[134] = new IL(0b10000110,
                new IE(0x86, "XCHG", "B", IF.None, "--------", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rb", "rmb" }),
                new IE(0x86, "XCHG", "B", IF.None, "--------", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmb", "rb" })
            );
            _opToList[135] = new IL(0b10000111,
                new IE(0x87, "XCHG", "W", IF.None, "--------", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmw", "rw" }),
                new IE(0x87, "XCHG", "W", IF.None, "--------", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rw", "rmw" })
            );
            _opToList[136] = new IL(0b10001000,
                new IE(0x88, "MOV", "B", IF.None, "--------", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmb", "rb" })
            );
            _opToList[137] = new IL(0b10001001,
                new IE(0x89, "MOV", "W", IF.None, "--------", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmw", "rw" })
            );
            _opToList[138] = new IL(0b10001010,
                new IE(0x8A, "MOV", "B", IF.None, "--------", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rb", "rmb" })
            );
            _opToList[139] = new IL(0b10001011,
                new IE(0x8B, "MOV", "W", IF.None, "--------", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rw", "rmw" })
            );
            _opToList[140] = new IL(0b10001100,
                new IE(0x8C, "MOV", "", IF.None, "--------", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmw", "sr" })
            );
            _opToList[141] = new IL(0b10001101,
                new IE(0x8D, "LEA", "", IF.None, "--------", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rw", "mw" })
            );
            _opToList[142] = new IL(0b10001110,
                new IE(0x8E, "MOV", "", IF.None, "--------", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "sr", "rmw" })
            );
            _opToList[143] = new IL(0b10001111,
                new IE(0x8F, "POP", "", IF.None, "--------", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmw" })
            );
            _opToList[144] = new IL(0b10010000,
                new IE(0x90, "NOP", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[145] = new IL(0b10010001,
                new IE(0x91, "XCHG", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ax", "cx" })
            );
            _opToList[146] = new IL(0b10010010,
                new IE(0x92, "XCHG", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ax", "dx" })
            );
            _opToList[147] = new IL(0b10010011,
                new IE(0x93, "XCHG", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ax", "bx" })
            );
            _opToList[148] = new IL(0b10010100,
                new IE(0x94, "XCHG", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ax", "sp" })
            );
            _opToList[149] = new IL(0b10010101,
                new IE(0x95, "XCHG", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ax", "bp" })
            );
            _opToList[150] = new IL(0b10010110,
                new IE(0x96, "XCHG", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ax", "si" })
            );
            _opToList[151] = new IL(0b10010111,
                new IE(0x97, "XCHG", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ax", "di" })
            );
            _opToList[152] = new IL(0b10011000,
                new IE(0x98, "CBW", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[153] = new IL(0b10011001,
                new IE(0x99, "CWD", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[154] = new IL(0b10011010,
                new IE(0x9A, "CALL", "", IF.Far, "--------", "8086", 5, 5, new Field[] { "o0", "o1", "sl", "sh" }, Array.Empty<Operand>())
            );
            _opToList[155] = new IL(0b10011011,
                new IE(0x9B, "WAIT", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[156] = new IL(0b10011100,
                new IE(0x9C, "PUSHF", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[157] = new IL(0b10011101,
                new IE(0x9D, "POPF", "", IF.None, "oditszap", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[158] = new IL(0b10011110,
                new IE(0x9E, "SAHF", "", IF.None, "----szap", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[159] = new IL(0b10011111,
                new IE(0x9F, "LAHF", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[160] = new IL(0b10100000,
                new IE(0xA0, "MOV", "B", IF.None, "--------", "8086", 3, 3, new Field[] { "d0", "d1" }, new Operand[] { "al", "rmb" })
            );
            _opToList[161] = new IL(0b10100001,
                new IE(0xA1, "MOV", "W", IF.None, "--------", "8086", 3, 3, new Field[] { "d0", "d1" }, new Operand[] { "ax", "rmw" })
            );
            _opToList[162] = new IL(0b10100010,
                new IE(0xA2, "MOV", "B", IF.None, "--------", "8086", 3, 3, new Field[] { "d0", "d1" }, new Operand[] { "rmb", "al" })
            );
            _opToList[163] = new IL(0b10100011,
                new IE(0xA3, "MOV", "W", IF.None, "--------", "8086", 3, 3, new Field[] { "d0", "d1" }, new Operand[] { "rmw", "ax" })
            );
            _opToList[164] = new IL(0b10100100,
                new IE(0xA4, "MOVSB", "B", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[165] = new IL(0b10100101,
                new IE(0xA5, "MOVSW", "W", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[166] = new IL(0b10100110,
                new IE(0xA6, "CMPSB", "B", IF.None, "od--szap", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[167] = new IL(0b10100111,
                new IE(0xA7, "CMPSW", "W", IF.None, "od--szap", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[168] = new IL(0b10101000,
                new IE(0xA8, "TEST", "B", IF.None, "0---szap", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "al", "ib" })
            );
            _opToList[169] = new IL(0b10101001,
                new IE(0xA9, "TEST", "W", IF.None, "0---szap", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "ax", "iw" })
            );
            _opToList[170] = new IL(0b10101010,
                new IE(0xAA, "STOSB", "B", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[171] = new IL(0b10101011,
                new IE(0xAB, "STOSW", "W", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[172] = new IL(0b10101100,
                new IE(0xAC, "LODSB", "B", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[173] = new IL(0b10101101,
                new IE(0xAD, "LODSW", "W", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[174] = new IL(0b10101110,
                new IE(0xAE, "SCASB", "B", IF.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[175] = new IL(0b10101111,
                new IE(0xAF, "SCASW", "W", IF.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[176] = new IL(0b10110000,
                new IE(0xB0, "MOV", "B", IF.None, "--------", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "al", "ib" })
            );
            _opToList[177] = new IL(0b10110001,
                new IE(0xB1, "MOV", "B", IF.None, "--------", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "cl", "ib" })
            );
            _opToList[178] = new IL(0b10110010,
                new IE(0xB2, "MOV", "B", IF.None, "--------", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "dl", "ib" })
            );
            _opToList[179] = new IL(0b10110011,
                new IE(0xB3, "MOV", "B", IF.None, "--------", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "bl", "ib" })
            );
            _opToList[180] = new IL(0b10110100,
                new IE(0xB4, "MOV", "B", IF.None, "--------", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "ah", "ib" })
            );
            _opToList[181] = new IL(0b10110101,
                new IE(0xB5, "MOV", "B", IF.None, "--------", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "ch", "ib" })
            );
            _opToList[182] = new IL(0b10110110,
                new IE(0xB6, "MOV", "B", IF.None, "--------", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "dh", "ib" })
            );
            _opToList[183] = new IL(0b10110111,
                new IE(0xB7, "MOV", "B", IF.None, "--------", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "bh", "ib" })
            );
            _opToList[184] = new IL(0b10111000,
                new IE(0xB8, "MOV", "W", IF.None, "--------", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "ax", "iw" })
            );
            _opToList[185] = new IL(0b10111001,
                new IE(0xB9, "MOV", "W", IF.None, "--------", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "cx", "iw" })
            );
            _opToList[186] = new IL(0b10111010,
                new IE(0xBA, "MOV", "W", IF.None, "--------", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "dx", "iw" })
            );
            _opToList[187] = new IL(0b10111011,
                new IE(0xBB, "MOV", "W", IF.None, "--------", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "bx", "iw" })
            );
            _opToList[188] = new IL(0b10111100,
                new IE(0xBC, "MOV", "W", IF.None, "--------", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "sp", "iw" })
            );
            _opToList[189] = new IL(0b10111101,
                new IE(0xBD, "MOV", "W", IF.None, "--------", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "bp", "iw" })
            );
            _opToList[190] = new IL(0b10111110,
                new IE(0xBE, "MOV", "W", IF.None, "--------", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "si", "iw" })
            );
            _opToList[191] = new IL(0b10111111,
                new IE(0xBF, "MOV", "W", IF.None, "--------", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "di", "iw" })
            );
            _opToList[194] = new IL(0b11000010,
                new IE(0xC2, "RET", "", IF.None, "--------", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "iw" })
            );
            _opToList[195] = new IL(0b11000011,
                new IE(0xC3, "RET", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[196] = new IL(0b11000100,
                new IE(0xC4, "LES", "", IF.None, "--------", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rw", "md" })
            );
            _opToList[197] = new IL(0b11000101,
                new IE(0xC5, "LDS", "", IF.None, "--------", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rw", "md" })
            );
            _opToList[198] = new IL(0b11000110,
                new IE(0xC6, "MOV", "B", IF.None, "--------", "8086", 3, 5, new Field[] { "mr", "d0", "d1", "i0" }, new Operand[] { "rmb", "ib" })
            );
            _opToList[199] = new IL(0b11000111,
                new IE(0xC7, "MOV", "W", IF.None, "--------", "8086", 4, 6, new Field[] { "mr", "d0", "d1", "i0", "i1" }, new Operand[] { "rmw", "iw" })
            );
            _opToList[202] = new IL(0b11001010,
                new IE(0xCA, "RETF", "", IF.None, "--------", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "iw" })
            );
            _opToList[203] = new IL(0b11001011,
                new IE(0xCB, "RETF", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[204] = new IL(0b11001100,
                new IE(0xCC, "INT", "", IF.None, "--00----", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "3" })
            );
            _opToList[205] = new IL(0b11001101,
                new IE(0xCD, "INT", "", IF.None, "--00----", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "ib" })
            );
            _opToList[206] = new IL(0b11001110,
                new IE(0xCE, "INTO", "", IF.None, "--00----", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[207] = new IL(0b11001111,
                new IE(0xCF, "IRET", "", IF.None, "oditszap", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[208] = new IL(0b11010000,
                new IE(0xD0, "RCL", "B", IF.None, "o-------", "8086", 2, 4, new Field[] { "/2", "d0", "d1" }, new Operand[] { "rmb", "1" }),
                new IE(0xD0, "RCR", "B", IF.None, "o-------", "8086", 2, 4, new Field[] { "/3", "d0", "d1" }, new Operand[] { "rmb", "1" }),
                new IE(0xD0, "ROL", "B", IF.None, "o-------", "8086", 2, 4, new Field[] { "/0", "d0", "d1" }, new Operand[] { "rmb", "1" }),
                new IE(0xD0, "ROR", "B", IF.None, "o-------", "8086", 2, 4, new Field[] { "/1", "d0", "d1" }, new Operand[] { "rmb", "1" }),
                new IE(0xD0, "SAL", "B", IF.None, "o-------", "8086", 2, 4, new Field[] { "/4", "d0", "d1" }, new Operand[] { "rmb", "1" }),
                new IE(0xD0, "SHL", "B", IF.None, "o-------", "8086", 2, 4, new Field[] { "/4", "d0", "d1" }, new Operand[] { "rmb", "1" }),
                new IE(0xD0, "SHR", "B", IF.None, "o-------", "8086", 2, 4, new Field[] { "/5", "d0", "d1" }, new Operand[] { "rmb", "1" })
            );
            _opToList[209] = new IL(0b11010001,
                new IE(0xD1, "RCL", "W", IF.None, "o-------", "8086", 2, 4, new Field[] { "/2", "d0", "d1" }, new Operand[] { "rmw", "1" }),
                new IE(0xD1, "RCR", "W", IF.None, "o-------", "8086", 2, 4, new Field[] { "/3", "d0", "d1" }, new Operand[] { "rmw", "1" }),
                new IE(0xD1, "ROL", "W", IF.None, "o-------", "8086", 2, 4, new Field[] { "/0", "d0", "d1" }, new Operand[] { "rmw", "1" }),
                new IE(0xD1, "ROR", "W", IF.None, "o-------", "8086", 2, 4, new Field[] { "/1", "d0", "d1" }, new Operand[] { "rmw", "1" }),
                new IE(0xD1, "SAL", "W", IF.None, "o-------", "8086", 2, 4, new Field[] { "/4", "d0", "d1" }, new Operand[] { "rmw", "1" }),
                new IE(0xD1, "SHL", "W", IF.None, "o-------", "8086", 2, 4, new Field[] { "/4", "d0", "d1" }, new Operand[] { "rmw", "1" }),
                new IE(0xD1, "SAR", "W", IF.None, "o-------", "8086", 2, 4, new Field[] { "/7", "d0", "d1" }, new Operand[] { "rmw", "1" }),
                new IE(0xD1, "SHR", "W", IF.None, "o-------", "8086", 2, 4, new Field[] { "/5", "d0", "d1" }, new Operand[] { "rmw", "1" })
            );
            _opToList[210] = new IL(0b11010010,
                new IE(0xD2, "RCL", "B", IF.None, "o-------", "8086", 2, 4, new Field[] { "/2", "d0", "d1" }, new Operand[] { "rmb", "cl" }),
                new IE(0xD2, "RCR", "B", IF.None, "o-------", "8086", 2, 4, new Field[] { "/3", "d0", "d1" }, new Operand[] { "rmb", "cl" }),
                new IE(0xD2, "ROL", "B", IF.None, "o-------", "8086", 2, 4, new Field[] { "/0", "d0", "d1" }, new Operand[] { "rmb", "cl" }),
                new IE(0xD2, "ROR", "B", IF.None, "o-------", "8086", 2, 4, new Field[] { "/1", "d0", "d1" }, new Operand[] { "rmb", "cl" }),
                new IE(0xD2, "SAL", "B", IF.None, "o-------", "8086", 2, 4, new Field[] { "/4", "d0", "d1" }, new Operand[] { "rmb", "cl" }),
                new IE(0xD2, "SHL", "B", IF.None, "o-------", "8086", 2, 4, new Field[] { "/4", "d0", "d1" }, new Operand[] { "rmb", "cl" }),
                new IE(0xD2, "SAR", "B", IF.None, "o-------", "8086", 2, 4, new Field[] { "/7", "d0", "d1" }, new Operand[] { "rmb", "cl" }),
                new IE(0xD2, "SHR", "B", IF.None, "o-------", "8086", 2, 4, new Field[] { "/5", "d0", "d1" }, new Operand[] { "rmb", "cl" })
            );
            _opToList[211] = new IL(0b11010011,
                new IE(0xD3, "RCL", "W", IF.None, "o-------", "8086", 2, 4, new Field[] { "/2", "d0", "d1" }, new Operand[] { "rmw", "cl" }),
                new IE(0xD3, "RCR", "W", IF.None, "o-------", "8086", 2, 4, new Field[] { "/3", "d0", "d1" }, new Operand[] { "rmw", "cl" }),
                new IE(0xD3, "ROL", "W", IF.None, "o-------", "8086", 2, 4, new Field[] { "/0", "d0", "d1" }, new Operand[] { "rmw", "cl" }),
                new IE(0xD3, "ROR", "W", IF.None, "o-------", "8086", 2, 4, new Field[] { "/1", "d0", "d1" }, new Operand[] { "rmw", "cl" }),
                new IE(0xD3, "SAL", "W", IF.None, "o-------", "8086", 2, 4, new Field[] { "/4", "d0", "d1" }, new Operand[] { "rmw", "cl" }),
                new IE(0xD3, "SHL", "W", IF.None, "o-------", "8086", 2, 4, new Field[] { "/4", "d0", "d1" }, new Operand[] { "rmw", "cl" }),
                new IE(0xD3, "SAR", "W", IF.None, "o-------", "8086", 2, 4, new Field[] { "/7", "d0", "d1" }, new Operand[] { "rmw", "cl" }),
                new IE(0xD3, "SHR", "W", IF.None, "o-------", "8086", 2, 4, new Field[] { "/5", "d0", "d1" }, new Operand[] { "rmw", "cl" })
            );
            _opToList[212] = new IL(0b11010100,
                new IE(0xD4, "AAM", "", IF.None, "----sz-p", "8086", 2, 2, new Field[] { "0A" }, Array.Empty<Operand>())
            );
            _opToList[213] = new IL(0b11010101,
                new IE(0xD5, "AAD", "", IF.None, "----sz-p", "8086", 2, 2, new Field[] { "0A" }, Array.Empty<Operand>())
            );
            _opToList[215] = new IL(0b11010111,
                new IE(0xD7, "XLAT", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[224] = new IL(0b11100000,
                new IE(0xE0, "LOOPNZ", "", IF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[225] = new IL(0b11100001,
                new IE(0xE1, "LOOPE", "", IF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[226] = new IL(0b11100010,
                new IE(0xE2, "LOOP", "", IF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[227] = new IL(0b11100011,
                new IE(0xE3, "JCXZ", "", IF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[228] = new IL(0b11100100,
                new IE(0xE4, "IN", "B", IF.None, "--------", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "al", "ib" })
            );
            _opToList[229] = new IL(0b11100101,
                new IE(0xE5, "IN", "W", IF.None, "--------", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "ax", "ib" })
            );
            _opToList[230] = new IL(0b11100110,
                new IE(0xE6, "OUT", "B", IF.None, "--------", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "ib", "al" })
            );
            _opToList[231] = new IL(0b11100111,
                new IE(0xE7, "OUT", "W", IF.None, "--------", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "ib", "ax" })
            );
            _opToList[232] = new IL(0b11101000,
                new IE(0xE8, "CALL", "", IF.None, "--------", "8086", 3, 3, new Field[] { "o0", "o1" }, Array.Empty<Operand>())
            );
            _opToList[233] = new IL(0b11101001,
                new IE(0xE9, "JMP", "", IF.None, "--------", "8086", 3, 3, new Field[] { "o0", "o1" }, Array.Empty<Operand>())
            );
            _opToList[234] = new IL(0b11101010,
                new IE(0xEA, "JMP", "", IF.Far, "--------", "8086", 5, 5, new Field[] { "o0", "o1", "s0", "s1" }, Array.Empty<Operand>())
            );
            _opToList[235] = new IL(0b11101011,
                new IE(0xEB, "JMP", "", IF.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "(short)sl" })
            );
            _opToList[236] = new IL(0b11101100,
                new IE(0xEC, "IN", "B", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "al", "dx" })
            );
            _opToList[237] = new IL(0b11101101,
                new IE(0xED, "IN", "W", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ax", "dx" })
            );
            _opToList[238] = new IL(0b11101110,
                new IE(0xEE, "OUT", "B", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "dx", "al" })
            );
            _opToList[239] = new IL(0b11101111,
                new IE(0xEF, "OUT", "W", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "dx", "ax" })
            );
            _opToList[240] = new IL(0b11110000,
                new IE(0xF0, "LOCK", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[242] = new IL(0b11110010,
                new IE(0xF2, "REPNE", "", IF.None, "-----z--", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[243] = new IL(0b11110011,
                new IE(0xF3, "REP", "", IF.None, "-----z--", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>()),
                new IE(0xF3, "REPE", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[244] = new IL(0b11110100,
                new IE(0xF4, "HLT", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[245] = new IL(0b11110101,
                new IE(0xF5, "CMC", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[246] = new IL(0b11110110,
                new IE(0xF6, "DIV", "B", IF.None, "o---szap", "8086", 2, 4, new Field[] { "/6", "d0", "d1" }, new Operand[] { "rmb" }),
                new IE(0xF6, "IDIV", "B", IF.None, "o---szap", "8086", 2, 4, new Field[] { "/7", "d0", "d1" }, new Operand[] { "rmb" }),
                new IE(0xF6, "IMUL", "B", IF.None, "o---szap", "8086", 2, 4, new Field[] { "/5", "d0", "d1" }, new Operand[] { "rmb" }),
                new IE(0xF6, "MUL", "B", IF.None, "o---szap", "8086", 2, 4, new Field[] { "/4", "d0", "d1" }, new Operand[] { "rmb" }),
                new IE(0xF6, "NEG", "B", IF.None, "o---szap", "8086", 2, 4, new Field[] { "/3", "d0", "d1" }, new Operand[] { "rmb" }),
                new IE(0xF6, "NOT", "B", IF.None, "--------", "8086", 2, 4, new Field[] { "/2", "d0", "d1" }, new Operand[] { "rmb" }),
                new IE(0xF6, "TEST", "B", IF.None, "0---szap", "8086", 3, 5, new Field[] { "/0", "d0", "d1", "i0" }, new Operand[] { "rmb", "ib" })
            );
            _opToList[247] = new IL(0b11110111,
                new IE(0xF7, "DIV", "W", IF.None, "o---szap", "8086", 2, 4, new Field[] { "/6", "d0", "d1" }, new Operand[] { "rmw" }),
                new IE(0xF7, "IDIV", "W", IF.None, "o---szap", "8086", 2, 4, new Field[] { "/7", "d0", "d1" }, new Operand[] { "rmw" }),
                new IE(0xF7, "IMUL", "W", IF.None, "o---szap", "8086", 2, 4, new Field[] { "/5", "d0", "d1" }, new Operand[] { "rmw" }),
                new IE(0xF7, "MUL", "W", IF.None, "o---szap", "8086", 2, 4, new Field[] { "/4", "d0", "d1" }, new Operand[] { "rmw" }),
                new IE(0xF7, "NEG", "W", IF.None, "o---szap", "8086", 2, 4, new Field[] { "/3", "d0", "d1" }, new Operand[] { "rmw" }),
                new IE(0xF7, "NOT", "W", IF.None, "--------", "8086", 2, 4, new Field[] { "/2", "d0", "d1" }, new Operand[] { "rmw" }),
                new IE(0xF7, "TEST", "W", IF.None, "0---szap", "8086", 4, 6, new Field[] { "/0", "d0", "d1", "i0", "i1" }, new Operand[] { "rmw", "iw" })
            );
            _opToList[248] = new IL(0b11111000,
                new IE(0xF8, "CLC", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[249] = new IL(0b11111001,
                new IE(0xF9, "STC", "", IF.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[250] = new IL(0b11111010,
                new IE(0xFA, "CLI", "", IF.None, "--0-----", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[251] = new IL(0b11111011,
                new IE(0xFB, "STI", "", IF.None, "--1-----", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[252] = new IL(0b11111100,
                new IE(0xFC, "CLD", "", IF.None, "-0------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[253] = new IL(0b11111101,
                new IE(0xFD, "STD", "", IF.None, "-1------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[254] = new IL(0b11111110,
                new IE(0xFE, "DEC", "", IF.None, "o---szap", "8086", 2, 4, new Field[] { "/1", "d0", "d1" }, new Operand[] { "rmb" }),
                new IE(0xFE, "INC", "", IF.None, "o---szap", "8086", 2, 4, new Field[] { "/0", "d0", "d1" }, new Operand[] { "rmb" })
            );
            _opToList[255] = new IL(0b11111111,
                new IE(0xFF, "CALL", "W", IF.None, "--------", "8086", 2, 4, new Field[] { "/2", "d0", "d1" }, new Operand[] { "rw" }),
                new IE(0xFF, "CALL", "W", IF.None, "--------", "8086", 2, 4, new Field[] { "/3", "d0", "d1" }, new Operand[] { "(dword ptr)rw" }),
                new IE(0xFF, "DEC", "", IF.None, "o---szap", "8086", 2, 4, new Field[] { "/1", "d0", "d1" }, new Operand[] { "rmw" }),
                new IE(0xFF, "INC", "", IF.None, "o---szap", "8086", 2, 4, new Field[] { "/0", "d0", "d1" }, new Operand[] { "rmw" }),
                new IE(0xFF, "JMP", "", IF.None, "--------", "8086", 2, 4, new Field[] { "/4", "d0", "d1" }, new Operand[] { "rmw" }),
                new IE(0xFF, "JMP", "", IF.None, "--------", "8086", 2, 4, new Field[] { "/5", "d0", "d1" }, new Operand[] { "(dword ptr)rmw" }),
                new IE(0xFF, "PUSH", "", IF.None, "--------", "8086", 2, 4, new Field[] { "/6", "d0", "d1" }, new Operand[] { "rmw" })
            );
        }

        public int Count => _opToList.Length;

        public IL GetOrCreate(byte op)
        {
            Debug.Assert(op < _opToList.Length);
            if (_opToList[op] == null)
                _opToList[op] = new IL(op);
            return _opToList[op];
        }

        public IEnumerator<IL> GetEnumerator() => _opToList.Cast<IL>().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
