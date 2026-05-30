using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Final.CPU8086.Types;

namespace Final.CPU8086.Controls
{
    // Picks a DataTemplate per AssemblyLine: source-label markers ("label0:") get a
    // dedicated template without the execution-marker image, everything else gets the
    // normal template. Selection is driven by AssemblyLine.IsSourceLabel.
    public class AssemblyLineTemplateSelector : IDataTemplate
    {
        public IDataTemplate Default { get; set; }
        public IDataTemplate SourceLabel { get; set; }

        public Control Build(object param)
        {
            if (param is AssemblyLine line && line.IsSourceLabel)
                return SourceLabel.Build(param);
            return Default.Build(param);
        }

        public bool Match(object data) => data is AssemblyLine;
    }
}
