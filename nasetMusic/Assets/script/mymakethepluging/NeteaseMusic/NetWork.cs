using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using System.Diagnostics;
using UnityEngine;
using System.IO;
using System.Text;
using Newtonsoft.Json;
//for unity 
//namespace NeteaseMusicAPI
//{

//    /// <summary>
//    /// 这个类只用于发送请求
//    /// </summary>
//    public class NetWork
//    {
//        private static Lazy<NetWork> Instance = new(() => new NetWork());
//        public static NetWork _instance = Instance.Value;
//        public async Task<string> GetAsync<T>(string Url, T @object, bool needCsrfToken = false, bool needCookies = false) where T : class
//        {
//            using UnityWebRequest webRequest = UnityWebRequest.Get(
//            Url);
//            SetRequestHeaders(webRequest, needCsrfToken);
//            await webRequest.SendWebRequest();
//            if (webRequest.result != UnityWebRequest.Result.Success)
//                throw new UnityException(webRequest.error);
//            var a = Encoding.UTF8.GetString(webRequest.downloadHandler.data);
//            return a;
//        }
//        public async Task<string> PostAsync<T>(string Url, T @object, bool needCsrfToken = false, bool needCookies = false) where T : class
//        {
//            var crypto = NeteaseCrypto.Create(@object);
//            using UnityWebRequest webRequest = UnityWebRequest.Post(
//            Url,
//            crypto.GetQuest());
//            SetRequestHeaders(webRequest, needCookies);
//            await webRequest.SendWebRequest();
//            if (webRequest.result != UnityWebRequest.Result.Success)
//                throw new UnityException(webRequest.error);
//            var a = Encoding.UTF8.GetString(webRequest.downloadHandler.data);
//            return a;
//        }
//        /// <summary>
//        /// 返回包含请求头的json
//        /// </summary>
//        /// <exception cref="UnityException"></exception>
//        public async Task<(string json, Dictionary<string, string> headers)> PostAsyncWithHeader<T>(string Url, T @object, bool needCsrfToken = false, bool needCookies = false) where T : class
//        {
//            var crypto = NeteaseCrypto.Create(@object);
//            using UnityWebRequest webRequest = UnityWebRequest.Post(
//            Url,
//            crypto.GetQuest());
//            SetRequestHeaders(webRequest, needCookies);
//            await webRequest.SendWebRequest();
//            if (webRequest.result != UnityWebRequest.Result.Success)
//                throw new UnityException(webRequest.error);
//            var a = Encoding.UTF8.GetString(webRequest.downloadHandler.data);
//            return (a, webRequest.GetResponseHeaders());
//        }
//        public async Task<string> PostAsync(string Url, string @object, bool needCsrfToken = false, bool needCookies = false)
//        {
//            var crypto = NeteaseCrypto.Create(@object);
//            using UnityWebRequest webRequest = UnityWebRequest.Post(
//            Url,
//            crypto.GetQuest());
//            SetRequestHeaders(webRequest, false);
//            await webRequest.SendWebRequest();
//            if (webRequest.result != UnityWebRequest.Result.Success)
//                throw new UnityException(webRequest.error);
//            var a = Encoding.UTF8.GetString(webRequest.downloadHandler.data);
//            return a;
//        }
//        /// <summary>
//        /// 添加请求头
//        /// </summary>
//        /// <param name="unityWebRequest"></param>
//        /// <param name="needCookies"></param>
//        private void SetRequestHeaders(UnityWebRequest unityWebRequest, bool needCookies)
//        {
//            unityWebRequest.SetRequestHeader("User-Agent",
//                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36");
//            unityWebRequest.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
//            unityWebRequest.SetRequestHeader("Accept", "*/*");
//            unityWebRequest.SetRequestHeader("Accept-Language",
//                "zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7,zh-TW;q=0.6,or;q=0.5");
//            if (needCookies)
//                unityWebRequest.SetRequestHeader("Cookie", "__csrf=a1f0fb45eba57a6f4ef6c397e95fb3f9;" +
//                    "MUSIC_U=000C27F0DAA27CBF67A90F39DCADC07CA78CF7F39504D8E052C2A8E5A7E2EB898DF676B1E8C506CB025F6BC1A1FF0DF24A743F6DF412D20239A71539D69EE31F587C15D451B9606B8B74AC676A5782F22745DF6C8C5F104C8D5F4F97A111002D396BDB8A6521E0228B90C933F18706A6922860D55C4EB964E9BB7432AF50DDA8A97B2C34D41A7CCC847C9C852723EE4DA59DF1C9D0A5FF8BE097A0FECD00A54178B33C8D5B617D87085EB7FD362D98867EE6294A363AABC2D43A32A84E93BEE6E76424416D85466C761D5B26DFC91B051C215BB3F5224C822931EE5B19B1675EF7D457FF185AC316F799FF45CC01944A6FED44346B36EDBB55D6E499B9D0AB06B7CFFB3250307899F1656DCB8856939721;" +
//                    "NMTID=YpG1fbxNTQXjldXE;__remember_me=True");
//        }
//    }
//}
namespace NeteaseMusicAPI
{
    /// <summary>
    /// 这个类只用于发送请求
    /// </summary>
    public class NetWork : IDisposable
    {

        private readonly HttpClient _httpClient;
        private readonly NeteaseMusic Api;
        public NetWork(NeteaseMusic api)
        {
            Api = api;
            var handler = new HttpClientHandler
            {
                UseProxy = false, // 如果不需代理
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                UseCookies = false // 手动处理cookie更高效
            };

            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(30) // 设置合理超时
            };
            // 设置默认请求头
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36");
            _httpClient.DefaultRequestHeaders.Accept.ParseAdd("*/*");
            _httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd(
                "zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7,zh-TW;q=0.6,or;q=0.5");
            Api = api;
        }

        public async Task<string> GetAsync(string Url, bool needCsrfToken = false, bool needCookies = false)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, Url);
            UnityEngine.Debug.Log("请求开始");
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            // 使用 Stream 读取并手动指定 UTF-8 编码
            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream, Encoding.UTF8);
            return await reader.ReadToEndAsync();
        }
        public async Task<(string json, Dictionary<string, string> headers)> GetAsyncWithHeader(string Url, bool needCsrfToken = false, bool needCookies = false)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, Url);
            SetRequestHeaders(request, needCookies);
            UnityEngine.Debug.Log("请求开始");
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var headers = new Dictionary<string, string>();
            foreach (var header in response.Headers)
            {
                headers[header.Key] = string.Join(", ", header.Value);
            }
            var json = await response.Content.ReadAsStringAsync();
            return (json, headers);

        }

        public async Task<string> PostAsync<T>(string Url, T @object, bool needCsrfToken = false, bool needCookies = false) where T : class
        {
            var crypto = NeteaseCrypto.Create(@object);
            var content = new FormUrlEncodedContent(crypto.GetQuest());

            using var request = new HttpRequestMessage(HttpMethod.Post, Url)
            {
                Content = content
            };
            SetRequestHeaders(request, needCookies);
            UnityEngine.Debug.Log("请求开始");
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            UnityEngine.Debug.Log("请求成功");
            var a = await response.Content.ReadAsStringAsync();
            UnityEngine.Debug.Log(a);
            return a;
        }

        /// <summary>
        /// 返回包含请求头的json
        /// </summary>
        public async Task<(string json, Dictionary<string, string> headers)> PostAsyncWithHeader<T>(
            string Url, T @object, bool needCsrfToken = false, bool needCookies = false) where T : class
        {
            var crypto = NeteaseCrypto.Create(@object);
            var content = new FormUrlEncodedContent(crypto.GetQuest());

            using var request = new HttpRequestMessage(HttpMethod.Post, Url)
            {
                Content = content
            };
            SetRequestHeaders(request, needCookies);
            UnityEngine.Debug.Log("请求开始");
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var headers = new Dictionary<string, string>();
            foreach (var header in response.Headers)
            {
                headers[header.Key] = string.Join(", ", header.Value);
            }
            var json = await response.Content.ReadAsStringAsync();
            return (json, headers);
        }

        public async Task<string> PostAsync(string Url, string @object, bool needCsrfToken = false, bool needCookies = false)
        {
            var crypto = NeteaseCrypto.Create(@object);
            var content = new FormUrlEncodedContent(crypto.GetQuest());

            using var request = new HttpRequestMessage(HttpMethod.Post, Url)
            {
                Content = content
            };
            SetRequestHeaders(request, needCookies);
            UnityEngine.Debug.Log("请求开始");
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }


        /// <summary>
        /// 添加请求头
        /// </summary>
        private void SetRequestHeaders(HttpRequestMessage request, bool needCookies)
        {
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            if(request.Method == HttpMethod.Post)
            {
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            }
            if (needCookies&& Api.login.TryGetCookie(out string cookie))
            {
                request.Headers.Add("Cookie", cookie);
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}