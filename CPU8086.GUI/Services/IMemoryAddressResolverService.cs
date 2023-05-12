using Final.CPU8086.Types;

namespace Final.CPU8086.Services
{
    public interface IMemoryAddressResolverService
    {
        uint Resolve(SegmentType segment, int displacement);
    }
}
