using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace GroupLaw
{
    public class DESHelper
    {
        public static byte[] EncryptStringToBytes(byte[] plainText, DESKeyInfo keyInfo)
        {
            using (var tdsAlg = new DESCryptoServiceProvider())
            {
                //   tdsAlg.Padding = PaddingMode.None; ;
                tdsAlg.Key = keyInfo.Key;
                tdsAlg.IV = keyInfo.IV;

                var encryptor = tdsAlg.CreateEncryptor(tdsAlg.Key, tdsAlg.IV);
                using (var memoryStream = new MemoryStream())
                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(plainText, 0, plainText.Length);
                    cryptoStream.FlushFinalBlock();
                    return memoryStream.ToArray();
                }
            }
        }

        public static byte[] DecryptStringFromBytes(byte[] cipherText, DESKeyInfo keyInfo)
        {
            using (var tdsAlg = new DESCryptoServiceProvider())
            {
                tdsAlg.Padding = PaddingMode.None; ;
                tdsAlg.Key = keyInfo.Key;
                tdsAlg.IV = keyInfo.IV;
                var decryptor = tdsAlg.CreateDecryptor(tdsAlg.Key, tdsAlg.IV);

                var plainTextBytes = new byte[cipherText.Length];

                using (var msDecrypt = new MemoryStream(cipherText))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        var decryptedByteCount = csDecrypt.Read
                        (
                            plainTextBytes,
                            0,
                            plainTextBytes.Length
                        );
                        return plainTextBytes.Take(decryptedByteCount).ToArray();
                    }
                }
            }
        }
    }
}
