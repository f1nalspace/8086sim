using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using IL = Final.CPU8086.InstructionList;
using IE = Final.CPU8086.InstructionEntry;
using IT = Final.CPU8086.InstructionType;
using IF = Final.CPU8086.InstructionFlags;
using DT = Final.CPU8086.DataType;
using MNE = Final.CPU8086.Mnemonic;
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
                new IE(0x00, new MNE(IT.ADD, "ADD"), "B", IF.None, DT.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmb", "rb" })
            );
            _opToList[1] = new IL(0b00000001,
                new IE(0x01, new MNE(IT.ADD, "ADD"), "W", IF.None, DT.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmw", "rw" })
            );
            _opToList[2] = new IL(0b00000010,
                new IE(0x02, new MNE(IT.ADD, "ADD"), "B", IF.None, DT.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rb", "rmb" })
            );
            _opToList[3] = new IL(0b00000011,
                new IE(0x03, new MNE(IT.ADD, "ADD"), "W", IF.None, DT.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rw", "rmw" })
            );
            _opToList[4] = new IL(0b00000100,
                new IE(0x04, new MNE(IT.ADD, "ADD"), "B", IF.None, DT.None, "o---szap", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "al", "ib" })
            );
            _opToList[5] = new IL(0b00000101,
                new IE(0x05, new MNE(IT.ADD, "ADD"), "W", IF.None, DT.None, "o---szap", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "ax", "iw" })
            );
            _opToList[6] = new IL(0b00000110,
                new IE(0x06, new MNE(IT.PUSH, "PUSH"), "", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "es" })
            );
            _opToList[7] = new IL(0b00000111,
                new IE(0x07, new MNE(IT.POP, "POP"), "", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "es" })
            );
            _opToList[8] = new IL(0b00001000,
                new IE(0x08, new MNE(IT.OR, "OR"), "B", IF.None, DT.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmb", "rb" })
            );
            _opToList[9] = new IL(0b00001001,
                new IE(0x09, new MNE(IT.OR, "OR"), "W", IF.None, DT.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmw", "rw" })
            );
            _opToList[10] = new IL(0b00001010,
                new IE(0x0A, new MNE(IT.OR, "OR"), "B", IF.None, DT.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rb", "rmb" })
            );
            _opToList[11] = new IL(0b00001011,
                new IE(0x0B, new MNE(IT.OR, "OR"), "W", IF.None, DT.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rw", "rmw" })
            );
            _opToList[12] = new IL(0b00001100,
                new IE(0x0C, new MNE(IT.OR, "OR"), "B", IF.None, DT.None, "o---szap", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "al", "ib" })
            );
            _opToList[13] = new IL(0b00001101,
                new IE(0x0D, new MNE(IT.OR, "OR"), "W", IF.None, DT.None, "o---szap", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "ax", "iw" })
            );
            _opToList[14] = new IL(0b00001110,
                new IE(0x0E, new MNE(IT.PUSH, "PUSH"), "", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "cs" })
            );
            _opToList[16] = new IL(0b00010000,
                new IE(0x10, new MNE(IT.ADC, "ADC"), "B", IF.None, DT.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmb", "rb" })
            );
            _opToList[17] = new IL(0b00010001,
                new IE(0x11, new MNE(IT.ADC, "ADC"), "W", IF.None, DT.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmw", "rw" })
            );
            _opToList[18] = new IL(0b00010010,
                new IE(0x12, new MNE(IT.ADC, "ADC"), "B", IF.None, DT.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rb", "rmb" })
            );
            _opToList[19] = new IL(0b00010011,
                new IE(0x13, new MNE(IT.ADC, "ADC"), "W", IF.None, DT.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rw", "rmw" })
            );
            _opToList[20] = new IL(0b00010100,
                new IE(0x14, new MNE(IT.ADC, "ADC"), "B", IF.None, DT.None, "o---szap", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "al", "ib" })
            );
            _opToList[21] = new IL(0b00010101,
                new IE(0x15, new MNE(IT.ADC, "ADC"), "W", IF.None, DT.None, "o---szap", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "ax", "iw" })
            );
            _opToList[22] = new IL(0b00010110,
                new IE(0x16, new MNE(IT.PUSH, "PUSH"), "", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ss" })
            );
            _opToList[23] = new IL(0b00010111,
                new IE(0x17, new MNE(IT.POP, "POP"), "", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ss" })
            );
            _opToList[24] = new IL(0b00011000,
                new IE(0x18, new MNE(IT.SBB, "SBB"), "B", IF.None, DT.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmb", "rb" })
            );
            _opToList[25] = new IL(0b00011001,
                new IE(0x19, new MNE(IT.SBB, "SBB"), "W", IF.None, DT.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmw", "rw" })
            );
            _opToList[26] = new IL(0b00011010,
                new IE(0x1A, new MNE(IT.SBB, "SBB"), "B", IF.None, DT.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rb", "rmb" })
            );
            _opToList[27] = new IL(0b00011011,
                new IE(0x1B, new MNE(IT.SBB, "SBB"), "W", IF.None, DT.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rw", "rmw" })
            );
            _opToList[28] = new IL(0b00011100,
                new IE(0x1C, new MNE(IT.SBB, "SBB"), "B", IF.None, DT.None, "o---szap", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "al", "ib" })
            );
            _opToList[29] = new IL(0b00011101,
                new IE(0x1D, new MNE(IT.SBB, "SBB"), "W", IF.None, DT.None, "o---szap", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "ax", "iw" })
            );
            _opToList[30] = new IL(0b00011110,
                new IE(0x1E, new MNE(IT.PUSH, "PUSH"), "", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ds" })
            );
            _opToList[31] = new IL(0b00011111,
                new IE(0x1F, new MNE(IT.POP, "POP"), "", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ds" })
            );
            _opToList[32] = new IL(0b00100000,
                new IE(0x20, new MNE(IT.AND, "AND"), "B", IF.None, DT.None, "0---sz-p", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmb", "rb" })
            );
            _opToList[33] = new IL(0b00100001,
                new IE(0x21, new MNE(IT.AND, "AND"), "W", IF.None, DT.None, "0---sz-p", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmw", "rw" })
            );
            _opToList[34] = new IL(0b00100010,
                new IE(0x22, new MNE(IT.AND, "AND"), "B", IF.None, DT.None, "0---sz-p", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rb", "rmb" })
            );
            _opToList[35] = new IL(0b00100011,
                new IE(0x23, new MNE(IT.AND, "AND"), "W", IF.None, DT.None, "0---sz-p", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rw", "rmw" })
            );
            _opToList[36] = new IL(0b00100100,
                new IE(0x24, new MNE(IT.AND, "AND"), "B", IF.None, DT.None, "0---sz-p", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "al", "ib" })
            );
            _opToList[37] = new IL(0b00100101,
                new IE(0x25, new MNE(IT.AND, "AND"), "W", IF.None, DT.None, "0---sz-p", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "ax", "iw" })
            );
            _opToList[38] = new IL(0b00100110,
                new IE(0x26, new MNE(IT.ES, "ES"), "", IF.Segment | IF.Prefix, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "es" })
            );
            _opToList[39] = new IL(0b00100111,
                new IE(0x27, new MNE(IT.DAA, "DAA"), "", IF.None, DT.None, "----szap", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[40] = new IL(0b00101000,
                new IE(0x28, new MNE(IT.SUB, "SUB"), "B", IF.None, DT.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmb", "rb" })
            );
            _opToList[41] = new IL(0b00101001,
                new IE(0x29, new MNE(IT.SUB, "SUB"), "W", IF.None, DT.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmw", "rw" })
            );
            _opToList[42] = new IL(0b00101010,
                new IE(0x2A, new MNE(IT.SUB, "SUB"), "B", IF.None, DT.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rb", "rmb" })
            );
            _opToList[43] = new IL(0b00101011,
                new IE(0x2B, new MNE(IT.SUB, "SUB"), "W", IF.None, DT.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rw", "rmw" })
            );
            _opToList[44] = new IL(0b00101100,
                new IE(0x2C, new MNE(IT.SUB, "SUB"), "B", IF.None, DT.None, "o---szap", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "al", "ib" })
            );
            _opToList[45] = new IL(0b00101101,
                new IE(0x2D, new MNE(IT.SUB, "SUB"), "W", IF.None, DT.None, "o---szap", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "ax", "iw" })
            );
            _opToList[46] = new IL(0b00101110,
                new IE(0x2E, new MNE(IT.CS, "CS"), "", IF.Segment | IF.Prefix, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "cs" })
            );
            _opToList[47] = new IL(0b00101111,
                new IE(0x2F, new MNE(IT.DAS, "DAS"), "", IF.None, DT.None, "----szap", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[48] = new IL(0b00110000,
                new IE(0x30, new MNE(IT.XOR, "XOR"), "B", IF.None, DT.None, "0---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmb", "rb" })
            );
            _opToList[49] = new IL(0b00110001,
                new IE(0x31, new MNE(IT.XOR, "XOR"), "W", IF.None, DT.None, "0---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmw", "rw" })
            );
            _opToList[50] = new IL(0b00110010,
                new IE(0x32, new MNE(IT.XOR, "XOR"), "B", IF.None, DT.None, "0---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rb", "rmb" })
            );
            _opToList[51] = new IL(0b00110011,
                new IE(0x33, new MNE(IT.XOR, "XOR"), "W", IF.None, DT.None, "0---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rw", "rmw" })
            );
            _opToList[52] = new IL(0b00110100,
                new IE(0x34, new MNE(IT.XOR, "XOR"), "B", IF.None, DT.None, "0---szap", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "al", "ib" })
            );
            _opToList[53] = new IL(0b00110101,
                new IE(0x35, new MNE(IT.XOR, "XOR"), "W", IF.None, DT.None, "0---szap", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "ax", "iw" })
            );
            _opToList[54] = new IL(0b00110110,
                new IE(0x36, new MNE(IT.SS, "SS"), "", IF.Segment | IF.Prefix, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ss" })
            );
            _opToList[55] = new IL(0b00110111,
                new IE(0x37, new MNE(IT.AAA, "AAA"), "", IF.None, DT.None, "------a-", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[56] = new IL(0b00111000,
                new IE(0x38, new MNE(IT.CMP, "CMP"), "B", IF.None, DT.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmb", "rb" })
            );
            _opToList[57] = new IL(0b00111001,
                new IE(0x39, new MNE(IT.CMP, "CMP"), "W", IF.None, DT.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmw", "rw" })
            );
            _opToList[58] = new IL(0b00111010,
                new IE(0x3A, new MNE(IT.CMP, "CMP"), "B", IF.None, DT.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rb", "rmb" })
            );
            _opToList[59] = new IL(0b00111011,
                new IE(0x3B, new MNE(IT.CMP, "CMP"), "W", IF.None, DT.None, "o---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rw", "rmw" })
            );
            _opToList[60] = new IL(0b00111100,
                new IE(0x3C, new MNE(IT.CMP, "CMP"), "B", IF.None, DT.None, "o---szap", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "al", "ib" })
            );
            _opToList[61] = new IL(0b00111101,
                new IE(0x3D, new MNE(IT.CMP, "CMP"), "W", IF.None, DT.None, "o---szap", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "ax", "iw" })
            );
            _opToList[62] = new IL(0b00111110,
                new IE(0x3E, new MNE(IT.DS, "DS"), "", IF.Segment | IF.Prefix, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ds" })
            );
            _opToList[63] = new IL(0b00111111,
                new IE(0x3F, new MNE(IT.AAS, "AAS"), "", IF.None, DT.None, "------a-", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[64] = new IL(0b01000000,
                new IE(0x40, new MNE(IT.INC, "INC"), "", IF.None, DT.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ax" })
            );
            _opToList[65] = new IL(0b01000001,
                new IE(0x41, new MNE(IT.INC, "INC"), "", IF.None, DT.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "cx" })
            );
            _opToList[66] = new IL(0b01000010,
                new IE(0x42, new MNE(IT.INC, "INC"), "", IF.None, DT.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "dx" })
            );
            _opToList[67] = new IL(0b01000011,
                new IE(0x43, new MNE(IT.INC, "INC"), "", IF.None, DT.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "bx" })
            );
            _opToList[68] = new IL(0b01000100,
                new IE(0x44, new MNE(IT.INC, "INC"), "", IF.None, DT.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "sp" })
            );
            _opToList[69] = new IL(0b01000101,
                new IE(0x45, new MNE(IT.INC, "INC"), "", IF.None, DT.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "bp" })
            );
            _opToList[70] = new IL(0b01000110,
                new IE(0x46, new MNE(IT.INC, "INC"), "", IF.None, DT.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "si" })
            );
            _opToList[71] = new IL(0b01000111,
                new IE(0x47, new MNE(IT.INC, "INC"), "", IF.None, DT.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "di" })
            );
            _opToList[72] = new IL(0b01001000,
                new IE(0x48, new MNE(IT.DEC, "DEC"), "", IF.None, DT.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ax" })
            );
            _opToList[73] = new IL(0b01001001,
                new IE(0x49, new MNE(IT.DEC, "DEC"), "", IF.None, DT.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "cx" }),
                new IE(0x49, new MNE(IT.DEC, "DEC"), "", IF.None, DT.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "dx" })
            );
            _opToList[74] = new IL(0b01001010,
                new IE(0x4A, new MNE(IT.DEC, "DEC"), "", IF.None, DT.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "bx" })
            );
            _opToList[75] = new IL(0b01001011,
                new IE(0x4B, new MNE(IT.DEC, "DEC"), "", IF.None, DT.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "sp" })
            );
            _opToList[76] = new IL(0b01001100,
                new IE(0x4C, new MNE(IT.DEC, "DEC"), "", IF.None, DT.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "bp" })
            );
            _opToList[77] = new IL(0b01001101,
                new IE(0x4D, new MNE(IT.DEC, "DEC"), "", IF.None, DT.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "si" })
            );
            _opToList[79] = new IL(0b01001111,
                new IE(0x4F, new MNE(IT.DEC, "DEC"), "", IF.None, DT.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "di" })
            );
            _opToList[80] = new IL(0b01010000,
                new IE(0x50, new MNE(IT.PUSH, "PUSH"), "", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ax" })
            );
            _opToList[81] = new IL(0b01010001,
                new IE(0x51, new MNE(IT.PUSH, "PUSH"), "", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "cx" })
            );
            _opToList[82] = new IL(0b01010010,
                new IE(0x52, new MNE(IT.PUSH, "PUSH"), "", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "dx" })
            );
            _opToList[83] = new IL(0b01010011,
                new IE(0x53, new MNE(IT.PUSH, "PUSH"), "", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "bx" })
            );
            _opToList[84] = new IL(0b01010100,
                new IE(0x54, new MNE(IT.PUSH, "PUSH"), "", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "sp" })
            );
            _opToList[85] = new IL(0b01010101,
                new IE(0x55, new MNE(IT.PUSH, "PUSH"), "", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "bp" })
            );
            _opToList[86] = new IL(0b01010110,
                new IE(0x56, new MNE(IT.PUSH, "PUSH"), "", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "si" })
            );
            _opToList[87] = new IL(0b01010111,
                new IE(0x57, new MNE(IT.PUSH, "PUSH"), "", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "di" })
            );
            _opToList[88] = new IL(0b01011000,
                new IE(0x58, new MNE(IT.POP, "POP"), "", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ax" })
            );
            _opToList[89] = new IL(0b01011001,
                new IE(0x59, new MNE(IT.POP, "POP"), "", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "cx" })
            );
            _opToList[90] = new IL(0b01011010,
                new IE(0x5A, new MNE(IT.POP, "POP"), "", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "dx" })
            );
            _opToList[91] = new IL(0b01011011,
                new IE(0x5B, new MNE(IT.POP, "POP"), "", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "bx" })
            );
            _opToList[92] = new IL(0b01011100,
                new IE(0x5C, new MNE(IT.POP, "POP"), "", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "sp" })
            );
            _opToList[93] = new IL(0b01011101,
                new IE(0x5D, new MNE(IT.POP, "POP"), "", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "bp" })
            );
            _opToList[94] = new IL(0b01011110,
                new IE(0x5E, new MNE(IT.POP, "POP"), "", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "si" })
            );
            _opToList[95] = new IL(0b01011111,
                new IE(0x5F, new MNE(IT.POP, "POP"), "", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "di" })
            );
            _opToList[100] = new IL(0b01100100,
                new IE(0x64, new MNE(IT.FS, "FS"), "", IF.Segment | IF.Prefix, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "fs" })
            );
            _opToList[101] = new IL(0b01100101,
                new IE(0x65, new MNE(IT.GS, "GS"), "", IF.Segment | IF.Prefix, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "gs" })
            );
            _opToList[102] = new IL(0b01100110,
                new IE(0x66, new MNE(IT.DATA8, "DATA8"), "B", IF.Prefix | IF.Override, DT.Byte, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>()),
                new IE(0x66, new MNE(IT.DATA16, "DATA16"), "W", IF.Prefix | IF.Override, DT.Word, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[103] = new IL(0b01100111,
                new IE(0x67, new MNE(IT.ADDR8, "ADDR8"), "B", IF.Prefix | IF.Override, DT.Byte, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>()),
                new IE(0x67, new MNE(IT.ADDR16, "ADDR16"), "W", IF.Prefix | IF.Override, DT.Word, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[105] = new IL(0b01101001,
                new IE(0x69, new MNE(IT.IMUL, "IMUL"), "W", IF.None, DT.None, "o---szap", "8086", 6, 6, new Field[] { "mr", "i0", "i1", "i2", "i3" }, new Operand[] { "rd", "id" }),
                new IE(0x69, new MNE(IT.IMUL, "IMUL"), "W", IF.None, DT.None, "o---szap", "8086", 6, 6, new Field[] { "mr", "d0", "d1", "i0~i3" }, new Operand[] { "rd", "rmd", "id" }),
                new IE(0x69, new MNE(IT.IMUL, "IMUL"), "B", IF.None, DT.None, "o---szap", "8086", 4, 4, new Field[] { "mr", "i0", "i1" }, new Operand[] { "rw", "iw" }),
                new IE(0x69, new MNE(IT.IMUL, "IMUL"), "B", IF.None, DT.None, "o---szap", "8086", 4, 6, new Field[] { "mr", "d0", "d1", "i0", "i1" }, new Operand[] { "rw", "rmw", "iw" })
            );
            _opToList[107] = new IL(0b01101011,
                new IE(0x6B, new MNE(IT.IMUL, "IMUL"), "W", IF.None, DT.None, "o---szap", "8086", 3, 3, new Field[] { "mr", "i0" }, new Operand[] { "rd", "ib" }),
                new IE(0x6B, new MNE(IT.IMUL, "IMUL"), "W", IF.None, DT.None, "o---szap", "8086", 3, 5, new Field[] { "mr", "d0", "d1", "i0" }, new Operand[] { "rd", "rmd", "ib" }),
                new IE(0x6B, new MNE(IT.IMUL, "IMUL"), "B", IF.None, DT.None, "o---szap", "8086", 3, 3, new Field[] { "mr", "i0" }, new Operand[] { "rw", "ib" }),
                new IE(0x6B, new MNE(IT.IMUL, "IMUL"), "B", IF.None, DT.None, "o---szap", "8086", 3, 5, new Field[] { "mr", "d0", "d1", "i0" }, new Operand[] { "rw", "rmw", "ib" })
            );
            _opToList[112] = new IL(0b01110000,
                new IE(0x70, new MNE(IT.JO, "JO"), "", IF.None, DT.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[113] = new IL(0b01110001,
                new IE(0x71, new MNE(IT.JNO, "JNO"), "", IF.None, DT.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[114] = new IL(0b01110010,
                new IE(0x72, new MNE(IT.JB, "JB"), "", IF.None, DT.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" }),
                new IE(0x72, new MNE(IT.JC, "JC"), "", IF.None, DT.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[115] = new IL(0b01110011,
                new IE(0x73, new MNE(IT.JAE, "JAE"), "", IF.None, DT.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" }),
                new IE(0x73, new MNE(IT.JNC, "JNC"), "", IF.None, DT.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[116] = new IL(0b01110100,
                new IE(0x74, new MNE(IT.JE, "JE"), "", IF.None, DT.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[117] = new IL(0b01110101,
                new IE(0x75, new MNE(IT.JNE, "JNE"), "", IF.None, DT.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[118] = new IL(0b01110110,
                new IE(0x76, new MNE(IT.JBE, "JBE"), "", IF.None, DT.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[119] = new IL(0b01110111,
                new IE(0x77, new MNE(IT.JA, "JA"), "", IF.None, DT.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[120] = new IL(0b01111000,
                new IE(0x78, new MNE(IT.JS, "JS"), "", IF.None, DT.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[121] = new IL(0b01111001,
                new IE(0x79, new MNE(IT.JNS, "JNS"), "", IF.None, DT.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[122] = new IL(0b01111010,
                new IE(0x7A, new MNE(IT.JP, "JP"), "", IF.None, DT.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[123] = new IL(0b01111011,
                new IE(0x7B, new MNE(IT.JNP, "JNP"), "", IF.None, DT.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[124] = new IL(0b01111100,
                new IE(0x7C, new MNE(IT.JL, "JL"), "", IF.None, DT.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[125] = new IL(0b01111101,
                new IE(0x7D, new MNE(IT.JGE, "JGE"), "", IF.None, DT.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[126] = new IL(0b01111110,
                new IE(0x7E, new MNE(IT.JLE, "JLE"), "", IF.None, DT.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[127] = new IL(0b01111111,
                new IE(0x7F, new MNE(IT.JG, "JG"), "", IF.None, DT.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[128] = new IL(0b10000000,
                new IE(0x80, new MNE(IT.ADC, "ADC"), "B", IF.None, DT.None, "o---szap", "8086", 3, 5, new Field[] { "/2", "d0", "d1", "i0" }, new Operand[] { "rmb", "ib" }),
                new IE(0x80, new MNE(IT.ADD, "ADD"), "B", IF.None, DT.None, "o---szap", "8086", 3, 5, new Field[] { "/0", "d0", "d1", "i0" }, new Operand[] { "rmb", "ib" }),
                new IE(0x80, new MNE(IT.AND, "AND"), "B", IF.None, DT.None, "0---sz-p", "8086", 3, 5, new Field[] { "/4", "d0", "d1", "i0" }, new Operand[] { "rmb", "ib" }),
                new IE(0x80, new MNE(IT.CMP, "CMP"), "B", IF.None, DT.None, "o---szap", "8086", 3, 5, new Field[] { "/7", "d0", "d1", "i0" }, new Operand[] { "rmb", "ib" }),
                new IE(0x80, new MNE(IT.OR, "OR"), "B", IF.None, DT.None, "o---szap", "8086", 3, 5, new Field[] { "/1", "d0", "d1", "i0" }, new Operand[] { "rmb", "ib" }),
                new IE(0x80, new MNE(IT.SBB, "SBB"), "B", IF.None, DT.None, "o---szap", "8086", 3, 5, new Field[] { "/3", "d0", "d1", "i0" }, new Operand[] { "rmb", "ib" }),
                new IE(0x80, new MNE(IT.SUB, "SUB"), "B", IF.None, DT.None, "o---szap", "8086", 3, 5, new Field[] { "/5", "d0", "d1", "i0" }, new Operand[] { "rmb", "ib" }),
                new IE(0x80, new MNE(IT.XOR, "XOR"), "B", IF.None, DT.None, "0---szap", "8086", 3, 5, new Field[] { "/6", "d0", "d1", "i0" }, new Operand[] { "rmb", "ib" })
            );
            _opToList[129] = new IL(0b10000001,
                new IE(0x81, new MNE(IT.ADC, "ADC"), "W", IF.None, DT.None, "o---szap", "8086", 4, 6, new Field[] { "/2", "d0", "d1", "i0", "i1" }, new Operand[] { "rmw", "iw" }),
                new IE(0x81, new MNE(IT.ADD, "ADD"), "W", IF.None, DT.None, "o---szap", "8086", 4, 6, new Field[] { "/0", "d0", "d1", "i0", "i1" }, new Operand[] { "rmw", "iw" }),
                new IE(0x81, new MNE(IT.AND, "AND"), "W", IF.None, DT.None, "0---sz-p", "8086", 4, 6, new Field[] { "/4", "d0", "d1", "i0", "i1" }, new Operand[] { "rmw", "iw" }),
                new IE(0x81, new MNE(IT.CMP, "CMP"), "W", IF.None, DT.None, "o---szap", "8086", 4, 6, new Field[] { "/7", "d0", "d1", "i0", "i1" }, new Operand[] { "rmw", "iw" }),
                new IE(0x81, new MNE(IT.OR, "OR"), "W", IF.None, DT.None, "o---szap", "8086", 4, 6, new Field[] { "/1", "d0", "d1", "i0", "i1" }, new Operand[] { "rmw", "iw" }),
                new IE(0x81, new MNE(IT.SBB, "SBB"), "W", IF.None, DT.None, "o---szap", "8086", 4, 6, new Field[] { "/3", "d0", "d1", "i0", "i1" }, new Operand[] { "rmw", "iw" }),
                new IE(0x81, new MNE(IT.SUB, "SUB"), "W", IF.None, DT.None, "o---szap", "8086", 4, 6, new Field[] { "/5", "d0", "d1", "i0", "i1" }, new Operand[] { "rmw", "iw" }),
                new IE(0x81, new MNE(IT.XOR, "XOR"), "W", IF.None, DT.None, "0---szap", "8086", 4, 6, new Field[] { "/6", "d0", "d1", "i0", "i1" }, new Operand[] { "rmw", "iw" })
            );
            _opToList[131] = new IL(0b10000011,
                new IE(0x83, new MNE(IT.ADC, "ADC"), "W", IF.SignExtendedImm8, DT.None, "o---szap", "8086", 3, 5, new Field[] { "/2", "d0", "d1", "i0" }, new Operand[] { "rmw", "ib" }),
                new IE(0x83, new MNE(IT.ADD, "ADD"), "W", IF.SignExtendedImm8, DT.None, "o---szap", "8086", 3, 5, new Field[] { "/0", "d0", "d1", "i0" }, new Operand[] { "rmw", "ib" }),
                new IE(0x83, new MNE(IT.AND, "AND"), "W", IF.SignExtendedImm8, DT.None, "0---sz-p", "8086", 3, 5, new Field[] { "/4", "d0", "d1", "i0" }, new Operand[] { "rmw", "ib" }),
                new IE(0x83, new MNE(IT.CMP, "CMP"), "W", IF.SignExtendedImm8, DT.None, "o---szap", "8086", 3, 5, new Field[] { "/7", "d0", "d1", "i0" }, new Operand[] { "rmw", "ib" }),
                new IE(0x83, new MNE(IT.OR, "OR"), "W", IF.SignExtendedImm8, DT.None, "o---szap", "8086", 3, 5, new Field[] { "/1", "d0", "d1", "i0" }, new Operand[] { "rmw", "ib" }),
                new IE(0x83, new MNE(IT.SBB, "SBB"), "W", IF.SignExtendedImm8, DT.None, "o---szap", "8086", 3, 5, new Field[] { "/3", "d0", "d1", "i0" }, new Operand[] { "rmw", "ib" }),
                new IE(0x83, new MNE(IT.SUB, "SUB"), "W", IF.SignExtendedImm8, DT.None, "o---szap", "8086", 3, 5, new Field[] { "/5", "d0", "d1", "i0" }, new Operand[] { "rmw", "ib" }),
                new IE(0x83, new MNE(IT.XOR, "XOR"), "W", IF.SignExtendedImm8, DT.None, "0---szap", "8086", 3, 5, new Field[] { "/6", "d0", "d1", "i0" }, new Operand[] { "rmw", "ib" })
            );
            _opToList[132] = new IL(0b10000100,
                new IE(0x84, new MNE(IT.TEST, "TEST"), "B", IF.None, DT.None, "0---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmb", "rmb" })
            );
            _opToList[133] = new IL(0b10000101,
                new IE(0x85, new MNE(IT.TEST, "TEST"), "W", IF.None, DT.None, "0---szap", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmw", "rmw" })
            );
            _opToList[134] = new IL(0b10000110,
                new IE(0x86, new MNE(IT.XCHG, "XCHG"), "B", IF.None, DT.None, "--------", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rb", "rmb" }),
                new IE(0x86, new MNE(IT.XCHG, "XCHG"), "B", IF.None, DT.None, "--------", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmb", "rb" })
            );
            _opToList[135] = new IL(0b10000111,
                new IE(0x87, new MNE(IT.XCHG, "XCHG"), "W", IF.None, DT.None, "--------", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmw", "rw" }),
                new IE(0x87, new MNE(IT.XCHG, "XCHG"), "W", IF.None, DT.None, "--------", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rw", "rmw" })
            );
            _opToList[136] = new IL(0b10001000,
                new IE(0x88, new MNE(IT.MOV, "MOV"), "B", IF.None, DT.None, "--------", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmb", "rb" })
            );
            _opToList[137] = new IL(0b10001001,
                new IE(0x89, new MNE(IT.MOV, "MOV"), "W", IF.None, DT.None, "--------", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmw", "rw" })
            );
            _opToList[138] = new IL(0b10001010,
                new IE(0x8A, new MNE(IT.MOV, "MOV"), "B", IF.None, DT.None, "--------", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rb", "rmb" })
            );
            _opToList[139] = new IL(0b10001011,
                new IE(0x8B, new MNE(IT.MOV, "MOV"), "W", IF.None, DT.None, "--------", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rw", "rmw" })
            );
            _opToList[140] = new IL(0b10001100,
                new IE(0x8C, new MNE(IT.MOV, "MOV"), "", IF.None, DT.None, "--------", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmw", "sr" })
            );
            _opToList[141] = new IL(0b10001101,
                new IE(0x8D, new MNE(IT.LEA, "LEA"), "", IF.None, DT.None, "--------", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rw", "mw" })
            );
            _opToList[142] = new IL(0b10001110,
                new IE(0x8E, new MNE(IT.MOV, "MOV"), "", IF.None, DT.None, "--------", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "sr", "rmw" })
            );
            _opToList[143] = new IL(0b10001111,
                new IE(0x8F, new MNE(IT.POP, "POP"), "", IF.None, DT.None, "--------", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rmw" })
            );
            _opToList[144] = new IL(0b10010000,
                new IE(0x90, new MNE(IT.NOP, "NOP"), "", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[145] = new IL(0b10010001,
                new IE(0x91, new MNE(IT.XCHG, "XCHG"), "", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ax", "cx" })
            );
            _opToList[146] = new IL(0b10010010,
                new IE(0x92, new MNE(IT.XCHG, "XCHG"), "", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ax", "dx" })
            );
            _opToList[147] = new IL(0b10010011,
                new IE(0x93, new MNE(IT.XCHG, "XCHG"), "", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ax", "bx" })
            );
            _opToList[148] = new IL(0b10010100,
                new IE(0x94, new MNE(IT.XCHG, "XCHG"), "", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ax", "sp" })
            );
            _opToList[149] = new IL(0b10010101,
                new IE(0x95, new MNE(IT.XCHG, "XCHG"), "", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ax", "bp" })
            );
            _opToList[150] = new IL(0b10010110,
                new IE(0x96, new MNE(IT.XCHG, "XCHG"), "", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ax", "si" })
            );
            _opToList[151] = new IL(0b10010111,
                new IE(0x97, new MNE(IT.XCHG, "XCHG"), "", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ax", "di" })
            );
            _opToList[152] = new IL(0b10011000,
                new IE(0x98, new MNE(IT.CBW, "CBW"), "", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[153] = new IL(0b10011001,
                new IE(0x99, new MNE(IT.CWD, "CWD"), "", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[154] = new IL(0b10011010,
                new IE(0x9A, new MNE(IT.CALL, "CALL"), "", IF.Far, DT.Pointer, "--------", "8086", 5, 5, new Field[] { "o0", "o1", "sl", "sh" }, new Operand[] { "far", "ptr", "fp" })
            );
            _opToList[155] = new IL(0b10011011,
                new IE(0x9B, new MNE(IT.WAIT, "WAIT"), "", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[156] = new IL(0b10011100,
                new IE(0x9C, new MNE(IT.PUSHF, "PUSHF"), "", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[157] = new IL(0b10011101,
                new IE(0x9D, new MNE(IT.POPF, "POPF"), "", IF.None, DT.None, "oditszap", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[158] = new IL(0b10011110,
                new IE(0x9E, new MNE(IT.SAHF, "SAHF"), "", IF.None, DT.None, "----szap", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[159] = new IL(0b10011111,
                new IE(0x9F, new MNE(IT.LAHF, "LAHF"), "", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[160] = new IL(0b10100000,
                new IE(0xA0, new MNE(IT.MOV, "MOV"), "B", IF.None, DT.None, "--------", "8086", 3, 3, new Field[] { "d0", "d1" }, new Operand[] { "al", "rmb" })
            );
            _opToList[161] = new IL(0b10100001,
                new IE(0xA1, new MNE(IT.MOV, "MOV"), "W", IF.None, DT.None, "--------", "8086", 3, 3, new Field[] { "d0", "d1" }, new Operand[] { "ax", "rmw" })
            );
            _opToList[162] = new IL(0b10100010,
                new IE(0xA2, new MNE(IT.MOV, "MOV"), "B", IF.None, DT.None, "--------", "8086", 3, 3, new Field[] { "d0", "d1" }, new Operand[] { "rmb", "al" })
            );
            _opToList[163] = new IL(0b10100011,
                new IE(0xA3, new MNE(IT.MOV, "MOV"), "W", IF.None, DT.None, "--------", "8086", 3, 3, new Field[] { "d0", "d1" }, new Operand[] { "rmw", "ax" })
            );
            _opToList[164] = new IL(0b10100100,
                new IE(0xA4, new MNE(IT.MOVSB, "MOVSB"), "B", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[165] = new IL(0b10100101,
                new IE(0xA5, new MNE(IT.MOVSW, "MOVSW"), "W", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[166] = new IL(0b10100110,
                new IE(0xA6, new MNE(IT.CMPSB, "CMPSB"), "B", IF.None, DT.None, "od--szap", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[167] = new IL(0b10100111,
                new IE(0xA7, new MNE(IT.CMPSW, "CMPSW"), "W", IF.None, DT.None, "od--szap", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[168] = new IL(0b10101000,
                new IE(0xA8, new MNE(IT.TEST, "TEST"), "B", IF.None, DT.None, "0---szap", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "al", "ib" })
            );
            _opToList[169] = new IL(0b10101001,
                new IE(0xA9, new MNE(IT.TEST, "TEST"), "W", IF.None, DT.None, "0---szap", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "ax", "iw" })
            );
            _opToList[170] = new IL(0b10101010,
                new IE(0xAA, new MNE(IT.STOSB, "STOSB"), "B", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[171] = new IL(0b10101011,
                new IE(0xAB, new MNE(IT.STOSW, "STOSW"), "W", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[172] = new IL(0b10101100,
                new IE(0xAC, new MNE(IT.LODSB, "LODSB"), "B", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[173] = new IL(0b10101101,
                new IE(0xAD, new MNE(IT.LODSW, "LODSW"), "W", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[174] = new IL(0b10101110,
                new IE(0xAE, new MNE(IT.SCASB, "SCASB"), "B", IF.None, DT.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[175] = new IL(0b10101111,
                new IE(0xAF, new MNE(IT.SCASW, "SCASW"), "W", IF.None, DT.None, "o---szap", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[176] = new IL(0b10110000,
                new IE(0xB0, new MNE(IT.MOV, "MOV"), "B", IF.None, DT.None, "--------", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "al", "ib" })
            );
            _opToList[177] = new IL(0b10110001,
                new IE(0xB1, new MNE(IT.MOV, "MOV"), "B", IF.None, DT.None, "--------", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "cl", "ib" })
            );
            _opToList[178] = new IL(0b10110010,
                new IE(0xB2, new MNE(IT.MOV, "MOV"), "B", IF.None, DT.None, "--------", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "dl", "ib" })
            );
            _opToList[179] = new IL(0b10110011,
                new IE(0xB3, new MNE(IT.MOV, "MOV"), "B", IF.None, DT.None, "--------", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "bl", "ib" })
            );
            _opToList[180] = new IL(0b10110100,
                new IE(0xB4, new MNE(IT.MOV, "MOV"), "B", IF.None, DT.None, "--------", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "ah", "ib" })
            );
            _opToList[181] = new IL(0b10110101,
                new IE(0xB5, new MNE(IT.MOV, "MOV"), "B", IF.None, DT.None, "--------", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "ch", "ib" })
            );
            _opToList[182] = new IL(0b10110110,
                new IE(0xB6, new MNE(IT.MOV, "MOV"), "B", IF.None, DT.None, "--------", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "dh", "ib" })
            );
            _opToList[183] = new IL(0b10110111,
                new IE(0xB7, new MNE(IT.MOV, "MOV"), "B", IF.None, DT.None, "--------", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "bh", "ib" })
            );
            _opToList[184] = new IL(0b10111000,
                new IE(0xB8, new MNE(IT.MOV, "MOV"), "W", IF.None, DT.None, "--------", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "ax", "iw" })
            );
            _opToList[185] = new IL(0b10111001,
                new IE(0xB9, new MNE(IT.MOV, "MOV"), "W", IF.None, DT.None, "--------", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "cx", "iw" })
            );
            _opToList[186] = new IL(0b10111010,
                new IE(0xBA, new MNE(IT.MOV, "MOV"), "W", IF.None, DT.None, "--------", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "dx", "iw" })
            );
            _opToList[187] = new IL(0b10111011,
                new IE(0xBB, new MNE(IT.MOV, "MOV"), "W", IF.None, DT.None, "--------", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "bx", "iw" })
            );
            _opToList[188] = new IL(0b10111100,
                new IE(0xBC, new MNE(IT.MOV, "MOV"), "W", IF.None, DT.None, "--------", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "sp", "iw" })
            );
            _opToList[189] = new IL(0b10111101,
                new IE(0xBD, new MNE(IT.MOV, "MOV"), "W", IF.None, DT.None, "--------", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "bp", "iw" })
            );
            _opToList[190] = new IL(0b10111110,
                new IE(0xBE, new MNE(IT.MOV, "MOV"), "W", IF.None, DT.None, "--------", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "si", "iw" })
            );
            _opToList[191] = new IL(0b10111111,
                new IE(0xBF, new MNE(IT.MOV, "MOV"), "W", IF.None, DT.None, "--------", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "di", "iw" })
            );
            _opToList[194] = new IL(0b11000010,
                new IE(0xC2, new MNE(IT.RET, "RET"), "", IF.None, DT.None, "--------", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "iw" })
            );
            _opToList[195] = new IL(0b11000011,
                new IE(0xC3, new MNE(IT.RET, "RET"), "", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[196] = new IL(0b11000100,
                new IE(0xC4, new MNE(IT.LES, "LES"), "", IF.None, DT.None, "--------", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rw", "md" })
            );
            _opToList[197] = new IL(0b11000101,
                new IE(0xC5, new MNE(IT.LDS, "LDS"), "", IF.None, DT.None, "--------", "8086", 2, 4, new Field[] { "mr", "d0", "d1" }, new Operand[] { "rw", "md" })
            );
            _opToList[198] = new IL(0b11000110,
                new IE(0xC6, new MNE(IT.MOV, "MOV"), "B", IF.None, DT.None, "--------", "8086", 3, 5, new Field[] { "mr", "d0", "d1", "i0" }, new Operand[] { "rmb", "ib" })
            );
            _opToList[199] = new IL(0b11000111,
                new IE(0xC7, new MNE(IT.MOV, "MOV"), "W", IF.None, DT.None, "--------", "8086", 4, 6, new Field[] { "mr", "d0", "d1", "i0", "i1" }, new Operand[] { "rmw", "iw" })
            );
            _opToList[202] = new IL(0b11001010,
                new IE(0xCA, new MNE(IT.RETF, "RETF"), "", IF.None, DT.None, "--------", "8086", 3, 3, new Field[] { "i0", "i1" }, new Operand[] { "iw" })
            );
            _opToList[203] = new IL(0b11001011,
                new IE(0xCB, new MNE(IT.RETF, "RETF"), "", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[204] = new IL(0b11001100,
                new IE(0xCC, new MNE(IT.INT, "INT"), "", IF.None, DT.None, "--00----", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "3" })
            );
            _opToList[205] = new IL(0b11001101,
                new IE(0xCD, new MNE(IT.INT, "INT"), "", IF.None, DT.None, "--00----", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "ib" })
            );
            _opToList[206] = new IL(0b11001110,
                new IE(0xCE, new MNE(IT.INTO, "INTO"), "", IF.None, DT.None, "--00----", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[207] = new IL(0b11001111,
                new IE(0xCF, new MNE(IT.IRET, "IRET"), "", IF.None, DT.None, "oditszap", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[208] = new IL(0b11010000,
                new IE(0xD0, new MNE(IT.RCL, "RCL"), "B", IF.None, DT.None, "o-------", "8086", 2, 4, new Field[] { "/2", "d0", "d1" }, new Operand[] { "rmb", "1" }),
                new IE(0xD0, new MNE(IT.RCR, "RCR"), "B", IF.None, DT.None, "o-------", "8086", 2, 4, new Field[] { "/3", "d0", "d1" }, new Operand[] { "rmb", "1" }),
                new IE(0xD0, new MNE(IT.ROL, "ROL"), "B", IF.None, DT.None, "o-------", "8086", 2, 4, new Field[] { "/0", "d0", "d1" }, new Operand[] { "rmb", "1" }),
                new IE(0xD0, new MNE(IT.ROR, "ROR"), "B", IF.None, DT.None, "o-------", "8086", 2, 4, new Field[] { "/1", "d0", "d1" }, new Operand[] { "rmb", "1" }),
                new IE(0xD0, new MNE(IT.SAL, "SAL"), "B", IF.None, DT.None, "o-------", "8086", 2, 4, new Field[] { "/4", "d0", "d1" }, new Operand[] { "rmb", "1" }),
                new IE(0xD0, new MNE(IT.SHL, "SHL"), "B", IF.None, DT.None, "o-------", "8086", 2, 4, new Field[] { "/4", "d0", "d1" }, new Operand[] { "rmb", "1" }),
                new IE(0xD0, new MNE(IT.SAR, "SAR"), "W", IF.None, DT.None, "o-------", "8086", 3, 5, new Field[] { "/7", "d0", "d1" }, new Operand[] { "rmb" }),
                new IE(0xD0, new MNE(IT.SHR, "SHR"), "B", IF.None, DT.None, "o-------", "8086", 2, 4, new Field[] { "/5", "d0", "d1" }, new Operand[] { "rmb", "1" })
            );
            _opToList[209] = new IL(0b11010001,
                new IE(0xD1, new MNE(IT.RCL, "RCL"), "W", IF.None, DT.None, "o-------", "8086", 2, 4, new Field[] { "/2", "d0", "d1" }, new Operand[] { "rmw", "1" }),
                new IE(0xD1, new MNE(IT.RCR, "RCR"), "W", IF.None, DT.None, "o-------", "8086", 2, 4, new Field[] { "/3", "d0", "d1" }, new Operand[] { "rmw", "1" }),
                new IE(0xD1, new MNE(IT.ROL, "ROL"), "W", IF.None, DT.None, "o-------", "8086", 2, 4, new Field[] { "/0", "d0", "d1" }, new Operand[] { "rmw", "1" }),
                new IE(0xD1, new MNE(IT.ROR, "ROR"), "W", IF.None, DT.None, "o-------", "8086", 2, 4, new Field[] { "/1", "d0", "d1" }, new Operand[] { "rmw", "1" }),
                new IE(0xD1, new MNE(IT.SAL, "SAL"), "W", IF.None, DT.None, "o-------", "8086", 2, 4, new Field[] { "/4", "d0", "d1" }, new Operand[] { "rmw", "1" }),
                new IE(0xD1, new MNE(IT.SHL, "SHL"), "W", IF.None, DT.None, "o-------", "8086", 2, 4, new Field[] { "/4", "d0", "d1" }, new Operand[] { "rmw", "1" }),
                new IE(0xD1, new MNE(IT.SAR, "SAR"), "W", IF.None, DT.None, "o-------", "8086", 2, 4, new Field[] { "/7", "d0", "d1" }, new Operand[] { "rmw", "1" }),
                new IE(0xD1, new MNE(IT.SHR, "SHR"), "W", IF.None, DT.None, "o-------", "8086", 2, 4, new Field[] { "/5", "d0", "d1" }, new Operand[] { "rmw", "1" })
            );
            _opToList[210] = new IL(0b11010010,
                new IE(0xD2, new MNE(IT.RCL, "RCL"), "B", IF.None, DT.None, "o-------", "8086", 2, 4, new Field[] { "/2", "d0", "d1" }, new Operand[] { "rmb", "cl" }),
                new IE(0xD2, new MNE(IT.RCR, "RCR"), "B", IF.None, DT.None, "o-------", "8086", 2, 4, new Field[] { "/3", "d0", "d1" }, new Operand[] { "rmb", "cl" }),
                new IE(0xD2, new MNE(IT.ROL, "ROL"), "B", IF.None, DT.None, "o-------", "8086", 2, 4, new Field[] { "/0", "d0", "d1" }, new Operand[] { "rmb", "cl" }),
                new IE(0xD2, new MNE(IT.ROR, "ROR"), "B", IF.None, DT.None, "o-------", "8086", 2, 4, new Field[] { "/1", "d0", "d1" }, new Operand[] { "rmb", "cl" }),
                new IE(0xD2, new MNE(IT.SAL, "SAL"), "B", IF.None, DT.None, "o-------", "8086", 2, 4, new Field[] { "/4", "d0", "d1" }, new Operand[] { "rmb", "cl" }),
                new IE(0xD2, new MNE(IT.SHL, "SHL"), "B", IF.None, DT.None, "o-------", "8086", 2, 4, new Field[] { "/4", "d0", "d1" }, new Operand[] { "rmb", "cl" }),
                new IE(0xD2, new MNE(IT.SAR, "SAR"), "B", IF.None, DT.None, "o-------", "8086", 2, 4, new Field[] { "/7", "d0", "d1" }, new Operand[] { "rmb", "cl" }),
                new IE(0xD2, new MNE(IT.SHR, "SHR"), "B", IF.None, DT.None, "o-------", "8086", 2, 4, new Field[] { "/5", "d0", "d1" }, new Operand[] { "rmb", "cl" })
            );
            _opToList[211] = new IL(0b11010011,
                new IE(0xD3, new MNE(IT.RCL, "RCL"), "W", IF.None, DT.None, "o-------", "8086", 2, 4, new Field[] { "/2", "d0", "d1" }, new Operand[] { "rmw", "cl" }),
                new IE(0xD3, new MNE(IT.RCR, "RCR"), "W", IF.None, DT.None, "o-------", "8086", 2, 4, new Field[] { "/3", "d0", "d1" }, new Operand[] { "rmw", "cl" }),
                new IE(0xD3, new MNE(IT.ROL, "ROL"), "W", IF.None, DT.None, "o-------", "8086", 2, 4, new Field[] { "/0", "d0", "d1" }, new Operand[] { "rmw", "cl" }),
                new IE(0xD3, new MNE(IT.ROR, "ROR"), "W", IF.None, DT.None, "o-------", "8086", 2, 4, new Field[] { "/1", "d0", "d1" }, new Operand[] { "rmw", "cl" }),
                new IE(0xD3, new MNE(IT.SAL, "SAL"), "W", IF.None, DT.None, "o-------", "8086", 2, 4, new Field[] { "/4", "d0", "d1" }, new Operand[] { "rmw", "cl" }),
                new IE(0xD3, new MNE(IT.SHL, "SHL"), "W", IF.None, DT.None, "o-------", "8086", 2, 4, new Field[] { "/4", "d0", "d1" }, new Operand[] { "rmw", "cl" }),
                new IE(0xD3, new MNE(IT.SAR, "SAR"), "W", IF.None, DT.None, "o-------", "8086", 2, 4, new Field[] { "/7", "d0", "d1" }, new Operand[] { "rmw", "cl" }),
                new IE(0xD3, new MNE(IT.SHR, "SHR"), "W", IF.None, DT.None, "o-------", "8086", 2, 4, new Field[] { "/5", "d0", "d1" }, new Operand[] { "rmw", "cl" })
            );
            _opToList[212] = new IL(0b11010100,
                new IE(0xD4, new MNE(IT.AAM, "AAM"), "", IF.None, DT.None, "----sz-p", "8086", 2, 2, new Field[] { "0A" }, Array.Empty<Operand>())
            );
            _opToList[213] = new IL(0b11010101,
                new IE(0xD5, new MNE(IT.AAD, "AAD"), "", IF.None, DT.None, "----sz-p", "8086", 2, 2, new Field[] { "0A" }, Array.Empty<Operand>())
            );
            _opToList[215] = new IL(0b11010111,
                new IE(0xD7, new MNE(IT.XLAT, "XLAT"), "", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[224] = new IL(0b11100000,
                new IE(0xE0, new MNE(IT.LOOPNZ, "LOOPNZ"), "", IF.None, DT.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[225] = new IL(0b11100001,
                new IE(0xE1, new MNE(IT.LOOPE, "LOOPE"), "", IF.None, DT.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[226] = new IL(0b11100010,
                new IE(0xE2, new MNE(IT.LOOP, "LOOP"), "", IF.None, DT.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[227] = new IL(0b11100011,
                new IE(0xE3, new MNE(IT.JCXZ, "JCXZ"), "", IF.None, DT.None, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "sl" })
            );
            _opToList[228] = new IL(0b11100100,
                new IE(0xE4, new MNE(IT.IN, "IN"), "B", IF.None, DT.None, "--------", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "al", "ib" })
            );
            _opToList[229] = new IL(0b11100101,
                new IE(0xE5, new MNE(IT.IN, "IN"), "W", IF.None, DT.None, "--------", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "ax", "ib" })
            );
            _opToList[230] = new IL(0b11100110,
                new IE(0xE6, new MNE(IT.OUT, "OUT"), "B", IF.None, DT.None, "--------", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "ib", "al" })
            );
            _opToList[231] = new IL(0b11100111,
                new IE(0xE7, new MNE(IT.OUT, "OUT"), "W", IF.None, DT.None, "--------", "8086", 2, 2, new Field[] { "i0" }, new Operand[] { "ib", "ax" })
            );
            _opToList[232] = new IL(0b11101000,
                new IE(0xE8, new MNE(IT.CALL, "CALL"), "", IF.None, DT.Pointer, "--------", "8086", 3, 3, new Field[] { "o0", "o1" }, new Operand[] { "np" })
            );
            _opToList[233] = new IL(0b11101001,
                new IE(0xE9, new MNE(IT.JMP, "JMP"), "", IF.None, DT.Pointer, "--------", "8086", 3, 3, new Field[] { "o0", "o1" }, new Operand[] { "np" })
            );
            _opToList[234] = new IL(0b11101010,
                new IE(0xEA, new MNE(IT.JMP, "JMP"), "", IF.Far, DT.Pointer, "--------", "8086", 5, 5, new Field[] { "o0", "o1", "s0", "s1" }, new Operand[] { "far", "ptr", "fp" })
            );
            _opToList[235] = new IL(0b11101011,
                new IE(0xEB, new MNE(IT.JMP, "JMP"), "", IF.None, DT.Word, "--------", "8086", 2, 2, new Field[] { "r0" }, new Operand[] { "short", "sl" })
            );
            _opToList[236] = new IL(0b11101100,
                new IE(0xEC, new MNE(IT.IN, "IN"), "B", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "al", "dx" })
            );
            _opToList[237] = new IL(0b11101101,
                new IE(0xED, new MNE(IT.IN, "IN"), "W", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "ax", "dx" })
            );
            _opToList[238] = new IL(0b11101110,
                new IE(0xEE, new MNE(IT.OUT, "OUT"), "B", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "dx", "al" })
            );
            _opToList[239] = new IL(0b11101111,
                new IE(0xEF, new MNE(IT.OUT, "OUT"), "W", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), new Operand[] { "dx", "ax" })
            );
            _opToList[240] = new IL(0b11110000,
                new IE(0xF0, new MNE(IT.LOCK, "LOCK"), "", IF.Prefix, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[242] = new IL(0b11110010,
                new IE(0xF2, new MNE(IT.REPNE, "REPNE"), "", IF.Prefix, DT.None, "-----z--", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[243] = new IL(0b11110011,
                new IE(0xF3, new MNE(IT.REP, "REP"), "", IF.Prefix, DT.None, "-----z--", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[244] = new IL(0b11110100,
                new IE(0xF4, new MNE(IT.HLT, "HLT"), "", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[245] = new IL(0b11110101,
                new IE(0xF5, new MNE(IT.CMC, "CMC"), "", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[246] = new IL(0b11110110,
                new IE(0xF6, new MNE(IT.DIV, "DIV"), "B", IF.None, DT.None, "o---szap", "8086", 2, 4, new Field[] { "/6", "d0", "d1" }, new Operand[] { "rmb" }),
                new IE(0xF6, new MNE(IT.IDIV, "IDIV"), "B", IF.None, DT.None, "o---szap", "8086", 2, 4, new Field[] { "/7", "d0", "d1" }, new Operand[] { "rmb" }),
                new IE(0xF6, new MNE(IT.IMUL, "IMUL"), "B", IF.None, DT.None, "o---szap", "8086", 2, 4, new Field[] { "/5", "d0", "d1" }, new Operand[] { "rmb" }),
                new IE(0xF6, new MNE(IT.MUL, "MUL"), "B", IF.None, DT.None, "o---szap", "8086", 2, 4, new Field[] { "/4", "d0", "d1" }, new Operand[] { "rmb" }),
                new IE(0xF6, new MNE(IT.NEG, "NEG"), "B", IF.None, DT.None, "o---szap", "8086", 2, 4, new Field[] { "/3", "d0", "d1" }, new Operand[] { "rmb" }),
                new IE(0xF6, new MNE(IT.NOT, "NOT"), "B", IF.None, DT.None, "--------", "8086", 2, 4, new Field[] { "/2", "d0", "d1" }, new Operand[] { "rmb" }),
                new IE(0xF6, new MNE(IT.TEST, "TEST"), "B", IF.None, DT.None, "0---szap", "8086", 3, 5, new Field[] { "/0", "d0", "d1", "i0" }, new Operand[] { "rmb", "ib" })
            );
            _opToList[247] = new IL(0b11110111,
                new IE(0xF7, new MNE(IT.DIV, "DIV"), "W", IF.None, DT.None, "o---szap", "8086", 2, 4, new Field[] { "/6", "d0", "d1" }, new Operand[] { "rmw" }),
                new IE(0xF7, new MNE(IT.IDIV, "IDIV"), "W", IF.None, DT.None, "o---szap", "8086", 2, 4, new Field[] { "/7", "d0", "d1" }, new Operand[] { "rmw" }),
                new IE(0xF7, new MNE(IT.IMUL, "IMUL"), "W", IF.None, DT.None, "o---szap", "8086", 2, 4, new Field[] { "/5", "d0", "d1" }, new Operand[] { "rmw" }),
                new IE(0xF7, new MNE(IT.MUL, "MUL"), "W", IF.None, DT.None, "o---szap", "8086", 2, 4, new Field[] { "/4", "d0", "d1" }, new Operand[] { "rmw" }),
                new IE(0xF7, new MNE(IT.NEG, "NEG"), "W", IF.None, DT.None, "o---szap", "8086", 2, 4, new Field[] { "/3", "d0", "d1" }, new Operand[] { "rmw" }),
                new IE(0xF7, new MNE(IT.NOT, "NOT"), "W", IF.None, DT.None, "--------", "8086", 2, 4, new Field[] { "/2", "d0", "d1" }, new Operand[] { "rmw" }),
                new IE(0xF7, new MNE(IT.TEST, "TEST"), "W", IF.None, DT.None, "0---szap", "8086", 4, 6, new Field[] { "/0", "d0", "d1", "i0", "i1" }, new Operand[] { "rmw", "iw" })
            );
            _opToList[248] = new IL(0b11111000,
                new IE(0xF8, new MNE(IT.CLC, "CLC"), "", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[249] = new IL(0b11111001,
                new IE(0xF9, new MNE(IT.STC, "STC"), "", IF.None, DT.None, "--------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[250] = new IL(0b11111010,
                new IE(0xFA, new MNE(IT.CLI, "CLI"), "", IF.None, DT.None, "--0-----", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[251] = new IL(0b11111011,
                new IE(0xFB, new MNE(IT.STI, "STI"), "", IF.None, DT.None, "--1-----", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[252] = new IL(0b11111100,
                new IE(0xFC, new MNE(IT.CLD, "CLD"), "", IF.None, DT.None, "-0------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[253] = new IL(0b11111101,
                new IE(0xFD, new MNE(IT.STD, "STD"), "", IF.None, DT.None, "-1------", "8086", 1, 1, Array.Empty<Field>(), Array.Empty<Operand>())
            );
            _opToList[254] = new IL(0b11111110,
                new IE(0xFE, new MNE(IT.DEC, "DEC"), "", IF.None, DT.None, "o---szap", "8086", 2, 4, new Field[] { "/1", "d0", "d1" }, new Operand[] { "rmb" }),
                new IE(0xFE, new MNE(IT.INC, "INC"), "", IF.None, DT.None, "o---szap", "8086", 2, 4, new Field[] { "/0", "d0", "d1" }, new Operand[] { "rmb" })
            );
            _opToList[255] = new IL(0b11111111,
                new IE(0xFF, new MNE(IT.CALL, "CALL"), "W", IF.None, DT.None, "--------", "8086", 2, 4, new Field[] { "/2", "d0", "d1" }, new Operand[] { "rw" }),
                new IE(0xFF, new MNE(IT.CALL, "CALL"), "W", IF.None, DT.DoubleWord | DT.Pointer, "--------", "8086", 2, 4, new Field[] { "/3", "d0", "d1" }, new Operand[] { "dword", "ptr", "rw" }),
                new IE(0xFF, new MNE(IT.DEC, "DEC"), "", IF.None, DT.None, "o---szap", "8086", 2, 4, new Field[] { "/1", "d0", "d1" }, new Operand[] { "rmw" }),
                new IE(0xFF, new MNE(IT.INC, "INC"), "", IF.None, DT.None, "o---szap", "8086", 2, 4, new Field[] { "/0", "d0", "d1" }, new Operand[] { "rmw" }),
                new IE(0xFF, new MNE(IT.JMP, "JMP"), "", IF.None, DT.None, "--------", "8086", 2, 4, new Field[] { "/4", "d0", "d1" }, new Operand[] { "rmw" }),
                new IE(0xFF, new MNE(IT.JMP, "JMP"), "", IF.None, DT.DoubleWord | DT.Pointer, "--------", "8086", 2, 4, new Field[] { "/5", "d0", "d1" }, new Operand[] { "dword", "ptr", "rmw" }),
                new IE(0xFF, new MNE(IT.PUSH, "PUSH"), "", IF.None, DT.None, "--------", "8086", 2, 4, new Field[] { "/6", "d0", "d1" }, new Operand[] { "rmw" })
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
