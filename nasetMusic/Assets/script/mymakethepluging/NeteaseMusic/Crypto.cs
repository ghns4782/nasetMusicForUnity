using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
/// <summary>
/// <br>可以直接使用new <see cref="NeteaseCrypto"/>(string)</br>
/// <br>或者你可以使用<see cref="NeteaseCrypto.Create{T}(T)"/>来直接将类进行序列化作为参数</br>
/// </summary>
/// <remarks>
/// 网易云加密方式是使用两次ase加密，第一次使用一个固定密钥，第二次使用一个随机数作为密钥加密，然后第二次随机数的密钥再次加密为sendkey作为第二个参数
/// </remarks>
/// <example>
/// <code>
/// <![CDATA[
///     //if you have json be like this "{""type"":1,""noCheckToken"":true}"
///     var crypto= new NeteaseCrypto("{""type"":1,""noCheckToken"":true}");
///     //Now you can use on post
///     UnityWebRequest.post(url , crypto.GetQuest());
/// 
///     //or if you want to use Litjson belike↓
///     var data = new { type = 1, noCheckToken = true }
///     //new { type = 1, noCheckToken = true } this code just creat new temp class,you can use your class
///     var crypto= new NeteaseCrypto (data)
///     //Now can use on post too
///     UnityWebRequest.post(url , crypto.GetQuest());
/// ]]>
/// </code>
/// </example>
///
namespace NeteaseMusicAPI
{
    public class NeteaseCrypto
    {
        private const string keycode = "0CoJUm6Qyw8W8jud";
        public string Parame;
        public string EncSecKey;
        /// <summary>
        /// 字符串构造加密
        /// </summary>
        public NeteaseCrypto(string plaintextString)
        {
            UnityEngine.Debug.Log(plaintextString);
            var crypto = Crypto(plaintextString);
            Parame = crypto.parame;
            EncSecKey = crypto.encSecKey;
        }
        /// <summary>
        /// 类构造加密
        /// </summary>
        public static NeteaseCrypto Create<T>(T plaintext) where T : class
        {
            return new NeteaseCrypto(JsonConvert.SerializeObject(plaintext));
        }
        /// <summary>
        /// 获取字典
        /// </summary>
        public Dictionary<string, string> GetQuest()
        {
            return new Dictionary<string, string>()
            {
                ["params"] = this.Parame,
                ["encSecKey"] = this.EncSecKey,
            };
        }
        #region 加密方法
        /// <summary>
        /// 通过这个办法可以直接获取parame和sendkey，并且使用的和网易云网页一样的随机密钥
        /// </summary>
        public static (string parame, string encSecKey) Crypto(string data)
        {
            //前两次是ase的固定加密，第二次的密钥因为网易中是随机值，现在本地直接使用一个固定值，EncSecKey由于只用于加密第二次随机数的密钥所以也一样使用的是固定值
            var asedata = EncryptAES(data, keycode);
            var key = GenerateRandomToken();
            var parame = EncryptAES(asedata, key);
            var sendkey = Encrypt(key);
            return (parame, sendkey);
        }
        /// <summary>
        /// AES加密作为Parame的加密方式
        /// </summary>
        private static string EncryptAES(string plainText, string key)
        {
            // 解析密钥和IV（与JavaScript代码中的一致）
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] ivBytes = Encoding.UTF8.GetBytes("0102030405060708");
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = keyBytes;
                aesAlg.IV = ivBytes;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7; // CryptoJS默认使用PKCS7填充

                // 创建加密器
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // 加密数据
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(plainBytes, 0, plainBytes.Length);
                        csEncrypt.FlushFinalBlock();

                        // 获取加密后的字节数组并转换为Base64字符串
                        byte[] encryptedBytes = msEncrypt.ToArray();
                        return Convert.ToBase64String(encryptedBytes);
                    }
                }
            }
        }
        #region EncSecKey部分
        /// <summary>
        /// 获取16位的随机令牌
        /// </summary>
        private static string GenerateRandomToken()
        {
            const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            var random = new System.Random();
            return new string(Enumerable.Repeat(chars, 16)
                .Select(s => s[random.Next(s.Length)])
                .ToArray());
        }
        /// <summary>
        /// RSA加密，EncSecKey的核心部分
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private static string Encrypt(string msg)
        {
            var modulus = new BigInteger("00e0b509f6259df8642dbc35662901477df22677ec152b5ff68ace615bb7b725152b3ab17a876aea8a5aa76d2e417629ec4ee341f56135fccf695280104e0312ecbda92557c93870114af6c9d05c4f7f0c3685b7a46bee255932575cce10b424d813cfe4875d3e82047b97ddef52741d546b8e289dc6935b3ece0462db0a22b8e7", 16);
            var exponent = new BigInteger("010001", 16);

            // 反转字符串（和 Python 代码一致）
            var reversedMsg = new string(msg.Reverse().ToArray());
            var msgBytes = Encoding.UTF8.GetBytes(reversedMsg);

            // 使用 BouncyCastle 进行无填充 RSA 加密
            var rsaEngine = new RsaEngine();
            var keyParameters = new RsaKeyParameters(false, modulus, exponent);
            rsaEngine.Init(true, keyParameters);

            var encryptedBytes = rsaEngine.ProcessBlock(msgBytes, 0, msgBytes.Length);

            // 转换为 Hex 字符串
            return BitConverter.ToString(encryptedBytes).Replace("-", "").ToLower();
        }
        #endregion
        #endregion
        #region 逆向解密Innermediaplayer项目的加密信息
        /// <summary>
        /// 用于解析网页信息，需要知道第二次加密的key，并且自行执行两次来得到最终发送信息
        /// </summary>
        /// <param name="cipherText"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string DecryptAES(string cipherText, string key)
        {
            // 解析密钥和IV（必须与加密时一致）
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] ivBytes = Encoding.UTF8.GetBytes("0102030405060708");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = keyBytes;
                aesAlg.IV = ivBytes;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                // 创建解密器
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (MemoryStream msOutput = new MemoryStream())
                        {
                            csDecrypt.CopyTo(msOutput);
                            byte[] decryptedBytes = msOutput.ToArray();
                            return Encoding.UTF8.GetString(decryptedBytes);
                        }
                    }
                }
            }
        }
        #endregion
        public override string ToString()
        {
            return $"Player(Parame={Parame}, EncSecKey={EncSecKey}";
        }

    }
}
