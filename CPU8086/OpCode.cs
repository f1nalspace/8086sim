namespace CPU8086
{
    public enum OpCode : int
    {
        Unknown = -1,

        ADD_dREG8_dMEM8_sREG8 = 0x00,
        ADD_dREG16_dMEM16_sREG16 = 0x01,
        ADD_dREG8_sREG8_sMEM8 = 0x02,
        ADD_dREG16_sREG16_sMEM16 = 0x03,
        ADD_dAL_sIMM8 = 0x04,
        ADD_dAX_sIMM16 = 0x05,

        PUSH_ES = 0x06,
        POP_ES = 0x07,

        OR_dREG8_dMEM8_sREG8 = 0x08,
        OR_dREG16_dMEM16_sREG16 = 0x09,
        OR_dREG8_sREG8_sMEM8 = 0x0A,
        OR_dREG16_sREG16_sMEM16 = 0x0B,
        OR_dAL_sIMM8 = 0x0C,
        OR_dAX_sIMM16 = 0x0D,

        PUSH_CS = 0x0E,
        // Unused = 0x0F,

        /// <summary>
        /// <para>8-bit Arithmetic instructions such as ADD, ADC, SBB, SUB, CMP, etc. writing to register or memory, using an immediate as source.</para>
        /// <para>REG field decides what type of arithmetic operating it actually is.</para>
        /// </summary>
        ARITHMETIC_dREG8_dMEM8_sIMM8 = 0x80,

        /// <summary>
        /// <para>16-bit Arithmetic instructions such as ADD, ADC, SBB, SUB, CMP, etc. writing to register or memory, using an immediate as source.</para>
        /// <para>REG field decides what type of arithmetic operating it actually is.</para>
        /// </summary>
        ARITHMETIC_dREG16_dMEM16_sIMM16 = 0x83,

        MOV_dREG8_dMEM8_sREG8 = 0x88,
        MOV_dREG16_dMEM16_sREG16 = 0x89,
        MOV_dREG8_sMEM8_sREG8 = 0x8A,
        MOV_dREG16_sMEM16_sREG16 = 0x8B,

        MOV_dAL_sMEM8 = 0xA0,
        MOV_dAX_sMEM16 = 0xA1,
        MOV_dMEM8_sAL = 0xA2,
        MOV_dMEM16_sAX = 0xA3,

        MOV_dAL_sIMM8 = 0xB0,
        MOV_dCL_sIMM8 = 0xB1,
        MOV_dDL_sIMM8 = 0xB2,
        MOV_dBL_sIMM8 = 0xB3,
        MOV_dAH_sIMM8 = 0xB4,
        MOV_dCH_sIMM8 = 0xB5,
        MOV_dDH_sIMM8 = 0xB6,
        MOV_BH_IMM8 = 0xB7,
        MOV_dAX_sIMM16 = 0xB8,
        MOV_dCX_sIMM16 = 0xB9,
        MOV_dDX_sIMM16 = 0xBA,
        MOV_dBX_sIMM16 = 0xBB,
        MOV_dSP_sIMM16 = 0xBC,
        MOV_dBP_sIMM16 = 0xBD,
        MOV_dSI_sIMM16 = 0xBE,
        MOV_dDI_sIMM16 = 0xBF,

        MOV_dMEM8_sIMM8 = 0xC6,
        MOV_dMEM16_sIMM16 = 0xC7,
    }
}
