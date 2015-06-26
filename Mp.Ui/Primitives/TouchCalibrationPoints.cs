using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Touch;

namespace Mp.Ui.Primitives
{
    class TouchCalibrationPoints
    {
        static ExtendedWeakReference _ewr;

        private TouchCalibrationData _calibrationData;

        [Serializable]
        public class TouchCalibrationData
        {
            public short[] screenX;
            public short[] screenY;
            public short[] touchX;
            public short[] touchY;
        }

        internal TouchCalibrationPoints()
        {
            this._calibrationData = new TouchCalibrationData();

            int calibrationPointCount = 0;
            Touch.ActiveTouchPanel.GetCalibrationPointCount(ref calibrationPointCount);

            this._calibrationData.screenX = new short[calibrationPointCount];
            this._calibrationData.screenY = new short[calibrationPointCount];
            this._calibrationData.touchX = new short[calibrationPointCount];
            this._calibrationData.touchY = new short[calibrationPointCount];

            // Get the points for calibration.
            for (int index = 0; index < calibrationPointCount; index++)
            {
                int x = 0;
                int y = 0;
                Touch.ActiveTouchPanel.GetCalibrationPoint(index, ref x, ref y);
                this._calibrationData.screenX[index] = (short)x;
                this._calibrationData.screenY[index] = (short)y;
            }
        }

        internal TouchCalibrationPoints(TouchCalibrationData touchCalibrationData)
        {
            this._calibrationData = touchCalibrationData;
        }

        public int CalibrationPointCount { get { return this._calibrationData.screenX.Length; } }
        public short[] ScreenX { get { return this._calibrationData.screenX; } }
        public short[] ScreenY { get { return this._calibrationData.screenY; } }
        public short[] TouchX { get { return this._calibrationData.touchX; } }
        public short[] TouchY { get { return this._calibrationData.touchY; } }

        public static TouchCalibrationPoints Load()
        {
            _ewr = ExtendedWeakReference.RecoverOrCreate(typeof(TouchCalibrationPoints), 0, ExtendedWeakReference.c_SurvivePowerdown);
            _ewr.Priority = (int)ExtendedWeakReference.PriorityLevel.Important;
            return _ewr.Target == null ? null : new TouchCalibrationPoints((TouchCalibrationData)_ewr.Target);
        }

        public void Save()
        {
            _ewr.Target = this._calibrationData;
        }

        public void SetCalibration()
        {
            Touch.ActiveTouchPanel.SetCalibration(this._calibrationData.screenX.Length, this._calibrationData.screenX, this._calibrationData.screenY, this._calibrationData.touchX, this._calibrationData.touchY);
        }
    }
}
