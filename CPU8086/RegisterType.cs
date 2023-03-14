namespace Final.CPU8086
{
    public enum RegisterType : byte
    {
        /// <summary>
        /// Unknown register
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// 16-bit accumulator register (AX)
        /// </summary>
        AX,
        /// <summary>
        /// 8-bit low accumulator register (AH)
        /// </summary>
        AL,
        /// <summary>
        /// 8-bit high accumulator register (AH)
        /// </summary>
        AH,

        /// <summary>
        /// 16-bit base register (BX)
        /// </summary>
        BX,
        /// <summary>
        /// 8-bit low base register (BL)
        /// </summary>
        BL,
        /// <summary>
        /// 8-bit high base register (BL)
        /// </summary>
        BH,

        /// <summary>
        /// 16-bit counting register (CX)
        /// </summary>
        CX,
        /// <summary>
        /// 8-bit low counting register (CL)
        /// </summary>
        CL,
        /// <summary>
        /// 8-bit high counting register (CL)
        /// </summary>
        CH,

        /// <summary>
        /// 16-bit data register (DX)
        /// </summary>
        DX,
        /// <summary>
        /// 8-bit low data register (DL)
        /// </summary>
        DL,
        /// <summary>
        /// 8-bit high data register (DL)
        /// </summary>
        DH,

        /// <summary>
        /// 16-bit stack pointer (SP)
        /// </summary>
        SP,
        /// <summary>
        /// 16-bit base pointer (BP)
        /// </summary>
        BP,
        /// <summary>
        /// 16-bit source index (SI)
        /// </summary>
        SI,
        /// <summary>
        /// 16-bit destination index (DI)
        /// </summary>
        DI,

        /// <summary>
        /// 16-bit code segment (CS)
        /// </summary>
        CS,
        /// <summary>
        /// 16-bit data segment (DS)
        /// </summary>
        DS,
        /// <summary>
        /// 16-bit stack segment (SS)
        /// </summary>
        SS,
        /// <summary>
        /// 16-bit extra segment (ES)
        /// </summary>
        ES,
    }
}
