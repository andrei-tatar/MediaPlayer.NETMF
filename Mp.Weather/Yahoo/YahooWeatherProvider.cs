using Microsoft.SPOT;

using System;
using System.IO;
using System.Xml;
using System.Net;
using System.Threading;
using System.Collections;

namespace Mp.Weather.Yahoo
{
    public sealed class YahooWeatherProvider
    {
        private const string _findPlaceUrl_part1 = "http://query.yahooapis.com/v1/public/yql?format=xml&q=select%20woeid%20from%20geo.places%20where%20text='";
        private const string _findPlaceUrl_part2 = "'%20limit%201";

        private const string _getWeatherUrl = "http://weather.yahooapis.com/forecastrss?u=c&w=";

        private const string _weatherImageUrl_part1 = "http://l.yimg.com/a/i/us/we/52/";
        private const string _weatherImageUrl_part2 = ".gif";

        private string _place;
        private string _placeWoeid;
        private bool _isValidPlace;

        private static Hashtable _stateImagesCache;
        private static Hashtable _placesWoeidCache;

        private YahooWeatherCondition _weatherCondition;

        public YahooWeatherCondition LastWeatherCondition { get { return _weatherCondition; } }
        public string Place { get { return _place; } }
        public bool PlaceValid { get { return _isValidPlace; } }

        public YahooWeatherProvider()
        {
            _place = string.Empty;
            _placeWoeid = string.Empty;
            _isValidPlace = false;

            if (_stateImagesCache == null)
            {
                _stateImagesCache = new Hashtable();
                _placesWoeidCache = new Hashtable();
            }
        }

        public static void ClearCache()
        {
            if (_stateImagesCache != null)
            {
                _stateImagesCache.Clear();
                _placesWoeidCache.Clear();
            }
        }

        public bool SetWeatherPlace(string place, WebProxy useProxy = null)
        {
            place = place.ToLower();

            _weatherCondition = null;

            //if in cache, don't do a query
            if (_placesWoeidCache.Contains(place))
            {
                _isValidPlace = true;
                _placeWoeid = (string)_placesWoeidCache[place];
                _place = place;
                return true;
            }
            else
            {
                _place = string.Empty;
                _isValidPlace = false;
            }

            #region format place (replace spaces with %20) and get query url
            string findPlaceUrl = _findPlaceUrl_part1;
            for (int i = 0; i < place.Length; i++)
            {
                char c = place[i];
                switch (c)
                {
                    case ' ':
                        findPlaceUrl += "%20";
                        break;
                    default:
                        findPlaceUrl += c;
                        break;
                }
            }
            findPlaceUrl += _findPlaceUrl_part2;
            #endregion

            HttpWebRequest webRequest = null;
            HttpWebResponse webResponse = null;
            XmlReader xmlReader = null;
            try
            {
                webRequest = (HttpWebRequest)HttpWebRequest.Create(findPlaceUrl);
                if (useProxy != null) webRequest.Proxy = useProxy;

                webRequest.Timeout = Utils.Timeout;
                webRequest.ReadWriteTimeout = Utils.Timeout;

                webResponse = (HttpWebResponse)webRequest.GetResponse();
                xmlReader = XmlReader.Create(webResponse.GetResponseStream());
                if (xmlReader.ReadToFollowing("woeid"))
                {
                    //store in cache the found woeid
                    string woeid = xmlReader.ReadElementString();
                    _placesWoeidCache.Add(place, woeid);
                    _placeWoeid = woeid;
                    _place = place;
                    _isValidPlace = true;
                }
            }
            catch (Exception)
            { }
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
            return _isValidPlace;
        }

        public bool GetWeather(WebProxy useProxy = null)
        {
            if (!_isValidPlace) return false;

            string getWeatherUrl = _getWeatherUrl + _placeWoeid;

            HttpWebRequest webRequest = null;
            HttpWebResponse webResponse = null;
            XmlReader xmlReader = null;

            YahooWeatherCondition condition = new YahooWeatherCondition();
            try
            {
                webRequest = (HttpWebRequest)HttpWebRequest.Create(getWeatherUrl);
                if (useProxy != null) webRequest.Proxy = useProxy;

                webRequest.Timeout = Utils.Timeout;
                webRequest.ReadWriteTimeout = Utils.Timeout;

                webResponse = (HttpWebResponse)webRequest.GetResponse();
                xmlReader = XmlReader.Create(webResponse.GetResponseStream());
                if (xmlReader.ReadToFollowing("yweather:location"))
                {
                    YahooLocation fLocation = new YahooLocation();
                    fLocation.City = xmlReader.GetAttribute("city");
                    fLocation.Region = xmlReader.GetAttribute("region");
                    fLocation.Country = xmlReader.GetAttribute("country");
                    condition.Location = fLocation;
                }
                else return false;

                string unitTemp, unitDistance, unitPressure, unitSpeed;
                if (xmlReader.ReadToFollowing("yweather:units"))
                {
                    unitTemp = xmlReader.GetAttribute("temperature");
                    unitDistance = xmlReader.GetAttribute("distance");
                    unitPressure = xmlReader.GetAttribute("pressure");
                    unitSpeed = xmlReader.GetAttribute("speed");
                }
                else return false;

                if (xmlReader.ReadToFollowing("yweather:wind"))
                {
                    YahooWindConditions fWind = new YahooWindConditions();
                    fWind.Chill = xmlReader.GetAttribute("chill");
                    fWind.Direction = double.Parse(xmlReader.GetAttribute("direction"));
                    fWind.Speed = xmlReader.GetAttribute("speed") + " " + unitSpeed;
                    condition.WindConditions = fWind;
                }
                else return false;

                if (xmlReader.ReadToFollowing("yweather:atmosphere"))
                {
                    YahooAtmosphereConditions fAtmoshpere = new YahooAtmosphereConditions();
                    fAtmoshpere.Humidity = xmlReader.GetAttribute("humidity");

                    string pressureString = xmlReader.GetAttribute("pressure");
                    double pressureValue = double.Parse(pressureString);
                    double deltaPressure = pressureValue - 1013.25;
                    fAtmoshpere.Pressure = pressureString + " " + unitPressure + " (" + (deltaPressure >= 0 ? "+" : string.Empty) + deltaPressure.ToString("f2") + ")";
                    fAtmoshpere.Visibility = xmlReader.GetAttribute("visibility") + " " + unitDistance;
                    condition.AtmosphereConditions = fAtmoshpere;
                }
                else return false;

                if (xmlReader.ReadToFollowing("yweather:astronomy"))
                {
                    YahooAstronomy fAstronomy = new YahooAstronomy();
                    fAstronomy.SunRise = xmlReader.GetAttribute("sunrise");
                    fAstronomy.SunSet = xmlReader.GetAttribute("sunset");
                    condition.Astronomy = fAstronomy;
                }
                else return false;

                if (xmlReader.ReadToFollowing("yweather:condition"))
                {
                    condition.State = xmlReader.GetAttribute("text");
                    condition.StateImage = GetImageForState(xmlReader.GetAttribute("code").Trim(), useProxy);
                    condition.Temperature = xmlReader.GetAttribute("temp") + "°" + unitTemp;
                }
                else return false;

                ArrayList forecasts = new ArrayList();
                while (xmlReader.ReadToFollowing("yweather:forecast"))
                {
                    YahooForecast fForecast = new YahooForecast();
                    fForecast.Day = xmlReader.GetAttribute("day");
                    fForecast.TemperatureLow = xmlReader.GetAttribute("low") + "°";
                    fForecast.TemperatureHigh = xmlReader.GetAttribute("high") + "°";
                    fForecast.State = xmlReader.GetAttribute("text");
                    fForecast.StateImage = GetImageForState(xmlReader.GetAttribute("code").Trim(), useProxy);

                    forecasts.Add(fForecast);
                }

                condition.Forecasts = (YahooForecast[])forecasts.ToArray(typeof(YahooForecast));

                condition.GetTime = DateTime.Now;
                _weatherCondition = condition;

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
                    webResponse.Dispose();
                    webResponse.Close();
                }
                if (webRequest != null) webRequest.Dispose();
            }
        }

        private Bitmap GetImageForState(string state, WebProxy useProxy)
        {
            //check if state image is in cache
            if (_stateImagesCache.Contains(state)) return (Bitmap)_stateImagesCache[state];

            int tries = 5;
            do
            {
                Bitmap ret = DownloadImageForState(state, useProxy);
                if (ret != null)
                {
                    //store the image in cache
                    _stateImagesCache.Add(state, ret);
                    return ret;
                }
                Thread.Sleep(300);
            } while (tries-- != 0);
            return null;
        }

        private Bitmap DownloadImageForState(string state, WebProxy useProxy)
        {
            string getImageUrl = _weatherImageUrl_part1 + state + _weatherImageUrl_part2;
            return Utils.DownloadImageFromUrl(getImageUrl, useProxy);
        }
    }
}
