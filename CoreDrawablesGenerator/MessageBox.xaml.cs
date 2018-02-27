using Avalonia;
using Avalonia.Controls;
using Avalonia.Direct2D1.Media;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace CoreDrawablesGenerator
{
    public class MessageBox : Window
    {
#pragma warning disable 0649
        [InjectControl]
        private TextBlock tbxMessage;

        [InjectControl]
        private Button btnClose;
#pragma warning restore 0649

        public static void ShowDialog(string message, string title)
        {
            MessageBox mb = new MessageBox(message, title);
            mb.ShowDialog();
        }

        private MessageBox()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            this.InjectControls();
            
            btnClose.Click += Close_Click;
        }
        
        public MessageBox(string message, string title) : this()
        {
            SetMessage(message);
            Title = title;
        }

        private void Close_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close();
        }

        public void SetMessage(string message)
        {
            tbxMessage.Text = message;
            UpdateSize();
        }

        /// <summary>
        /// Guesstimates the necessary MessageBox size.
        /// </summary>
        public void UpdateSize()
        {
            // TODO. DesiredSize doesn't work. Can't figure out a workaround.
            // Temporary solution: Allow resizing.
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
