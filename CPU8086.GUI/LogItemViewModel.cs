using DevExpress.Mvvm;
using System;

namespace Final.CPU8086
{
    public class LogItemViewModel : ViewModelBase
    {
        public uint Position { get; }
        public string Message { get; }
        public DateTimeOffset DateTime { get; }

        public LogItemViewModel(uint position, string message, DateTimeOffset dateTime)
        {
            Position = position;
            Message = message;
            DateTime = dateTime;
        }

        public override string ToString() => $"[{DateTime:yyyy-mm-dd HH:mm::ss:fff}]:{Position} {Message}";
    }
}
