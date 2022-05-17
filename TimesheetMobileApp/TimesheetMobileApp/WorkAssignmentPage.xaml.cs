using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TimesheetBackend.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using TimesheetMobileApp.Models; // tää ei ollu videolla mut oli pakko lisätä?

namespace TimesheetMobileApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WorkAssignmentPage : ContentPage
    {
        int empoyeeId;
        string lat;
        string lon;

        HttpClientHandler GetInsecureHandler()
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                if (cert.Issuer.Equals("CN=localhost"))
                    return true;
                return errors == System.Net.Security.SslPolicyErrors.None;
            };
            return handler;
        }

        ObservableCollection<WorkAssignment> workdataa = new ObservableCollection<WorkAssignment>();
        public WorkAssignmentPage(int id)
        {
            InitializeComponent();

            empoyeeId = id;
            lon_label.Text = "Sijaintia haetaan";
            work_lataus.Text = "Ladataan työtehtäviä...";

            GetCurrentLocation();

            //------sijainnin haku ja näyttäminen-----------------------

            async void GetCurrentLocation()
            {
                try
                {
                    var request = new GeolocationRequest(GeolocationAccuracy.High , TimeSpan.FromSeconds(10));  //videolla medium

                    var location = await Geolocation.GetLocationAsync(request);

                    if (location != null)
                    {
                        lon_label.Text = "Longitude: " + location.Longitude;
                        lat_label.Text = $"Latitude: {location.Latitude}"; //template-string, ei pakko tehdä näin

                        lat = location.Latitude.ToString();
                        lon = location.Longitude.ToString();
                    }
                }
                catch (FeatureNotSupportedException fnsEx)
                {
                    await DisplayAlert("Virhe", fnsEx.ToString(), "ok");
                }
                catch (FeatureNotEnabledException fneEx)
                {
                    await DisplayAlert("Virhe", fneEx.ToString(), "ok");
                }
                catch (PermissionException pEx)
                {
                    await DisplayAlert("Virhe", pEx.ToString(), "ok");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Virhe", ex.ToString(), "ok");
                }
            }

            // -------------------------------------------------------

            LoadDataFromRestAPI();

            async void LoadDataFromRestAPI()
            { 
                try
                {

#if DEBUG
                    HttpClientHandler insecureHandler = GetInsecureHandler();
                    HttpClient client = new HttpClient(insecureHandler);
#else
                    HttpClient client = new HttpClient();
#endif
                    client.BaseAddress = new Uri("https://10.0.2.2:7086/");
                    string json = await client.GetStringAsync("api/workassignments");

                    IEnumerable<WorkAssignment> assignments = JsonConvert.DeserializeObject<WorkAssignment[]>(json);
                    // dataa -niminen observableCollection on alustettukin jo ylhäällä päätasolla että hakutoiminto,
                    // pääsee siihen käsiksi.
                    // asetetaan sen sisältö ensi kerran tässä pienellä kepulikonstilla:

                    ObservableCollection<WorkAssignment> dataa3 = new ObservableCollection<WorkAssignment>(assignments);
                    workdataa = dataa3;
                    //dataa = new ObservableCollection<Employee>(employees); //tää vois olla vaihtoehtoinen, jäi auki vielä

                    // Asetetaan datat näkyviin xaml tiedostossa olevalle listalle
                    assignmentList.ItemsSource = workdataa;

                    // Tyhjennetään latausilmoitus label
                    work_lataus.Text = "";
                }

                catch (Exception e)
                {
                    await DisplayAlert("Virhe", e.Message.ToString(), "SELVÄ!");
                }
            }
        }

        async void Start_Clicked(object sender, EventArgs e)
        {
            WorkAssignment assignments = (WorkAssignment)assignmentList.SelectedItem;

            if (assignments == null)
            {
                await DisplayAlert("Valinta puuttuu", "Valitse työtehtävä.", "OK");
                return;
            }

            try
            {
                Operation op = new Operation
                {
                    EmployeeID = empoyeeId,
                    WorkAssignmentID = assignments.IdWorkAssignment,
                    CustomerID = assignments.IdCustomer,
                    OperationType = "start",
                    Comment = "Aloitettu",
                    Latitude = lat,
                    Longitude = lon
                };

#if DEBUG
                HttpClientHandler insecureHandler = GetInsecureHandler();
                HttpClient client = new HttpClient(insecureHandler);
#else
                HttpClient client = new HttpClient();
#endif
                client.BaseAddress = new Uri("https://10.0.2.2:7086/");

                // Muutetaan em. data objekti Jsoniksi
                string input = JsonConvert.SerializeObject(op);
                StringContent content = new StringContent(input, Encoding.UTF8, "application/json");

                // Lähetetään serialisoitu objekti back-endiin Post pyyntönä
                HttpResponseMessage message = await client.PostAsync("/api/workassignments", content);

                // Otetaan vastaan palvelimen vastaus
                string reply = await message.Content.ReadAsStringAsync();

                //Asetetaan vastaus serialisoituna success muuttujaan
                bool success = JsonConvert.DeserializeObject<bool>(reply);

                if (success == false)
                {
                    await DisplayAlert("Ei voida aloittaa", "Työ on jo käynnissä", "OK");
                }
                else if (success == true)
                {
                    await DisplayAlert("Työ aloitettu", "Työ on aloitettu", "OK");
                }
            }

            catch (Exception ex)
            {
                await DisplayAlert(ex.GetType().Name, ex.Message, "OK");
            }
        }

        async void Stop_Clicked(object sender, EventArgs e)
        {
            await DisplayAlert("Stop", empoyeeId.ToString(), "ok");
        }
    }
}