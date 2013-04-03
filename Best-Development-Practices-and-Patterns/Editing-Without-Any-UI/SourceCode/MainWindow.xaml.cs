using System.Linq;
using System;
using System.Windows;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Local;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.FeatureService;
using ESRI.ArcGIS.Client.Geometry;

namespace EditingWithoutAnyUI
{
    public partial class MainWindow : Window
    {
        FeatureLayer _featureLayerTable;
        LocalFeatureService _localFeatureService;

        public MainWindow()
        {
            InitializeComponent();

            AddRecordButton.IsEnabled = false;

            // Create the LocalFeatureService and set MaxRecords property
            _localFeatureService = new LocalFeatureService(@"..\Maps-and-Data\CitiesOverOneMillion.mpk")
            {
                MaxRecords = 100000,
            };

            // Register an event handler for the PropertyChanged Event
            _localFeatureService.PropertyChanged += (s, propertyChangedEventArgs) =>
            {
                // Get the property
                var property = _localFeatureService.GetType().GetProperty(propertyChangedEventArgs.PropertyName);

                // Get the Value
                var value = property.GetValue(_localFeatureService, null);

                if (value == null)
                    return;

                // Get the property type
                string varType = value.GetType().ToString();

                // Display the property info
                switch (varType)
                {
                    case "System.Collections.ObjectModel.ReadOnlyCollection`1[ESRI.ArcGIS.Client.Local.LayerDetails]":
                        statusDisplay.Items.Insert(0, propertyChangedEventArgs.PropertyName + ": " + _localFeatureService.FeatureLayers.Count.ToString());
                        break;
                    default:
                        statusDisplay.Items.Insert(0, propertyChangedEventArgs.PropertyName + ": " + value.ToString());
                        break;
                }

                // Display the error
                if (_localFeatureService.Error != null)
                    statusDisplay.Items.Insert(0, "Error: " + _localFeatureService.Error.Message);
            };

            // Start the LocalFeatureService
            _localFeatureService.StartAsync(x =>
            {
                // Create a new FeatureLayer (to contain the table)
                _featureLayerTable = new FeatureLayer()
                {
                    Url = _localFeatureService.UrlFeatureService + "/0",
                    ID = "EditTable",
                    DisableClientCaching = true,
                    AutoSave = false,
                    Mode = ESRI.ArcGIS.Client.FeatureLayer.QueryMode.Snapshot,
                    OutFields = new OutFields() { "*" },
                };

                /*
                * Register a series of inline event handlers...
                */

                // Register an handler for the InitializationFailed event
                _featureLayerTable.InitializationFailed += (s, e) =>
                {
                    statusDisplay.Items.Insert(0, _featureLayerTable.InitializationFailure.Message);
                };

                // Register a handler for the Initialized event (raised by an explicit Initialize call)
                _featureLayerTable.Initialized += (s, e) =>
                {
                    statusDisplay.Items.Insert(0, "FeatureLayer Initialized Event Raised");
                    statusDisplay.Items.Insert(0, "FeatureLayer contains: " + _featureLayerTable.Graphics.Count.ToString() + " features"); //This will be 0.
                };

                // Register a handler for the UpdateCompleted event (raised by an explicit Update call)
                _featureLayerTable.UpdateCompleted += (senderObj, eventArgs) =>
                {
                    statusDisplay.Items.Insert(0, "FeatureLayer UpdateCompleted Event Raised");
                    statusDisplay.Items.Insert(0, "FeatureLayer contains: " + _featureLayerTable.Graphics.Count.ToString() + " features"); //This will be the total number (n) graphic features (up to service query limit).
                    AddRecordButton.IsEnabled = true;
                };

                // Register a handler for the Begin Save Edits event (raised by the explicit Save Edits call)
                _featureLayerTable.BeginSaveEdits += (s1, beginEditEventArgs) =>
                {
                    statusDisplay.Items.Insert(0, "FeatureLayer BeginSaveEdits Event Raised");
                };

                // Register a handler for the End Save Edits event (raised by server response)
                _featureLayerTable.EndSaveEdits += (s2, endEditEventArgs) =>
                {
                    statusDisplay.Items.Insert(0, "## Edit Successful ##");
                    // Edit was successful - call Update to trigger a refresh and display the number of features
                    _featureLayerTable.Update();
                };

                // Register a handler for the Save Edits Failed event (raised by server response)
                _featureLayerTable.SaveEditsFailed += (s3, taskFailedEventArgs) =>
                {
                    statusDisplay.Items.Insert(0, "FeatureLayer SaveEditsFailed Event Raised: " + taskFailedEventArgs.Error.Message);
                };

                MyMap.Layers.Add(_featureLayerTable);
            });
        }

        private void AddRecord_Click(object sender, RoutedEventArgs e)
        {
            // Create a new Graphic (represents a record in the standalone table)
            var graphic = new Graphic();

            //use PrototypeAttributes from the layer templates
            FeatureTemplate featureTemplate = null;
            if (_featureLayerTable.LayerInfo.Templates != null && _featureLayerTable.LayerInfo.Templates.Count > 0)
                featureTemplate = _featureLayerTable.LayerInfo.Templates.FirstOrDefault().Value;

            if (featureTemplate != null && featureTemplate.PrototypeAttributes != null)
            {
                foreach (var item in featureTemplate.PrototypeAttributes)
                    graphic.Attributes[item.Key] = item.Value;
            }

            Random random = new Random();
            graphic.Geometry = new MapPoint(random.Next(-180, 180), random.Next(-90, 90));

            // Set a specific attribute value
            graphic.Attributes["CNTRY_NAME"] = DateTime.Now.ToString();

            // Add the Graphic to the FeatureLayer
            _featureLayerTable.Graphics.Add(graphic);

            // Call Save Edits (will raise event handlers registered in the constructor).
            _featureLayerTable.SaveEdits();
        }
    }
}
