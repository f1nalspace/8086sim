namespace CPU8086
{
    public class InstructionTable
    {
        private readonly Instruction[] _table;

        public ref readonly Instruction this[int index] => ref _table[index];

        public InstructionTable()
        {
            _table = new Instruction[256];

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

            // 00000000 to 00000111 (ADD)
            _table[0x00 /* 0000 0000 */] = new Instruction(OpCode.ADD_dREG8_dMEM8_sREG8, OpFamily.Add8_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.Add, "Adds 8-bit Register to 8-bit Register/Memory");
            _table[0x01 /* 0000 0001 */] = new Instruction(OpCode.ADD_dREG16_dMEM16_sREG16, OpFamily.Add16_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.Add, "Adds 16-bit Register to 16-bit Register/Memory");
            _table[0x02 /* 0000 0010 */] = new Instruction(OpCode.ADD_dREG8_sREG8_sMEM8, OpFamily.Add8_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.Add, "Adds 8-bit Register/Memory to 8-bit Register");
            _table[0x03 /* 0000 0011 */] = new Instruction(OpCode.ADD_dREG16_sREG16_sMEM16, OpFamily.Add16_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.Add, "Adds 16-bit Register/Memory to 16-bit Register");
            _table[0x04 /* 0000 0100 */] = new Instruction(OpCode.ADD_dAL_sIMM8, OpFamily.Add8_FixedReg_Imm, FieldEncoding.None, RegisterType.AL, 2, Mnemonics.Add, "Adds 8-bit Immediate to 8-bit " + RegisterType.AL + " Register");
            _table[0x05 /* 0000 0101 */] = new Instruction(OpCode.ADD_dAX_sIMM16, OpFamily.Add16_FixedReg_Imm, FieldEncoding.None, RegisterType.AX, 3, Mnemonics.Add, "Adds 16-bit Immediate to 16-bit " + RegisterType.AX + " Register");

            // 00000110 to 00000111 (PUSH/POP)
            _table[0x06 /* 0000 0110 */] = new Instruction(OpCode.PUSH_ES, OpFamily.Push_FixedReg, FieldEncoding.None, RegisterType.ES, 1, Mnemonics.Push, "Decrements the 16-bit " + RegisterType.SP + " by the amount of the " + RegisterType.ES + " Register");
            _table[0x07 /* 0000 0111 */] = new Instruction(OpCode.POP_ES, OpFamily.Pop_FixedReg, FieldEncoding.None, RegisterType.ES, 1, Mnemonics.Pop, "Increments the 16-bit " + RegisterType.SP + " by the amount of the " + RegisterType.ES + " Register");

            // 00001000 to 0000 1111
            _table[0x08 /* 0000 1000 */] = new Instruction(OpCode.OR_dREG8_dMEM8_sREG8, OpFamily.Or8_RegOrMem_RegOrMem, FieldEncoding.None, 2, 4, Mnemonics.Or, "Logical or 8-bit Register with 8-bit Register/Memory");
            _table[0x09 /* 0000 1001 */] = new Instruction(OpCode.OR_dREG16_dMEM16_sREG16, OpFamily.Or8_RegOrMem_RegOrMem, FieldEncoding.None, 2, 4, Mnemonics.Or, "Logical or 16-bit Register with 16-bit Register/Memory");
            _table[0x0A /* 0000 1010 */] = new Instruction(OpCode.OR_dREG8_sREG8_sMEM8, OpFamily.Or8_RegOrMem_RegOrMem, FieldEncoding.None, 2, 4, Mnemonics.Or, "Logical or 8-bit Register/Memory with 8-bit Register");
            _table[0x0B /* 0000 1011 */] = new Instruction(OpCode.OR_dREG16_sREG16_sMEM16, OpFamily.Or16_RegOrMem_RegOrMem, FieldEncoding.None, 2, 4, Mnemonics.Or, "Logical or 16-bit Register/Memory with 16-bit Register");
            _table[0x0C /* 0000 1100 */] = new Instruction(OpCode.OR_dAL_sIMM8, OpFamily.Or8_FixedReg_Imm, FieldEncoding.None, RegisterType.AL, 2, Mnemonics.Or, "Logical or 8-bit Immediate with 8-bit " + RegisterType.AL + " Register");
            _table[0x0D /* 0000 1101 */] = new Instruction(OpCode.OR_dAX_sIMM16, OpFamily.Or16_FixedReg_Imm, FieldEncoding.None, RegisterType.AX, 3, Mnemonics.Or, "Logical or 16-bit Immediate with 16-bit " + RegisterType.AX + " Register");
            _table[0x0E /* 0000 1110 */] = new Instruction(OpCode.PUSH_CS, OpFamily.Push_FixedReg, FieldEncoding.None, RegisterType.CS, 1, Mnemonics.Or, "Decrements the 16-bit " + RegisterType.SP + " by the amount of the " + RegisterType.CS + " Register");
            _table[0x0F /* 0000 1111 */] = null; // Unused

            // 10000000 (ADD/ADC/SUB,etc.)
            _table[0x80 /* 1000 0000 */] = new Instruction(OpCode.ARITHMETIC_dREG8_dMEM8_sIMM8, OpFamily.Arithmetic8_RegOrMem_Imm, FieldEncoding.ModRemRM, 3, 5, Mnemonics.Dynamic, "Arithmetic 8-bit Immediate to 8-bit Register/Memory");

            // 10000011 (ADD/ADC/SUB,etc.)
            _table[0x83 /* 1000 0011 */] = new Instruction(OpCode.ARITHMETIC_dREG16_dMEM16_sIMM16, OpFamily.Arithmetic16_RegOrMem_Imm, FieldEncoding.ModRemRM, 3, 5, Mnemonics.Dynamic, "Arithmetic 16-bit Immediate to 16-bit Register/Memory");

            // 1 0 0 0 1 0 d w (MOV)
            _table[0x88 /* 100010 00 */] = new Instruction(OpCode.MOV_dREG8_dMEM8_sREG8, OpFamily.Move8_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.Mov, "Copy 8-bit Register to 8-bit Register/Memory");
            _table[0x89 /* 100010 01 */] = new Instruction(OpCode.MOV_dREG16_dMEM16_sREG16, OpFamily.Move16_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.Mov, "Copy 16-bit Register to 16-bit Register/Memory");
            _table[0x8A /* 100010 10 */] = new Instruction(OpCode.MOV_dREG8_sMEM8_sREG8, OpFamily.Move8_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.Mov, "Copy 8-bit Register/Memory to 8-bit Register");
            _table[0x8B /* 100010 11 */] = new Instruction(OpCode.MOV_dREG16_sMEM16_sREG16, OpFamily.Move16_RegOrMem_RegOrMem, FieldEncoding.ModRemRM, 2, 4, Mnemonics.Mov, "Copy 8-bit Register/Memory to 8-bit Register");

            // 10100000 to 10100011 (MOV)
            _table[0xA0 /* 1010000 */] = new Instruction(OpCode.MOV_dAL_sMEM8, OpFamily.Move8_FixedReg_Mem, FieldEncoding.None, RegisterType.AL, 3, Mnemonics.Mov, "Copy 8-bit Memory to 8-bit " + RegisterType.AL + " Register");
            _table[0xA1 /* 1010001 */] = new Instruction(OpCode.MOV_dAX_sMEM16, OpFamily.Move16_FixedReg_Mem, FieldEncoding.None, RegisterType.AX, 3, Mnemonics.Mov, "Copy 16-bit Memory to 16-bit " + RegisterType.AX + " Register");
            _table[0xA2 /* 1010010 */] = new Instruction(OpCode.MOV_dMEM8_sAL, OpFamily.Move8_Mem_FixedReg, FieldEncoding.None, RegisterType.AL, 3, Mnemonics.Mov, "Copy 8-bit " + RegisterType.AL + " Register to 8-bit Memory");
            _table[0xA3 /* 1010011 */] = new Instruction(OpCode.MOV_dMEM16_sAX, OpFamily.Move16_Mem_FixedReg, FieldEncoding.None, RegisterType.AX, 3, Mnemonics.Mov, "Copy 16-bit " + RegisterType.AX + " Register to 16-bit Memory"); // BUG(final): Page 174, instruction $A3: MOV16, AL is wrong, correct is MOV16, AX

            // 1 0 1 1 w reg (MOV)
            _table[0xB0 /* 1011 0 000 */] = new Instruction(OpCode.MOV_dAL_sIMM8, OpFamily.Move8_Reg_Imm, FieldEncoding.None, RegisterType.AL, 2, Mnemonics.Mov, "Copy 8-bit Immediate to 8-bit " + RegisterType.AL + " Register");
            _table[0xB1 /* 1011 0 001 */] = new Instruction(OpCode.MOV_dCL_sIMM8, OpFamily.Move8_Reg_Imm, FieldEncoding.None, RegisterType.CL, 2, Mnemonics.Mov, "Copy 8-bit Immediate to 8-bit " + RegisterType.CL + " Register");
            _table[0xB2 /* 1011 0 010 */] = new Instruction(OpCode.MOV_dDL_sIMM8, OpFamily.Move8_Reg_Imm, FieldEncoding.None, RegisterType.DL, 2, Mnemonics.Mov, "Copy 8-bit Immediate to 8-bit " + RegisterType.DL + " Register");
            _table[0xB3 /* 1011 0 011 */] = new Instruction(OpCode.MOV_dBL_sIMM8, OpFamily.Move8_Reg_Imm, FieldEncoding.None, RegisterType.BL, 2, Mnemonics.Mov, "Copy 8-bit Immediate to 8-bit " + RegisterType.BL + " Register");
            _table[0xB4 /* 1011 0 100 */] = new Instruction(OpCode.MOV_dAH_sIMM8, OpFamily.Move8_Reg_Imm, FieldEncoding.None, RegisterType.AH, 2, Mnemonics.Mov, "Copy 8-bit Immediate to 8-bit " + RegisterType.AH + " Register");
            _table[0xB5 /* 1011 0 101 */] = new Instruction(OpCode.MOV_dCH_sIMM8, OpFamily.Move8_Reg_Imm, FieldEncoding.None, RegisterType.CH, 2, Mnemonics.Mov, "Copy 8-bit Immediate to 8-bit " + RegisterType.CH + " Register");
            _table[0xB6 /* 1011 0 110 */] = new Instruction(OpCode.MOV_dDH_sIMM8, OpFamily.Move8_Reg_Imm, FieldEncoding.None, RegisterType.DH, 2, Mnemonics.Mov, "Copy 8-bit Immediate to 8-bit " + RegisterType.DH + " Register");
            _table[0xB7 /* 1011 0 111 */] = new Instruction(OpCode.MOV_dDH_sIMM8, OpFamily.Move8_Reg_Imm, FieldEncoding.None, RegisterType.BH, 2, Mnemonics.Mov, "Copy 8-bit Immediate to 8-bit " + RegisterType.BH + " Register");
            _table[0xB8 /* 1011 1 000 */] = new Instruction(OpCode.MOV_dAX_sIMM16, OpFamily.Move16_Reg_Imm, FieldEncoding.None, RegisterType.AX, 3, Mnemonics.Mov, "Copy 16-bit Immediate to 16-bit " + RegisterType.AX + " Register");
            _table[0xB9 /* 1011 1 001 */] = new Instruction(OpCode.MOV_dCX_sIMM16, OpFamily.Move16_Reg_Imm, FieldEncoding.None, RegisterType.CX, 3, Mnemonics.Mov, "Copy 16-bit Immediate to 16-bit " + RegisterType.CX + " Register");
            _table[0xBA /* 1011 1 010 */] = new Instruction(OpCode.MOV_dDX_sIMM16, OpFamily.Move16_Reg_Imm, FieldEncoding.None, RegisterType.DX, 3, Mnemonics.Mov, "Copy 16-bit Immediate to 16-bit " + RegisterType.DX + " Register");
            _table[0xBB /* 1011 1 011 */] = new Instruction(OpCode.MOV_dBX_sIMM16, OpFamily.Move16_Reg_Imm, FieldEncoding.None, RegisterType.BX, 3, Mnemonics.Mov, "Copy 16-bit Immediate to 16-bit " + RegisterType.BX + " Register");
            _table[0xBC /* 1011 1 100 */] = new Instruction(OpCode.MOV_dSP_sIMM16, OpFamily.Move16_Reg_Imm, FieldEncoding.None, RegisterType.SP, 3, Mnemonics.Mov, "Copy 16-bit Immediate to 16-bit " + RegisterType.SP + " Register");
            _table[0xBD /* 1011 1 101 */] = new Instruction(OpCode.MOV_dBP_sIMM16, OpFamily.Move16_Reg_Imm, FieldEncoding.None, RegisterType.BP, 3, Mnemonics.Mov, "Copy 16-bit Immediate to 16-bit " + RegisterType.BP + " Register");
            _table[0xBE /* 1011 1 110 */] = new Instruction(OpCode.MOV_dSI_sIMM16, OpFamily.Move16_Reg_Imm, FieldEncoding.None, RegisterType.SI, 3, Mnemonics.Mov, "Copy 16-bit Immediate to 16-bit " + RegisterType.SI + " Register");
            _table[0xBF /* 1011 1 111 */] = new Instruction(OpCode.MOV_dDI_sIMM16, OpFamily.Move16_Reg_Imm, FieldEncoding.None, RegisterType.DI, 3, Mnemonics.Mov, "Copy 16-bit Immediate to 16-bit " + RegisterType.DI + " Register");

            // 11000110 to 11000111 (MOV)
            _table[0xC6 /* 1100 0110 */] = new Instruction(OpCode.MOV_dMEM8_sIMM8, OpFamily.Move8_Mem_Imm, FieldEncoding.ModRemRM, 3, 5, Mnemonics.Mov, "Copy 8-bit Immediate to 8-bit Memory");
            _table[0xC7 /* 1100 0111 */] = new Instruction(OpCode.MOV_dMEM16_sIMM16, OpFamily.Move16_Mem_Imm, FieldEncoding.ModRemRM, 4, 6, Mnemonics.Mov, "Copy 16-bit Immediate to 16-bit Memory");
        }
    }
}
