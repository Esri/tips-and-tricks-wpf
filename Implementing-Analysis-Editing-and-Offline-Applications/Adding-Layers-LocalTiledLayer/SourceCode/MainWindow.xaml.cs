using System.Windows;
using ESRI.ArcGIS.Client;
using System.Diagnostics;
using ESRI.ArcGIS.Client.Symbols;
using System.Windows.Media;
using System.Windows.Controls;

namespace TileCache
{

    public partial class MainWindow : Window
    {
        GraphicsLayer graphicsLayer;

        public MainWindow()
        {
            // License setting and ArcGIS Runtime initialization is done in Application.xaml.cs.

            InitializeComponent();

            MyMap.Layers.LayersInitialized += (s, e) =>
            {
                graphicsLayer = MyMap.Layers["ExtentGraphicsLayer"] as GraphicsLayer;

                ArcGISLocalTiledLayer localTiledLayer = MyMap.Layers["EastCoastUS"] as ArcGISLocalTiledLayer;

                // Add graphic to represent Initial Extent
                Graphic initialExtentGraphic = new Graphic()
                {
                    Symbol = new SimpleFillSymbol()
                    {
                        BorderBrush = Brushes.Green,
                        BorderThickness = 5
                    },
                    Geometry = localTiledLayer.InitialExtent,
                };
                initialExtentGraphic.Attributes["MapTipText"] = "Initial Extent";
                graphicsLayer.Graphics.Add(initialExtentGraphic);

                // Add graphic to represent Full Extent
                Graphic fullExtentGraphic = new Graphic()
                {
                    Symbol = new SimpleFillSymbol()
                    {
                        BorderBrush = Brushes.Red,
                        BorderThickness = 5
                    },
                    Geometry = localTiledLayer.FullExtent,
                };
                fullExtentGraphic.Attributes["MapTipText"] = "Full Extent";
                graphicsLayer.Graphics.Add(fullExtentGraphic);

                MyMap.ZoomTo(initialExtentGraphic.Geometry);
            };

    }

        private void MyMap_ExtentChanged(object sender, ExtentEventArgs e)
        {
           
            

            
        }
    }
}
