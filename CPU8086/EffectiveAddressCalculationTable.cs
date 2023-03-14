namespace CPU8086
{
    public class EffectiveAddressCalculationTable
    {
        // R/M + MOD = 8 * 3
        public readonly EffectiveAddressCalculation[] _table;
        private readonly byte[] _displacementLengths;

        public EffectiveAddressCalculationTable()
        {
            _table = new EffectiveAddressCalculation[8 * 3];

            _table[0b00 * 8 + 0b000] = EffectiveAddressCalculation.BX_SI;
            _table[0b00 * 8 + 0b001] = EffectiveAddressCalculation.BX_DI;
            _table[0b00 * 8 + 0b010] = EffectiveAddressCalculation.BP_SI;
            _table[0b00 * 8 + 0b011] = EffectiveAddressCalculation.BP_DI;
            _table[0b00 * 8 + 0b100] = EffectiveAddressCalculation.SI;
            _table[0b00 * 8 + 0b101] = EffectiveAddressCalculation.DI;
            _table[0b00 * 8 + 0b110] = EffectiveAddressCalculation.DirectAddress;
            _table[0b00 * 8 + 0b111] = EffectiveAddressCalculation.BX;

            _table[0b01 * 8 + 0b000] = EffectiveAddressCalculation.BX_SI_D8;
            _table[0b01 * 8 + 0b001] = EffectiveAddressCalculation.BX_DI_D8;
            _table[0b01 * 8 + 0b010] = EffectiveAddressCalculation.BP_SI_D8;
            _table[0b01 * 8 + 0b011] = EffectiveAddressCalculation.BP_DI_D8;
            _table[0b01 * 8 + 0b100] = EffectiveAddressCalculation.SI_D8;
            _table[0b01 * 8 + 0b101] = EffectiveAddressCalculation.DI_D8;
            _table[0b01 * 8 + 0b110] = EffectiveAddressCalculation.BP_D8;
            _table[0b01 * 8 + 0b111] = EffectiveAddressCalculation.BX_D8;

            _table[0b10 * 8 + 0b000] = EffectiveAddressCalculation.BX_SI_D16;
            _table[0b10 * 8 + 0b001] = EffectiveAddressCalculation.BX_DI_D16;
            _table[0b10 * 8 + 0b010] = EffectiveAddressCalculation.BP_SI_D16;
            _table[0b10 * 8 + 0b011] = EffectiveAddressCalculation.BP_DI_D16;
            _table[0b10 * 8 + 0b100] = EffectiveAddressCalculation.SI_D16;
            _table[0b10 * 8 + 0b101] = EffectiveAddressCalculation.DI_D16;
            _table[0b10 * 8 + 0b110] = EffectiveAddressCalculation.BP_D16;
            _table[0b10 * 8 + 0b111] = EffectiveAddressCalculation.BX_D16;

            _displacementLengths = new byte[32];

            _displacementLengths[(byte)EffectiveAddressCalculation.BX_SI_D8] = 1;
            _displacementLengths[(byte)EffectiveAddressCalculation.BX_DI_D8] = 1;
            _displacementLengths[(byte)EffectiveAddressCalculation.BP_SI_D8] = 1;
            _displacementLengths[(byte)EffectiveAddressCalculation.BP_DI_D8] = 1;
            _displacementLengths[(byte)EffectiveAddressCalculation.SI_D8] = 1;
            _displacementLengths[(byte)EffectiveAddressCalculation.DI_D8] = 1;
            _displacementLengths[(byte)EffectiveAddressCalculation.BP_D8] = 1;
            _displacementLengths[(byte)EffectiveAddressCalculation.BX_D8] = 1;

            _displacementLengths[(byte)EffectiveAddressCalculation.BX_SI_D16] = 2;
            _displacementLengths[(byte)EffectiveAddressCalculation.BX_DI_D16] = 2;
            _displacementLengths[(byte)EffectiveAddressCalculation.BP_SI_D16] = 2;
            _displacementLengths[(byte)EffectiveAddressCalculation.BP_DI_D16] = 2;
            _displacementLengths[(byte)EffectiveAddressCalculation.SI_D16] = 2;
            _displacementLengths[(byte)EffectiveAddressCalculation.DI_D16] = 2;
            _displacementLengths[(byte)EffectiveAddressCalculation.BP_D16] = 2;
            _displacementLengths[(byte)EffectiveAddressCalculation.BX_D16] = 2;
            _displacementLengths[(byte)EffectiveAddressCalculation.DirectAddress] = 2;
        }

        public EffectiveAddressCalculation Get(byte rm, byte mod)
        {
            byte index = (byte)(mod * 8 + rm);
            return _table[index];
        }

        public byte GetDisplacementLength(EffectiveAddressCalculation eac) => _displacementLengths[(byte)eac];
    }
}
