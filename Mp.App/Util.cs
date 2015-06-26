using System;
using Microsoft.SPOT;

namespace Mp.App
{
    class Util
    {
        public static string GetLongDateString(DateTime date)
        {
            string dateStr = "";

            switch (date.DayOfWeek)
            {
                case DayOfWeek.Monday: dateStr += "Monday"; break;
                case DayOfWeek.Tuesday: dateStr += "Tuesday"; break;
                case DayOfWeek.Wednesday: dateStr += "Wednesday"; break;
                case DayOfWeek.Thursday: dateStr += "Thursday"; break;
                case DayOfWeek.Friday: dateStr += "Friday"; break;
                case DayOfWeek.Saturday: dateStr += "Saturday"; break;
                case DayOfWeek.Sunday: dateStr += "Sunday"; break;
            }

            dateStr += ", " + date.Day + " ";

            switch (date.Month)
            {
                case 1: dateStr += "Ianuarie"; break;
                case 2: dateStr += "Februarie"; break;
                case 3: dateStr += "Martie"; break;
                case 4: dateStr += "Aprilie"; break;
                case 5: dateStr += "Mai"; break;
                case 6: dateStr += "Iunie"; break;
                case 7: dateStr += "Iulie"; break;
                case 8: dateStr += "August"; break;
                case 9: dateStr += "Septembrie"; break;
                case 10: dateStr += "Octombrie"; break;
                case 11: dateStr += "Noiembrie"; break;
                case 12: dateStr += "Decembrie"; break;
            }

            dateStr += " " + date.Year;

            return dateStr;
        }

        public static string ToString2Digit(int nr)
        {
            return (nr / 10).ToString() + (nr % 10).ToString();
        }
    }
}
