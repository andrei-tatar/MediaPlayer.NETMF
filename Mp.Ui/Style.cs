using System;

using Mp.Ui.Managers;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

namespace Mp.Ui
{
    public abstract class Style
    {
        #region Container
        /// <summary>Gets the container background color</summary>
        public abstract Color BackgroundColor { get; }
        /// <summary>Gets the modal window border color</summary>
        public abstract Color BackgroundBorderColor { get; }
        #endregion

        #region Button
        /// <summary>Gets the font used to draw text on a text button</summary>
        public abstract Font ButtonFont { get; }

        /// <summary>Gets the color of the button border when pressed</summary>
        public abstract Color ButtonPressedBorder { get; }
        /// <summary>Gets the top background color of the button when pressed</summary>
        public abstract Color ButtonPressedBack1 { get; }
        /// <summary>Gets the bottom background color of the button when pressed</summary>
        public abstract Color ButtonPressedBack2 { get; }
        /// <summary>Gets the text color of the button when pressed</summary>
        public abstract Color ButtonPressedTextColor { get; }

        /// <summary>Gets the color of the button border when enabled</summary>
        public abstract Color ButtonEnabledBorder { get; }
        /// <summary>Gets the top background color of the button when enabled</summary>
        public abstract Color ButtonEnabledBack1 { get; }
        /// <summary>Gets the bottom background color of the button when enabled</summary>
        public abstract Color ButtonEnabledBack2 { get; }
        /// <summary>Gets the text color of the button when enabled</summary>
        public abstract Color ButtonEnabledTextColor { get; }

        /// <summary>Gets the color of the button border when disabled</summary>
        public abstract Color ButtonDisabledBorder { get; }
        /// <summary>Gets the top background color of the button when disabled</summary>
        public abstract Color ButtonDisabledBack1 { get; }
        /// <summary>Gets the bottom background color of the button when disabled</summary>
        public abstract Color ButtonDisabledBack2 { get; }
        /// <summary>Gets the text color of the button when disabled</summary>
        public abstract Color ButtonDisabledTextColor { get; }
        #endregion

        #region TextBox
        /// <summary>Gets the font used to draw text in a text box</summary>
        public abstract Font TextBoxFont { get; }

        /// <summary>Gets the border color of the textbox when pressed</summary>
        public abstract Color TextBoxPressedBorder { get; }
        /// <summary>Gets the top background color of the textbox when pressed</summary>
        public abstract Color TextBoxPressedBack1 { get; }
        /// <summary>Gets the bottom background color of the textbox when pressed</summary>
        public abstract Color TextBoxPressedBack2 { get; }
        /// <summary>Gets the text color of the textbox when pressed</summary>
        public abstract Color TextBoxPressedTextColor { get; }

        /// <summary>Gets the border color of the textbox when invalid</summary>
        public abstract Color TextBoxEnabledInvalidBorder { get; }
        /// <summary>Gets the top background color of the textbox when invalid</summary>
        public abstract Color TextBoxEnabledInvalidBack { get; }

        /// <summary>Gets the border color of the textbox when enabled</summary>
        public abstract Color TextBoxEnabledBorder { get; }
        /// <summary>Gets the top background color of the textbox when enabled</summary>
        public abstract Color TextBoxEnabledBack1 { get; }
        /// <summary>Gets the bottom background color of the textbox when enabled</summary>
        public abstract Color TextBoxEnabledBack2 { get; }
        /// <summary>Gets the text color of the textbox when enabled</summary>
        public abstract Color TextBoxEnabledTextColor { get; }

        /// <summary>Gets the border color of the textbox when disabled</summary>
        public abstract Color TextBoxDisabledBorder { get; }
        /// <summary>Gets the top background color of the textbox when disabled</summary>
        public abstract Color TextBoxDisabledBack1 { get; }
        /// <summary>Gets the bottom background color of the textbox when disabled</summary>
        public abstract Color TextBoxDisabledBack2 { get; }
        /// <summary>Gets the text color of the textbox when disabled</summary>
        public abstract Color TextBoxDisabledTextColor { get; }

        #endregion

        #region Label
        /// <summary>Gets the font of the label</summary>
        public abstract Font LabelFont { get; }
        /// <summary>Gets the text color of the label when enabled</summary>
        public abstract Color LabelEnabledColor { get; }
        /// <summary>Gets the text color of the label when disabled</summary>
        public abstract Color LabelDisabledColor { get; }
        #endregion

        #region CheckBox
        /// <summary>Gets the border color of the check box when enabled</summary>
        public abstract Color CheckBoxEnabledBorder { get; }
        /// <summary>Gets the top background color of the check box when enabled</summary>
        public abstract Color CheckBoxEnabledBack1 { get; }
        /// <summary>Gets the bottom background color of the check box when enabled</summary>
        public abstract Color CheckBoxEnabledBack2 { get; }
        /// <summary>Gets the cross color of the check box when enabled</summary>
        public abstract Color CheckBoxEnabledCross { get; }

        /// <summary>Gets the border color of the check box when disabled</summary>
        public abstract Color CheckBoxDisabledBorder { get; }
        /// <summary>Gets the top background color of the check box when disabled</summary>
        public abstract Color CheckBoxDisabledBack1 { get; }
        /// <summary>Gets the bottom background color of the check box when disabled</summary>
        public abstract Color CheckBoxDisabledBack2 { get; }
        /// <summary>Gets the cross color of the check box when disabled</summary>
        public abstract Color CheckBoxDisabledCross { get; }

        /// <summary>Gets the border color of the check box when pressed</summary>
        public abstract Color CheckBoxPressedBorder { get; }
        /// <summary>Gets the top background color of the check box when pressed</summary>
        public abstract Color CheckBoxPressedBack1 { get; }
        /// <summary>Gets the bottom background color of the check box when pressed</summary>
        public abstract Color CheckBoxPressedBack2 { get; }
        /// <summary>Gets the cross color of the check box when pressed</summary>
        public abstract Color CheckBoxPressedCross { get; }
        #endregion

        #region RadioButton
        /// <summary>Gets the border color of the radio button when enabled</summary>
        public abstract Color RadioButtonEnabledBorder { get; }
        /// <summary>Gets the background color of the radio button when enabled</summary>
        public abstract Color RadioButtonEnabledBack { get; }
        /// <summary>Gets the point color of the radio button when enabled</summary>
        public abstract Color RadioButtonEnabledPoint { get; }

        /// <summary>Gets the border color of the radio button when disabled</summary>
        public abstract Color RadioButtonDisabledBorder { get; }
        /// <summary>Gets the background color of the radio button when disabled</summary>
        public abstract Color RadioButtonDisabledBack { get; }
        /// <summary>Gets the point color of the radio button when disabled</summary>
        public abstract Color RadioButtonDisabledPoint { get; }

        /// <summary>Gets the border color of the radio button when pressed</summary>
        public abstract Color RadioButtonPressedBorder { get; }
        /// <summary>Gets the background color of the radio button when pressed</summary>
        public abstract Color RadioButtonPressedBack { get; }
        /// <summary>Gets the point color of the radio button when pressed</summary>
        public abstract Color RadioButtonPressedPoint { get; }
        #endregion

        #region ComboBox
        /// <summary>Gets the font of the combobox</summary>
        public abstract Font ComboBoxFont { get; }

        /// <summary>Gets the border color of the combobox when pressed</summary>
        public abstract Color ComboBoxPressedBorder { get; }
        /// <summary>Gets the top background color of the combobox when pressed</summary>
        public abstract Color ComboBoxPressedBack1 { get; }
        /// <summary>Gets the bottom background color of the combobox when pressed</summary>
        public abstract Color ComboBoxPressedBack2 { get; }
        /// <summary>Gets the text color of the combobox when pressed</summary>
        public abstract Color ComboBoxPressedTextColor { get; }

        /// <summary>Gets the border color of the combobox when enabled</summary>
        public abstract Color ComboBoxEnabledBorder { get; }
        /// <summary>Gets the top background color of the combobox when enabled</summary>
        public abstract Color ComboBoxEnabledBack1 { get; }
        /// <summary>Gets the bottom background color of the combobox when enabled</summary>
        public abstract Color ComboBoxEnabledBack2 { get; }
        /// <summary>Gets the text color of the combobox when enabled</summary>
        public abstract Color ComboBoxEnabledTextColor { get; }

        /// <summary>Gets the border color of the combobox when disabled</summary>
        public abstract Color ComboBoxDisabledBorder { get; }
        /// <summary>Gets the top background color of the combobox when disabled</summary>
        public abstract Color ComboBoxDisabledBack1 { get; }
        /// <summary>Gets the bottom background color of the combobox when disabled</summary>
        public abstract Color ComboBoxDisabledBack2 { get; }
        /// <summary>Gets the text color of the combobox when disabled</summary>
        public abstract Color ComboBoxDisabledTextColor { get; }
        #endregion

        #region ListBox
        /// <summary>Gets the top background color of the listbox when enabled</summary>
        public abstract Color ListBoxEnabledBack1 { get; }
        /// <summary>Gets the botom background color of the listbox when enabled</summary>
        public abstract Color ListBoxEnabledBack2 { get; }
        /// <summary>Gets the border color of the listbox when enabled</summary>
        public abstract Color ListBoxEnabledBorder { get; }

        /// <summary>Gets the top background color of the listbox when disabled</summary>
        public abstract Color ListBoxDisabledBack1 { get; }
        /// <summary>Gets the bottom background color of the listbox when disabled</summary>
        public abstract Color ListBoxDisabledBack2 { get; }
        /// <summary>Gets the border color of the listbox when disabled</summary>
        public abstract Color ListBoxDisabledBorder { get; }

        /// <summary>Gets the top background color of a selected listbox item</summary>
        public abstract Color ListBoxSelectedItemBack1 { get; }
        /// <summary>Gets the bottom background color of a selected listbox item</summary>
        public abstract Color ListBoxSelectedItemBack2 { get; }
        /// <summary>Gets the scroll bar color of a listbox</summary>
        public abstract Color ListBoxScrollBarColor { get; }
        #endregion

        #region ProgressBar
        /// <summary>Gets the top background color of the progress bar when enabled</summary>
        public abstract Color ProgressBarEnabledBack1 { get; }
        /// <summary>Gets the bottom background color of the progress bar when enabled</summary>
        public abstract Color ProgressBarEnabledBack2 { get; }
        /// <summary>Gets the border color of the progress bar when enabled</summary>
        public abstract Color ProgressBarEnabledBorder { get; }

        /// <summary>Gets the top background color of the progress bar when disabled</summary>
        public abstract Color ProgressBarDisabledBack1 { get; }
        /// <summary>Gets the bottom background color of the progress bar when disabled</summary>
        public abstract Color ProgressBarDisabledBack2 { get; }
        /// <summary>Gets the border color of the progress bar when disabled</summary>
        public abstract Color ProgressBarDisabledBorder { get; }

        /// <summary>Gets the top background color of progressbar bar</summary>
        public abstract Color ProgressBarBack1 { get; }
        /// <summary>Gets the bottom background color of progressbar bar</summary>
        public abstract Color ProgressBarBack2 { get; }
        #endregion
    }
}
