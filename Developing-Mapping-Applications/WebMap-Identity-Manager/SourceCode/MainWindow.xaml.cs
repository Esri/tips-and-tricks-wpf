using System.Windows;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.WebMap;
using ESRI.ArcGIS.Client.Toolkit;
using System;

namespace WebMapIdentityManager
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            // License setting and ArcGIS Runtime initialization is done in Application.xaml.cs.

            InitializeComponent();

            // Display the SignIn dialog:
            IdentityManager.Current.ChallengeMethod = SignInDialog.DoSignIn;
            
            // Alternatively handle obtaining the user credentials another way.
            //IdentityManager.Current.ChallengeMethod = GetCredentials; 
        }

        private void LoadWebMapButton_Click(object sender, RoutedEventArgs e)
        {
            Document webMap = new Document();
            webMap.GetMapCompleted += webMap_GetMapCompleted;
            webMap.GetMapAsync(WebMapTextBox.Text);
        }

        void webMap_GetMapCompleted(object sender, GetMapCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                e.Map.UseAcceleratedDisplay = true;   // Enable accelerated display
                MainGrid.Children.Add(e.Map); // Add map to UI
            }
            else
            {
                MessageBox.Show("Failed to load web map!");
            }
        }

        // Alternative login approach (used by commented out IdentityManager ChallengeMethod registration in the constructor)
        private void GetCredentials(string url, System.Action<IdentityManager.Credential, Exception> callback, IdentityManager.GenerateTokenOptions options)
        {
            // TODO get user / password from a UI displayed to user or from a secure location
            string user = "TODO";
            string password = "TODO"; 

            // When challenged for a token generate a set of credentials based on the specified username and password.
            IdentityManager.Current.GenerateCredentialAsync(
                url,
                user,
                password,
                (credential, ex) =>
                {
                    // Raise the Action to return to the method that triggered the challenge (GetMapAsync).
                    callback(credential, ex);
                }, options);
        }
    }
}
