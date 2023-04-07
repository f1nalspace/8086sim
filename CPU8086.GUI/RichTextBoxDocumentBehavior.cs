using DevExpress.Mvvm.UI.Interactivity;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Final.CPU8086
{
    public class RichTextBoxDocumentBehavior : Behavior<RichTextBox>
    {
        public static readonly DependencyProperty DocumentProperty =
            DependencyProperty.Register(nameof(Document), typeof(FlowDocument), typeof(RichTextBoxDocumentBehavior), new PropertyMetadata(null, DocumentPropertyChanged));

        private static void DocumentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RichTextBoxDocumentBehavior behavior)
                behavior.SetDocument(e.NewValue as FlowDocument);
        }

        public FlowDocument Document
        {
            get => GetValue(DocumentProperty) as FlowDocument;
            set => SetCurrentValue(DocumentProperty, value);
        }

        private void SetDocument(FlowDocument doc)
        {
            if (AssociatedObject != null)
                AssociatedObject.Document = doc;
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            SetDocument(Document);
        }

        protected override void OnDetaching()
        {
            SetDocument(null);
            base.OnDetaching();
        }
    }
}
