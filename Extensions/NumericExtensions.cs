using System;
using System.Globalization;
using System.IO;

namespace Extensions
{
    public static class NumericExtensions
    {
        // Extension method, call for any object, eg "if (x.IsNumeric())..."
        public static bool IsNumeric(this object x) { return (x == null ? false : IsNumeric(x.GetType())); }

        // Method where you know the type of the object
        public static bool IsNumeric(Type type) { return IsNumeric(type, Type.GetTypeCode(type)); }

        // Method where you know the type and the type code of the object
        public static bool IsNumeric(Type type, TypeCode typeCode) { return (typeCode == TypeCode.Decimal || (type.IsPrimitive && typeCode != TypeCode.Object && typeCode != TypeCode.Boolean && typeCode != TypeCode.Char)); }


        public static string ToInvariantString(this double d, string format = null)
        {
            if(format == null)
                return d.ToString(CultureInfo.InvariantCulture);
            return d.ToString(format, CultureInfo.InvariantCulture);
        }

        public static string ToInvariantString(this float f, string format = null)
        {
            if(format == null)
                return f.ToString(CultureInfo.InvariantCulture);
            return f.ToString(format, CultureInfo.InvariantCulture);
        }

        public static string ToInvariantString(this object o)
        {
            if (!IsNumeric(o)) return o.ToString();

            if (o is double) return ((double)o).ToInvariantString();
            if (o is float) return ((float)o).ToInvariantString();

            return o.ToString();
        }
        
        public static double DivisionNotNaN(this double numerator, double denominator)
        {
            return denominator == 0 ? 0 : numerator / denominator;
        }


        public static object Convert(this object o, Type type)
        {
            if (type.IsEnum)
                return Enum.Parse(type, o.ToString());

            return System.Convert.ChangeType(o, type);
        }


        public static string ToQuotedString(this object o)
        {
            return "nId" + o.ToString();
        }

        public static string ToUnderscoredString(this float n)
        {
            return n.ToString().Replace('-', '_');
        }

        public static float ToMinusFloat(this string s)
        {
            return float.Parse(s.Replace('_', '-'));
        }

        public static float ParseInvariant(string f)
        {
            return float.Parse(f, CultureInfo.InvariantCulture);
        }


        public static void WriteToFile(this string s, string filename)
        {
            using (var writer = new StreamWriter(filename))
            {
                writer.WriteLine(s);
            }
        }
    }
}
