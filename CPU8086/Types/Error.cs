using System.Diagnostics;

namespace Final.CPU8086.Types
{
    public readonly struct Error
    {
        public ErrorCode Code { get; }
        public string Message { get; }
        public uint Position { get; }

        public Error(ErrorCode code, string message, uint position)
        {
            Code = code;
            Message = message;
            Position = position;
        }

        public Error(Error error, string message, uint position)
        {
            Code = error.Code;
            Message = $"{message}: {error.Message}";
            Position = position;
        }

        public override string ToString() => $"[{Position}][{Code}] {Message}";
    }
}
