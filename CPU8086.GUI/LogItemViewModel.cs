using DevExpress.Mvvm;
using System;

namespace Final.CPU8086
{
    public class LogItemViewModel : ViewModelBase
    {
        public int Position { get; }
        public string Message { get; }
        public DateTimeOffset DateTime { get; }

        public LogItemViewModel(int position, string message, DateTimeOffset dateTime)
        {
            Position = position;
            Message = message;
            DateTime = dateTime;
        }

        public override string ToString() => $"[{DateTime:yyyy-mm-dd HH:mm::ss:fff}]:{Position} {Message}";
    }
}
