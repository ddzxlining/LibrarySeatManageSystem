using System;
namespace LibraryOperation
{
    public static class MyConvert
    {
        public static int toInt(object o)
        {
           return  Convert.ToInt32(o);
        }
        public static bool toBool(object o)
        {
            return Convert.ToBoolean(o);
        }
        internal static string toString(object o)
        {
            return Convert.ToString(o);
        }
    }
}
