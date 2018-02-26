using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace CoreDrawablesGenerator
{
    public class TextWindow : Window
    {
        [InjectControl("text")]
        TextBox tbText;

        public TextWindow(string text)
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            this.InjectControls();
            tbText.Text = text;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
