using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSDictionaryEditor
{
    static class TimeStamp
    {
        //zwraca godzine w formacie int
        public static int godz(DateTime dateTime)
        {
            return dateTime.Hour * 3600 + dateTime.Minute * 60 + dateTime.Second; 
        }

        //zwraca date bez godziny 
        public static DateTime date(DateTime dateTime)
        {
            string sDateTime = dateTime.ToString("yyyy-mm-dd 00:00:00");
            DateTime newDate = new DateTime();

            newDate.AddYears(dateTime.Year);
            newDate.AddMonths(dateTime.Month);
            newDate.AddDays(dateTime.Day);

            return newDate;

        }

        //zwraca date 1800-01-01
        public static DateTime nullDate()
        {
            DateTime newDate = new DateTime();
            newDate.AddYears(2);
            newDate.AddMonths(01);
            newDate.AddDays(01);

            return newDate;

        }
    }
}
