using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.IO;

namespace CoreDrawablesGenerator
{
    public class TextWindow : Window
    {
#pragma warning disable 0649
        [InjectControl("text")]
        TextBox tbxText;
        [InjectControl]
        Button btnToggleFormatting, btnSave, btnCopy;
#pragma warning restore 0649

        private string originalText;
        private bool formatted = true;

        public TextWindow(string text)
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            this.InjectControls();

            originalText = text;
            tbxText.Text = text;

            btnToggleFormatting.Click += ToggleFormatting_Click;
            btnSave.Click += Save_Click;
            btnCopy.Click += Copy_Click;
        }

        private void ToggleFormatting_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            formatted = !formatted;
            tbxText.Text = formatted ? originalText : originalText.Replace(Environment.NewLine, "").Replace(" ", "");
            tbxText.TextWrapping = formatted ? Avalonia.Media.TextWrapping.NoWrap : Avalonia.Media.TextWrapping.Wrap;
        }

        private void Save_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            SaveFileAsync();
        }

        private async void SaveFileAsync()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            string s = await sfd.ShowAsync(this);

            if (string.IsNullOrEmpty(s)) return;

            try
            {
                File.WriteAllText(s, tbxText.Text);
            }
            catch (Exception e)
            {
                MessageBox.ShowDialog(e.Message, "Error");
            }
        }
        private void Copy_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(tbxText.Text);
            }
            catch (NotSupportedException nse)
            {
                MessageBox.ShowDialog(nse.Message, "Error");
            }
        }

        public TextWindow(string text, string title) : this(text)
        {
            Title = title;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
