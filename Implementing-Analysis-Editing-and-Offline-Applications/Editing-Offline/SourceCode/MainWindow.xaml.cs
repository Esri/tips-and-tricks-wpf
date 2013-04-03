using System.Windows;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Local;

namespace EditingOffline
{

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            // License setting and ArcGIS Runtime initialization is done in Application.xaml.cs.

            InitializeComponent();

            // Start a LocalGeometryService for the Editor Widget.
            LocalGeometryService.GetServiceAsync(serviceCallback =>
            {
                MyEditorWidget.GeometryServiceUrl = serviceCallback.UrlGeometryService;
            });

            MyMap.Layers.LayersInitialized += (s, e) => 
            {
                ArcGISLocalFeatureLayer evacPerimeter = MyMap.Layers["Evacuation Perimeter"] as ArcGISLocalFeatureLayer;
                MyMap.ZoomTo(evacPerimeter.FullExtent.Expand(1.5));
            };
        }

        private void MyDataGrid_Initialized(object sender, System.EventArgs e)
        {
            MyDataGrid.GraphicsLayer = MyMap.Layers["Points of Interest"] as GraphicsLayer;
        }

        private void FeatureLayer_MouseLeftButtonUp(object sender, GraphicMouseButtonEventArgs e)
        {
            var fl = (sender) as FeatureLayer;
            e.Graphic.Select();
            MyDataGrid.ScrollIntoView(e.Graphic, null);
        }
    }
}
