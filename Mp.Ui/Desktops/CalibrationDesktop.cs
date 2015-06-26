using System;
using System.Threading;

using Mp.Ui.Managers;
using Mp.Ui.Primitives;
using Mp.Ui.Controls;
using Mp.Ui.Controls.Containers;
using Mp.Ui.Controls.TouchControls.Buttons;

using Microsoft.SPOT;
using Microsoft.SPOT.Touch;
using Microsoft.SPOT.Presentation.Media;

namespace Mp.Ui.Desktops
{
    internal class CalibrationDesktop : Desktop
    {
        #region members
        private TouchCalibrationPoints _calibrationPoints;
        private int _currentCalibrationPoint;

        private bool _isCalibrating;
        private int _timeout;

        private TextButton _doneButton;
        private Label _timerLabel;
        private Timer _calDoneTimer;
        #endregion

        #region properties
        public TouchCalibrationPoints CalibrationPoints { get { return _calibrationPoints; } }
        #endregion

        #region events
        public event UiEventHandler CalibrationComplete;
        #endregion

        #region event handlers
        private void CalibrationDesktop_DrawBackground(Bitmap screen, int width, int height)
        {
            Style cStyle = StyleManager.CurrentStyle;

            _screenBuffer.SetClippingRectangle(0, 0, _width, _height);
            _screenBuffer.DrawRectangle(cStyle.BackgroundColor, 1, _x, _y, _width, _height, 0, 0,
                cStyle.BackgroundColor, 0, 0, cStyle.BackgroundColor, 0, 0, 256);

            if (_isCalibrating)
            {
                Color drColor = cStyle.LabelEnabledColor;
                int x = _calibrationPoints.ScreenX[_currentCalibrationPoint],
                    y = _calibrationPoints.ScreenY[_currentCalibrationPoint];
                _screenBuffer.DrawLine(drColor, 1, x - 10, y, x + 10, y);
                _screenBuffer.DrawLine(drColor, 1, x, y - 10, x, y + 10);
                _screenBuffer.DrawTextInRect("Touch the points to calibrate the screen", 20, 20, _width, _height, Bitmap.DT_AlignmentCenter,
                    cStyle.LabelEnabledColor, cStyle.LabelFont);
            }
        }
        #endregion

        #region functionality
        public override void AddChild(Controls.Control control)
        { throw new NotSupportedException("Can not modify the desktop"); }
        public override void ClearChilds()
        { throw new NotSupportedException("Can not modify the desktop"); }
        public override Control FindChild(int id)
        { throw new NotSupportedException("Can not modify the desktop"); }
        public override void RemoveChild(Control control)
        { throw new NotSupportedException("Can not modify the desktop"); }

        internal override bool OnTouchDown(int x, int y)
        {
            if (_isCalibrating) return true;
            return base.OnTouchDown(x, y);
        }

        internal override bool OnTouchMove(int x, int y)
        {
            if (_isCalibrating) return true;
            return base.OnTouchMove(x, y);
        }

        internal override bool OnTouchUp(int x, int y)
        {
            if (!_isCalibrating)
            {
                return base.OnTouchUp(x, y);
            }

            _calibrationPoints.TouchX[_currentCalibrationPoint] = (short)x;
            _calibrationPoints.TouchY[_currentCalibrationPoint] = (short)y;
            _currentCalibrationPoint++;

            if (_currentCalibrationPoint == _calibrationPoints.CalibrationPointCount)
            {
                _timeout = 11;
                _isCalibrating = false;
                _calibrationPoints.SetCalibration();

                Suspended = true;
                _doneButton.Visible = true;
                _timerLabel.Visible = true;

                _calDoneTimer = new Timer((o) =>
                    {
                        _timeout--;
                        _timerLabel.Text = "Press 'Done' in " + _timeout.ToString() + " sec to complete calibration";
                        if (_timeout == 0)
                        {
                            _calDoneTimer.Dispose();
                            _isCalibrating = true;
                            _currentCalibrationPoint = 0;

                            _doneButton.Visible = false;
                            _timerLabel.Visible = false;
                        }
                    }, null, 0, 1000);
                Suspended = false;
            }
            else
                Refresh();

            return true;
        }

        public void StartCalibration()
        {
            Touch.ActiveTouchPanel.StartCalibration();
            ReleaseCapture();
            _isCalibrating = true;
        }

        public override void Dispose()
        {
            if (_calDoneTimer != null)
            {
                _calDoneTimer.Dispose();
                _calDoneTimer = null;
            }
            base.Dispose();
        }
        #endregion

        #region constructor
        public CalibrationDesktop(TouchCalibrationPoints calPoints)
        {
            DrawBackground += CalibrationDesktop_DrawBackground;
            _calibrationPoints = calPoints;
            _currentCalibrationPoint = 0;
            _isCalibrating = false;

            _doneButton = new TextButton("Done", _width / 2 - 50, _height - 40, 100, 35);
            _doneButton.Visible = false;
            _doneButton.ButtonPressed += (s) =>
            {
                _calDoneTimer.Dispose();
                if (CalibrationComplete != null) CalibrationComplete(this);
            };
            base.AddChild(_doneButton);

            _timerLabel = new Label(string.Empty, 10, 10, _width - 20, 0);
            _timerLabel.CenterText = true;
            _timerLabel.AutoHeight = true;
            _timerLabel.Visible = false;
            base.AddChild(_timerLabel);
        }
        #endregion
    }
}
