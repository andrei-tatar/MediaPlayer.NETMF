using System;
using System.Net;
using System.Xml;
using System.Collections;
using System.Threading;

using Microsoft.SPOT;
using System.IO;

namespace Mp.Weather.Google
{
    public sealed class GoogleWeatherProvider
    {
        private const string _getBaseUrl = "http://www.google.com";
        private const string _getWeatherUrl = _getBaseUrl + "/ig/api?weather=";


        private static Hashtable _conditionImagesCache;
        private GoogleForecast _weatherCondition;

        public GoogleForecast LastWeatherCondition { get { return _weatherCondition; } }

        public GoogleWeatherProvider()
        {
            _weatherCondition = null;
            if (_conditionImagesCache == null)
                _conditionImagesCache = new Hashtable();
        }

        public static void ClearCache()
        {
            if (_conditionImagesCache != null)
                _conditionImagesCache.Clear();
        }

        private Bitmap GetImage(string relativeUrl, WebProxy useProxy)
        {
            //check if state image is in cache
            if (_conditionImagesCache.Contains(relativeUrl)) return (Bitmap)_conditionImagesCache[relativeUrl];

            int tries = 5;
            do
            {
                Bitmap ret = DownloadImageFromRelativeUrl(relativeUrl, useProxy);
                if (ret != null)
                {
                    //store the image in cache
                    _conditionImagesCache.Add(relativeUrl, ret);
                    return ret;
                }
                Thread.Sleep(300);
            } while (tries-- != 0);
            return null;
        }

        private Bitmap DownloadImageFromRelativeUrl(string relativeUrl, WebProxy useProxy)
        {
            return Utils.DownloadImageFromUrl(_getBaseUrl + relativeUrl, useProxy);
        }

        public bool GetWeather(string place, WebProxy useProxy = null)
        {
            _weatherCondition = null;
            string requestUrl = _getWeatherUrl;

            #region format place (replace spaces with %20) and get query url
            for (int i = 0; i < place.Length; i++)
            {
                char c = place[i];
                switch (c)
                {
                    case ' ':
                        requestUrl += "%20";
                        break;
                    default:
                        requestUrl += c;
                        break;
                }
            }
            #endregion

            HttpWebRequest webRequest = null;
            HttpWebResponse webResponse = null;
            XmlReader xmlReader = null;
            try
            {
                webRequest = (HttpWebRequest)HttpWebRequest.Create(requestUrl);
                if (useProxy != null) webRequest.Proxy = useProxy;

                webRequest.Timeout = Utils.Timeout;
                webRequest.ReadWriteTimeout = Utils.Timeout;

                webResponse = (HttpWebResponse)webRequest.GetResponse();
                xmlReader = XmlReader.Create(webResponse.GetResponseStream());

                if (!xmlReader.ReadToFollowing("forecast_information")) return false;

                GoogleForecast newForecast = new GoogleForecast();
                if (!xmlReader.ReadToDescendant("city")) return false;
                newForecast.City = xmlReader.GetAttribute("data");
                if (!xmlReader.ReadToNextSibling("unit_system")) return false;
                newForecast.UnitSystem = xmlReader.GetAttribute("data");

                GoogleCurrentCondition currentCondition;
                newForecast.CurrentCondition = currentCondition = new GoogleCurrentCondition();

                if (!xmlReader.ReadToFollowing("current_conditions")) return false;
                if (!xmlReader.ReadToDescendant("condition")) return false;
                currentCondition.Condition = xmlReader.GetAttribute("data");
                if (!xmlReader.ReadToNextSibling("temp_c")) return false;
                currentCondition.Temp = xmlReader.GetAttribute("data") + "°C";
                if (!xmlReader.ReadToNextSibling("humidity")) return false;
                currentCondition.Humidity = xmlReader.GetAttribute("data");
                if (!xmlReader.ReadToNextSibling("icon")) return false;
                currentCondition.Icon = GetImage(xmlReader.GetAttribute("data"), useProxy);
                if (!xmlReader.ReadToNextSibling("wind_condition")) return false;
                currentCondition.Wind = xmlReader.GetAttribute("data");

                ArrayList forecasts = new ArrayList();
                while (xmlReader.ReadToFollowing("forecast_conditions"))
                {
                    GoogleForecastCondition forecast = new GoogleForecastCondition();
                    if (!xmlReader.ReadToDescendant("day_of_week")) return false;
                    forecast.DayOfWeek = xmlReader.GetAttribute("data");
                    if (!xmlReader.ReadToNextSibling("low")) return false;
                    forecast.TempLow = xmlReader.GetAttribute("data");
                    if (newForecast.UnitSystem == "US")
                        forecast.TempLow = Utils.ConvertFahrenheitToCelsius(forecast.TempLow);

                    if (!xmlReader.ReadToNextSibling("high")) return false;
                    forecast.TempHigh = xmlReader.GetAttribute("data");
                    if (newForecast.UnitSystem == "US")
                        forecast.TempHigh = Utils.ConvertFahrenheitToCelsius(forecast.TempHigh);

                    if (!xmlReader.ReadToNextSibling("icon")) return false;
                    forecast.Icon = GetImage(xmlReader.GetAttribute("data"), useProxy);
                    if (!xmlReader.ReadToNextSibling("condition")) return false;
                    forecast.Condition = xmlReader.GetAttribute("data");
                    forecasts.Add(forecast);
                }
                newForecast.Forecasts = (GoogleForecastCondition[])forecasts.ToArray(typeof(GoogleForecastCondition));
                newForecast.ForecastTime = DateTime.Now;
                _weatherCondition = newForecast;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (xmlReader != null)
                {
                    xmlReader.Close();
                    xmlReader.Dispose();
                }
                if (webResponse != null)
                {
                    webResponse.Close();
                    webResponse.Dispose();
                }
                if (webRequest != null) webRequest.Dispose();
            }
        }
    }
}
