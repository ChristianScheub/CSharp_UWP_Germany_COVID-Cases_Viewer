using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Deutschland_Fallzahlen.Services
{
    class OldDataService
    {
        public static String getOldLandkreis()
        {
            object storedValue = ApplicationData.Current.LocalSettings.Values["landkreis"];

            // check if it is null so you don't throw a null reference exception
            if (storedValue != null)
            {
                return (String)storedValue; // cast to int after you're sure it is not null.                
            }
            else
            {
                return "Flensburg";
            }
        }
    }
}
