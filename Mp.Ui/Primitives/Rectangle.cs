using System;
using Microsoft.SPOT;

namespace UI.Primitives
{
    public struct Rectangle
    {
        public static readonly Rectangle Zero = new Rectangle();

        private int _x, _y, _width, _height;

        public int X { get { return _x; } set { _x = value; } }
        public int Y { get { return _y; } set { _y = value; } }
        public int Width { get { return _width; } set { _width = value; } }
        public int Height { get { return _height; } set { _height = value; } }

        public Rectangle(int X, int Y, int Width, int Height)
        {
            _x = X;
            _y = Y;
            _width = Width;
            _height = Height;
        }
    }
}
