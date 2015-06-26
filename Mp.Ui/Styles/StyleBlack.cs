using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

namespace Mp.Ui.Styles
{
    public class StyleDark : Style
    {
        #region private static members
        private static Color _backgroundColor = ColorUtility.ColorFromRGB(10, 10, 20);
        private static Color _backgroundBoderColor = Colors.Gray;

        #region Buttons
        private static Color _buttonPressedBorder = ColorUtility.ColorFromRGB(100, 100, 100);
        private static Color _buttonPressedBack1 = ColorUtility.ColorFromRGB(220, 220, 220);
        private static Color _buttonPressedBack2 = ColorUtility.ColorFromRGB(220, 220, 220);
        private static Color _buttonPressedTextColor = ColorUtility.ColorFromRGB(100, 100, 100);

        private static Color _buttonEnabledBorder = ColorUtility.ColorFromRGB(0, 0, 0);
        private static Color _buttonEnabledBack1 = ColorUtility.ColorFromRGB(48, 47, 55);
        private static Color _buttonEnabledBack2 = ColorUtility.ColorFromRGB(48, 47, 55);
        private static Color _buttonEnabledTextColor = ColorUtility.ColorFromRGB(213, 213, 213);

        private static Color _buttonDisabledBorder = ColorUtility.ColorFromRGB(0, 0, 0);
        private static Color _buttonDisabledBack1 = ColorUtility.ColorFromRGB(29, 28, 33);
        private static Color _buttonDisabledBack2 = ColorUtility.ColorFromRGB(29, 28, 33);
        private static Color _buttonDisabledTextColor = ColorUtility.ColorFromRGB(113, 113, 113);
        #endregion

        #region TextBoxes
        private static Color _textBoxPressedBorder = ColorUtility.ColorFromRGB(100, 100, 100);
        private static Color _textBoxPressedBack1 = ColorUtility.ColorFromRGB(220, 220, 220);
        private static Color _textBoxPressedBack2 = ColorUtility.ColorFromRGB(220, 220, 220);
        private static Color _textBoxPressedTextColor = ColorUtility.ColorFromRGB(100, 100, 100);

        private static Color _textBoxEnabledInvalidBorder = ColorUtility.ColorFromRGB(255, 0, 0);
        private static Color _textBoxEnabledInvalidBack = ColorUtility.ColorFromRGB(255, 230, 230);
        private static Color _textBoxEnabledBorder = ColorUtility.ColorFromRGB(0, 0, 0);
        private static Color _textBoxEnabledBack1 = ColorUtility.ColorFromRGB(250, 250, 250);
        private static Color _textBoxEnabledBack2 = ColorUtility.ColorFromRGB(250, 250, 250);
        private static Color _textBoxEnabledTextColor = ColorUtility.ColorFromRGB(60, 60, 60);

        private static Color _textBoxDisabledBorder = ColorUtility.ColorFromRGB(0, 0, 0);
        private static Color _textBoxDisabledBack1 = ColorUtility.ColorFromRGB(29, 28, 33);
        private static Color _textBoxDisabledBack2 = ColorUtility.ColorFromRGB(29, 28, 33);
        private static Color _textBoxDisabledTextColor = ColorUtility.ColorFromRGB(113, 113, 113);
        #endregion

        #region Label
        private static Color _labelEnabledColor = ColorUtility.ColorFromRGB(213, 213, 213);
        private static Color _labelDisabledColor = ColorUtility.ColorFromRGB(113, 113, 113);
        #endregion

        #region CheckBox
        private static Color _checkBoxEnabledBorder = ColorUtility.ColorFromRGB(130, 130, 130);
        private static Color _checkBoxEnabledBack1 = ColorUtility.ColorFromRGB(10, 10, 20);
        private static Color _checkBoxEnabledBack2 = ColorUtility.ColorFromRGB(10, 10, 20);
        private static Color _checkBoxEnabledCross = ColorUtility.ColorFromRGB(170, 170, 190);

        private static Color _checkBoxDisabledBorder = ColorUtility.ColorFromRGB(130, 130, 130);
        private static Color _checkBoxDisabledBack1 = ColorUtility.ColorFromRGB(60, 60, 60);
        private static Color _checkBoxDisabledBack2 = ColorUtility.ColorFromRGB(60, 60, 60);
        private static Color _checkBoxDisabledCross = ColorUtility.ColorFromRGB(200, 200, 220);

        private static Color _checkBoxPressedBorder = ColorUtility.ColorFromRGB(100, 100, 100);
        private static Color _checkBoxPressedBack1 = ColorUtility.ColorFromRGB(30, 30, 30);
        private static Color _checkBoxPressedBack2 = ColorUtility.ColorFromRGB(30, 30, 30);
        private static Color _checkBoxPressedCross = ColorUtility.ColorFromRGB(100, 100, 120);
        #endregion

        #endregion

        #region public static properties
        public override Color BackgroundColor { get { return _backgroundColor; } }
        public override Color BackgroundBorderColor { get { return _backgroundBoderColor; } }

        #region Buttons
        public override Font ButtonFont { get { return Fonts.Arial; } }

        public override Color ButtonPressedBorder { get { return _buttonPressedBorder; } }
        public override Color ButtonPressedBack1 { get { return _buttonPressedBack1; } }
        public override Color ButtonPressedBack2 { get { return _buttonPressedBack2; } }
        public override Color ButtonPressedTextColor { get { return _buttonPressedTextColor; } }

        public override Color ButtonEnabledBorder { get { return _buttonEnabledBorder; } }
        public override Color ButtonEnabledBack1 { get { return _buttonEnabledBack1; } }
        public override Color ButtonEnabledBack2 { get { return _buttonEnabledBack2; } }
        public override Color ButtonEnabledTextColor { get { return _buttonEnabledTextColor; } }

        public override Color ButtonDisabledBorder { get { return _buttonDisabledBorder; } }
        public override Color ButtonDisabledBack1 { get { return _buttonDisabledBack1; } }
        public override Color ButtonDisabledBack2 { get { return _buttonDisabledBack2; } }
        public override Color ButtonDisabledTextColor { get { return _buttonDisabledTextColor; } }
        #endregion

        #region TextBoxes
        public override Font TextBoxFont { get { return Fonts.Arial; } }

        public override Color TextBoxPressedBorder { get { return _textBoxPressedBorder; } }
        public override Color TextBoxPressedBack1 { get { return _textBoxPressedBack1; } }
        public override Color TextBoxPressedBack2 { get { return _textBoxPressedBack2; } }
        public override Color TextBoxPressedTextColor { get { return _textBoxPressedTextColor; } }

        public override Color TextBoxEnabledInvalidBorder { get { return _textBoxEnabledInvalidBorder; } }
        public override Color TextBoxEnabledInvalidBack { get { return _textBoxEnabledInvalidBack; } }
        public override Color TextBoxEnabledBorder { get { return _textBoxEnabledBorder; } }
        public override Color TextBoxEnabledBack1 { get { return _textBoxEnabledBack1; } }
        public override Color TextBoxEnabledBack2 { get { return _textBoxEnabledBack2; } }
        public override Color TextBoxEnabledTextColor { get { return _textBoxEnabledTextColor; } }

        public override Color TextBoxDisabledBorder { get { return _textBoxDisabledBorder; } }
        public override Color TextBoxDisabledBack1 { get { return _textBoxDisabledBack1; } }
        public override Color TextBoxDisabledBack2 { get { return _textBoxDisabledBack2; } }
        public override Color TextBoxDisabledTextColor { get { return _textBoxDisabledTextColor; } }
        #endregion

        #region Label
        public override Font LabelFont { get { return Fonts.Arial; } }
        public override Color LabelEnabledColor { get { return _labelEnabledColor; } }
        public override Color LabelDisabledColor { get { return _labelDisabledColor; } }
        #endregion

        #region CheckBox
        public override Color CheckBoxEnabledBorder { get { return _checkBoxEnabledBorder; } }
        public override Color CheckBoxEnabledBack1 { get { return _checkBoxEnabledBack1; } }
        public override Color CheckBoxEnabledBack2 { get { return _checkBoxEnabledBack2; } }
        public override Color CheckBoxEnabledCross { get { return _checkBoxEnabledCross; } }

        public override Color CheckBoxDisabledBorder { get { return _checkBoxDisabledBorder; } }
        public override Color CheckBoxDisabledBack1 { get { return _checkBoxDisabledBack1; } }
        public override Color CheckBoxDisabledBack2 { get { return _checkBoxDisabledBack2; } }
        public override Color CheckBoxDisabledCross { get { return _checkBoxDisabledCross; } }

        public override Color CheckBoxPressedBorder { get { return _checkBoxPressedBorder; } }
        public override Color CheckBoxPressedBack1 { get { return _checkBoxPressedBack1; } }
        public override Color CheckBoxPressedBack2 { get { return _checkBoxPressedBack2; } }
        public override Color CheckBoxPressedCross { get { return _checkBoxPressedCross; } }
        #endregion

        #region RadioButton
        public override Color RadioButtonEnabledBorder { get { return _checkBoxEnabledBorder; } }
        public override Color RadioButtonEnabledBack { get { return _checkBoxEnabledBack1; } }
        public override Color RadioButtonEnabledPoint { get { return _checkBoxEnabledCross; } }

        public override Color RadioButtonDisabledBorder { get { return _checkBoxDisabledBorder; } }
        public override Color RadioButtonDisabledBack { get { return _checkBoxDisabledBack1; } }
        public override Color RadioButtonDisabledPoint { get { return _checkBoxDisabledCross; } }

        public override Color RadioButtonPressedBorder { get { return _checkBoxPressedBorder; } }
        public override Color RadioButtonPressedBack { get { return _checkBoxPressedBack1; } }
        public override Color RadioButtonPressedPoint { get { return _checkBoxPressedCross; } }
        #endregion

        #region ComboBox
        public override Font ComboBoxFont { get { return Fonts.Arial; } }

        public override Color ComboBoxPressedBorder { get { return _textBoxPressedBorder; } }
        public override Color ComboBoxPressedBack1 { get { return _textBoxPressedBack1; } }
        public override Color ComboBoxPressedBack2 { get { return _textBoxPressedBack2; } }
        public override Color ComboBoxPressedTextColor { get { return _textBoxPressedTextColor; } }

        public override Color ComboBoxEnabledBorder { get { return _textBoxEnabledBorder; } }
        public override Color ComboBoxEnabledBack1 { get { return _textBoxEnabledBack1; } }
        public override Color ComboBoxEnabledBack2 { get { return _textBoxEnabledBack2; } }
        public override Color ComboBoxEnabledTextColor { get { return _textBoxEnabledTextColor; } }

        public override Color ComboBoxDisabledBorder { get { return _textBoxDisabledBorder; } }
        public override Color ComboBoxDisabledBack1 { get { return _textBoxDisabledBack1; } }
        public override Color ComboBoxDisabledBack2 { get { return _textBoxDisabledBack2; } }
        public override Color ComboBoxDisabledTextColor { get { return _textBoxDisabledTextColor; } }
        #endregion

        #region ListBox
        public override Color ListBoxEnabledBack1 { get { return _textBoxEnabledBack1; } }
        public override Color ListBoxEnabledBack2 { get { return _textBoxEnabledBack2; } }
        public override Color ListBoxEnabledBorder { get { return _textBoxEnabledBorder; } }

        public override Color ListBoxDisabledBack1 { get { return _textBoxDisabledBack1; } }
        public override Color ListBoxDisabledBack2 { get { return _textBoxDisabledBack2; } }
        public override Color ListBoxDisabledBorder { get { return _textBoxDisabledBorder; } }

        public override Color ListBoxSelectedItemBack1 { get { return _textBoxPressedBack1; } }
        public override Color ListBoxSelectedItemBack2 { get { return _textBoxPressedBack2; } }
        public override Color ListBoxScrollBarColor { get { return _textBoxEnabledBorder; } }
        #endregion

        #region ProgressBar
        public override Color ProgressBarEnabledBack1 { get { return Colors.LightGray; } }
        public override Color ProgressBarEnabledBack2 { get { return Colors.Gray; } }
        public override Color ProgressBarEnabledBorder { get { return Colors.DarkGray; } }

        public override Color ProgressBarDisabledBack1 { get { return Colors.LightGray; } }
        public override Color ProgressBarDisabledBack2 { get { return Colors.Gray; } }
        public override Color ProgressBarDisabledBorder { get { return Colors.DarkGray; } }

        public override Color ProgressBarBack1 { get { return Colors.LightBlue; } }
        public override Color ProgressBarBack2 { get { return Colors.LightBlue; } }
        #endregion

        #endregion
    }
}
