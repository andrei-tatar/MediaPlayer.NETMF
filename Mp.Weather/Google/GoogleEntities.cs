using System;
using Microsoft.SPOT;

namespace Mp.Weather.Google
{
    public sealed class GoogleForecast
    {
        public string City { get; internal set; }
        public string UnitSystem { get; internal set; }
        public GoogleCurrentCondition CurrentCondition { get; internal set; }
        public GoogleForecastCondition[] Forecasts { get; internal set; }
        public DateTime ForecastTime { get; internal set; }
    }

    public sealed class GoogleCurrentCondition
    {
        public string Condition { get; internal set; }
        public string Temp { get; internal set; }
        public string Humidity { get; internal set; }
        public Bitmap Icon { get; internal set; }
        public string Wind { get; internal set; }
    }

    public sealed class GoogleForecastCondition
    {
        public string DayOfWeek { get; internal set; }
        public string TempLow { get; internal set; }
        public string TempHigh { get; internal set; }
        public Bitmap Icon { get; internal set; }
        public string Condition { get; internal set; }
    }
}
