using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game.Base.Extensions
{
    public static class ByteArrayExtensions
    {
        public static string ToHexString(this byte[] that)
        {
            var sb = new StringBuilder(that.Length * 2);
            foreach (var b in that)
            {
                sb.AppendFormat("{0:x2}", b);
            }
            return sb.ToString();
        }
    }
}
