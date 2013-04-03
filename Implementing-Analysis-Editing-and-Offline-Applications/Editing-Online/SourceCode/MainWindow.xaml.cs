using System.Windows;
using ESRI.ArcGIS.Client;

namespace EditingOnline
{

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            // License setting and ArcGIS Runtime initialization is done in Application.xaml.cs.

            InitializeComponent();
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
