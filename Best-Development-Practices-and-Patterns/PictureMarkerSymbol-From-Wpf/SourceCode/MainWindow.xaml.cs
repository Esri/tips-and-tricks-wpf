using System.Windows;
using ESRI.ArcGIS.Client;
using System.IO;
using System.Xml;
using ESRI.ArcGIS.Client.Symbols;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using System;
using System.Windows.Media.Imaging;
using ESRI.ArcGIS.Client.Geometry;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace PictureMarkerSymbolFromWpf
{

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            // License setting and ArcGIS Runtime initialization is done in Application.xaml.cs.

            InitializeComponent();

            
        }

        private void AddGraphicToMap_Click(object sender, RoutedEventArgs e)
        {            
                      
            // Create a diagonal linear gradient with four stops.   
            // http://msdn.microsoft.com/en-us/library/system.windows.media.lineargradientbrush.aspx
            LinearGradientBrush myLinearGradientBrush =
                new LinearGradientBrush();
            myLinearGradientBrush.StartPoint = new Point(0, 0);
            myLinearGradientBrush.EndPoint = new Point(1, 1);
            myLinearGradientBrush.GradientStops.Add(
                new GradientStop(Colors.Yellow, 0.0));
            myLinearGradientBrush.GradientStops.Add(
                new GradientStop(Colors.Red, 0.25));
            myLinearGradientBrush.GradientStops.Add(
                new GradientStop(Colors.Blue, 0.75));
            myLinearGradientBrush.GradientStops.Add(
                new GradientStop(Colors.LimeGreen, 1.0));
 
            // Add an Ellipse Element
            // http://msdn.microsoft.com/en-us/library/system.windows.shapes.ellipse.aspx
            Ellipse myEllipse = new Ellipse();
            myEllipse.Stroke = System.Windows.Media.Brushes.Black;
            myEllipse.Fill = myLinearGradientBrush;
            myEllipse.HorizontalAlignment = HorizontalAlignment.Left;
            myEllipse.VerticalAlignment = VerticalAlignment.Center;
            myEllipse.Width = 50;
            myEllipse.Height = 50;
            
 
            //Force render
            myEllipse.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            myEllipse.Arrange(new Rect(myEllipse.DesiredSize));
 
            RenderTargetBitmap render = new RenderTargetBitmap(200, 200, 150, 150, PixelFormats.Pbgra32);
            render.Render(myEllipse);
 
            PictureMarkerSymbol pms = new PictureMarkerSymbol() 
            {
                Source = render,
            };

            Random random = new Random();
            var graphic = new Graphic() { Geometry = new MapPoint(random.Next(-20000000, 20000000), random.Next(-20000000, 20000000)) };
            graphic.Symbol = pms;
            MarkerGraphicsLayer.Graphics.Add(graphic);
        
        }
    }
}
