using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

using Mp.Ui.Controls;
using Mp.Ui.Controls.Containers;
using Mp.Ui.Controls.TouchControls;
using Mp.Ui.Controls.TouchControls.Buttons;
using Mp.Ui.Managers;
using Mp.Ui.Primitives;

namespace Mp.Ui.Desktops
{
    public sealed class EditDesktop : Desktop
    {
        #region enums
        [Flags]
        private enum KeyFlags : int
        {
            KeyMask = 0xFFF,
            TypeMask = 0xF000,

            NormalKey = 0x1000,
            CapsLock = 0x2000,
            BackSpace = 0x3000,
            ModeSwitch = 0x4000,
            Enter = 0x5000,
        }
        #endregion

        #region constants
        private const string charsAlphaLower = "abcdefghijklmnopqrstuvwxyz";
        private const string charsAlphaUpper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string charsNumeric = "0123456789";
        private const string charsSymbols = "@$%&*-_()!\"':;/?., ";
        private const string charsSymbolsEmail = ".-_@";
        private const string charsSymbolsIp = ".";

        private const int _charSets = 3;
        private const int _uiMargin = 1;
        #endregion

        #region static members/properties
        private static EditDesktop _instance;
        public static EditDesktop Current { get { return _instance ?? (_instance = new EditDesktop()); } }

        private static string[][] Keys = new string[3][] 
        {
            new string[_charSets] 
            { 
                "qwertyuiop", 
                "QWERTYUIOP",
                "1234567890",
            },
            new string[_charSets] 
            { 
                "asdfghjkl", 
                "ASDFGHJKL",
                "@$%&*-+()",
            },
            new string[_charSets] 
            { 
                "zxcvbnm", 
                "ZXCVBNM",
                "!\"':;/?",
            }
        };

        private static string[] NumericKeys = new string[]
        {
            "123",
            "456",
            "789",
            ".0-",
        };
        #endregion

        #region private members
        private Desktop _lastDesktop;
        private Panel _qwertyPanel;
        private Panel _numericPanel;

        private EditableTextBox _editTextbox;
        private Button _enterButton, _doneButton;
        private TextButton _capsButton;
        private ImageButton _leftBut, _rightBut, _clearBut;
        private Label _editLabel;
        private string _initialText;

        private bool _shiftLocked = false;
        private bool _shiftActive = false;
        private bool _allowCloseOnlyWhenValid = true;
        private bool _ignoreAllowedCharsChanges;

        private int _charSetUsed = 0;

        private AllowedCharTypesEnum _allowedCharTypes;
        private string _allowedChars;

        private Bitmap _bmpEnter, _bmpBackspace;
        #endregion

        #region public properties
        public override bool Enabled
        {
            get { return base.Enabled; }
            set { throw new NotSupportedException("Can not modify the Edit desktop"); }
        }

        public bool AllowMultiLine
        {
            get { return _enterButton.Enabled; }
            set { _enterButton.Enabled = value; }
        }

        public string Text
        {
            get { return _editTextbox.Text; }
            set
            {
                _initialText = value;
                _editTextbox.Text = value;
                RemakeButtonsEnabled();
            }
        }

        public StringValidator TextValidator
        {
            get { return _editTextbox.Validator; }
            set
            {
                _editTextbox.Validator = value;
                RemakeButtonsEnabled();
            }
        }

        public bool TextValid
        {
            get { return _editTextbox.Valid; }
        }

        public bool AllowCloseOnlyWhenValid
        {
            get { return _allowCloseOnlyWhenValid; }
            set
            {
                _allowCloseOnlyWhenValid = value;
                RemakeButtonsEnabled();
            }
        }

        public AllowedCharTypesEnum AllowedCharTypes
        {
            get { return _allowedCharTypes; }
            set
            {
                if (_allowedCharTypes == value) return;
                _allowedCharTypes = value;

                string allowedChars = string.Empty;

                if (((byte)_allowedCharTypes & (byte)AllowedCharTypesEnum.AlphaLower) != 0)
                    allowedChars += charsAlphaLower;

                if (((byte)_allowedCharTypes & (byte)AllowedCharTypesEnum.AlphaUpper) != 0)
                    allowedChars += charsAlphaUpper;

                if (((byte)_allowedCharTypes & (byte)AllowedCharTypesEnum.Numeric) != 0)
                    allowedChars += charsNumeric;

                if (((byte)_allowedCharTypes & (byte)AllowedCharTypesEnum.Symbols) != 0)
                    allowedChars += charsSymbols;
                else
                {
                    if (((byte)_allowedCharTypes & (byte)AllowedCharTypesEnum.SymbolsEmail) != 0)
                        allowedChars += charsSymbolsEmail;

                    if (((byte)_allowedCharTypes & (byte)AllowedCharTypesEnum.SymbolsIp) != 0)
                        allowedChars += charsSymbolsIp;
                }

                AllowedChars = allowedChars;
            }
        }

        public string AllowedChars
        {
            get { return _allowedChars; }
            private set
            {
                if (value == _allowedChars) return;
                _allowedChars = value;

                if (_ignoreAllowedCharsChanges) return;
                RemakeAllowedChars(_qwertyPanel);
                RemakeAllowedChars(_numericPanel);
            }
        }

        public string EditLabelText
        {
            get { return _editLabel.Text; }
            set
            {
                string oldValue = _editLabel.Text;
                if (value == null) value = string.Empty;
                _editLabel.Text = value;
                if (oldValue.Length == 0 && value.Length != 0)
                {
                    _editTextbox.Y += _editLabel.Height + _uiMargin;
                    _editTextbox.Height -= _editLabel.Height + _uiMargin;
                }
                else if (oldValue.Length != 0 && value.Length == 0)
                {
                    _editTextbox.Y -= _editLabel.Height + _uiMargin;
                    _editTextbox.Height += _editLabel.Height + _uiMargin;
                }
            }
        }
        #endregion

        #region events
        public event UiEventHandler Closed;
        #endregion

        #region private functionality
        private void ChangeQwertyLayout(Container p, int newMode)
        {
            p.Suspended = true;
            foreach (Control key in p.Childs)
            {
                if (!(key is TextButton) || key.Tag == null) continue;

                TextButton but = (TextButton)key;
                KeyFlags flags = (KeyFlags)((int)but.Tag & (int)KeyFlags.TypeMask);
                if (flags != KeyFlags.NormalKey) continue;

                char keyChar = (char)((int)but.Tag & (int)KeyFlags.KeyMask);

                int index = 0, row = 0;
                for (int i = 0; i < 3; i++)
                {
                    if ((index = Keys[i][_charSetUsed].IndexOf(keyChar)) != -1)
                    {
                        row = i;
                        break;
                    }
                }
                if (index == -1) continue;

                char newChar = Keys[row][newMode][index];
                but.Text = newChar.ToString();
                but.Tag = (int)KeyFlags.NormalKey | ((int)newChar & (int)KeyFlags.KeyMask);
                but.Enabled = _allowedChars.IndexOf(newChar) >= 0;
            }
            _capsButton.Enabled = newMode < 2;
            _charSetUsed = newMode;
            p.Suspended = false;
        }

        private void CreateCommonLayout()
        {
            string keyLine = Keys[0][_charSetUsed];
            int keyWidth = (_width - (keyLine.Length + 1) * _uiMargin) / keyLine.Length;
            int keyHeight = (_height / 2 - 5 * _uiMargin) / 4;

            int widthRemaining = _width - ((keyLine.Length + 1) * _uiMargin + keyLine.Length * keyWidth);

            int x = _uiMargin + widthRemaining / 2,
                y = _uiMargin;

            _editLabel = new Label(string.Empty, x, y, _width - 2 * _uiMargin - widthRemaining, StyleManager.CurrentStyle.LabelFont.Height);
            base.AddChild(_editLabel);

            _editTextbox = new EditableTextBox(string.Empty, x, y, _editLabel._width, _height / 4 - 2 * _uiMargin);
            base.AddChild(_editTextbox);

            #region done/cancel buttons
            x = _editTextbox.ScreenRight - 2 * keyWidth - _uiMargin;
            y = _editTextbox.ScreenBottom + _uiMargin;

            _doneButton = new TextButton("Done", x, y, 2 * keyWidth + _uiMargin, keyHeight);
            _doneButton.ButtonPressed += (sender) => { Close(); };
            base.AddChild(_doneButton);

            x -= keyWidth * 2 + _uiMargin * 2;
            TextButton _cancelButton = new TextButton("Cancel", x, y, keyWidth * 2 + _uiMargin, keyHeight);
            _cancelButton.ButtonPressed += (sender) =>
            {
                _editTextbox.Text = _initialText;
                if (_allowCloseOnlyWhenValid)
                {
                    if (_editTextbox.Valid)
                        Close();
                }
                else Close();
            };
            base.AddChild(_cancelButton);
            #endregion

            #region left/right/clear buttons
            Color transparentColor = ColorUtility.ColorFromRGB(255, 0, 255);
            Bitmap leftImg = Resources.Images.GetBitmap(Resources.Images.BitmapResources.arrowLeft);

            x = _uiMargin + widthRemaining / 2;
            y = _editTextbox.Height + 2 * _uiMargin;

            leftImg.MakeTransparent(transparentColor);
            _leftBut = new ImageButton(leftImg, x, y, keyHeight, keyHeight);
            _leftBut.RepeatKeyPress = true;
            _leftBut.ButtonPressed += (sender) =>
            {
                if (_editTextbox.CarretPosition != 0)
                {
                    _editTextbox.CarretPosition--;
                    RemakeButtonsEnabled();
                }
            };
            _leftBut.Enabled = false;
            x += _leftBut.Width + _uiMargin;
            base.AddChild(_leftBut);

            Bitmap rightImg = Resources.Images.GetBitmap(Resources.Images.BitmapResources.arrowRight);
            rightImg.MakeTransparent(transparentColor);
            _rightBut = new ImageButton(rightImg, x, y, keyHeight, keyHeight);
            _rightBut.RepeatKeyPress = true;
            _rightBut.ButtonPressed += (sender) =>
            {
                if (_editTextbox.CarretPosition != _editTextbox.Text.Length)
                {
                    _editTextbox.CarretPosition++;
                    RemakeButtonsEnabled();
                }
            };
            _rightBut.Enabled = false;
            x += _rightBut.Width + _uiMargin;
            base.AddChild(_rightBut);

            Bitmap clearImg = Resources.Images.GetBitmap(Resources.Images.BitmapResources.clear);
            clearImg.MakeTransparent(transparentColor);
            _clearBut = new ImageButton(clearImg, x, y, keyHeight, keyHeight);
            _clearBut.ButtonPressed += (sender) =>
            {
                if (_editTextbox.Text.Length != 0)
                {
                    _editTextbox.Text = string.Empty;
                    RemakeButtonsEnabled();
                }
            };
            _clearBut.Enabled = false;
            base.AddChild(_clearBut);
            #endregion
        }

        private void CreateQwertyLayout(Container p)
        {
            string keyLine = Keys[0][_charSetUsed];

            int keyWidth = (p._width - (keyLine.Length + 1) * _uiMargin) / keyLine.Length;
            int keyHeight = (p._height - 5 * _uiMargin) / 4;

            int widthRemaining = p._width - ((keyLine.Length + 1) * _uiMargin + keyLine.Length * keyWidth);

            int x, y;

            #region 1st row keys
            x = _uiMargin + widthRemaining / 2;
            y = 0;
            for (int i = 0; i < keyLine.Length; i++)
            {
                TextButton keyBut = new TextButton(keyLine[i].ToString(), x, y, keyWidth, keyHeight) { Tag = (int)KeyFlags.NormalKey | ((int)keyLine[i] & (int)KeyFlags.KeyMask) };
                keyBut.ButtonPressed += _key_ButtonPressed;
                p.AddChild(keyBut);
                x += _uiMargin + keyWidth;
            }
            #endregion

            #region 2nd row keys
            x = _uiMargin + keyWidth / 2 + widthRemaining / 2;
            y += _uiMargin + keyHeight;
            keyLine = Keys[1][_charSetUsed];
            for (int i = 0; i < keyLine.Length; i++)
            {
                TextButton keyBut = new TextButton(keyLine[i].ToString(), x, y, keyWidth, keyHeight) { Tag = (int)KeyFlags.NormalKey | ((int)keyLine[i] & (int)KeyFlags.KeyMask) };
                keyBut.ButtonPressed += _key_ButtonPressed;
                p.AddChild(keyBut);
                x += _uiMargin + keyWidth;
            }
            #endregion

            #region 3rd row keys
            x = _uiMargin + widthRemaining / 2;
            y += _uiMargin + keyHeight;

            _capsButton = new TextButton("abc", x, y, keyWidth + keyWidth / 2, keyHeight) { Tag = (int)KeyFlags.CapsLock };
            _capsButton.ButtonPressed += _key_ButtonPressed;
            p.AddChild(_capsButton);
            x += _uiMargin + _capsButton._width;

            keyLine = Keys[2][_charSetUsed];
            for (int i = 0; i < keyLine.Length; i++)
            {
                TextButton keyBut = new TextButton(keyLine[i].ToString(), x, y, keyWidth, keyHeight) { Tag = (int)KeyFlags.NormalKey | ((int)keyLine[i] & (int)KeyFlags.KeyMask) };
                keyBut.ButtonPressed += _key_ButtonPressed;
                p.AddChild(keyBut);
                x += _uiMargin + keyWidth;
            }

            ImageButton backBut = new ImageButton(_bmpBackspace, x, y, keyWidth + keyWidth / 2, keyHeight) { Tag = (int)KeyFlags.BackSpace };
            backBut.RepeatKeyPress = true;
            backBut.ButtonPressed += _key_ButtonPressed;
            p.AddChild(backBut);
            #endregion

            #region 4th row keys
            x = _uiMargin + widthRemaining / 2;
            y += _uiMargin + keyHeight;

            TextButton modeSwitch = new TextButton("123?", x, y, keyWidth + keyWidth / 2, keyHeight) { Tag = (int)KeyFlags.ModeSwitch };
            modeSwitch.ButtonPressed += _key_ButtonPressed;
            modeSwitch.ButtonLongPressed += _keyModeSwitch_ButtonPressed;

            p.AddChild(modeSwitch);
            x += modeSwitch._width + _uiMargin;

            TextButton slashButton = new TextButton(",", x, y, keyWidth, keyHeight) { Tag = (int)KeyFlags.NormalKey | ((int)',' & (int)KeyFlags.KeyMask) };
            slashButton.ButtonPressed += _key_ButtonPressed;
            p.AddChild(slashButton);
            x += slashButton._width + _uiMargin;

            TextButton spaceButton = new TextButton("________", x, y, keyWidth * 4 + _uiMargin * 4 + keyWidth / 2, keyHeight) { Tag = (int)KeyFlags.NormalKey | ((int)' ' & (int)KeyFlags.KeyMask) };
            spaceButton.ButtonPressed += _key_ButtonPressed;
            p.AddChild(spaceButton);
            x += spaceButton._width + _uiMargin;

            TextButton pointButton = new TextButton(".", x, y, keyWidth, keyHeight) { Tag = (int)KeyFlags.NormalKey | ((int)'.' & (int)KeyFlags.KeyMask) };
            pointButton.ButtonPressed += _key_ButtonPressed;
            p.AddChild(pointButton);
            x += pointButton._width + _uiMargin;

            _enterButton = new ImageButton(_bmpEnter, x, y, keyWidth * 2, keyHeight) { Tag = (int)KeyFlags.Enter };
            _enterButton.ButtonPressed += _key_ButtonPressed;
            p.AddChild(_enterButton);
            #endregion
        }

        private void CreateNumericLayout(Container p)
        {
            string keyLine = NumericKeys[0];

            int keyWidth = (p._width - 20 - (keyLine.Length + 2) * _uiMargin) / (keyLine.Length + 1);
            int keyHeight = (p._height - (NumericKeys.Length + 1) * _uiMargin) / NumericKeys.Length;

            int widthRemaining = p._width - ((keyLine.Length + 2) * _uiMargin + (keyLine.Length + 1) * keyWidth);

            int x, y;

            x = _uiMargin;
            y = 0;
            for (int i = 0; i < NumericKeys.Length; i++)
            {
                x = _uiMargin + widthRemaining / 2;
                string line = NumericKeys[i];
                for (int j = 0; j < line.Length; j++)
                {
                    TextButton numBut = new TextButton(line[j].ToString(), x, y, keyWidth, keyHeight) { Tag = (int)KeyFlags.NormalKey | ((int)line[j] & (int)KeyFlags.KeyMask) }; ;
                    numBut.ButtonPressed += _key_ButtonPressed;
                    p.AddChild(numBut);
                    x += _uiMargin + keyWidth;
                }
                y += keyHeight + _uiMargin;
            }

            y = 0;
            ImageButton backBut = new ImageButton(_bmpBackspace, x, y, keyWidth, keyHeight) { Tag = (int)KeyFlags.BackSpace };
            backBut.RepeatKeyPress = true;
            backBut.ButtonPressed += _key_ButtonPressed;
            p.AddChild(backBut);
            y += backBut._height + _uiMargin;

            TextButton modeSwitch = new TextButton("Mode", x, y, keyWidth, keyHeight) { Tag = (int)KeyFlags.ModeSwitch };
            modeSwitch.ButtonPressed += _keyModeSwitch_ButtonPressed;
            p.AddChild(modeSwitch);
            y += modeSwitch._height + _uiMargin;
        }

        private void RemakeButtonsEnabled()
        {
            if (_editTextbox.CarretPosition == 0)
            { if (_leftBut.Enabled) _leftBut.Enabled = false; }
            else
            { if (!_leftBut.Enabled) _leftBut.Enabled = true; }

            if (_editTextbox.CarretPosition == _editTextbox.Text.Length)
            { if (_rightBut.Enabled) _rightBut.Enabled = false; }
            else
            { if (!_rightBut.Enabled) _rightBut.Enabled = true; }

            if (_allowCloseOnlyWhenValid)
                if (_editTextbox.Valid)
                { if (!_doneButton.Enabled) _doneButton.Enabled = true; }
                else
                { if (_doneButton.Enabled) _doneButton.Enabled = false; }
            else
            { if (!_doneButton.Enabled) _doneButton.Enabled = true; }

            if (_editTextbox.Text.Length == 0)
            { if (_clearBut.Enabled) _clearBut.Enabled = false; }
            else
            { if (!_clearBut.Enabled) _clearBut.Enabled = true; }
        }

        private void RemakeAllowedChars(Container p)
        {
            p.Suspended = true;
            foreach (Control key in p.Childs)
            {
                if (!(key is TextButton) || key.Tag == null) continue;

                TextButton but = (TextButton)key;
                KeyFlags flags = (KeyFlags)((int)but.Tag & (int)KeyFlags.TypeMask);
                if (flags != KeyFlags.NormalKey) continue;

                char keyChar = (char)((int)but.Tag & (int)KeyFlags.KeyMask);

                but.Enabled = _allowedChars.IndexOf(keyChar) >= 0;
            }
            p.Suspended = false;
        }
        #endregion

        #region event handlers
        void _key_ButtonPressed(Control but)
        {
            KeyFlags flags = (KeyFlags)((int)but.Tag & (int)KeyFlags.TypeMask);
            char key = (char)((int)but.Tag & (int)KeyFlags.KeyMask);
            switch (flags)
            {
                case KeyFlags.Enter:
                    _editTextbox.InsertAtCaret("\n");
                    RemakeButtonsEnabled();
                    break;

                case KeyFlags.NormalKey:
                    _editTextbox.InsertAtCaret(_shiftActive ? key.ToString().ToUpper() : key.ToString().ToLower());
                    if (_shiftActive && !_shiftLocked)
                    {
                        _shiftActive = false;
                        int newShiftMode = _shiftActive ? 1 : 0;
                        if (newShiftMode != _charSetUsed)
                        {
                            ChangeQwertyLayout(_qwertyPanel, newShiftMode);
                            _charSetUsed = newShiftMode;
                        }
                        _capsButton.Text = _shiftActive ? (_shiftLocked ? "ABC" : "Abc") : "abc";
                    }

                    RemakeButtonsEnabled();
                    break;

                case KeyFlags.BackSpace:
                    _editTextbox.BackspaceAtCarret();
                    RemakeButtonsEnabled();
                    break;

                case KeyFlags.ModeSwitch:
                    int newMode = _charSetUsed < 2 ? 2 : _charSetUsed + 1;
                    if (newMode == _charSets) newMode = _shiftActive ? 1 : 0;
                    ChangeQwertyLayout(_qwertyPanel, newMode);
                    _charSetUsed = newMode;
                    break;

                case KeyFlags.CapsLock:
                    if (!_shiftActive)
                        _shiftActive = true;
                    else if (!_shiftLocked)
                        _shiftLocked = true;
                    else
                        _shiftActive = _shiftLocked = false;

                    int newCapsMode = _shiftActive ? 1 : 0;
                    if (newCapsMode != _charSetUsed)
                    {
                        ChangeQwertyLayout(_qwertyPanel, newCapsMode);
                        _charSetUsed = newCapsMode;
                    }
                    _capsButton.Text = _shiftActive ? (_shiftLocked ? "ABC" : "Abc") : "abc";
                    break;
            }
        }

        void _keyModeSwitch_ButtonPressed(Control sender)
        {
            Suspended = true;
            _qwertyPanel.Visible = !_qwertyPanel.Visible;
            _numericPanel.Visible = !_numericPanel.Visible;
            Suspended = false;
        }
        #endregion

        #region public functionality
        public void Open()
        {
            if (DesktopManager.Instance.CurrentDesktop == this) return;
            _lastDesktop = DesktopManager.Instance.CurrentDesktop;
            DesktopManager.Instance.SwitchDesktop(this);
        }

        public void Close()
        {
            if (_lastDesktop == null) return;
            DesktopManager.Instance.SwitchDesktop(_lastDesktop);
            if (Closed != null) Closed(this);
        }

        public override void AddChild(Control control)
        { throw new NotSupportedException("Can not modify the desktop"); }
        public override void ClearChilds()
        { throw new NotSupportedException("Can not modify the desktop"); }
        public override Control FindChild(int id)
        { throw new NotSupportedException("Can not modify the desktop"); }
        public override void RemoveChild(Control control)
        { throw new NotSupportedException("Can not modify the desktop"); }
        #endregion

        #region constructor
        private EditDesktop()
        {
            _lastDesktop = null;

            Suspended = true;

            _bmpEnter = Mp.Ui.Resources.Images.GetBitmap(Resources.Images.BitmapResources.enter);
            _bmpEnter.MakeTransparent(Colors.Fuchsia);
            Utils.ChangeBitmapColor(_bmpEnter, Colors.Fuchsia, StyleManager.CurrentStyle.ButtonEnabledTextColor);

            _bmpBackspace = Mp.Ui.Resources.Images.GetBitmap(Resources.Images.BitmapResources.backspace);
            _bmpBackspace.MakeTransparent(Colors.Fuchsia);
            Utils.ChangeBitmapColor(_bmpBackspace, Colors.Fuchsia, StyleManager.CurrentStyle.ButtonEnabledTextColor);

            StyleManager.StyleChanged += (oldStyle, newStyle) =>
            {
                Utils.ChangeBitmapColor(_bmpEnter, Colors.Fuchsia, newStyle.ButtonEnabledTextColor);
                Utils.ChangeBitmapColor(_bmpBackspace, Colors.Fuchsia, newStyle.ButtonEnabledTextColor);
            };

            CreateCommonLayout();

            int panelHeight = _height - _doneButton.ScreenBottom - _uiMargin;

            CreateQwertyLayout(_qwertyPanel = new Panel(0, _height - panelHeight, _width, panelHeight));
            CreateNumericLayout(_numericPanel = new Panel(0, _height - panelHeight, _width, panelHeight) { _visible = false });

            base.AddChild(_qwertyPanel);
            base.AddChild(_numericPanel);

            _ignoreAllowedCharsChanges = true;
            AllowedCharTypes = AllowedCharTypesEnum.All;
            _ignoreAllowedCharsChanges = false;
        }
        #endregion
    }

    [Flags]
    public enum AllowedCharTypesEnum : byte
    {
        AlphaLower = 0x01,
        AlphaUpper = 0x02,
        Numeric = 0x04,
        Symbols = 0x08,
        SymbolsEmail = 0x10,
        SymbolsIp = 0x20,

        All = AlphaLower | AlphaUpper | Numeric | Symbols,
        Email = AlphaLower | AlphaUpper | Numeric | SymbolsEmail,
        Ip = Numeric | SymbolsIp,
    }
}
