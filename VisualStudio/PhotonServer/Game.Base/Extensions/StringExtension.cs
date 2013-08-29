using System.Security.Cryptography;
using System.Text;

namespace Game.Base.Extensions
{
    public static class StringExtension
    {
        public static string ToSha1(this string that)
        {
            return DoHash(that, new SHA1CryptoServiceProvider());
        }

        public static string ToMd5(this string that)
        {
            return DoHash(that, new MD5CryptoServiceProvider());
        }

        private static string DoHash(string that, HashAlgorithm hasher)
        {
            return hasher.ComputeHash(Encoding.UTF8.GetBytes(that)).ToHexString();
        }
    }
}