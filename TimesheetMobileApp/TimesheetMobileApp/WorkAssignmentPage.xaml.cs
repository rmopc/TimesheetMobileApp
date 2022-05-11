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

namespace TimesheetMobileApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WorkAssignmentPage : ContentPage
    {
        int empoyeeId;

        ObservableCollection<WorkAssignment> workdataa = new ObservableCollection<WorkAssignment>();
        public WorkAssignmentPage(int id)
        {
            InitializeComponent();

            empoyeeId = id;

            LoadDataFromRestAPI();

            async void LoadDataFromRestAPI()
            {

                work_lataus.Text = "Ladataan työtehtäviä...";

                try
                {
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
            await DisplayAlert("Start", empoyeeId.ToString(), "ok");
        }

        async void Stop_Clicked(object sender, EventArgs e)
        {
            await DisplayAlert("Stop", empoyeeId.ToString(), "ok");
        }

    }
}