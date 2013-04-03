using System.Windows;
using System.Reflection;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Geometry;

namespace GraphicsToFromJson
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            // License setting and ArcGIS Runtime initialization is done in Application.xaml.cs.

            InitializeComponent();

            try
            {
                // Load initial extent from user settings
                var ext = Properties.Settings.Default.InitialExtent;
                if (ext != null && ext.Length > 0)
                {
                    MyMap.Extent = Envelope.FromJson(ext) as Envelope;
                }

            }
            catch (System.Exception exception)
            {
                MessageBox.Show(exception.Message);
            }

            AddGraphicFromJsonFile();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            try
            {
                // Save current extent to user settings
                Properties.Settings.Default.InitialExtent = MyMap.Extent.ToJson();
                Properties.Settings.Default.Save();
            }
            catch (System.Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void AddGraphicFromJsonFile()
        {
            try
            {
                // Load and display a polygon from a text file.
                string filename = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\BlueTrail.txt";
                string json = System.IO.File.ReadAllText(filename);

                var line = ESRI.ArcGIS.Client.Geometry.Geometry.FromJson(json);
                var graphic = new Graphic() { Geometry = line, Symbol = new SimpleLineSymbol() { Color = Brushes.Blue, Width = 5 } };
                _graphicsLayer.Graphics.Add(graphic);

                MyMap.ZoomTo(line.Extent.Expand(1.5));
            }
            catch (System.Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
    }
}
