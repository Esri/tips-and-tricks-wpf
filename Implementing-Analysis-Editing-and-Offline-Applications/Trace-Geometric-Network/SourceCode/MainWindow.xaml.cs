using System.Windows;
using System;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Local;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Geometry;
using System.Collections.Generic;
using ESRI.ArcGIS.Client.Symbols;

namespace TraceGeometricNetworkApp
{
    public partial class MainWindow : Window
    {
        LocalGeoprocessingService traceNetworkLocalGpService;
        Geoprocessor geoprocessorTask;
        LocalGeometryService localGeometryService;
        GeometryService geometryTask;
        LocalMapService waterNetworkLocalMapService;
        GraphicsLayer inputGraphicsLayer;
        GraphicsLayer valvesGraphicsLayer;
        GraphicsLayer mainsGraphicsLayer;

        public MainWindow()
        {
            // License setting and ArcGIS Runtime initialization is done in Application.xaml.cs.

            InitializeComponent();

            /*
             * Asynchronously start all the services...
             */

            LocalGeoprocessingService.GetServiceAsync(@"..\Maps-and-Data\TraceGeometricNetwork.gpk", 
                GPServiceType.SubmitJob,
                localServiceCallback =>
            {
                if (localServiceCallback.Error != null)
                { MessageBox.Show(localServiceCallback.Error.Message); return; }

                traceNetworkLocalGpService = localServiceCallback;

                geoprocessorTask = new Geoprocessor(traceNetworkLocalGpService.Tasks[0].Url);
                
                // Enable the UI when the Trace Geometric Network service is available
                TraceUpstreamButton.IsEnabled = true;
            });

            LocalGeometryService.GetServiceAsync(callback =>
            {
                localGeometryService = callback;
                geometryTask = new GeometryService();
                geometryTask.Url = localGeometryService.UrlGeometryService;
            });

            LocalMapService.GetServiceAsync(@"..\Maps-and-Data\WaterDistributionNetwork.mpk", callback =>
            {
                if (callback.Error != null)
                { MessageBox.Show("WaterDistributionNetwork map failed to load"); return; }
                waterNetworkLocalMapService = callback;
                ArcGISLocalDynamicMapServiceLayer WaterDistributionNetworkLayer =
                    new ArcGISLocalDynamicMapServiceLayer(waterNetworkLocalMapService);
                MyMap.Layers.Insert(1, WaterDistributionNetworkLayer);
            });

            /*
             * Create the Results GraphicsLayers and associated Renderers
             */
            valvesGraphicsLayer = new GraphicsLayer()
            {
                Renderer = new SimpleRenderer()
                {
                    Symbol = MainGrid.Resources["ValveMarkerSymbol"] as Symbol
                }
            };
            mainsGraphicsLayer = new GraphicsLayer()
            {
                Renderer = new SimpleRenderer()
                {
                    Symbol = new SimpleLineSymbol()
                    {
                        Color = new SolidColorBrush(Colors.Red),
                        Width = 8,
                        Style = SimpleLineSymbol.LineStyle.Dash
                    }
                }
            };

            // Add the GraphicsLayers to the Map
            MyMap.Layers.Add(mainsGraphicsLayer);
            MyMap.Layers.Add(valvesGraphicsLayer);
        }


        void MyMap_MouseClick(object sender, Map.MouseEventArgs e)
        {
            /*
             * Add a graphic at the user input location
             */
            MyMap.MouseClick -= MyMap_MouseClick;

            if (inputGraphicsLayer == null)
            {
                inputGraphicsLayer = new GraphicsLayer();
                MyMap.Layers.Add(inputGraphicsLayer);
            }

            // Add a Graphic at the click point
            Graphic graphic = new Graphic()
            {
                Symbol = MainGrid.Resources["IncidentMarkerSymbol"] as Symbol,
                Geometry = e.MapPoint
            };
            inputGraphicsLayer.Graphics.Add(graphic);

            /*
             * Reproject the mouseclick into the GP coordinate system
             */
            // Declare the ProjectCompleted Handler
            EventHandler<GraphicsEventArgs> ProjectCompletedHandler = null;

            // Handle the event
            ProjectCompletedHandler = (sender2, graphicsEventArgs) =>
            {
                // Unregister the handler
                geometryTask.ProjectCompleted -= ProjectCompletedHandler;

                // Cancel any existing Jobs
                geoprocessorTask.CancelAsync();

                // Handle the JobCompleted
                geoprocessorTask.JobCompleted += (sender3, jobInfoEventArgs) =>
                {
                    // Check whether it succeeded
                    if (jobInfoEventArgs.JobInfo.JobStatus != esriJobStatus.esriJobSucceeded)
                    {
                        //Do Something 
                    };

                    /*
                     * Create two new Geoprocessor Tasks to fetch the result data (thereby allowing them to run concurrently)
                     * Each will use the same event handler and we'll choose based on the Parameter Name. 
                     * Alternatively could use the overload which takes an object (userToken).
                     */
                    Geoprocessor gpTaskSysvalves_Layer =
                        new Geoprocessor(traceNetworkLocalGpService.Tasks[0].Url);

                    gpTaskSysvalves_Layer.GetResultDataCompleted
                        += geoprocessorTask_GetResultDataCompleted;

                    gpTaskSysvalves_Layer.GetResultDataAsync(
                        jobInfoEventArgs.JobInfo.JobId, "Sysvalves_Layer");

                    Geoprocessor gpTaskDistribMains_Layer =
                        new Geoprocessor(traceNetworkLocalGpService.Tasks[0].Url);

                    gpTaskDistribMains_Layer.GetResultDataCompleted
                        += geoprocessorTask_GetResultDataCompleted;

                    gpTaskDistribMains_Layer.GetResultDataAsync(
                        jobInfoEventArgs.JobInfo.JobId, "DistribMains_Layer");
                };

                // Create the GP Parameter List
                List<GPParameter> parameters = new List<GPParameter>();
                parameters.Add(new GPFeatureRecordSetLayer("Flags", graphicsEventArgs.Results[0].Geometry));
                geoprocessorTask.SubmitJobAsync(parameters);
            };
            // Register the handler for the ProjectCompleted event.
            geometryTask.ProjectCompleted += ProjectCompletedHandler;

            // Project the input point into the coordinate system of the data
            geometryTask.ProjectAsync(new List<Graphic>() 
            { 
                new Graphic() 
                { 
                    Geometry = e.MapPoint 
                } 
            },
            waterNetworkLocalMapService.SpatialReference);
        }

        void geoprocessorTask_GetResultDataCompleted(object sender, GPParameterEventArgs gpParameterEventArgs)
        {
            // Get the result features from the parameter
            GPFeatureRecordSetLayer gpLayer;
            gpLayer = gpParameterEventArgs.Parameter as GPFeatureRecordSetLayer;

            // Check the parameter name, reproject and add to the appropriate graphics layer.
            // again - two separate tasks are created allowing them to run conncurrently.
            switch (gpParameterEventArgs.Parameter.Name)
            {
                case "Sysvalves_Layer":
                    GeometryService valvesProjectTask =
                        new GeometryService(localGeometryService.UrlGeometryService);

                    valvesProjectTask.ProjectCompleted += (sender2, graphicsEventArgs) =>
                    {
                        valvesGraphicsLayer.Graphics.AddRange(graphicsEventArgs.Results);
                    };
                    valvesProjectTask.ProjectAsync(gpLayer.FeatureSet.Features,
                        new SpatialReference(3857));
                    break;
                case "DistribMains_Layer":
                    GeometryService mainsProjectTask =
                        new GeometryService(localGeometryService.UrlGeometryService);

                    mainsProjectTask.ProjectCompleted += (sender2, graphicsEventArgs) =>
                    {
                        mainsGraphicsLayer.Graphics.AddRange(graphicsEventArgs.Results);
                    };
                    mainsProjectTask.ProjectAsync(gpLayer.FeatureSet.Features,
                        new SpatialReference(3857));
                    break;
            }
        }

        // Clear any existing graphics and register the mouse click handler.
        private void TraceUpstreamButton_Click(object sender, RoutedEventArgs e)
        {
            ClearMap();
            MyMap.MouseClick += MyMap_MouseClick;
        }

        // Clear any existing graphics
        private void ClearMap()
        {
            if (inputGraphicsLayer != null)
                inputGraphicsLayer.Graphics.Clear();
            mainsGraphicsLayer.Graphics.Clear();
            valvesGraphicsLayer.Graphics.Clear();
        }
    }
}
