using System.Windows;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Collections.Generic;

namespace GeocodingAndRouting
{
    public partial class MainWindow : Window
    {
        // License setting and ArcGIS Runtime initialization is done in Application.xaml.cs.

        public MainWindow()
        {
            InitializeComponent();
        }

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SearchTextBox.Text))
                return;

            // Clear existing graphics for a new search
            FindResultLocationsGraphicsLayer.Graphics.Clear();
            MyLocationGraphicsLayer.Graphics.Clear();
            MyRoutesGraphicsLayer.Graphics.Clear();

            // Initialize the LocatorTask with the Esri World Geocoding Service
            var locatorTask = new Locator("http://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer");

            // In this sample, the center of the map is used as the location from which results will be ranked and distance calculated. 
            // The distance from the location is optional.  Specifies the radius of an area around a point location which is used to boost
            // the rank of geocoding candidates so that candidates closest to the location are returned first. The distance value is in meters. 
            LocatorFindParameters locatorFindParams = new LocatorFindParameters()
            {
                Text = SearchTextBox.Text,
                Location = MyMap.Extent.GetCenter(),
                Distance = MyMap.Extent.Width / 2,
                MaxLocations = 5,
                OutSpatialReference = MyMap.SpatialReference,
                SearchExtent = MyMap.Extent
            };
            locatorFindParams.OutFields.AddRange(new string[] { "PlaceName", "City", "Region", "Country", "Score", "Distance", "Type" });

            locatorTask.FindCompleted += (s, ee) =>
            {
                // When a Find operation is initiated, get the find results and add to a graphics layer for display in the map
                LocatorFindResult locatorFindResult = ee.Result;
                foreach (Location location in locatorFindResult.Locations)
                {
                    // Make sure results have the right spatial reference
                    location.Graphic.Geometry.SpatialReference = MyMap.SpatialReference;

                    FindResultLocationsGraphicsLayer.Graphics.Add(location.Graphic);
                }
            };

            locatorTask.Failed += (s, ee) =>
            {
                MessageBox.Show("Locator service failed: " + ee.Error);
            };

            locatorTask.FindAsync(locatorFindParams);
        }

        private void MyMap_MouseClick(object sender, Map.MouseEventArgs e)
        {
            if (FindResultLocationsGraphicsLayer.Graphics.Count == 0)
                return; 
            
            // This will create new routes to the search results
            MyLocationGraphicsLayer.Graphics.Clear();
            MyRoutesGraphicsLayer.Graphics.Clear();

            var myLocation = new Graphic()
            {
                Geometry = e.MapPoint,
                Symbol = new PictureMarkerSymbol()
                {
                    Source = new BitmapImage(new System.Uri("http://static.arcgis.com/images/Symbols/Basic/CrossHair.png")),
                    Width = 48,
                    Height = 48,
                    OffsetX = 24,
                    OffsetY = 24
                }
            };
            MyLocationGraphicsLayer.Graphics.Add(myLocation);

            // For each of the search results - find a route to them
            foreach (var item in FindResultLocationsGraphicsLayer.Graphics)
            {
                SolveRoute(e.MapPoint, item.Geometry as MapPoint);
            }
        }

        void SolveRoute(MapPoint start, MapPoint end)
        {
            // The Esri World Network Analysis Services requires an ArcGIS Online Subscription.
            // In this sample use the North America only service on tasks.arcgisonline.com.
            var routeTask = new RouteTask("http://tasks.arcgisonline.com/ArcGIS/rest/services/NetworkAnalysis/ESRI_Route_NA/NAServer/Route");

            // Create a new list of graphics to send to the route task. Note don't use the graphcis returned from find
            // as the attributes cause issues with the route finding.
            var stops = new List<Graphic>()
            { 
                new Graphic() { Geometry = start}, 
                new Graphic() { Geometry = end}
            };

            var routeParams = new RouteParameters()
            {
                Stops = stops,
                UseTimeWindows = false,
                OutSpatialReference = MyMap.SpatialReference,
                ReturnDirections = false,
                IgnoreInvalidLocations = true
            };

            routeTask.Failed += (s, e) =>
            {
                MessageBox.Show("Route Task failed:" + e.Error);
            };

            // Register an inline handler for the SolveCompleted event.
            routeTask.SolveCompleted += (s, e) =>
            {
                // Add the returned route to the Map
                var myRoute = new Graphic()
                {
                    Geometry = e.RouteResults[0].Route.Geometry,
                    Symbol = new SimpleLineSymbol()
                    {
                        Width = 10,
                        Color = new SolidColorBrush(Color.FromArgb(127, 0, 0, 255))
                    }
                };

                // Add a MapTip
                decimal totalTime = (decimal)e.RouteResults[0].Route.Attributes["Total_Time"];
                string tip = string.Format("{0} minutes", totalTime.ToString("#0.0"));
                myRoute.Attributes.Add("TIP", tip);

                MyRoutesGraphicsLayer.Graphics.Add(myRoute);
            };

            // Call the SolveAsync method
            routeTask.SolveAsync(routeParams);
        }
    }
}

