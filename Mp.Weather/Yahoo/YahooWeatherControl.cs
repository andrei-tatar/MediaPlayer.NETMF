using System;
using System.Threading;

using Microsoft.SPOT;

using Mp.Ui;
using Mp.Ui.Controls;
using Mp.Ui.Managers;

namespace Mp.Weather.Yahoo
{
    public sealed class YahooWeatherControl : UserControl
    {
        private YahooWeatherCondition _weather;
        private Bitmap _buffer;
        private Timer _refreshTimer;
        private bool _remakeBuffer;

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

        public YahooWeatherCondition WeatherCondition
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
                if (!Suspended) RefreshParent();
            }
        }

        private void RemakeControlBuffer()
        {
            Style cStyle = StyleManager.CurrentStyle;

            _buffer.DrawRectangle(cStyle.BackgroundColor, 0, 0, 0, Width, Height, 0, 0, cStyle.BackgroundColor, 0, 0, cStyle.BackgroundColor, 0, 0, 256);

            const int margin = 2;

            int x = 0, y = 0;

            if (_weather.Location != null)
            {
                _buffer.DrawText(_weather.Location.ToString(), Fonts.ArialBold, cStyle.LabelEnabledColor, x, y);
                y += Fonts.ArialBold.Height + margin;
            }

            if (_weather.StateImage != null)
            {
                _buffer.StretchImage(x, y, 52, 52, _weather.StateImage, 0, 0, _weather.StateImage.Width, _weather.StateImage.Height, 256);
                x += _weather.StateImage.Width + margin * 2;
            }

            _buffer.DrawText(_weather.Temperature, Fonts.ArialBold, cStyle.LabelEnabledColor, x, y);
            y += Fonts.ArialBold.Height + margin;

            _buffer.DrawText(_weather.State, Fonts.Arial, cStyle.LabelEnabledColor, x, y);
            y += Fonts.Arial.Height + margin;

            if (_weather.WindConditions != null)
            {
                string windText = "Wind: " + _weather.WindConditions;
                _buffer.DrawText(windText, Fonts.Arial, cStyle.LabelEnabledColor, x, y);

                y += Fonts.Arial.Height + margin;
            }

            _buffer.DrawText("Pressure: " + _weather.AtmosphereConditions.Pressure, Fonts.Arial, cStyle.LabelEnabledColor, 0, y);
            y += Fonts.Arial.Height + margin;
            _buffer.DrawText(_weather.AtmosphereConditions.ToString(), Fonts.Arial, cStyle.LabelEnabledColor, 0, y);
            y += Fonts.Arial.Height + margin;
            _buffer.DrawText(_weather.Astronomy.ToString(), Fonts.Arial, cStyle.LabelEnabledColor, 0, y);
            y += Fonts.Arial.Height + margin;
            _buffer.DrawText("Forecasted " + Utils.GetDeltaTime(DateTime.Now - _weather.GetTime) + " ago", Fonts.Arial, cStyle.LabelEnabledColor, 0, y);
            y += Fonts.Arial.Height + margin;

            x = 208;
            _buffer.DrawLine(Colors.DarkGray, 1, x, 0, x, y);
            x += 4;

            const int fWidth = 88;
            foreach (YahooForecast f in _weather.Forecasts)
            {
                if (f.StateImage == null) continue;
                int xx = x;
                int yy = Fonts.ArialBold.Height + margin;

                const int smImg = 52;

                _buffer.DrawTextInRect(f.Day, xx, yy, fWidth, 0, Bitmap.DT_IgnoreHeight | Bitmap.DT_AlignmentCenter, cStyle.LabelEnabledColor, Fonts.ArialBold);
                yy += Fonts.ArialBold.Height + margin;
                _buffer.StretchImage(xx + fWidth / 2 - smImg / 2, yy, smImg, smImg, f.StateImage, 0, 0, f.StateImage.Width, f.StateImage.Height, 256);
                yy += smImg + margin;

                _buffer.DrawTextInRect("Max:" + f.TemperatureHigh + " Min:" + f.TemperatureLow, xx, yy, fWidth, 0, Bitmap.DT_IgnoreHeight | Bitmap.DT_AlignmentCenter, cStyle.LabelEnabledColor, Fonts.Arial);
                yy += Fonts.Arial.Height + margin;
                _buffer.DrawTextInRect(f.State, xx, yy, fWidth, 0, Bitmap.DT_IgnoreHeight | Bitmap.DT_AlignmentCenter, cStyle.LabelEnabledColor, Fonts.Arial);

                x += fWidth;
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
                    RemakeControlBuffer();
                    _remakeBuffer = false;
                }
                screen.DrawImage(ScreenLeft, ScreenTop, _buffer, 0, 0, _buffer.Width, _buffer.Height, (ushort)(Enabled ? 256 : 128));
            }
        }

        public YahooWeatherControl(YahooWeatherCondition initialCondition, int X, int Y)
            : base(X, Y, 400, 136)
        {
            _buffer = new Bitmap(Width, Height);
            _remakeBuffer = true;
            _weather = initialCondition;

            _refreshTimer = new Timer((o) =>
                {
                    _remakeBuffer = true;
                    RefreshParent();
                }, null, Timeout.Infinite, Timeout.Infinite);

            RefreshMethod = new RefreshCallback(Refresh);
        }

        public override void Dispose()
        {
            _refreshTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _refreshTimer.Dispose();
            _buffer.Dispose();
            base.Dispose();
        }
    }
}
