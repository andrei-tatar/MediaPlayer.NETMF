using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

namespace Mp.Ui.Styles
{
    public class StyleMetroLight : Style
    {
        #region private static members
        private static Color _backgroundColor = ColorUtility.ColorFromRGB(245, 245, 245);
        private static Color _backgroundBoderColor = ColorUtility.ColorFromRGB(165, 165, 165);

        #region Buttons
        private static Color _buttonPressedBorder = ColorUtility.ColorFromRGB(155, 155, 155);
        private static Color _buttonPressedBack = ColorUtility.ColorFromRGB(35, 35, 35);
        private static Color _buttonPressedTextColor = ColorUtility.ColorFromRGB(155, 155, 155);

        private static Color _buttonEnabledBack = ColorUtility.ColorFromRGB(207, 208, 200);
        private static Color _buttonEnabledTextColor = ColorUtility.ColorFromRGB(42, 42, 42);

        private static Color _buttonDisabledBack = ColorUtility.ColorFromRGB(226, 227, 222);
        private static Color _buttonDisabledTextColor = ColorUtility.ColorFromRGB(142, 142, 142);
        #endregion

        #region TextBoxes
        private static Color _textBoxPressedBack = ColorUtility.ColorFromRGB(135, 135, 135);
        private static Color _textBoxPressedTextColor = ColorUtility.ColorFromRGB(225, 225, 225);

        private static Color _textBoxEnabledBack = ColorUtility.ColorFromRGB(50, 50, 50);
        private static Color _textBoxEnabledTextColor = ColorUtility.ColorFromRGB(195, 195, 195);

        private static Color _textBoxEnabledInvalidBorder = Colors.Red;
        private static Color _textBoxEnabledInvalidBack = Colors.GetMedianColor(_textBoxEnabledBack, Colors.Red);

        private static Color _textBoxDisabledBack = ColorUtility.ColorFromRGB(226, 227, 222);
        private static Color _textBoxDisabledTextColor = ColorUtility.ColorFromRGB(142, 142, 142);
        #endregion

        #region Label
        private static Color _labelEnabledColor = ColorUtility.ColorFromRGB(42, 42, 42);
        private static Color _labelDisabledColor = ColorUtility.ColorFromRGB(142, 142, 142);
        #endregion

        #region CheckBox
        private static Color _checkBoxEnabledBack = ColorUtility.ColorFromRGB(50, 50, 50);
        private static Color _checkBoxEnabledCross = ColorUtility.ColorFromRGB(222, 222, 222);

        private static Color _checkBoxDisabledBack = Colors.GetMedianColor(_backgroundColor, _checkBoxEnabledBack);
        private static Color _checkBoxDisabledCross = Colors.GetMedianColor(_backgroundColor, _checkBoxEnabledCross);

        private static Color _checkBoxPressedBack = ColorUtility.ColorFromRGB(65, 65, 65);
        private static Color _checkBoxPressedCross = Colors.GetMedianColor(_checkBoxEnabledCross, _checkBoxEnabledBack);

        private static Color _checkBoxBorder = Colors.GetMedianColor(_checkBoxEnabledBack, _backgroundColor);
        #endregion

        #region ProgressBar
        private static Color _progressBarEnabledBack1 = ColorUtility.ColorFromRGB(166, 166, 166);
        private static Color _progressBarDisabledBack1 = Colors.GetMedianColor(_backgroundColor, _progressBarEnabledBack1);
        private static Color _progressBarBack = ColorUtility.ColorFromRGB(31, 204, 255);
        #endregion

        #region ListBox
        private static Color _listBoxScrollColor = Colors.GetMedianColor(_progressBarBack, _backgroundColor);
        #endregion

        #endregion

        #region public static properties
        public override Color BackgroundColor { get { return _backgroundColor; } }
        public override Color BackgroundBorderColor { get { return _backgroundBoderColor; } }

        #region Buttons
        public override Font ButtonFont { get { return Fonts.Arial; } }

        public override Color ButtonPressedBorder { get { return _buttonPressedBorder; } }
        public override Color ButtonPressedBack1 { get { return _buttonPressedBack; } }
        public override Color ButtonPressedBack2 { get { return _buttonPressedBack; } }
        public override Color ButtonPressedTextColor { get { return _buttonPressedTextColor; } }

        public override Color ButtonEnabledBorder { get { return _buttonEnabledBack; } }
        public override Color ButtonEnabledBack1 { get { return _buttonEnabledBack; } }
        public override Color ButtonEnabledBack2 { get { return _buttonEnabledBack; } }
        public override Color ButtonEnabledTextColor { get { return _buttonEnabledTextColor; } }

        public override Color ButtonDisabledBorder { get { return _buttonDisabledBack; } }
        public override Color ButtonDisabledBack1 { get { return _buttonDisabledBack; } }
        public override Color ButtonDisabledBack2 { get { return _buttonDisabledBack; } }
        public override Color ButtonDisabledTextColor { get { return _buttonDisabledTextColor; } }
        #endregion

        #region TextBoxes
        public override Font TextBoxFont { get { return Fonts.Arial; } }

        public override Color TextBoxPressedBorder { get { return _textBoxPressedBack; } }
        public override Color TextBoxPressedBack1 { get { return _textBoxPressedBack; } }
        public override Color TextBoxPressedBack2 { get { return _textBoxPressedBack; } }
        public override Color TextBoxPressedTextColor { get { return _textBoxPressedTextColor; } }

        public override Color TextBoxEnabledInvalidBorder { get { return _textBoxEnabledInvalidBorder; } }
        public override Color TextBoxEnabledInvalidBack { get { return _textBoxEnabledInvalidBack; } }
        public override Color TextBoxEnabledBorder { get { return _textBoxEnabledBack; } }
        public override Color TextBoxEnabledBack1 { get { return _textBoxEnabledBack; } }
        public override Color TextBoxEnabledBack2 { get { return _textBoxEnabledBack; } }
        public override Color TextBoxEnabledTextColor { get { return _textBoxEnabledTextColor; } }

        public override Color TextBoxDisabledBorder { get { return _textBoxDisabledBack; } }
        public override Color TextBoxDisabledBack1 { get { return _textBoxDisabledBack; } }
        public override Color TextBoxDisabledBack2 { get { return _textBoxDisabledBack; } }
        public override Color TextBoxDisabledTextColor { get { return _textBoxDisabledTextColor; } }
        #endregion

        #region Label
        public override Font LabelFont { get { return Fonts.Arial; } }
        public override Color LabelEnabledColor { get { return _labelEnabledColor; } }
        public override Color LabelDisabledColor { get { return _labelDisabledColor; } }
        #endregion

        #region CheckBox
        public override Color CheckBoxEnabledBorder { get { return _checkBoxBorder; } }
        public override Color CheckBoxEnabledBack1 { get { return _checkBoxEnabledBack; } }
        public override Color CheckBoxEnabledBack2 { get { return _checkBoxEnabledBack; } }
        public override Color CheckBoxEnabledCross { get { return _checkBoxEnabledCross; } }

        public override Color CheckBoxDisabledBorder { get { return _checkBoxBorder; } }
        public override Color CheckBoxDisabledBack1 { get { return _checkBoxDisabledBack; } }
        public override Color CheckBoxDisabledBack2 { get { return _checkBoxDisabledBack; } }
        public override Color CheckBoxDisabledCross { get { return _checkBoxDisabledCross; } }

        public override Color CheckBoxPressedBorder { get { return _checkBoxBorder; } }
        public override Color CheckBoxPressedBack1 { get { return _checkBoxPressedBack; } }
        public override Color CheckBoxPressedBack2 { get { return _checkBoxPressedBack; } }
        public override Color CheckBoxPressedCross { get { return _checkBoxPressedCross; } }
        #endregion

        #region RadioButton
        public override Color RadioButtonEnabledBorder { get { return _checkBoxBorder; } }
        public override Color RadioButtonEnabledBack { get { return _checkBoxEnabledBack; } }
        public override Color RadioButtonEnabledPoint { get { return _checkBoxEnabledCross; } }

        public override Color RadioButtonDisabledBorder { get { return _checkBoxBorder; } }
        public override Color RadioButtonDisabledBack { get { return _checkBoxDisabledBack; } }
        public override Color RadioButtonDisabledPoint { get { return _checkBoxDisabledCross; } }

        public override Color RadioButtonPressedBorder { get { return _checkBoxBorder; } }
        public override Color RadioButtonPressedBack { get { return _checkBoxPressedBack; } }
        public override Color RadioButtonPressedPoint { get { return _checkBoxPressedCross; } }
        #endregion

        #region ComboBox
        public override Font ComboBoxFont { get { return Fonts.Arial; } }

        public override Color ComboBoxPressedBorder { get { return _textBoxPressedBack; } }
        public override Color ComboBoxPressedBack1 { get { return _textBoxPressedBack; } }
        public override Color ComboBoxPressedBack2 { get { return _textBoxPressedBack; } }
        public override Color ComboBoxPressedTextColor { get { return _textBoxPressedTextColor; } }

        public override Color ComboBoxEnabledBorder { get { return _textBoxEnabledBack; } }
        public override Color ComboBoxEnabledBack1 { get { return _textBoxEnabledBack; } }
        public override Color ComboBoxEnabledBack2 { get { return _textBoxEnabledBack; } }
        public override Color ComboBoxEnabledTextColor { get { return _textBoxEnabledTextColor; } }

        public override Color ComboBoxDisabledBorder { get { return _textBoxDisabledBack; } }
        public override Color ComboBoxDisabledBack1 { get { return _textBoxDisabledBack; } }
        public override Color ComboBoxDisabledBack2 { get { return _textBoxDisabledBack; } }
        public override Color ComboBoxDisabledTextColor { get { return _textBoxDisabledTextColor; } }
        #endregion

        #region ListBox
        public override Color ListBoxEnabledBack1 { get { return _textBoxEnabledBack; } }
        public override Color ListBoxEnabledBack2 { get { return _textBoxEnabledBack; } }
        public override Color ListBoxEnabledBorder { get { return _textBoxEnabledBack; } }

        public override Color ListBoxDisabledBack1 { get { return _textBoxDisabledBack; } }
        public override Color ListBoxDisabledBack2 { get { return _textBoxDisabledBack; } }
        public override Color ListBoxDisabledBorder { get { return _textBoxDisabledBack; } }

        public override Color ListBoxSelectedItemBack1 { get { return _textBoxPressedBack; } }
        public override Color ListBoxSelectedItemBack2 { get { return _textBoxPressedBack; } }
        public override Color ListBoxScrollBarColor { get { return _listBoxScrollColor; } }
        #endregion

        #region ProgressBar
        public override Color ProgressBarEnabledBack1 { get { return _progressBarEnabledBack1; } }
        public override Color ProgressBarEnabledBack2 { get { return _progressBarEnabledBack1; } }
        public override Color ProgressBarEnabledBorder { get { return _progressBarEnabledBack1; } }

        public override Color ProgressBarDisabledBack1 { get { return _progressBarDisabledBack1; } }
        public override Color ProgressBarDisabledBack2 { get { return _progressBarDisabledBack1; } }
        public override Color ProgressBarDisabledBorder { get { return _progressBarDisabledBack1; } }

        public override Color ProgressBarBack1 { get { return _progressBarBack; } }
        public override Color ProgressBarBack2 { get { return _progressBarBack; } }
        #endregion

        #endregion


    }
}
