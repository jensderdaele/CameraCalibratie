using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BundleAdjuster
{
    public static class EXT
    {
        public static Enum Or(this Enum a, Enum b)
        {
            if (Enum.GetUnderlyingType(a.GetType()) != typeof(ulong))
                return (Enum)Enum.ToObject(a.GetType(), Convert.ToInt64(a) | Convert.ToInt64(b));
            else
                return (Enum)Enum.ToObject(a.GetType(), Convert.ToUInt64(a) | Convert.ToUInt64(b));
        }
        public static Enum And(this Enum a, Enum b)
        {
            if (Enum.GetUnderlyingType(a.GetType()) != typeof(ulong))
                return (Enum)Enum.ToObject(a.GetType(), Convert.ToInt64(a) & Convert.ToInt64(b));
            else
                return (Enum)Enum.ToObject(a.GetType(), Convert.ToUInt64(a) & Convert.ToUInt64(b));
        }
        public static Enum SetOn(this Enum a, Enum b)
        {
            if (Enum.GetUnderlyingType(a.GetType()) != typeof(ulong))
                return (Enum)Enum.ToObject(a.GetType(), Convert.ToInt64(a) | Convert.ToInt64(b));
            else
                return (Enum)Enum.ToObject(a.GetType(), Convert.ToUInt64(a) | Convert.ToUInt64(b));
        }
        public static Enum SetOff(this Enum a, Enum b)
        {
            if (Enum.GetUnderlyingType(a.GetType()) != typeof(ulong))
                return (Enum)Enum.ToObject(a.GetType(), Convert.ToInt64(a) & (Convert.ToInt64(b) ^ -1));
            else
                return (Enum)Enum.ToObject(a.GetType(), Convert.ToUInt64(a) & (Convert.ToUInt64(b) ^ 0xFFFFFFFFFFFFFFFF));
        }
    }
}
