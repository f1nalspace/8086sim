namespace Final.CPU8086
{
    public enum RegisterType : byte
    {
        /// <summary>
        /// Unknown register
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// 64-bit accumulator register (RAX)
        /// </summary>
        RAX,
        /// <summary>
        /// 32-bit accumulator register (EAX)
        /// </summary>
        EAX,
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
        /// 64-bit base register (RBX)
        /// </summary>
        RBX,
        /// <summary>
        /// 32-bit base register (EBX)
        /// </summary>
        EBX,
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
        /// 64-bit counting register (RCX)
        /// </summary>
        RCX,
        /// <summary>
        /// 32-bit counting register (ECX)
        /// </summary>
        ECX,
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
        /// 64-bit data register (RDX)
        /// </summary>
        RDX,
        /// <summary>
        /// 32-bit data register (EDX)
        /// </summary>
        EDX,
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
        /// 64-bit stack pointer (RSP)
        /// </summary>
        RSP,
        /// <summary>
        /// 32-bit stack pointer (ESP)
        /// </summary>
        ESP,
        /// <summary>
        /// 16-bit stack pointer (SP)
        /// </summary>
        SP,

        /// <summary>
        /// 64-bit base pointer (RBP)
        /// </summary>
        RBP,
        /// <summary>
        /// 32-bit base pointer (EBP)
        /// </summary>
        EBP,
        /// <summary>
        /// 16-bit base pointer (BP)
        /// </summary>
        BP,

        /// <summary>
        /// 64-bit source index (RSI)
        /// </summary>
        RSI,
        /// <summary>
        /// 32-bit source index (ESI)
        /// </summary>
        ESI,
        /// <summary>
        /// 16-bit source index (SI)
        /// </summary>
        SI,

        /// <summary>
        /// 64-bit destination index (RDI)
        /// </summary>
        RDI,
        /// <summary>
        /// 32-bit destination index (EDI)
        /// </summary>
        EDI,
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

        /// <summary>
        /// Control Register (CR)
        /// </summary>
        CR,
        /// <summary>
        /// Debug Register (DR)
        /// </summary>
        DR,
        /// <summary>
        /// Task Register (TR)
        /// </summary>
        TR,
        /// <summary>
        /// Extra Segment 2 (FS)
        /// </summary>
        FS,
        /// <summary>
        /// Extra Segment 3 (GS)
        /// </summary>
        GS,
    }
}
