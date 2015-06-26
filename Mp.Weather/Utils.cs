using System;
using System.IO;
using System.Net;

using Microsoft.SPOT;

namespace Mp.Weather
{
    internal static class Utils
    {
        public static int Timeout = 10000;

        public static Bitmap DownloadImageFromUrl(string url, WebProxy useProxy = null)
        {
            HttpWebRequest webRequest = null;
            HttpWebResponse webResponse = null;

            try
            {
                webRequest = (HttpWebRequest)HttpWebRequest.Create(url);
                if (useProxy != null) webRequest.Proxy = useProxy;

                webRequest.Timeout = Utils.Timeout;
                webRequest.ReadWriteTimeout = Utils.Timeout;

                webResponse = (HttpWebResponse)webRequest.GetResponse();
                Stream sr = webResponse.GetResponseStream();

                int pos = 0, length;
                byte[] gifBuffer;

                gifBuffer = new byte[(int)webResponse.ContentLength];

                while (pos != webResponse.ContentLength)
                {
                    length = sr.Read(gifBuffer, pos, gifBuffer.Length - pos);
                    pos += length;
                }
                Bitmap retImg = new Bitmap(gifBuffer, Bitmap.BitmapImageType.Gif);

                return retImg;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                if (webResponse != null)
                {
                    webResponse.Close();
                    webResponse.Dispose();
                }
                if (webRequest != null) webRequest.Dispose();
            }
        }

        public static string GetDeltaTime(TimeSpan d)
        {
            if (d.Days != 0) return d.Days.ToString() + " day" + (d.Days != 1 ? "s" : string.Empty);
            if (d.Hours != 0) return d.Hours.ToString() + " hour" + (d.Hours != 1 ? "s" : string.Empty);
            if (d.Minutes != 0) return d.Minutes.ToString() + " minute" + (d.Minutes != 1 ? "s" : string.Empty);
            return "less than a minute";
        }

        public static string ConvertFahrenheitToCelsius(string temp)
        {
            //C = F - 32 / 1.8
            return ((double.Parse(temp) - 32) * 5 / 9).ToString("f0");
        }
    }
}
