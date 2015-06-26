using System;
using Microsoft.SPOT;
using Mp.Ui.Controls;
using Mp.Ui;
using System.Threading;
using Mp.Ui.Managers;

namespace Mp.Weather.Google
{
    public sealed class GoogleWeatherControl : UserControl
    {
        private Bitmap _buffer;
        private Timer _refreshTimer;
        private bool _remakeBuffer;
        private GoogleForecast _weather;

        public GoogleForecast WeatherCondition
        {
            get { return _weather; }
            set
            {
                if (_weather == null)
                {
                    if (value != null)
                        _refreshTimer.Change(60000, 60000);
                    else
                        _refreshTimer.Change(Timeout.Infinite, Timeout.Infinite);
                }

                _weather = value;
                _remakeBuffer = true;
                Refresh();
            }
        }

        public override int Width
        {
            get { return base.Width; }
            set { throw new Exception("Width can not be set"); }
        }

        public override int Height
        {
            get { return base.Height; }
            set { throw new Exception("Height can not be set"); }
        }

        private void RemakeBuffer()
        {
            Style cStyle = StyleManager.CurrentStyle;

            _buffer.DrawRectangle(cStyle.BackgroundColor, 0, 0, 0, _buffer.Width, _buffer.Height, 0, 0, cStyle.BackgroundColor, 0, 0, cStyle.BackgroundColor, 0, 0, 256);


            const int margin = 2;
            int x = 0, y = 0;

            _buffer.DrawText(_weather.City, Fonts.ArialBold, cStyle.LabelEnabledColor, x, y);
            y += Fonts.ArialBold.Height + margin;

            int imgSize = Fonts.ArialBold.Height + Fonts.Arial.Height * 2 + margin * 2;
            _buffer.Scale9Image(x, y, imgSize, imgSize, _weather.CurrentCondition.Icon, 0, 0, 0, 0, 256);
            x += imgSize + margin;

            _buffer.DrawText(_weather.CurrentCondition.Temp, Fonts.ArialBold, cStyle.LabelEnabledColor, x, y);
            y += Fonts.ArialBold.Height + margin;

            _buffer.DrawText(_weather.CurrentCondition.Condition, Fonts.Arial, cStyle.LabelEnabledColor, x, y);
            y += Fonts.Arial.Height + margin;

            _buffer.DrawText(_weather.CurrentCondition.Wind, Fonts.Arial, cStyle.LabelEnabledColor, x, y);
            y += Fonts.Arial.Height + margin;
            x = 0;

            _buffer.DrawText(_weather.CurrentCondition.Humidity, Fonts.Arial, cStyle.LabelEnabledColor, x, y);
            y += Fonts.Arial.Height + margin;

            _buffer.DrawText("Forecasted " + Utils.GetDeltaTime(DateTime.Now - _weather.ForecastTime) + " ago", Fonts.Arial, cStyle.LabelEnabledColor, 0, y);

            x = 200;
            _buffer.DrawLine(Colors.DarkGray, 1, x, 0, x, Height - 1);

            int forecastSize = Height / _weather.Forecasts.Length;
            y = 0;
            foreach (GoogleForecastCondition forecast in _weather.Forecasts)
            {
                x = 210;

                int auxY = y + (forecastSize - forecast.Icon.Height) / 2;
                _buffer.DrawImage(x, auxY, forecast.Icon, 0, 0, forecast.Icon.Width, forecast.Icon.Height);
                x += forecastSize + margin;

                _buffer.DrawText(forecast.DayOfWeek + ", " + forecast.Condition, Fonts.Arial, cStyle.LabelEnabledColor, x, auxY);
                auxY += Fonts.Arial.Height + margin;

                _buffer.DrawText("Min: " + forecast.TempLow + " °C, Max: " + forecast.TempHigh + " °C", Fonts.Arial, cStyle.LabelEnabledColor, x, auxY);

                y += forecastSize;
            }
        }

        private void Refresh(Bitmap screen)
        {
            if (_weather == null)
            {
                Style cStyle = StyleManager.CurrentStyle;
                screen.DrawRectangle(cStyle.BackgroundColor, 0, ScreenLeft, ScreenTop, Width, Height, 0, 0, cStyle.BackgroundColor, 0, 0, cStyle.BackgroundColor, 0, 0, 256);
                screen.DrawTextInRect("No weather information, invalid location or no network connection available!", ScreenLeft + 20, ScreenTop + 30, Width - 40, Height - 60,
                    Bitmap.DT_AlignmentCenter | Bitmap.DT_WordWrap, StyleManager.CurrentStyle.TextBoxEnabledInvalidBorder, Fonts.ArialItalic);
            }
            else
            {
                if (_remakeBuffer)
                {
                    RemakeBuffer();
                    _remakeBuffer = false;
                }
                screen.DrawImage(ScreenLeft, ScreenTop, _buffer, 0, 0, _buffer.Width, _buffer.Height, (ushort)(Enabled ? 256 : 128));
            }
        }

        public GoogleWeatherControl(GoogleForecast initialCondition, int X, int Y)
            : base(X, Y, 400, 180)
        {
            _buffer = new Bitmap(Width, Height);
            _remakeBuffer = true;

            _refreshTimer = new Timer((o) =>
            {
                _remakeBuffer = true;
                RefreshParent();
            }, null, Timeout.Infinite, Timeout.Infinite);

            RefreshMethod = Refresh;
            WeatherCondition = initialCondition;
        }

        public override void Dispose()
        {
            _buffer.Dispose();
            base.Dispose();
        }
    }
}
