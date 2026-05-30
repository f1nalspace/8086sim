using Final.CPU8086.Types;
using Xunit;

namespace Final.CPU8086
{
    public class DataTypeSizeTests
    {
        [Fact]
        public void PlainTypesHaveExpectedSizes()
        {
            Assert.Equal(1, CPU.GetDataTypeSize(DataType.Byte));
            Assert.Equal(2, CPU.GetDataTypeSize(DataType.Word));
            Assert.Equal(4, CPU.GetDataTypeSize(DataType.DoubleWord));
            Assert.Equal(8, CPU.GetDataTypeSize(DataType.QuadWord));
        }

        [Fact]
        public void BarePointerResolvesToPointerWidth()
        {
            // DataType.Pointer alone maps to the pointer's underlying type (Word -> 2).
            Assert.Equal(2, CPU.GetDataTypeSize(DataType.Pointer));
        }

        [Theory]
        [InlineData(DataType.Byte, 1)]
        [InlineData(DataType.Word, 2)]
        [InlineData(DataType.DoubleWord, 4)]
        [InlineData(DataType.QuadWord, 8)]
        public void PointerFlaggedTypesStripPointerAndKeepUnderlyingSize(DataType underlying, int expected)
        {
            // Previously the Pointer flag was cleared with XOR (^ ~Pointer), which corrupted the
            // value and made these return 0. Masking with & ~Pointer keeps the underlying type.
            Assert.Equal(expected, CPU.GetDataTypeSize(DataType.Pointer | underlying));
        }
    }
}
