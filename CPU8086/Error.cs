using System.Diagnostics;

namespace Final.CPU8086
{
    public readonly struct Error
    {
        public ErrorCode Code { get; }
        public string Message { get; }
        public int Position { get; }

        public Error(ErrorCode code, string message, int position)
        {
            Code = code;
            Message = message;
            Position = position;
        }

        public Error(Error error, string message, int position)
        {
            Code = error.Code;
            Message = $"{message}: {error.Message}";
            Position = position;
        }

        public override string ToString() => $"[{Position}][{Code}] {Message}";
    }
}
