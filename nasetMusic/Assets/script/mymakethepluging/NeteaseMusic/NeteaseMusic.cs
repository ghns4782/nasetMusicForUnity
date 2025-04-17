
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
        /// 直接设置登陆cookie
        /// </summary>
        /// <param name="cookieJson"></param>
        public void SetloginJeson(string cookieJson)
        {
            login.SetCookies(cookieJson);
        }
        /// <summary>
        /// 返回cookie可以用于重复登陆
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
        /// 扫码登陆onQrCodeGenerated这个时间会在得到二维码的时候吧png返回回来，OutManage只是用于你检查的返回信息
        /// </summary>
        public void willbe(Action<byte[]> onQrCodeGenerated, Action<string> OutManage = null, Action isOk = null)
        {
            login.willbe(onQrCodeGenerated, OutManage,isOk);
        }
        /// <summary>
        /// 停止扫码登陆
        /// </summary>
        public void stopLogin()
        {
            login.CancelOperation();
        }
        
    }
}