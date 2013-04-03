using System;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using System.Windows;
using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Xml.XPath;

using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Local;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Toolkit;

namespace DynamicLayers
{

public partial class MainWindow : Window
{
    // Get the path of the "empty" MPK from the application folder
    string _emptyMpkPath = @"..\Maps-and-Data\EmptyMPK_WGS84.mpk";

    public MainWindow()
    {
        // License setting and ArcGIS Runtime initialization is done in Application.xaml.cs.
        InitializeComponent();
    }

    /// <summary>
    /// Handles the Click event of the AddShapefileButton control.
    /// </summary>
    private void AddShapefileButton_Click(object sender, RoutedEventArgs e)
    {
        // Setup the OpenFiledialog.
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "Shapefiles (*.shp)|*.shp";
        openFileDialog.RestoreDirectory = true;
        openFileDialog.Multiselect = true;

        if (openFileDialog.ShowDialog() == true)
        {
            try
            {
                List<string> fileNames = new List<string>();
                foreach (var item in openFileDialog.SafeFileNames)
                {
                    fileNames.Add(Path.GetFileNameWithoutExtension(item));
                }
                // Call the add dataset method with workspace type, parent directory path, file names (without extensions) and delegate.
                AddFileDatasetToDynamicMapServiceLayer(WorkspaceFactoryType.Shapefile,
                    Path.GetDirectoryName(openFileDialog.FileName),
                    fileNames,
                    arcGisLocalDynamicMapServiceLayer =>
                    {
                        // Add the dynamic map service layer to the map.                           
                        MyMap.Layers.Add(arcGisLocalDynamicMapServiceLayer);
                    });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
            }
        }
    }

    /// <summary>
    /// Handles the Click event of the AddRasterButton control.
    /// </summary>
    private void AddRasterButton_Click(object sender, RoutedEventArgs e)
    {
        // Setup the OpenFiledialog.
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "Image files (*.bmp,*.png,*.sid,*.tif)|*.bmp;*.png;*.sid;*.tif;";
        openFileDialog.RestoreDirectory = true;
        openFileDialog.Multiselect = true;
        if (openFileDialog.ShowDialog() == true)
        {
            try
            {
                // Call the add dataset method with workspace type, parent directory path, actual file names and delegate.
                AddFileDatasetToDynamicMapServiceLayer(WorkspaceFactoryType.Raster,
                    Path.GetDirectoryName(openFileDialog.FileName),
                    new List<string>(openFileDialog.SafeFileNames),
                    arcGisLocalDynamicMapServiceLayer =>
                    {
                        // Add the dynamic map service layer to the map.
                        MyMap.Layers.Add(arcGisLocalDynamicMapServiceLayer);
                    });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
            }
        }
    }

    /// <summary>
    /// Adds a file dataset (Shapefile or raster) to a new local dynamic map service layer.
    /// </summary>
    /// <param name="workspaceType">The workspace type (FileGDB, Raster, SDE, Shapefile) <see cref="http://resources.arcgis.com/en/help/runtime-wpf/apiref/index.html?ESRI.ArcGIS.Client.Local~ESRI.ArcGIS.Client.Local.WorkspaceFactoryType.html"/>.</param>
    /// <param name="directoryPath">A <see cref="System.String"/> representing the directory path.</param>
    /// <param name="fileNames">A <see cref="System.Collections.Generic.List{System.String}"/> representing the name of the file (for raster datasets this must include the extension).</param>
    /// <param name="callback">The Action delegate to call back on once the dynamic layer work is complete.</param>                
    public void AddFileDatasetToDynamicMapServiceLayer(WorkspaceFactoryType workspaceType, string directoryPath, List<string> fileNames, Action<ArcGISLocalDynamicMapServiceLayer> callback)
    {
        try
        {
            // Generate a unique workspace ID (any unique string).
            string uniqueId = Guid.NewGuid().ToString();

            // Create a new WorkspaceInfo object with a unique ID.
            WorkspaceInfo workspaceInfo = new WorkspaceInfo(uniqueId, workspaceType, "DATABASE=" + directoryPath);

            // Create a new LocalMapService instance.
            LocalMapService localMapService = new LocalMapService
            {
                // Set the path property.
                Path = _emptyMpkPath,

                // Enable the dynamic layers capability.
                EnableDynamicLayers = true
            };

            // Register the workspace to be used with this service.
            localMapService.DynamicWorkspaces.Add(workspaceInfo);

            // Asynchronously start the local map service.
            localMapService.StartAsync(x =>
            {
                // Create the local dynamic map service layer.
                ArcGISLocalDynamicMapServiceLayer arcGisLocalDynamicMapServiceLayer = null;

                // Create a new ArcGISLocalDynamicMapServiceLayer passing in the newly started local service.
                arcGisLocalDynamicMapServiceLayer = new ArcGISLocalDynamicMapServiceLayer(localMapService)
                {
                    // Assign the filename as the map layer ID.
                    ID = "Workspace: " + (new DirectoryInfo(directoryPath)).Name,

                    // Enable the dynamic layers capability.
                    EnableDynamicLayers = true,
                };

                // Handle the layer initialized event inline to perform the layer dynamic layer management.
                arcGisLocalDynamicMapServiceLayer.Initialized += (s, e) =>
                {
                    // Create a DynamicLayerInfoCollection to hold the new datasets as "dynamic layers".
                    DynamicLayerInfoCollection dynamicLayerInfoCollection = new DynamicLayerInfoCollection();

                    // Create a LayerDrawingOptionsCollection to specify the symbology for each layer.
                    LayerDrawingOptionsCollection layerDrawingOptionsCollection = new LayerDrawingOptionsCollection();

                    // Iterate over each of the selected files in the workspace.
                    int counter = 0;
                    foreach (string fileName in fileNames)
                    {
                        // Create a new DynamicLayerInfo (to make changes to existing map service layers use the CreateDynamicLayerInfosFromLayerInfos() method.
                        DynamicLayerInfo dynamicLayerInfo = new DynamicLayerInfo
                        {
                            // Assign a layer ID.
                            ID = counter,

                            // Specify a friendly name.
                            Name = "Dataset: " + fileName
                        };

                        // Create a DataSource object to represent the physical datasource implementation (table or raster) which will become the DataSource 
                        // property of a new LayerDataSource in the map service. Other supported datasource types are JoinDataSource and QueryDataSource.
                        DataSource dataSource = null;

                        // If the workspace type is Raster create a new RasterDataSource.
                        if (workspaceInfo.FactoryType == WorkspaceFactoryType.Raster)
                        {
                            // Create a new RasterDataSource object
                            dataSource = new RasterDataSource
                            {
                                // Match the DataSourceName to the physical filename on disk (including extension).
                                DataSourceName = fileName,

                                // Provide the WorkspaceID (the unique workspace identifier created earlier). A LocalMapService may have multiple dynamic workspaces.
                                WorkspaceID = workspaceInfo.Id
                            };
                        }
                        else
                        {
                            // Else if the workspace is not Raster create a new TableDataSource
                            dataSource = new TableDataSource
                            {
                                // Match the DataSourceName to the physical filename on disk (excluding extension).
                                DataSourceName = fileName,

                                // Provide the WorkspaceID (the unique workspace identifier created earlier). A LocalMapService may have multiple dynamic workspaces.
                                WorkspaceID = workspaceInfo.Id
                            };

                            /* 
                                * Apply a renderer for vector layers.
                                * Note: It is always necessary to provide a renderer when the layer being added (represented by a DynamicLayerInfo) is part of a new 
                                * DynamicLayerInfoCollection as opposed to using the CreateDynamicLayerInfosFromLayerInfos() method which creates a DynamicLayerInfoCollection 
                                * containing the existing layers in the map service.
                            */

                            // Create a new LayerDrawingOptions object to hold the renderer information.
                            var layerDrawOpt = new LayerDrawingOptions()
                            {
                                // Match up the LayerID to the ID of the layer within the service.
                                LayerID = counter,
                            };

                            // We need to determine the geometry type of the new feature class.  
                            // To do this, we will submit a request to the ..\MapServer\dynamicLayer?.. endpoint which will return the service level metadata,
                            // allowing us to identify the geometry type and create an appropriate renderer. 
                            // Note:- This is a workaround until the next release where GetAllDetails will honor the DynamicLayerInfoCollection.

                            // Create a new WebClient instance to make the request and download the response.
                            WebClient webClient = new WebClient();

                            // Register an asynchronous handler in which to create the renderers and apply the to the dynamic map service layer.
                            webClient.DownloadDataCompleted += (client, downloadDataEventArgs) =>
                            {

                                // Read the JSON response as XML
                                XmlReader reader = System.Runtime.Serialization.Json.JsonReaderWriterFactory.CreateJsonReader(downloadDataEventArgs.Result, new XmlDictionaryReaderQuotas());

                                // Get the root XML element
                                XElement root = XElement.Load(reader);

                                // Query for the "geometryType" element
                                XElement geometryType = root.XPathSelectElement("//geometryType");

                                // Create the render based on the geometry type
                                switch (geometryType.Value)
                                {
                                    case "esriGeometryPoint":
                                        layerDrawOpt.Renderer = new SimpleRenderer() { Symbol = new SimpleMarkerSymbol() { Color = new SolidColorBrush(GetRandomColor()), Size = 8 } };
                                        break;
                                    case "esriGeometryPolyline":
                                        layerDrawOpt.Renderer = new SimpleRenderer() { Symbol = new SimpleLineSymbol() { Color = new SolidColorBrush(GetRandomColor()) } };
                                        break;
                                    case "esriGeometryPolygon":
                                        layerDrawOpt.Renderer = new SimpleRenderer() { Symbol = new SimpleFillSymbol() { Fill = new SolidColorBrush(GetRandomColor()), BorderBrush = new SolidColorBrush(GetRandomColor()) } };
                                        break;
                                }

                                // Set the LayerDrawingOptions property on the local dynamic map service layer (the LayerID property ties this to the DynamicLayerInfo object).
                                layerDrawingOptionsCollection.Add(layerDrawOpt);

                                // Update the layer drawing options property on the dynamic map service layer.
                                arcGisLocalDynamicMapServiceLayer.LayerDrawingOptions = layerDrawingOptionsCollection;

                                // Need to refresh the layer after the renderer(s) have been applied.
                                arcGisLocalDynamicMapServiceLayer.Refresh();

                            };

                            // Make the request for the service metadata
                            // e.g. http://127.0.0.1:<PORT>/arcgis/rest/services/<MPK_NAME>/MapServer/dynamicLayer?layer={"id":0,"source":{"type":"dataLayer","dataSource":{"type":"table","workspaceId":"MyWorkspace","dataSourceName":"MyFeatureClassName"}}}
                            webClient.DownloadDataAsync(new Uri(arcGisLocalDynamicMapServiceLayer.Url
                                + "/dynamicLayer?layer={'id':" + counter.ToString() + ","
                                + "'source':{'type':'dataLayer','dataSource':{"
                                + "'type':'table',"
                                + "'workspaceId':'" + workspaceInfo.Id + "',"
                                + "'dataSourceName':'" + fileName + "'"
                                + "}}}"));
                        }

                        // Set the Source property of the DynamicLayerInfo object.
                        dynamicLayerInfo.Source = new LayerDataSource { DataSource = dataSource };

                        // Add the new DynamicLayerInfo object to the collection.
                        dynamicLayerInfoCollection.Add(dynamicLayerInfo);

                        // Increment the counter which is being used to assign Layer IDs.
                        counter++;
                    }

                    // Update the DynamicLayerInfos property on the dynamic map service layer.
                    arcGisLocalDynamicMapServiceLayer.DynamicLayerInfos = dynamicLayerInfoCollection;

                    // Call the Action delegate.
                    callback(arcGisLocalDynamicMapServiceLayer);
                };

                // Call the Initialize method on the layer to initialize the layer properties.
                arcGisLocalDynamicMapServiceLayer.Initialize();
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Utility function: Generate a random System.Windows.Media.Color
    Random _random = new Random();
    private Color GetRandomColor()
    {
        var colorBytes = new byte[3];
        _random.NextBytes(colorBytes);
        Color randomColor = Color.FromRgb(colorBytes[0], colorBytes[1], colorBytes[2]);
        return randomColor;
    }

    private void Legend_Refreshed(object sender, Legend.RefreshedEventArgs e)
    {
        // Clear the sub items from the basemap layer.
        if (e.LayerItem.Layer == _worldTopographicBasemap)
            e.LayerItem.LayerItems.Clear();
    }

}
}
