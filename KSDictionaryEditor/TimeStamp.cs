using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSDictionaryEditor
{
    static class TimeStamp
    {

        public static int godz(DateTime dateTime)
        {
            return dateTime.Hour * 3600 + dateTime.Minute * 60 + dateTime.Second; 
        }

        public static string date(DateTime dateTime)
        {
            return dateTime.ToString("yyyy-mm-dd 00:00:00");            
        }
    }
}
