using System;
using Microsoft.SPOT;

namespace Mp.Weather.Yahoo
{
    public sealed class YahooLocation
    {
        private string _toString = null;

        public string City { get; internal set; }
        public string Region { get; internal set; }
        public string Country { get; internal set; }

        public override string ToString()
        {
            return _toString ?? (_toString = City + (City.Length != 0 ? ", " : string.Empty) + Region + (Region.Length != 0 ? ", " : string.Empty) + Country);
        }

        internal YahooLocation() { }
    }

    public sealed class YahooWindConditions
    {
        private string _toString = null;

        public string Chill { get; internal set; }
        public double Direction { get; internal set; }
        public string Speed { get; internal set; }

        public string GetPole()
        {
            if (Direction > 337.5 || Direction <= 22.5) return "N";
            else if (Direction > 22.5 && Direction <= 67.5) return "NE";
            else if (Direction > 68.5 && Direction <= 112.5) return "E";
            else if (Direction > 113.5 && Direction <= 157.5) return "SE";
            else if (Direction > 158.5 && Direction <= 202.5) return "S";
            else if (Direction > 203.5 && Direction <= 247.5) return "SW";
            else if (Direction > 248.5 && Direction <= 292.5) return "W";
            else if (Direction > 293.5 && Direction <= 337.5) return "NW";
            else throw new Exception("Invalid Direction");
        }

        public override string ToString()
        {
            return _toString ?? (_toString = GetPole() + " - " + Speed);
        }

        internal YahooWindConditions() { }
    }

    public sealed class YahooAtmosphereConditions
    {
        private string _toString = null;

        public string Humidity { get; internal set; }
        public string Visibility { get; internal set; }
        public string Pressure { get; internal set; }

        public override string ToString()
        {
            return _toString ?? (_toString = "Humidity: " + Humidity.ToString() + "%" + ", Visibility: " + Visibility);
        }

        internal YahooAtmosphereConditions() { }
    }

    public sealed class YahooAstronomy
    {
        private string _toString = null;

        public string SunRise { get; internal set; }
        public string SunSet { get; internal set; }

        public override string ToString()
        {
            return _toString ?? (_toString = "Sunrise: " + SunRise + "; Sunset: " + SunSet);
        }

        internal YahooAstronomy() { }
    }

    public sealed class YahooForecast
    {
        public string Day { get; internal set; }
        public string TemperatureLow { get; internal set; }
        public string TemperatureHigh { get; internal set; }
        public string State { get; internal set; }
        public Bitmap StateImage { get; internal set; }

        internal YahooForecast() { }
    }

    public sealed class YahooWeatherCondition
    {
        public YahooLocation Location { get; internal set; }
        public YahooWindConditions WindConditions { get; internal set; }
        public YahooAtmosphereConditions AtmosphereConditions { get; internal set; }
        public YahooAstronomy Astronomy { get; internal set; }

        public string State { get; internal set; }
        public Bitmap StateImage { get; internal set; }
        public string Temperature { get; internal set; }

        public YahooForecast[] Forecasts { get; internal set; }

        public DateTime GetTime { get; internal set; }

        internal YahooWeatherCondition() { }
    }
}
