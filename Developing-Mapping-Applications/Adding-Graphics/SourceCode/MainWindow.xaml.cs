using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using System.Collections.Generic;
using System;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;

namespace AddingGraphics
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            // License setting and ArcGIS Runtime initialization is done in Application.xaml.cs.
            InitializeComponent();
        }

        private void InitializationFailed(object sender, System.EventArgs e)
        {
            var layer = (sender as Layer);
            MessageBox.Show("Failed to Initialize:" + layer.ID + layer.InitializationFailure.Message);
        }

        private void ClickMe_Button_Click(object sender, RoutedEventArgs e)
        {
            AddGraphics();
            MoveGraphics();
            HitGraphics();
        }

        // Adds graphics in an efficient way (bulk add)
        private void AddGraphics()
        {
            _graphicsLayer.Graphics.Clear();

            Random random = new Random();

            // Add graphics to a list and add in bulk (AddRange)
            var graphicsList = new List<Graphic>();
            for (int i = 0; i < 5000; i++)
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

            // Create a renderer with 3 symbols (as opposed to a symbol per graphic)
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

            // Assign the renderer
            _graphicsLayer.Renderer = renderer;

            // Bulk add Graphics to the GraphicsLayer - more efficient than adding one graphic at a time
            _graphicsLayer.Graphics.AddRange(graphicsList);
        }


        // Move graphics on a timer
        DispatcherTimer _timer;
        private void MoveGraphics()
        {
            _timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(0.05)  // Move 20 times a second
            };
            _timer.Tick += (s, e) =>
            {
                int id = 0;
                foreach (var g in _graphicsLayer)
                {
                    var p = g.Geometry as MapPoint;

                    var dx = Math.Cos(id * .2);
                    var dy = Math.Sin(id * .2);

                    // For points - its most efficient to do one MoveTo on the graphics geometry
                    p.MoveTo(p.X + (dx * 50000d), p.Y + (dy * 5000d));
                    id++;
                }
            };
            _timer.Start();
        }

        // Listen to graphics that have mouse interaction
        int score = 0;
        private void HitGraphics()
        {
            _graphicsLayer.MouseEnter += (s, e) =>
                {
                    // The GraphicMouseEventArgs class contains the Graphic the mouse entered
                    // Remove the Graphic from the GraphicsLayer
                    _graphicsLayer.Graphics.Remove(e.Graphic);
                    ClickMe_Button.Content = score.ToString();
                    score++;
                };
        }
    }
}
