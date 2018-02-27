using System;
using System.Diagnostics;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using AvaImage = Avalonia.Controls.Image;
using Newtonsoft.Json.Linq;
using System.Reflection;
using SixLabors.ImageSharp;
using Avalonia.Threading;
using System.Collections.Generic;

using CoreDrawablesGenerator.Generator;
using CoreDrawablesGenerator.Exporter;
using Newtonsoft.Json;

namespace CoreDrawablesGenerator
{
    public class MainWindow : Window
    {
        private static readonly string
            DIRECTORY_TEMPLATES = "Templates",
            DIRECTORY_OUTPUT = "Output";

        private static readonly int
            PREVIEW_MARGIN_LEFT = 153,
            PREVIEW_MARGIN_BOTTOM = 67;

        public GeneratorDataBindings Data
        {
            get
            {
                return DataContext as GeneratorDataBindings;
            }
        }

        #region Controls

        [InjectControl]
        private AvaImage imgPreview, imgPreviewBackground;
        [InjectControl]
        private Grid gridPreview;
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
        [InjectControl]
        private Button btnMove;
        [InjectControl]
        private Button btnThemeNatural, btnThemeLight, btnThemeDark;

        #endregion

        private DirectoryInfo dApp, dTemplates, dOutput;
        private FileWatcher fileWatcher = null;
        private Image<Rgba32> image = null;
        private int x = 0, y = 0;
        bool movingPreview = false;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif      
            
            DataContext = new GeneratorDataBindings();
            
            // Bind controls
            this.InjectControls();

            // Subscribe to events
            SubscribeControls();

            // Initialize
            try
            {
                CreateDirectories();
            }
            catch(DirectoryNotFoundException d)
            {
                MessageBox.ShowDialog(d.Message, "Error");
            }

            PopulateTemplates();
        }

        #region Initialization

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
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
            
            // Get default templates
            Data.Templates.Add(new Template("Common Pistol", ResourceManager.TextResource(Properties.Resources.Gun)));
            Data.Templates.Add(new Template("Common Shortsword", ResourceManager.TextResource(Properties.Resources.Sword)));
            Data.Templates.Add(new Template("Tesla Staff", ResourceManager.TextResource(Properties.Resources.TeslaStaff)));

            // Get user templates
            foreach (FileInfo file in dTemplates.EnumerateFiles())
            {
                try
                {
                    JObject template = JObject.Parse(File.ReadAllText(file.FullName));
                    string fileName = Path.GetFileNameWithoutExtension(file.Name);
                    Data.Templates.Add(new Template(fileName, template));
                }
                catch(Exception e)
                {
                    Trace.TraceWarning(e.ToString());
                    continue;
                }
            }

            // Select first template.
            if (Data.Templates.Count > 0)
                ddTemplate.SelectedIndex = 0;
        }

        /// <summary>
        /// Subscribe all controls to their respective event handlers.
        /// </summary>
        private void SubscribeControls()
        {
            btnSelectFile.Click += SelectFile_Click;
            
            btnItemDescriptor.Click += ItemDescriptor_Click;
            btnSpawnCommand.Click += SpawnCommand_Click;
            btnInventoryIcon.Click += InventoryIcon_Click;
            btnSingleTexture.Click += SingleTexture_Click;

            tbxHandX.PropertyChanged += Hand_PropertyChanged;
            tbxHandY.PropertyChanged += Hand_PropertyChanged;

            btnMove.KeyDown += Move_KeyDown;
            gridPreview.PointerPressed += Preview_PointerPressed;
            gridPreview.PointerMoved += Preview_PointerMoved;
            gridPreview.PointerReleased += Preview_PointerReleased;
            Deactivated += Preview_PointerReleased;

            btnThemeNatural.Click += ChangeTheme_Click;
            btnThemeLight.Click += ChangeTheme_Click;
            btnThemeDark.Click += ChangeTheme_Click;

            btnThemeNatural.PointerEnter += Preview_PointerReleased;
            btnThemeLight.PointerEnter += Preview_PointerReleased;
            btnThemeDark.PointerEnter += Preview_PointerReleased;
        }

        #endregion

        #region Theme

        /// <summary>
        /// Changes the preview background.
        /// </summary>
        private void ChangeTheme_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Control c = sender as Control;

            if (!imgPreviewBackground.Resources.ContainsKey(c.Tag))
                    throw new ArgumentException("Unknown theme " + c.Tag);


            imgPreviewBackground.Source = (imgPreviewBackground.Resources[c.Tag] as AvaImage).Source;
        }

        #endregion

        #region Select File

        private void SelectFile_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            SelectFileAsync();
        }

        private async void SelectFileAsync()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filters = new List<FileDialogFilter>()
            {
                new FileDialogFilter()
                {
                    Name = "Image files",
                    Extensions = new List<string>()
                    {
                        "png", "jpg", "bmp"
                    }
                }
            };
            ofd.Title = "Select image.";

            string[] files = await ofd.ShowAsync();
            if (files != null && files.Length > 0)
            {
                UpdatePreview(files[0]);
                WatchImage(files[0]);
            }
        }

        private void StopWatching()
        {
            // Dispose previous, if any.
            if (fileWatcher != null && !fileWatcher.IsDisposed)
            {
                fileWatcher.Dispose();
            }
            fileWatcher = null;
        }

        #endregion

        #region FileWatcher

        private void WatchImage(string file)
        {
            StopWatching();

            // Watch file for changes.
            fileWatcher = new FileWatcher(file);
            fileWatcher.FileChanged += FileWatcher_FileChanged;
            fileWatcher.FileDeleted += FileWatcher_FileDeleted;
        }

        /// <summary>
        /// Can no longer track file for changes; keep using image in memory.
        /// </summary>
        private void FileWatcher_FileDeleted()
        {
            StopWatching();
        }

        /// <summary>
        /// Update preview.
        /// </summary>
        private void FileWatcher_FileChanged()
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                UpdatePreview(fileWatcher.FilePath);
            });
        }

        #endregion

        #region Preview

        /// <summary>
        /// Start tracking movement.
        /// </summary>
        private void Preview_PointerPressed(object sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            movingPreview = true;
            Preview_PointerMoved(sender, e);
        }
        
        /// <summary>
        /// Stop tracking movement.
        /// </summary>
        private void Preview_PointerReleased(object sender, EventArgs e)
        {
            movingPreview = false;
        }

        /// <summary>
        /// While moving the preview image, update the position.
        /// </summary>
        private void Preview_PointerMoved(object sender, Avalonia.Input.PointerEventArgs e)
        {
            if (movingPreview)
            {
                Point p = e.GetPosition(gridPreview);

                int x = (int)Math.Floor((p.X - PREVIEW_MARGIN_LEFT) / 2 - imgPreview.Width / 4);
                tbxHandX.Text = x.ToString();
                tbxHandX.InvalidateVisual();

                int y = (int)Math.Floor((gridPreview.Height - p.Y - PREVIEW_MARGIN_BOTTOM) / 2 - imgPreview.Height / 4);
                tbxHandY.Text = y.ToString();
                tbxHandY.InvalidateVisual();
            }
        }

        /// <summary>
        /// While Move control has focus, user can use the arrow keys to move the preview.
        /// </summary>
        private void Move_KeyDown(object sender, Avalonia.Input.KeyEventArgs e)
        {
            try
            {
                int x = Convert.ToInt32(tbxHandX.Text);
                int y = Convert.ToInt32(tbxHandY.Text);

                switch (e.Key)
                {
                    case Avalonia.Input.Key.Left:
                        tbxHandX.Text = (--x).ToString();
                        break;
                    case Avalonia.Input.Key.Right:
                        tbxHandX.Text = (++x).ToString();
                        break;
                    case Avalonia.Input.Key.Up:
                        tbxHandY.Text = (++y).ToString();
                        break;
                    case Avalonia.Input.Key.Down:
                        tbxHandY.Text = (--y).ToString();
                        break;
                    default:
                        return;
                }

                this.x = x;
                this.y = y;
            }
            catch
            {
                tbxHandX.Text = "0";
                tbxHandX.InvalidateVisual();
                tbxHandY.Text = "0";
                tbxHandY.InvalidateVisual();
            }
        }

        private void TbxHand_KeyDown(object sender, Avalonia.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == Avalonia.Input.Key.Up || e.Key == Avalonia.Input.Key.Down)
                {
                    TextBox tbxSender = sender as TextBox;

                    int v = Convert.ToInt32(tbxSender.Text);

                    switch (e.Key)
                    {
                        case Avalonia.Input.Key.Up:
                            v++;
                            break;
                        case Avalonia.Input.Key.Down:
                            v--;
                            break;
                        default:
                            return;
                    }

                    tbxSender.Text = v.ToString();
                }
            }
            catch
            {

            }
        }

        private void Hand_PropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property.Name == "Text")
            {
                UpdatePosition();
            }
        }
        
        /// <summary>
        /// Updates the preview image, using the given file.
        /// </summary>
        /// <param name="file">File path of image.</param>
        private void UpdatePreview(string file)
        {
            try
            {
                using (FileStream stream = File.OpenRead(file))
                {
                    image?.Dispose();
                    image = SixLabors.ImageSharp.Image.Load<Rgba32>(stream);

                    // Upscale without losing quality (currently pointless but as Avalonia grows perhaps not).
                    using (Image<Rgba32> scaledImage = new Image<Rgba32>(image.Width * 2, image.Height * 2))
                    {
                        for (int y = 0; y < image.Height; y++)
                        {
                            for (int x = 0; x < image.Width; x++)
                            {
                                scaledImage[x * 2, y * 2] = scaledImage[x * 2 + 1, y * 2] = scaledImage[x * 2, y * 2 + 1] = scaledImage[x * 2 + 1, y * 2 + 1] = image[x, y];
                            }
                        }

                        // Set preview image.
                        imgPreview.Width = scaledImage.Width;
                        imgPreview.Height = scaledImage.Height;
                        imgPreview.Source = scaledImage.ToAvaloniaBitmap();

                        UpdatePosition();
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());

                imgPreview.Source = null;
                image?.Dispose();
                image = null;

                MessageBox.ShowDialog("Couldn't load image. Please ensure you have selected a valid png, jpg or bmp image file.", "Error");
            }

            // Warn for large images
            if (image != null && image.Width * image.Height > 32768)
            {
                MessageBox.ShowDialog(string.Format("The selected image is {0} by {1} pixels, which is huge! Please consider using smaller images.", image.Width, image.Height), "Warning");
            }
        }

        private void UpdatePosition()
        {
            try
            {
                x = Convert.ToInt32(tbxHandX.Text);
                y = Convert.ToInt32(tbxHandY.Text);
                imgPreview.Margin = new Thickness(PREVIEW_MARGIN_LEFT + x * 2, 0, 0, PREVIEW_MARGIN_BOTTOM + y * 2);
            }
            catch
            {

            }
        }

        #endregion

        #region Export

        private void ItemDescriptor_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (image == null)
            {
                MessageBox.ShowDialog("Please select an image first!", "Error");
                return;
            }

            try
            {
                DrawablesGenerator generator = new DrawablesGenerator(image);
                generator.OffsetX = Convert.ToInt32(tbxHandX.Text) + 1;
                generator.OffsetY = Convert.ToInt32(tbxHandY.Text);

                generator.FlipY = true;
                generator.ReplaceBlank = true;
                generator.ReplaceWhite = true;
                
                if (Data.IgnoreWhite)
                    generator.IgnoreColor = new Rgba32(255, 255, 255, 255);

                DrawablesOutput output = generator.Generate();
                Template t = GetTemplate();

                Exporter.Exporter exporter = new Exporter.Exporter(output, t.Config);
                if (Data.WeaponGroup)
                    exporter.Groups.Add("weapon");

                JObject descriptor = exporter.GetDescriptor(Data.InventoryIcon);

                TextWindow tw = new TextWindow(descriptor.ToString(Formatting.Indented), "Item Descriptor");
                tw.ShowDialog();
            }
            catch (ArgumentException exc)
            {
                MessageBox.ShowDialog("Illegal argument:\n" + exc.Message, "Error");
            }
            catch (FormatException)
            {
                MessageBox.ShowDialog("Could not convert hand offsets to numbers.", "Error");
            }
            catch (Exception exc)
            {
                MessageBox.ShowDialog("Uncaught exception:\n" + exc.Message, "Error");
            }
        }

        private void SpawnCommand_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (image == null)
            {
                MessageBox.ShowDialog("Please select an image first!", "Error");
                return;
            }

            try
            {
                DrawablesGenerator generator = new DrawablesGenerator(image);
                generator.OffsetX = Convert.ToInt32(tbxHandX.Text);
                generator.OffsetY = Convert.ToInt32(tbxHandY.Text);

                generator.FlipY = true;

                if (Data.IgnoreWhite)
                    generator.IgnoreColor = new Rgba32(255, 255, 255, 255);

                DrawablesOutput output = generator.Generate();
                Template t = GetTemplate();

                Exporter.Exporter exporter = new Exporter.Exporter(output, t.Config);
                if (Data.WeaponGroup)
                    exporter.Groups.Add("weapon");

                string command = exporter.GetCommand(Data.InventoryIcon);

                TextWindow tw = new TextWindow(command, "Spawn Command");
                tw.ShowDialog();
            }
            catch (ArgumentException exc)
            {
                MessageBox.ShowDialog("Illegal argument:\n" + exc.Message, "Error");
            }
            catch (FormatException)
            {
                MessageBox.ShowDialog("Could not convert hand offsets to numbers.", "Error");
            }
            catch (Exception exc)
            {
                MessageBox.ShowDialog("Uncaught exception:\n" + exc.Message, "Error");
            }
        }

        private void InventoryIcon_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (image == null)
            {
                MessageBox.ShowDialog("Please select an image first!", "Error");
                return;
            }

            try
            {
                DrawablesGenerator generator = new DrawablesGenerator(image);

                generator.FlipY = true;

                if (Data.IgnoreWhite)
                    generator.IgnoreColor = new Rgba32(255, 255, 255, 255);
                
                DrawablesOutput output = generator.Generate();

                TextWindow tw = new TextWindow(DrawablesGenerator.GenerateInventoryIcon(output).ToString(Formatting.Indented), "Inventory Icon");
                tw.ShowDialog();
            }
            catch (ArgumentNullException)
            {
                MessageBox.ShowDialog("Argument may not be null. Did you select a valid image?", "Error");
            }
            catch (DrawableException exc)
            {
                MessageBox.ShowDialog(exc.Message, "Error");
            }
        }

        private void SingleTexture_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (image == null)
            {
                MessageBox.ShowDialog("Please select an image first!", "Error");
                return;
            }

            try
            {
                DrawablesGenerator generator = new DrawablesGenerator(image);
                generator.OffsetX = Convert.ToInt32(tbxHandX.Text);
                generator.OffsetY = Convert.ToInt32(tbxHandY.Text);

                generator.FlipY = true;

                generator.ReplaceBlank = true;
                generator.ReplaceWhite = true;

                if (Data.IgnoreWhite)
                    generator.IgnoreColor = new Rgba32(255, 255, 255, 255);

                DrawablesOutput output = generator.Generate();

                int j = int.Parse(tbxSourceImageSize.Text);
                TextWindow tw = new TextWindow(DrawablesGenerator.GenerateSingleTextureDirectives(output, j), "Single Texture Directives");
                tw.ShowDialog();
            }
            catch (ArgumentNullException)
            {
                MessageBox.ShowDialog("Could not parse the hand position or the source image size (text field next to the button). Please hover over it for more information.", "Error");
            }
            catch (FormatException)
            {
                MessageBox.ShowDialog("Could not parse the hand position or source image size (text field next to the button). Please hover over it for more information.", "Error");
            }
            catch (DrawableException exc)
            {
                MessageBox.ShowDialog(exc.Message, "Error");
            }
        }

        private Template GetTemplate()
        {
            if (ddTemplate.SelectedIndex == -1) ddTemplate.SelectedIndex = 0;
            
            return Data.Templates[ddTemplate.SelectedIndex];
        }

        #endregion

        /// <summary>
        /// Because I'd forget how to call it minutes after using it once.
        /// </summary>
        private async void RunOnUI(Action a)
        {
            await Dispatcher.UIThread.InvokeAsync(a);
        }
    }
}
