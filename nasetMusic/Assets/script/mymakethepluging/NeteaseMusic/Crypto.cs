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
/// <br>����ֱ��ʹ��new <see cref="NeteaseCrypto"/>(string)</br>
/// <br>���������ʹ��<see cref="NeteaseCrypto.Create{T}(T)"/>��ֱ�ӽ���������л���Ϊ����</br>
/// </summary>
/// <remarks>
/// �����Ƽ��ܷ�ʽ��ʹ������ase���ܣ���һ��ʹ��һ���̶���Կ���ڶ���ʹ��һ���������Ϊ��Կ���ܣ�Ȼ��ڶ������������Կ�ٴμ���Ϊsendkey��Ϊ�ڶ�������
/// </remarks>
/// <example>
/// <code>
/// <![CDATA[
///     //if you have json be like this "{""type"":1,""noCheckToken"":true}"
///     var crypto= new NeteaseCrypto("{""type"":1,""noCheckToken"":true}");
///     //Now you can use on post
///     UnityWebRequest.post(url , crypto.GetQuest());
/// 
///     //or if you want to use Litjson belike��
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
        /// �ַ����������
        /// </summary>
        public NeteaseCrypto(string plaintextString)
        {
            UnityEngine.Debug.Log(plaintextString);
            var crypto = Crypto(plaintextString);
            Parame = crypto.parame;
            EncSecKey = crypto.encSecKey;
        }
        /// <summary>
        /// �๹�����
        /// </summary>
        public static NeteaseCrypto Create<T>(T plaintext) where T : class
        {
            return new NeteaseCrypto(JsonConvert.SerializeObject(plaintext));
        }
        /// <summary>
        /// ��ȡ�ֵ�
        /// </summary>
        public Dictionary<string, string> GetQuest()
        {
            return new Dictionary<string, string>()
            {
                ["params"] = this.Parame,
                ["encSecKey"] = this.EncSecKey,
            };
        }
        #region ���ܷ���
        /// <summary>
        /// ͨ������취����ֱ�ӻ�ȡparame��sendkey������ʹ�õĺ���������ҳһ���������Կ
        /// </summary>
        public static (string parame, string encSecKey) Crypto(string data)
        {
            //ǰ������ase�Ĺ̶����ܣ��ڶ��ε���Կ��Ϊ�����������ֵ�����ڱ���ֱ��ʹ��һ���̶�ֵ��EncSecKey����ֻ���ڼ��ܵڶ������������Կ����Ҳһ��ʹ�õ��ǹ̶�ֵ
            var asedata = EncryptAES(data, keycode);
            var key = GenerateRandomToken();
            var parame = EncryptAES(asedata, key);
            var sendkey = Encrypt(key);
            return (parame, sendkey);
        }
        /// <summary>
        /// AES������ΪParame�ļ��ܷ�ʽ
        /// </summary>
        private static string EncryptAES(string plainText, string key)
        {
            // ������Կ��IV����JavaScript�����е�һ�£�
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] ivBytes = Encoding.UTF8.GetBytes("0102030405060708");
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = keyBytes;
                aesAlg.IV = ivBytes;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7; // CryptoJSĬ��ʹ��PKCS7���

                // ����������
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // ��������
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(plainBytes, 0, plainBytes.Length);
                        csEncrypt.FlushFinalBlock();

                        // ��ȡ���ܺ���ֽ����鲢ת��ΪBase64�ַ���
                        byte[] encryptedBytes = msEncrypt.ToArray();
                        return Convert.ToBase64String(encryptedBytes);
                    }
                }
            }
        }
        #region EncSecKey����
        /// <summary>
        /// ��ȡ16λ���������
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
        /// RSA���ܣ�EncSecKey�ĺ��Ĳ���
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private static string Encrypt(string msg)
        {
            var modulus = new BigInteger("00e0b509f6259df8642dbc35662901477df22677ec152b5ff68ace615bb7b725152b3ab17a876aea8a5aa76d2e417629ec4ee341f56135fccf695280104e0312ecbda92557c93870114af6c9d05c4f7f0c3685b7a46bee255932575cce10b424d813cfe4875d3e82047b97ddef52741d546b8e289dc6935b3ece0462db0a22b8e7", 16);
            var exponent = new BigInteger("010001", 16);

            // ��ת�ַ������� Python ����һ�£�
            var reversedMsg = new string(msg.Reverse().ToArray());
            var msgBytes = Encoding.UTF8.GetBytes(reversedMsg);

            // ʹ�� BouncyCastle ��������� RSA ����
            var rsaEngine = new RsaEngine();
            var keyParameters = new RsaKeyParameters(false, modulus, exponent);
            rsaEngine.Init(true, keyParameters);

            var encryptedBytes = rsaEngine.ProcessBlock(msgBytes, 0, msgBytes.Length);

            // ת��Ϊ Hex �ַ���
            return BitConverter.ToString(encryptedBytes).Replace("-", "").ToLower();
        }
        #endregion
        #endregion
        #region �������Innermediaplayer��Ŀ�ļ�����Ϣ
        /// <summary>
        /// ���ڽ�����ҳ��Ϣ����Ҫ֪���ڶ��μ��ܵ�key����������ִ���������õ����շ�����Ϣ
        /// </summary>
        /// <param name="cipherText"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string DecryptAES(string cipherText, string key)
        {
            // ������Կ��IV�����������ʱһ�£�
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] ivBytes = Encoding.UTF8.GetBytes("0102030405060708");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = keyBytes;
                aesAlg.IV = ivBytes;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                // ����������
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
