using System;
using System.Windows;
using System.Diagnostics;
using System.Collections.Generic;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using System.Windows.Media;
using ESRI.ArcGIS.Client;

namespace EfficientlyAddingGraphics
{

    public partial class MainWindow : Window
    {
        const string defaultTime = "... Seconds";

        public MainWindow()
        {
            // License setting and ArcGIS Runtime initialization is done in Application.xaml.cs.
            InitializeComponent();

            IndividualPerformanceDisplay.Text = defaultTime;
            BulkPerformanceDisplay.Text = defaultTime;
        }

        private void InitializationFailed(object sender, System.EventArgs e)
        {
            var layer = (sender as Layer);
            MessageBox.Show("Failed to Initialize:" + layer.ID + layer.InitializationFailure.Message);
        }

        // Add graphics individually - Demonstrates the least efficient approach
        private void AddGraphicsIndividually_Click(object sender, RoutedEventArgs e)
        {
            ClearGraphicsFromGraphicsLayer();
            IndividualPerformanceDisplay.Text = defaultTime;
            
            Stopwatch sw = new Stopwatch();
            sw.Start();

            Random random = new Random();

            // Add graphics to a list and add in bulk (AddRange)
            var graphicsList = new List<Graphic>();
            for (int i = 0; i < 100000; i++)
            {
                var x = (random.NextDouble() * 40000000) - 20000000;
                var y = (random.NextDouble() * 40000000) - 20000000;
                var graphic = new Graphic()
                {
                    // assign each graphic a number 0, 1, 2 - we'll use this to represent the color
                    Geometry = new MapPoint(x, y),
                    Attributes = { { "Color", i % 3 } }
                };

                switch (graphic.Attributes["Color"].ToString())
                {
                    case "0":
                        graphic.Symbol = new SimpleMarkerSymbol() { Color = Brushes.Red, Size = 20 };
                        break;
                    case "1":
                        graphic.Symbol = new SimpleMarkerSymbol() { Color = Brushes.Green, Style = SimpleMarkerSymbol.SimpleMarkerStyle.Diamond, Size = 20 };
                        break;
                    case "2":
                        graphic.Symbol = new SimpleMarkerSymbol() { Color = Brushes.Blue, Style = SimpleMarkerSymbol.SimpleMarkerStyle.Triangle, Size = 20 };
                        break;
                    default:
                        break;
                }
                _graphicsLayer.Graphics.Add(graphic);
            }

            sw.Stop();
            IndividualPerformanceDisplay.Text = (sw.ElapsedMilliseconds / 1000.0).ToString() + " Seconds";

        }

        // Adds graphics in bulk and use a renderer - Demonstrates the most efficient approach
        private void AddGraphicsInBulk_Click(object sender, RoutedEventArgs e)
        {
            // Clear the existing Graphics
            ClearGraphicsFromGraphicsLayer();
            BulkPerformanceDisplay.Text = defaultTime;

            // Create a stopwatch to time the add
            Stopwatch sw = new Stopwatch();
            sw.Start();

            // Create a instance of the Random class to generate random coordinate pairs.
            Random random = new Random();

            // Add graphics to a List<Graphic> 
            var graphicsList = new List<Graphic>();
            for (int i = 0; i < 100000; i++)
            {
                var x = (random.NextDouble() * 40000000) - 20000000;
                var y = (random.NextDouble() * 40000000) - 20000000;
                var graphic = new Graphic()
                {
                    // assign each graphic a number 0, 1, 2 - we''ll use this to represent he color
                    Geometry = new MapPoint(x, y),
                    Attributes = { { "Color", i % 3 } }
                };
                graphicsList.Add(graphic);
            }

            // create a renderer with 3 symbols, rather than a symbol per graphic
            var renderer = new UniqueValueRenderer()
            {
                Field = "Color",
                Infos = { 
                    new UniqueValueInfo()
                    { 
                        Value = 0, 
                        Symbol = new SimpleMarkerSymbol() { Color = Brushes.Red, Size=20  }
                    },
                    new UniqueValueInfo()
                    { 
                        Value = 1,
                        Symbol = new SimpleMarkerSymbol() { Color = Brushes.Green, Style=SimpleMarkerSymbol.SimpleMarkerStyle.Diamond, Size=20 }
                    },
                    new UniqueValueInfo()
                    {
                        Value = 2, 
                        Symbol = new SimpleMarkerSymbol() { Color = Brushes.Blue, Style=SimpleMarkerSymbol.SimpleMarkerStyle.Triangle, Size=20 }
                    }
                }
            };

            _graphicsLayer.Renderer = renderer;

            // Bulk add of graphics - more efficient than adding one graphic at a time
            _graphicsLayer.Graphics.AddRange(graphicsList);
            sw.Stop();
            BulkPerformanceDisplay.Text = (sw.ElapsedMilliseconds / 1000.0).ToString() + " Seconds";       
        }

        private void ClearGraphicsFromGraphicsLayer() 
        {
            _graphicsLayer.Graphics.Clear();
        }

        private void ClearGraphics_Click(object sender, RoutedEventArgs e)
        {
            ClearGraphicsFromGraphicsLayer();
        }
    }
}
