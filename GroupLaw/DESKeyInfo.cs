using System.Linq;
using System.Security.Cryptography;

namespace GroupLaw
{
    public class DESKeyInfo
    {
        public byte[] Key { get; set; }
        public byte[] IV { get; set; }

        public static DESKeyInfo Generate()
        {
            var keyInfo = new DESKeyInfo();
            using (var myTripleDES = new DESCryptoServiceProvider())
            {
                keyInfo.Key = myTripleDES.Key.ToArray();
                keyInfo.IV = myTripleDES.IV.ToArray();
            }

            return keyInfo;
        }
    }
}
