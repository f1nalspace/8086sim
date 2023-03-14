namespace Final.CPU8086
{
    public enum EffectiveAddressCalculation : byte
    {
        None = 0,

        // Mod == 00
        BX_SI,
        BX_DI,
        BP_SI,
        BP_DI,
        SI,
        DI,
        DirectAddress,
        BX,

        // Mod == 01
        BX_SI_D8,
        BX_DI_D8,
        BP_SI_D8,
        BP_DI_D8,
        SI_D8,
        DI_D8,
        BP_D8,
        BX_D8,

        // Mod == 10
        BX_SI_D16,
        BX_DI_D16,
        BP_SI_D16,
        BP_DI_D16,
        SI_D16,
        DI_D16,
        BP_D16,
        BX_D16,
    }
}
