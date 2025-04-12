using Newtonsoft.Json;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
namespace NeteaseMusicAPI
{

    /// <summary>
    /// 
    /// </summary>
    internal class Login
    {
        private const string LoginUrl = "https://music.163.com/weapi/login/qrcode";
        private const string loginUrlGetkey = "/unikey";
        private const string loginHeart = "/client/login";
        private const string verifyUrl = "https://music.163.com/weapi/w/nuser/account/get?csrf_token=";
        private const string VpiVerify = "https://music.163.com/weapi/music-vip-membership/front/vip/info?csrf_token=";
        public Cookies Cookie; 
        #region 登陆部分
        private CancellationTokenSource _cancellationTokenSource;
        private NeteaseMusic Api;
        public Login(NeteaseMusic api)
        {
            Api = api;
        }
        public Login(NeteaseMusic api,string json)
        {
            Api = api;
            SetCookies(json);
        }
        public void SetCookies(string json)
        {
            UnityEngine.Debug.Log(json);
            Cookie = JsonConvert.DeserializeObject<Cookies>(json);
        }
        bool isloging;
        /// <summary>
        /// 二维码网络请求cookie
        /// </summary>
        public async void willbe(Action<byte[]> onQrCodeGenerated, Action<string> OutManage =null)
        {
            if (isloging)
            {
                OutManage?.Invoke("方法已经在执行中，忽略本次调用");
                return;
            }
            _cancellationTokenSource = new CancellationTokenSource();
            try
            {
                using var _= new BusyDisposable(value=> isloging = value) ;
                var json = await Api.netWork.PostAsync(LoginUrl + loginUrlGetkey, new { type = 1, noCheckToken = true });
                _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                Dictionary<string, object> backjson = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                string unikey = backjson["unikey"].ToString();
                onQrCodeGenerated.Invoke(GenerateQrCode($"http://music.163.com/login?codekey={unikey}"));
                do
                {
                    _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    var HeartPack = await Api.netWork.PostAsyncWithHeader(LoginUrl + loginHeart, new { type = 1, noCheckToken = true, key = unikey });
                    _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    Dictionary<string, object> HeartPackbackjson = JsonConvert.DeserializeObject<Dictionary<string, object>>(HeartPack.json);
                    string heart = HeartPackbackjson["code"].ToString();
                    OutManage?.Invoke(heart);
                    switch (heart)
                    {
                        case "800":
                            //过期
                            OutManage?.Invoke("反馈码:" + HeartPackbackjson["code"].ToString() + "，" + HeartPackbackjson["message"].ToString());
                            return;
                        case "801":
                            //等待扫码；
                            OutManage?.Invoke("反馈码:" + HeartPackbackjson["code"].ToString() + "，" + HeartPackbackjson["message"].ToString());
                            break;
                        case "802":
                            //已扫码等待授权
                            OutManage?.Invoke("反馈码:" + HeartPackbackjson["code"].ToString() + "，" + HeartPackbackjson["message"].ToString());
                            OutManage?.Invoke("头像url：" + HeartPackbackjson["avatarUrl"].ToString() + "，网易云名称：" + HeartPackbackjson["nickname"].ToString());
                            break;
                        case "803":
                            //已授权，获取反馈
                            OutManage?.Invoke("反馈码:" + HeartPackbackjson["code"].ToString() + "，" + HeartPackbackjson["message"].ToString());
                            Setcookie(HeartPack.headers["Set-Cookie"]);
                            return;
                    }
                    await Task.Delay(1000, _cancellationTokenSource.Token);
                } while (true);
            }
            catch (OperationCanceledException)
            {
                OutManage?.Invoke("操作已取消");
            }
        }
        /// <summary>
        /// 取消登录状态
        /// </summary>
        public void CancelOperation()
        {
            _cancellationTokenSource?.Cancel();
        }
        /// <summary>
        /// 设置cookie
        /// </summary>
        private void Setcookie(string cookie, Action<string> OutManage = null)
        {
            OutManage?.Invoke("cookie为" + cookie);
            const string csrf = "__csrf";
            int csrfUIndex = cookie.LastIndexOf(csrf, StringComparison.Ordinal);
            string csrfUString = cookie.Substring(csrfUIndex + csrf.Length + 1);
            int csrfLength = csrfUString.IndexOf(';');
            const string musicU = "MUSIC_U";
            int musicUIndex = cookie.LastIndexOf(musicU, StringComparison.Ordinal);
            string musicUString = cookie.Substring(musicUIndex + musicU.Length + 1);
            int musicLength = musicUString.IndexOf(';');
            Cookie = new Cookies();
            Cookie.__csrf = csrfUString.Substring(0, csrfLength);
            Cookie.MUSIC_U = musicUString.Substring(0, musicLength);
            Cookie.__remember_me = "true";
            OutManage?.Invoke(Cookie.ToString());
        }
        #endregion
        #region 二维码绘制
        private byte[] GenerateQrCode(string content)
        {
            using QRCodeGenerator qrCodeGenerator = new QRCodeGenerator();
            using QRCodeData qrCodeData = qrCodeGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.M, true);
            using PngByteQRCode pngQrCode = new PngByteQRCode(qrCodeData);
            byte[] pngBytes = pngQrCode.GetGraphic(20);
            return pngBytes;
        }
        #endregion

        public bool TryGetCookie(out string cookie)
        {
            cookie = "";
            if (Cookie == null)
            {
                return false;
            }
            else
            {
                cookie = Cookie.ToString();
                return true;
            }
        }
        public string TryGetCsrf()
        {
            if (Cookie == null)
            {
                return "";
            }
            else
            {
                return Cookie.__csrf;
            }
        }
        /// <summary>
        /// 验证
        /// </summary>
        /// <returns></returns>
        public async Task<string> verifyCsrf()
        {
            var json = await Api.netWork.PostAsync(verifyUrl + TryGetCsrf(), new { noCheckToken = true, csrf_token = "" });
            return json;
        }
        /// <summary>
        /// vip信息
        /// </summary>
        /// <returns></returns>
        public async Task<string> Viplog()
        {
            if (Cookie == null||Cookie.__csrf == default)
            {
                var josn = await Api.netWork.PostAsync(verifyUrl + TryGetCsrf(), new { noCheckToken = true, csrf_token = Cookie.__csrf });
                return josn;
            }
            else
            {
                return "";
            }
        }
    }
}
[Serializable]
public class Cookies
{
    public string NMTID;
    public string MUSIC_U;
    public string __csrf;
    public string __remember_me = "true";
    public Cookies()
    {
        
    }
    public override string ToString()
    {
        return string.Format("NMTID={0};MUSIC_U={1};__csrf={2};__remember_me={3};",
            NMTID, MUSIC_U, __csrf, __remember_me);
    }
}
class BusyDisposable : IDisposable
{
    private readonly Action<bool> _callback;
    public BusyDisposable(Action<bool> callback)
    {
        _callback = callback;
        _callback(true);
    }

    public void Dispose()
    {
        _callback.Invoke(false);
    }
}
/*
登陆时请求
https://music.163.com/weapi/login/qrcode/unikey
传入
"{"type":1,"noCheckToken":true}"
返回200
其中附带一个unikey然后附带http://music.163.com/login?codekey=“unikey”

每秒进行一次询问
https://music.163.com/weapi/login/qrcode/client/login
{"type":1,"noCheckToken":true,"key":"unikey"}
code：800 二维码不存在或者过期
code：801 等待扫码
code：802 授权中 avatarUrl为头像，nickname为网易云名称
code：803 登陆成功
登陆成功后使用803中相应标头的set-cookie:来获取tocken
再使用{"noCheckToken":true,"csrf_token":"you_token"}
到 
https://music.163.com/weapi/w/nuser/account/get?csrf_token=you_token
获取用户具体信息
*/
/*
用户信息
GET：
https://music.163.com/discover/g/attr?csrf_token=you_token
其中包含了用户id
POST
https://music.163.com/weapi/w/nuser/account/get?csrf_token=you_token
传入
{"csrf_token":"d90cf0865a613764d0d7e88e76bb4e48"}
*/
/*
VIP信息获取
https://music.163.com/weapi/music-vip-membership/front/vip/info?csrf_token=
传入
{"userId":"1408203055","csrf_token":"d90cf0865a613764d0d7e88e76bb4e48"}
 */
