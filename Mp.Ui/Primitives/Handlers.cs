using System;

using Microsoft.SPOT;

using Mp.Ui.Controls;

namespace Mp.Ui.Primitives
{
    public delegate void StyleChangedHandler(Style oldStyle, Style newStyle);
    public delegate void TextChangedHandler(string newText, bool valid);
    public delegate void UiEventHandler(Control sender);
    public delegate bool TouchEventHandler(TouchControl sender, int x, int y);
    public delegate void DrawBackgroundHandler(Bitmap screen, int width, int height);
}
