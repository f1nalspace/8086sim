using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Final.CPUEmulation
{
    public readonly struct Error
    {
        public uint Code { get; }
        public uint Position { get; }
        public string Message { get; }

        public Error(uint code, uint position, string message)
        {
            Code = code;
            Position = position;
            Message = message;
        }

        public Error(Error error, uint position, string message)
        {
            Code = error.Code;
            Position = position;
            Message = $"{message}: {error.Message}";
        }

        public override string ToString() => $"[{Position}][{Code}] {Message}";
    }
}
