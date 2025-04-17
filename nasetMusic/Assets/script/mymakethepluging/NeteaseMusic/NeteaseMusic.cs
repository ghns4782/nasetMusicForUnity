
using JetBrains.Annotations;
using Newtonsoft.Json;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System;
using System.Threading.Tasks;
using Unity.VisualScripting;

namespace NeteaseMusicAPI
{
    public class NeteaseMusic
    {
        internal Login login;
        internal NetWork netWork;
        public Search search;
        public NeteaseMusic()
        {
            login = new Login(this);
            netWork = new NetWork(this);
            search = new Search(this);
        }
        public NeteaseMusic(string cookieJson)
        {
            login = new Login(this,cookieJson);
            netWork = new NetWork(this);
            search = new Search(this);
        }
        /// <summary>
        /// ֱ�����õ�½cookie
        /// </summary>
        /// <param name="cookieJson"></param>
        public void SetloginJeson(string cookieJson)
        {
            login.SetCookies(cookieJson);
        }
        /// <summary>
        /// ����cookie���������ظ���½
        /// </summary>
        /// <returns></returns>
        public bool GetCookies(out string cookie)
        {
            return login.TryGetCookie(out cookie);
        }
        public string GetCookieJson()
        {
            return JsonConvert.SerializeObject(login.Cookie, Formatting.Indented);
        }
        /// <summary>
        /// ɨ���½onQrCodeGenerated���ʱ����ڵõ���ά���ʱ���png���ػ�����OutManageֻ����������ķ�����Ϣ
        /// </summary>
        public void willbe(Action<byte[]> onQrCodeGenerated, Action<string> OutManage = null, Action isOk = null)
        {
            login.willbe(onQrCodeGenerated, OutManage,isOk);
        }
        /// <summary>
        /// ֹͣɨ���½
        /// </summary>
        public void stopLogin()
        {
            login.CancelOperation();
        }
        
    }
}