using System;

namespace Mp.Ui.Primitives
{
    public struct Point
    {
        public static readonly Point Zero = new Point();

        private int _x, _y;

        public int X { get { return _x; } set { _x = value; } }
        public int Y { get { return _y; } set { _y = value; } }

        public Point(int X, int Y)
        {
            _x = X;
            _y = Y;
        }
    }
}
