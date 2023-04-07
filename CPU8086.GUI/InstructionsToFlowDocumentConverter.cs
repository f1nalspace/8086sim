using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Markup;

namespace Final.CPU8086
{
    public class InstructionsToFlowDocumentConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ImmutableArray<Instruction> instructions)
            {
                FlowDocument doc = new FlowDocument();
                foreach (var instruction in instructions)
                {
                    Paragraph p = new Paragraph(new Run() { Text = instruction.Asm(OutputValueMode.Auto) })
                    {
                        Margin = new System.Windows.Thickness(0)
                    };
                    doc.Blocks.Add(p);
                }
                return doc;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();

        public override object ProvideValue(IServiceProvider serviceProvider)
            => this;
    }
}
