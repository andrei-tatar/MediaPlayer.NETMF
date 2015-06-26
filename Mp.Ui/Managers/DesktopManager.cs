using System;
using System.Threading;
using System.Collections;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Presentation.Media;

using Mp.Ui.Controls;
using Mp.Ui.Controls.Containers;
using Mp.Ui.Desktops;
using Mp.Ui.Primitives;

using System.Runtime.CompilerServices;
using Mp.Input;

namespace Mp.Ui.Managers
{
    public sealed class DesktopManager
    {
        #region static members/properties
        public static readonly int ScreenWidth;
        public static readonly int ScreenHeight;
        public static readonly int ScreenBitsPerPixel;
        public static readonly int ScreenOrientationDeg;

        private static DesktopManager _instance;
        public static DesktopManager Instance
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return _instance ?? (_instance = new DesktopManager()); }
        }
        #endregion

        #region constants
        private const int _reservedDesktops = 2;

        private const int DESK_DEFAULT = 0;
        private const int DESK_EDIT = 1;
        #endregion

        #region members
        private ArrayList _desktops;
        private Desktop _currentDesktop;
        private InputManager _inputManager;

        //used for dispatching ui events
        //private Queue _uiEvents = new Queue();
        //private AutoResetEvent _uiConsumerWait = new AutoResetEvent(false);
        //private bool _uiHandlerStarted;

        private bool _firstInputManagerSet;

        private Timer _inputTimeoutTimer;
        private int _inputTimeoutMs;
        #endregion

        #region properties
        public Desktop CurrentDesktop { get { return _currentDesktop; } }
        public Desktop DefaultDesktop { get { return (Desktop)_desktops[DESK_DEFAULT]; } }

        public InputManager InputManager
        {
            get { return _inputManager; }
            set
            {
                if (_inputManager != null)
                {
                    _inputManager.TouchDown -= new OnTouchEvent(_inputManager_TouchDown);
                    _inputManager.TouchMove -= new OnTouchEvent(_inputManager_TouchMove);
                    _inputManager.TouchUp -= new OnTouchEvent(_inputManager_TouchUp);
                }
                _inputManager = value;
                if (_inputManager != null)
                {
                    _inputManager.TouchDown += new OnTouchEvent(_inputManager_TouchDown);
                    _inputManager.TouchMove += new OnTouchEvent(_inputManager_TouchMove);
                    _inputManager.TouchUp += new OnTouchEvent(_inputManager_TouchUp);

                    if (_firstInputManagerSet)
                    {
                        _firstInputManagerSet = false;
                        TouchCalibrationPoints touchCalibrationPoints = TouchCalibrationPoints.Load();
                        if (touchCalibrationPoints == null)
                        {
                            StartCalibration();
                        }
                        else
                        {
                            touchCalibrationPoints.SetCalibration();
                            _currentDesktop.Refresh();
                        }
                    }
                }
            }
        }
        #endregion

        #region events
        public delegate void CalibrationCompleteEventHandler();
        public event CalibrationCompleteEventHandler CalibrationComplete;

        public delegate void InputTimeoutEventHandler();
        public event InputTimeoutEventHandler InputTimeout;
        #endregion

        #region event handlers
        private void calDesktop_CalibrationComplete(Control sender)
        {
            CalibrationDesktop calDesktop = (CalibrationDesktop)sender;
            calDesktop.CalibrationComplete -= calDesktop_CalibrationComplete;
            calDesktop.CalibrationPoints.Save();
            RemoveDesktop(calDesktop, prevDesktop);
            prevDesktop = null;
            calDesktop.Dispose();

            if (CalibrationComplete != null) CalibrationComplete();
            _inputTimeoutTimer.Change(_inputTimeoutMs, Timeout.Infinite);
        }

        void _inputManager_TouchDown(int x, int y)
        {
            //AddUiEvent(new UiEvent() { X = x, Y = y, Type = 0 });
            _currentDesktop.OnTouchDown(x, y);
            _inputTimeoutTimer.Change(_inputTimeoutMs, Timeout.Infinite);
        }

        void _inputManager_TouchMove(int x, int y)
        {
            //AddUiEvent(new UiEvent() { X = x, Y = y, Type = 1 });
            _currentDesktop.OnTouchMove(x, y);
            _inputTimeoutTimer.Change(_inputTimeoutMs, Timeout.Infinite);
        }

        void _inputManager_TouchUp(int x, int y)
        {
            //AddUiEvent(new UiEvent() { X = x, Y = y, Type = 2 });
            _currentDesktop.OnTouchUp(x, y);
            _inputTimeoutTimer.Change(_inputTimeoutMs, Timeout.Infinite);
        }
        #endregion

        #region constructors
        static DesktopManager()
        {
            // Get the screen dimensions
            HardwareProvider.HwProvider.GetLCDMetrics(out ScreenWidth, out ScreenHeight, out ScreenBitsPerPixel, out ScreenOrientationDeg);
        }

        internal DesktopManager()
        {
            _firstInputManagerSet = true;
            _desktops = new ArrayList();
            //_uiHandlerStarted = false;

            SwitchDesktop(AddDesktop(new Desktop()));
            AddDesktop(EditDesktop.Current);
            _inputTimeoutTimer = new Timer((o) => { if (InputTimeout != null) InputTimeout(); }, null, Timeout.Infinite, Timeout.Infinite);
            _inputTimeoutMs = Timeout.Infinite;
            //_uiConsumerWait = new AutoResetEvent(false);
            //_uiEvents = new Queue();
        }
        #endregion

        #region internal functionality
        internal Desktop AddDesktop(Desktop desktop)
        {
            if (_desktops.Contains(desktop)) return desktop;
            desktop.Visible = false;
            desktop.Suspended = true;
            _desktops.Add(desktop);
            return desktop;
        }
        #endregion

        #region private functionality
        //class UiEvent
        //{
        //    public int X, Y, Type;
        //}

        //[MethodImpl(MethodImplOptions.Synchronized)]
        //private void AddUiEvent(UiEvent e)
        //{
        //    _uiEvents.Enqueue(e);
        //    _uiConsumerWait.Set();
        //}

        //[MethodImpl(MethodImplOptions.Synchronized)]
        //private UiEvent GetUiEvent()
        //{
        //    return (_uiEvents.Count == 0 ? null : (UiEvent)_uiEvents.Dequeue());
        //}

        //private void HandleUiTask()
        //{
        //    while (true)
        //    {
        //        UiEvent next = GetUiEvent();
        //        if (next == null) _uiConsumerWait.WaitOne();
        //        else
        //        {
        //            try
        //            {
        //                switch (next.Type)
        //                {
        //                    case 0: _currentDesktop.OnTouchDown(next.X, next.Y); break;
        //                    case 1: _currentDesktop.OnTouchMove(next.X, next.Y); break;
        //                    case 2: _currentDesktop.OnTouchUp(next.X, next.Y); break;
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                Debug.Print("Unhandeled exception in UI thread:");
        //                Debug.Print(ex.Message);
        //                Debug.Print(ex.StackTrace);
        //            }
        //        }
        //    }
        //}
        #endregion

        #region public functionality
        //public void HandleUi(bool blockCurrentThread = true, ThreadPriority uiPriority = ThreadPriority.Normal)
        //{
        //    if (_uiHandlerStarted) return;
        //    _uiHandlerStarted = true;
        //    if (blockCurrentThread)
        //        HandleUiTask();
        //    new Thread(HandleUiTask) { Priority = uiPriority }.Start();
        //}
        public void SetInputTimeout(int timeoutMs)
        {
            _inputTimeoutMs = timeoutMs;
            _inputTimeoutTimer.Change(_inputTimeoutMs, Timeout.Infinite);
        }

        private Desktop prevDesktop = null;

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void StartCalibration()
        {
            _inputTimeoutTimer.Change(Timeout.Infinite, Timeout.Infinite);

            CalibrationDesktop calDesktop = new CalibrationDesktop(new TouchCalibrationPoints());
            calDesktop.CalibrationComplete += calDesktop_CalibrationComplete;
            calDesktop.StartCalibration();
            prevDesktop = _currentDesktop;
            AddDesktop(calDesktop);
            SwitchDesktop(calDesktop);
        }

        public Desktop AddNewDesktop()
        {
            return AddDesktop(new Desktop());
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public Desktop AddNewDesktopAndSwitchIt()
        {
            Desktop newDesktop = AddNewDesktop();
            SwitchDesktop(newDesktop);
            return newDesktop;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void RemoveDesktop(Desktop toRemove, Desktop toSwitch)
        {
            if (!_desktops.Contains(toRemove) || (toSwitch != null && !_desktops.Contains(toSwitch))) return;
            int index = _desktops.IndexOf(toRemove);

            if (index < _reservedDesktops) return;

            _desktops.RemoveAt(index);

            if (_currentDesktop == toRemove)
            {
                if (toSwitch == null)
                {
                    int newDesktopIndex = index - 1;
                    if (newDesktopIndex < _reservedDesktops) newDesktopIndex = 0;
                    SwitchDesktop((Desktop)_desktops[newDesktopIndex]);
                }
                else
                    SwitchDesktop(toSwitch);
            }
        }

        public void RemoveDesktop(Desktop desktop)
        {
            RemoveDesktop(desktop, null);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool SwitchDesktop(Desktop desktop)
        {
            if (_currentDesktop != null)
            {
                _currentDesktop.Visible = false;
                _currentDesktop.Suspended = true;
                _currentDesktop.ReleaseTouchCapture();
            }

            Desktop lastDesktop = _currentDesktop;
            _currentDesktop = desktop;
            _currentDesktop.Visible = true;
            _currentDesktop.Suspended = false;

            return true;
        }
        #endregion
    }
}
