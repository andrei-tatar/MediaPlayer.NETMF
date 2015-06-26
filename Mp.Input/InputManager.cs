using System;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Touch;

namespace Mp.Input
{
    public class InputManager : IEventListener
    {
        private static InputManager _instance;
        public static InputManager Current { get { return _instance ?? (_instance = new InputManager()); } }

        #region Variables
        private Thread _touchThread;
        private bool _penDown;
        private int _x, _y, _lastX, _lastY;
        #endregion

        #region events
        public event OnTouchEvent TouchDown;
        public event OnTouchEvent TouchMove;
        public event OnTouchEvent TouchUp;
        #endregion

        #region Constructor
        private InputManager()
        {
            Touch.Initialize(this);
            TouchCollectorConfiguration.CollectionMode = CollectionMode.InkOnly;
            TouchCollectorConfiguration.CollectionMethod = CollectionMethod.Native;

            _touchThread = new Thread(myTouch_Tick);
            _touchThread.Start();
        }
        #endregion

        #region Private Methods
        private void myTouch_Tick()
        {
            int x = 0;
            int y = 0;

            while (true)
            {
                TouchCollectorConfiguration.GetLastTouchPoint(ref x, ref y);

                if (x != 1022 && x > 0 || y != 1022 && y > 0)
                {
                    _x = x;
                    _y = y;

                    if (_penDown)
                    {
                        if (System.Math.Abs(_lastX - x) > 4 || System.Math.Abs(_lastY - y) > 4)
                        {
                            if (TouchMove != null) TouchMove(x, y);
                            _lastX = x;
                            _lastY = y;
                        }
                    }
                    else
                    {
                        _penDown = true;
                        if (TouchDown != null) TouchDown(x, y);
                        _lastX = x;
                        _lastY = y;
                    }
                }
                else
                {
                    if (_penDown)
                    {
                        _penDown = false;
                        if (TouchUp != null) TouchUp(_lastX, _lastY);
                    }
                }

                Thread.Sleep(20);
            }
        }
        #endregion

        #region IEventListener methods
        public void InitializeForEventSource()
        { }

        public bool OnEvent(BaseEvent baseEvent)
        { return true; }
        #endregion
    }
}