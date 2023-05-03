namespace Final.CPU8086.Types
{
    public enum EffectiveAddressCalculation : int
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

    public class EffectiveAddressCalculationTable
    {
        // R/M + MOD = 8 * 3
        public readonly EffectiveAddressCalculation[] _table;
        private readonly byte[] _displacementLengths;
        private readonly byte[] _cycles;

        public static EffectiveAddressCalculationTable Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new EffectiveAddressCalculationTable();
                return _instance;
            }
        }
        private static EffectiveAddressCalculationTable _instance = null;

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

            _cycles = new byte[32];
            // Direct
            _cycles[(byte)EffectiveAddressCalculation.DirectAddress] = 6;
            // Register indirect
            _cycles[(byte)EffectiveAddressCalculation.SI] = 5;
            _cycles[(byte)EffectiveAddressCalculation.DI] = 5;
            _cycles[(byte)EffectiveAddressCalculation.BX] = 5;
            // Register relative
            _cycles[(byte)EffectiveAddressCalculation.SI_D8] = 9;
            _cycles[(byte)EffectiveAddressCalculation.SI_D16] = 9;
            _cycles[(byte)EffectiveAddressCalculation.DI_D8] = 9;
            _cycles[(byte)EffectiveAddressCalculation.DI_D16] = 9;
            _cycles[(byte)EffectiveAddressCalculation.BX_D8] = 9;
            _cycles[(byte)EffectiveAddressCalculation.BX_D16] = 9;
            _cycles[(byte)EffectiveAddressCalculation.BP_D8] = 9;
            _cycles[(byte)EffectiveAddressCalculation.BP_D16] = 9;
            // Based indexed
            _cycles[(byte)EffectiveAddressCalculation.BP_DI] = 7;
            _cycles[(byte)EffectiveAddressCalculation.BX_SI] = 7;
            _cycles[(byte)EffectiveAddressCalculation.BP_SI] = 8;
            _cycles[(byte)EffectiveAddressCalculation.BX_DI] = 8;
            // Based indexed relative
            _cycles[(byte)EffectiveAddressCalculation.BP_DI_D8] = 11;
            _cycles[(byte)EffectiveAddressCalculation.BP_DI_D16] = 11;
            _cycles[(byte)EffectiveAddressCalculation.BX_SI_D8] = 11;
            _cycles[(byte)EffectiveAddressCalculation.BX_SI_D16] = 11;
            _cycles[(byte)EffectiveAddressCalculation.BP_SI_D8] = 12;
            _cycles[(byte)EffectiveAddressCalculation.BP_SI_D16] = 12;
            _cycles[(byte)EffectiveAddressCalculation.BX_DI_D8] = 12;
            _cycles[(byte)EffectiveAddressCalculation.BX_DI_D16] = 12;
        }

        public EffectiveAddressCalculation Get(byte rm, byte mod)
        {
            byte index = (byte)(mod * 8 + rm);
            return _table[index];
        }

        public byte GetCycles(EffectiveAddressCalculation eac) => _cycles[(byte)eac];

        public byte GetDisplacementLength(EffectiveAddressCalculation eac) => _displacementLengths[(byte)eac];
    }
}
