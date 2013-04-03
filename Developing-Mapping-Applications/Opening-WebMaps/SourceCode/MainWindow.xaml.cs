using System.Windows;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.WebMap;

namespace OpeningWebMaps
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            // License setting and ArcGIS Runtime initialization is done in Application.xaml.cs.

            InitializeComponent();

            var webMap = new ESRI.ArcGIS.Client.WebMap.Document();
            webMap.GetMapCompleted += webMap_GetMapCompleted;

            // Get webmap from ArcGIS Online, this item is at this address, note the item Id in the Url:
            // http://www.arcgis.com/home/item.html?id=88b187a860934d8491bdff591d0b1e1a

            webMap.GetMapAsync("88b187a860934d8491bdff591d0b1e1a");
        }

        void webMap_GetMapCompleted(object sender, GetMapCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                // Enable accelerated display
                e.Map.UseAcceleratedDisplay = true;   

                // Add Map to the UI
                MainGrid.Children.Add(e.Map);

                // Add bookmarks form Webmap to the UI too.
                if (e.WebMap.Bookmarks.Count > 0)
                {
                    // Add bookmarks
                    var bookmarkControl = new ESRI.ArcGIS.Client.Toolkit.Bookmark();
                    foreach (var item in e.WebMap.Bookmarks)
                    {
                        bookmarkControl.AddBookmark(item.Name, item.Extent);
                    }
                    // Note this could equally be pre defined in Xaml too.
                    bookmarkControl.Map = e.Map;
                    bookmarkControl.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                    bookmarkControl.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                    // Add to the UI
                    MainGrid.Children.Add(bookmarkControl);
                }
            }
            else
            {
                MessageBox.Show("Failed to load web map!");
            }
        }
    }
}
