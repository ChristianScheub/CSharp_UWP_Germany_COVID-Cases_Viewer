using Deutschland_Fallzahlen.BackgroundTasks;
using Deutschland_Fallzahlen.Services;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace Deutschland_Fallzahlen.Views
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        Windows.Storage.StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
        CSV_Parser parser;
        ListView Daten;
        public MainPage()
        {
            InitializeComponent();

            ladeDaten();
            //  setHintergrund();
            starteBackground();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));



        public void ladeGrid()
        {
            Daten = new ListView();

            if (parser == null)
            {
                parser = new CSV_Parser();
            }
            for (int i = 0; i <= 410; i++)
            {

                Daten.Items.Add(parser.getDaten(i));
            }

            alleDatenPanel.Children.Add(Daten);
        }


        public void ladeCustomGrid(String[] neueDaten)
        {
            alleDatenPanel.Children.Clear();
            ListView DatenNew = new ListView();

            if (parser == null)
            {
                parser = new CSV_Parser();
            }
            for (int i = 0; i <= 410; i++)
            {
                if (neueDaten[i] != null)
                {
                    DatenNew.Items.Add(neueDaten[i]);
                }
            }

            alleDatenPanel.Children.Add(DatenNew);
        }

        private async void ladeDaten()
        {
            try
            {
                Uri source = new Uri("https://opendata.arcgis.com/datasets/917fc37a709542548cc3be077a786c17_0.csv", UriKind.Absolute);
                BackgroundDownloader downloader = new BackgroundDownloader();

                Debug.WriteLine(Windows.Storage.ApplicationData.Current.LocalFolder);
                StorageFile testfile = await folder.CreateFileAsync("data.csv", CreationCollisionOption.ReplaceExisting);
                DownloadOperation download = downloader.CreateDownload(source, testfile);
                await download.StartAsync();
                await Task.Delay(TimeSpan.FromSeconds(5));
                if (parser == null)
                {
                    parser = new CSV_Parser();
                }
                parser.getDaten();
                await Task.Delay(TimeSpan.FromSeconds(5));
                int lol = parser.getDatenLange();
                Debug.WriteLine(lol);
                LiveKachelCreator library = new LiveKachelCreator();
                library.sendMessage(OldDataService.getOldLandkreis(), parser.getIndex(OldDataService.getOldLandkreis()));
                try
                {
                    ladeGrid();
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Download Error", e);
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Download Error", ex);
            }
        }

        private void button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            String[] neueDaten = new string[413];
            String pruefen;
            for (int i = 1; i <= 409; i++)
            {
                pruefen = parser.getLandkreis(i);
                if (pruefen.Contains(textBox_suchen.Text))
                {
                    neueDaten[i] = parser.getDaten(i);
                }
            }
            ladeCustomGrid(neueDaten);

        }

        private void textBox_suchen_DragEnter(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                String[] neueDaten = new string[413];
                String pruefen;
                for (int i = 1; i <= 409; i++)
                {
                    pruefen = parser.getLandkreis(i);
                    if (pruefen.Contains(textBox_suchen.Text))
                    {
                        neueDaten[i] = parser.getDaten(i);
                    }
                }
                ladeCustomGrid(neueDaten);
            }
        }
        private async void starteBackground() { 

           // BackgroundTaskService hintergrudnService = new BackgroundTaskService();
            BackgroundTask1 hintergrundTask = new BackgroundTask1();
          //  await hintergrudnService.RegisterBackgroundTasksAsync();
            //hintergrudnService.Start();
            hintergrundTask.Register();

        }


     /*   private void setHintergrund()
        {
            var taskRegistered = false;
            var exampleTaskName = "sendeNachricht";

            foreach (var task1 in BackgroundTaskRegistration.AllTasks)
            {
                if (task1.Value.Name == exampleTaskName)
                {
                    taskRegistered = true;
                    break;
                }
            }
            if (!taskRegistered)
            {

                var builder = new BackgroundTaskBuilder();

                builder.Name = exampleTaskName;
                builder.TaskEntryPoint = "FallzahlenDE.backgroundTask";
                builder.SetTrigger(new SystemTrigger(SystemTriggerType.TimeZoneChange, false));
                BackgroundTaskRegistration task = builder.Register();
            }

        }*/
    }
}
