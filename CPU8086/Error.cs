using System.Diagnostics;

namespace Final.CPU8086
{
    public readonly struct Error
    {
        public ErrorCode Code { get; }
        public string Message { get; }

        public Error(ErrorCode code, string message)
        {
            Code = code;
            Message = message;
        }

        public Error(Error error, string message)
        {
            Code = error.Code;
            Message = $"{message}: {error.Message}";
        }

        public override string ToString() => $"[{Code}] {Message}";
    }
}
