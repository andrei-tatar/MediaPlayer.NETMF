using System;
using Microsoft.SPOT;
using Mp.Ui.Primitives;

namespace Mp.Ui.Validators
{
    public static class NetValidators
    {
        private static StringValidator _ipValidator, _ipAllowEmptyValidator, _emailValidator, _netMaskValidator;

        public static StringValidator IpValidator
        {
            get
            {
                if (_ipValidator == null)
                {
                    char[] ipAddrSeparator = new char[] { '.' };
                    _ipValidator = (s) =>
                    {
                        string[] words = s.Split(ipAddrSeparator);
                        if (words.Length != 4) return false;
                        foreach (string word in words)
                            if (!NumberValidators.ByteValidator(word)) return false;
                        return true;
                    };
                }
                return _ipValidator;
            }
        }
        public static StringValidator NetMaskValidator
        {
            get
            {
                if (_netMaskValidator == null)
                {
                    char[] ipAddrSeparator = new char[] { '.' };
                    _netMaskValidator = (s) =>
                    {
                        string[] words = s.Split(ipAddrSeparator);

                        if (words.Length != 4) return false;
                        int tt = 0;
                        try
                        {
                            foreach (string word in words)
                            {
                                byte value = byte.Parse(word);
                                bool isLeading = (value == 254 || value == 252 || value == 248 || value == 240 || value == 224 || value == 192 || value == 128 || value == 0);

                                switch (tt)
                                {
                                    case 0:
                                        if (value == 255) continue;
                                        else if (isLeading) tt = 1;
                                        else return false;
                                        break;
                                    case 1:
                                        if (value != 0) return false;
                                        break;
                                }
                            }
                        }
                        catch (Exception) { return false; }

                        return true;
                    };
                }
                return _netMaskValidator;
            }
        }
        public static StringValidator IpAllowEmptyValidator
        {
            get
            {
                if (_ipAllowEmptyValidator == null)
                {
                    _ipAllowEmptyValidator = (s) =>
                    {
                        return s.Length == 0 || IpValidator(s);
                    };
                }
                return _ipAllowEmptyValidator;
            }
        }
        public static StringValidator EmailValidator
        {
            get
            {
                if (_emailValidator == null)
                {
                    char[] addrSeparator = new char[] { '@' };
                    _emailValidator = (s) =>
                    {
                        string[] parts = s.Split(addrSeparator);
                        if (parts.Length != 2 || parts[0].Length == 0 || parts[1].Length == 0) return false;

                        if (parts[0][0] == '.' || parts[1][0] == '.' || parts[0][parts[0].Length - 1] == '.' || parts[1][parts[1].Length - 1] == '.') return false;
                        if (parts[0].IndexOf("..") >= 0 || parts[1].IndexOf("..") >= 0) return false;

                        return true;
                    };
                }
                return _emailValidator;
            }
        }
    }
}
