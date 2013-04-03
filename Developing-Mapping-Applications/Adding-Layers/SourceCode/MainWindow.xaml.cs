using System.Windows;
using ESRI.ArcGIS.Client;
using System.Diagnostics;

namespace AddingLayers
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

    }
}
