using System;
using Microsoft.SPOT;
using Mp.Ui.Primitives;

namespace Mp.Ui.Validators
{
    public static class NumberValidators
    {
        private static StringValidator _doubleValidator;
        private static StringValidator _intValidator;
        private static StringValidator _shortValidator;
        private static StringValidator _byteValidator;
        private static StringValidator _uShortValidator;
        private static StringValidator _uLongValidator;

        public static StringValidator DoubleValidator
        {
            get
            {
                if (_doubleValidator == null)
                    _doubleValidator = (s) =>
                    {
                        try { double value = double.Parse(s); }
                        catch (Exception) { return false; }
                        return true;
                    };
                return _doubleValidator;
            }
        }
        public static StringValidator IntegerValidator
        {
            get
            {
                if (_intValidator == null)
                    _intValidator = (s) =>
                    {
                        try { int value = int.Parse(s); }
                        catch (Exception) { return false; }
                        return true;
                    };
                return _intValidator;
            }
        }
        public static StringValidator ShortValidator
        {
            get
            {
                if (_shortValidator == null)
                    _shortValidator = (s) =>
                    {
                        try { short value = short.Parse(s); }
                        catch (Exception) { return false; }
                        return true;
                    };
                return _shortValidator;
            }
        }
        public static StringValidator ByteValidator
        {
            get
            {
                if (_byteValidator == null)
                    _byteValidator = (s) =>
                    {
                        try { byte value = byte.Parse(s); }
                        catch (Exception) { return false; }
                        return true;
                    };
                return _byteValidator;
            }
        }
        public static StringValidator UShortValidator
        {
            get
            {
                if (_uShortValidator == null)
                    _uShortValidator = (s) =>
                    {
                        try { ushort value = ushort.Parse(s); }
                        catch (Exception) { return false; }
                        return true;
                    };
                return _uShortValidator;
            }
        }
        public static StringValidator ULongValidator
        {
            get
            {
                if (_uLongValidator == null)
                    _uLongValidator = (s) =>
                    {
                        try { ulong value = ulong.Parse(s); }
                        catch (Exception) { return false; }
                        return true;
                    };
                return _uLongValidator;
            }
        }
    }
}
