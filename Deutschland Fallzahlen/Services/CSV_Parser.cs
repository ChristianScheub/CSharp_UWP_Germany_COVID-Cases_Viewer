using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deutschland_Fallzahlen.Services
{
    class CSV_Parser
    {
        private Windows.Storage.StorageFile file;
        public static String[,] daten;


        public String getDaten(int i)
        {
            return daten[i, 0] + " " + daten[i, 1];
        }
        public String getLandkreis(int i)
        {
            return daten[i, 0];
        }
        public int getDatenLange()
        {
            return daten.Length / 2;
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
                Debug.WriteLine(einzelneZeilen);
                daten = new string[einzelneZeilen.Length, 2];

                String[] geteilteZeilen;
                for (int i = 1; i < einzelneZeilen.Length; i++)
                {
                    geteilteZeilen = einzelneZeilen[i].Split(',');
                    daten[i, 0] = geteilteZeilen[7];
                    daten[i, 1] = geteilteZeilen[44] + "," + geteilteZeilen[45];
                    daten[i, 1] = daten[i, 1].Replace(@"\", string.Empty);
                }
                Debug.WriteLine(daten);

            }

        }


    }





}
