using System;
using System.Diagnostics;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using AvaImage = Avalonia.Controls.Image;
using Newtonsoft.Json.Linq;
using SixLabors.ImageSharp;
using System.Reflection;

namespace CoreDrawablesGenerator
{
    public class MainWindow : Window
    {
        private static readonly string
            DIRECTORY_TEMPLATES = "Templates",
            DIRECTORY_OUTPUT = "Output";

        [InjectControl]
        private AvaImage imgPreview;
        [InjectControl]
        private Button btnSelectFile;
        [InjectControl]
        private CheckBox chkIgnoreWhite;
        [InjectControl]
        private TextBox tbxHandX, tbxHandY;
        [InjectControl]
        private DropDown ddTemplate;
        [InjectControl]
        private CheckBox chkWeaponGroup, chkInventoryIcon;
        [InjectControl]
        private Button btnItemDescriptor, btnSpawnCommand, btnInventoryIcon, btnSingleTexture;
        [InjectControl]
        private TextBox tbxSourceImageSize;
        
        private DirectoryInfo dApp, dTemplates, dOutput;
        
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif      
            
            DataContext = new DataBindings();
            
            // Bind controls
            this.InjectControls();

            // Subscribe to events
            SubscribeControls();

            // Initialize
            CreateDirectories();
            PopulateTemplates();
        }

        private void CreateDirectories()
        {
            dApp = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            if (!dApp.Exists)
            {
                throw new DirectoryNotFoundException("Application directory could not be determined.");
            }

            dOutput = new DirectoryInfo(Path.Combine(dApp.FullName, DIRECTORY_OUTPUT));
            dTemplates = new DirectoryInfo(Path.Combine(dApp.FullName, DIRECTORY_TEMPLATES));

            if (!dOutput.Exists)
                dOutput.Create();
            if (!dTemplates.Exists)
                dTemplates.Create();
        }

        private void PopulateTemplates()
        {
            ddTemplate.SelectedIndex = -1;

            DataBindings data = DataContext as DataBindings;

            // Get default templates
            data.Templates.Add(new Template("Common Pistol", ResourceManager.TextResource(Properties.Resources.Gun)));
            data.Templates.Add(new Template("Common Shortsword", ResourceManager.TextResource(Properties.Resources.Sword)));
            data.Templates.Add(new Template("Tesla Staff", ResourceManager.TextResource(Properties.Resources.TeslaStaff)));

            // Get user templates
            foreach (FileInfo file in dTemplates.EnumerateFiles())
            {
                try
                {
                    JObject template = JObject.Parse(File.ReadAllText(file.FullName));
                    string fileName = Path.GetFileNameWithoutExtension(file.Name);
                    data.Templates.Add(new Template(fileName, template));
                }
                catch(Exception e)
                {
                    Trace.TraceWarning(e.ToString());
                    continue;
                }
            }

            // Select first template.
            if (data.Templates.Count > 0)
                ddTemplate.SelectedIndex = 0;
        }

        private void SubscribeControls()
        {
            btnSelectFile.Click += SelectFile_Click;
            
            btnItemDescriptor.Click += ItemDescriptor_Click;
            btnSpawnCommand.Click += SpawnCommand_Click;
            btnInventoryIcon.Click += InventoryIcon_Click;
            btnSingleTexture.Click += SingleTexture_Click;
        }

        private void SelectFile_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            SelectFileAsync();
        }

        private void ItemDescriptor_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void SpawnCommand_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void InventoryIcon_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void SingleTexture_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        async void SelectFileAsync()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            string[] files = await ofd.ShowAsync();
            if (files.Length > 0)
            {
                this.FindControl<AvaImage>("previewImage").Source = new Bitmap(files[0]);

                using (FileStream stream = File.OpenRead(files[0]))
                using (Image<Rgba32> image = SixLabors.ImageSharp.Image.Load<Rgba32>(stream))
                {

                }
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
