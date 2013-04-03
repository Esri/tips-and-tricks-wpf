using System.Windows;
using ESRI.ArcGIS.Client;
using System.ComponentModel;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Local;
using System.Collections.Generic;

namespace PersistSchemaOnOutput
{

    public partial class MainWindow : Window, INotifyPropertyChanged

    {
        #region

        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isBusy = true;

        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                _isBusy = value;

                if (PropertyChanged != null)
                {

                    PropertyChanged(this, new PropertyChangedEventArgs("IsBusy"));
                }
            }
        }
        #endregion

        Geoprocessor _gpTask;

        // Declare a static WebMercator to access the help methods.
        private static ESRI.ArcGIS.Client.Projection.WebMercator _mercator =
                new ESRI.ArcGIS.Client.Projection.WebMercator();

        public MainWindow()
        {
            InitializeComponent();

            // Start the LocalGeoprocessing Service
            LocalGeoprocessingService.GetServiceAsync(@"..\Maps-and-Data\SimpleBuffer.gpk", GPServiceType.Execute, (gpService) =>
            {
                // Check the error...
                if (gpService.Error != null)
                { MessageBox.Show(gpService.Error.Message); return; }


                _gpTask = new Geoprocessor(gpService.Tasks[0].Url);
                MyMap.MouseClick += MyMap_MouseClick;
                DataContext = this;
                IsBusy = false;
            });
        }

        private void MyMap_MouseClick(object sender, ESRI.ArcGIS.Client.Map.MouseEventArgs e)
        {
            IsBusy = true;
            // Cancel any outstanding Tasks
            _gpTask.CancelAsync();

            // Get the GraphicsLayer
            GraphicsLayer graphicsLayer = MyMap.Layers["MyGraphicsLayer"] as GraphicsLayer;
            graphicsLayer.ClearGraphics();
            e.MapPoint.SpatialReference = MyMap.SpatialReference;
            
            // Add a graphic at the click point
            Graphic graphic = new ESRI.ArcGIS.Client.Graphic()
            {
                Geometry = e.MapPoint,
                Symbol = LayoutRoot.Resources["DefaultClickSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol
            };
            graphic.SetZIndex(1);
            graphicsLayer.Graphics.Add(graphic);

            // Create the graphic to submit.
            Graphic g = new Graphic() { Geometry = _mercator.ToGeographic(e.MapPoint) };

            // Create a new list of GP Parameters
            List<GPParameter> gpParams = new List<GPParameter>();

            // We want to specify input attributes - create a new FeatureSet.
            FeatureSet featureSet = new FeatureSet();

            // Create the Fields and add one called "VALUE".
            featureSet.Fields = new List<Field> { new Field() { FieldName = "VALUE", Type = Field.FieldType.String, Alias = "VALUE" } };

            //var fs = new FeatureSet(new List<Graphic> { g });
            // Add the graphic to the FeatureSet
            featureSet.Features.Add(g);

            // Set the graphic's attribute
            featureSet.Features[0].Attributes["VALUE"] = valueText.Text;

            // Add the GP Paramr
            gpParams.Add(new GPFeatureRecordSetLayer("InputFeatures", featureSet));
            gpParams.Add(new GPLinearUnit("Distance", esriUnits.esriKilometers, 500));

            // Register an inline handler for ExecuteCompleted event
            _gpTask.ExecuteCompleted += (s, e1) =>
            {
                // Clear the graphics layer (will remove the input Pushpin)
                graphicsLayer.Graphics.Clear();

                // Get the results
                GPExecuteResults results = e1.Results;
                GPFeatureRecordSetLayer rs = results.OutParameters[0] as GPFeatureRecordSetLayer;
 
                // Get the result feature
                Graphic graphicBuff = rs.FeatureSet.Features[0];
                
                // Set the symbol
                graphicBuff.Symbol = LayoutRoot.Resources["DefaultFillSymbol"] as ESRI.ArcGIS.Client.Symbols.Symbol;
                
                // Add to the layer
                graphicsLayer.Graphics.Add(graphicBuff);

                // Stop the progress bar
                IsBusy = false;
            };

            // Register an inline handler for the failed event (just in case)
            _gpTask.Failed += (s2, e2) =>
            {
                MessageBox.Show(e2.Error.Message);
                IsBusy = false;
            };

            // Execute the Buffer asynchronously
            _gpTask.ExecuteAsync(gpParams);
        }

    }
}
