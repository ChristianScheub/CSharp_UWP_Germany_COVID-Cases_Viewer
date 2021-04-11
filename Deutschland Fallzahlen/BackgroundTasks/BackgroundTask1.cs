using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.System.Threading;
using Windows.UI.Notifications;

using Windows.ApplicationModel.Background;
using Windows.System.Threading;

namespace Deutschland_Fallzahlen.BackgroundTasks
{
    public sealed class BackgroundTask1 : BackgroundTask
    {
        Windows.Storage.StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
        private Windows.Storage.StorageFile file;
        public static String[,] daten;
        public static string Message { get; set; }

        private volatile bool _cancelRequested = false;
        private IBackgroundTaskInstance _taskInstance;
        private BackgroundTaskDeferral _deferral;

        public override void Register()
        {
            var taskName = GetType().Name;
            var taskRegistration = BackgroundTaskRegistration.AllTasks.FirstOrDefault(t => t.Value.Name == taskName).Value;

            if (taskRegistration == null)
            {
                var builder = new BackgroundTaskBuilder()
                {
                    Name = taskName
                };

                // TODO WTS: Define the trigger for your background task and set any (optional) conditions
                // More details at https://docs.microsoft.com/windows/uwp/launch-resume/create-and-register-an-inproc-background-task
                builder.SetTrigger(new TimeTrigger(15, false));

                builder.Register();
            }
        }

        public override Task RunAsyncInternal(IBackgroundTaskInstance taskInstance)
        {
            if (taskInstance == null)
            {
                return null;
            }

            _deferral = taskInstance.GetDeferral();

            return Task.Run(async () =>
            {
                //// TODO WTS: Insert the code that should be executed in the background task here.
                //// This sample initializes a timer that counts to 100 in steps of 10.  It updates Message each time.

                //// Documentation:
                ////      * General: https://docs.microsoft.com/windows/uwp/launch-resume/support-your-app-with-background-tasks
                ////      * Debug: https://docs.microsoft.com/windows/uwp/launch-resume/debug-a-background-task
                ////      * Monitoring: https://docs.microsoft.com/windows/uwp/launch-resume/monitor-background-task-progress-and-completion

                //// To show the background progress and message on any page in the application,
                //// subscribe to the Progress and Completed events.
                //// You can do this via "BackgroundTaskService.GetBackgroundTasksRegistration"
                ///

                try
                {
                    Uri source = new Uri("https://opendata.arcgis.com/datasets/917fc37a709542548cc3be077a786c17_0.csv", UriKind.Absolute);
                    BackgroundDownloader downloader = new BackgroundDownloader();

                    StorageFile testfile = await folder.CreateFileAsync("data.csv", CreationCollisionOption.ReplaceExisting);
                    DownloadOperation download = downloader.CreateDownload(source, testfile);
                    await download.StartAsync();
                    await Task.Delay(TimeSpan.FromSeconds(5));
                    getDaten();
                    await Task.Delay(TimeSpan.FromSeconds(5));
                    sendMessage(getOldLandkreis() + "", getIndex(getOldLandkreis()));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Download Error", ex);
                }

            });
        }

        public override void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            _cancelRequested = true;

           // TODO WTS: Insert code to handle the cancelation request here.
           // Documentation: https://docs.microsoft.com/windows/uwp/launch-resume/handle-a-cancelled-background-task
        }

  

        public async void getDaten()
        {
            if (file == null)
            {
                Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                file = await storageFolder.GetFileAsync("data.csv");
                getDaten();
            }
            else
            {
                string[] einzelneZeilen = File.ReadAllLines(file.Path);
                daten = new string[einzelneZeilen.Length, 2];
                String[] geteilteZeilen;
                for (int i = 1; i < einzelneZeilen.Length; i++)
                {
                    geteilteZeilen = einzelneZeilen[i].Split(',');
                    daten[i, 0] = geteilteZeilen[7];
                    daten[i, 1] = geteilteZeilen[44] + "," + geteilteZeilen[45];
                    daten[i, 1] = daten[i, 1].Replace(@"\", string.Empty);
                }
            }

        }

        public String getIndex(String landkreis)
        {
            String returnString = "ERROR 404: Dieser Landkreis wurde nicht gefunden.";
            String vergleichbar = "";
            for (int i1 = 1; i1 < 413; i1++)
            {
                vergleichbar = daten[i1, 0];
                if (vergleichbar.Equals(landkreis))
                {

                    returnString = daten[i1, 1];
                }
            }
            return returnString;
        }


        public static String getOldLandkreis()
        {
            object storedValue = ApplicationData.Current.LocalSettings.Values["landkreis"];
            if (storedValue != null)
            {
                return (String)storedValue;
            }
            else
            {
                return "Flensburg";
            }
        }


        public void sendMessage(String landkreis, String anzahl)
        {
            // Construct the tile content
            TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            TileContent content = new TileContent()
            {
                Visual = new TileVisual()
                {
                    TileMedium = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            Children =
                {
                    new AdaptiveText()
                    {
                        Text = "Corona Index"
                    },

                    new AdaptiveText()
                    {
                        Text = "von "+landkreis,
                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                    },

                    new AdaptiveText()
                    {
                        Text = anzahl,
                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                    }
                }
                        }
                    },

                    TileWide = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            Children =
                {
                    new AdaptiveText()
                    {
                        Text = "Corona Index",
                        HintStyle = AdaptiveTextStyle.Subtitle
                    },

                    new AdaptiveText()
                    {
                        Text =  "von "+landkreis,
                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                    },

                    new AdaptiveText()
                    {
                        Text = anzahl,
                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                    }
                }
                        }
                    }
                }
            };


            var notification = new TileNotification(content.GetXml());
            notification.ExpirationTime = DateTimeOffset.UtcNow.AddMinutes(100);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(notification);
            _deferral?.Complete();

        }
    }
}
