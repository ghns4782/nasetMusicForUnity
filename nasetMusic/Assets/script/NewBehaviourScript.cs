using System.Collections.Generic;
using UnityEngine;
using System.Text;
using UnityEngine.Networking;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using NeteaseMusicAPI;
using System.Threading;
using System;
using System.IO;
using Newtonsoft.Json;
using HtmlAgilityPack;
public class NewBehaviourScript : MonoBehaviour
{
    public Image image;
    // Start is called before the first frame update
    //string data = "{\"hlpretag\":\"<span class=\\\"s-fc7\\\">\",\"hlposttag\":\"</span>\",\"s\":\"TRUE\",\"type\":\"1\",\"offset\":\"0\",\"total\":\"true\",\"limit\":\"30\",\"csrf_token\":\"d90cf0865a613764d0d7e88e76bb4e48\"}";
    public string vas = "";
    public string id = "";
    public NeteaseMusic netease =new NeteaseMusic();
    public Song data;
    private static NewBehaviourScript Instanc;
    public static NewBehaviourScript _instanc
    {
        get
        {
            if (Instanc == null)
            {
                Instanc = FindFirstObjectByType<NewBehaviourScript>();
            }
            return Instanc;
        }
    }
    [Button("解密vas")]
    void Start()
    {
        image = GetComponent<Image>();
        LoadCookiesFromFile();
    }
    [Button]
    void clear()
    {
        data = new Song();
    }
    [Button("保存cookie")]
    public void SaveCookiesToFile()
    {
        if (netease.GetCookies(out string cookie))
        {
            // 序列化为 JSON 字符串
            string json = netease.GetCookieJson();

            // 写入文件
            File.WriteAllText(Application.persistentDataPath + "/cookies.json", json);

            Debug.Log("Cookies 已保存到: " + Application.persistentDataPath + "/cookies.json");
        }
    }
    [Button("加载cookie")]
    public void LoadCookiesFromFile()
    { 
        if (!File.Exists(Application.persistentDataPath + "/cookies.json"))
        {
            Debug.LogError("文件不存在: " + Application.persistentDataPath + "/cookies.json");
            return ;
        }

        // 读取 JSON 内容
        string json = File.ReadAllText(Application.persistentDataPath + "/cookies.json");


        netease.SetloginJeson(json);
        Debug.Log("Cookies 已加载: " + json);
    }

    [Button("加密显示")]
    void CryptoVas()
    {
        netease.willbe(
            (pngBytes) =>
            {
                Texture2D texture = new Texture2D(2, 2); // 初始尺寸不重要，LoadImage会覆盖
                texture.LoadImage(pngBytes); // 自动解析PNG字节数组
                texture.Apply(); // 应用纹理更改

                // 将Texture2D转换为Sprite
                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f) // 轴心点居中
                );
                texture.filterMode = FilterMode.Point;
                // 设置到Unity UI Image
                image.sprite = sprite;
            },
            (loge) => {
                Debug.Log(loge);
            }
            );
    }
    [Button]
    private void OnDestroy()
    {
        netease.login.CancelOperation();
    }
    public string serchname;
    public Result song;
    public PlayList ascas;
    [Button]
    public async void search()
    {
        var a= await netease.search.GetplayList("405849034");
        ascas = a;
        //string folderPath = Path.Combine(Application.persistentDataPath, "JsonData");
        //string filePath = Path.Combine(folderPath, "songs.json");

        //// 如果目录不存在，则创建
        //if (!Directory.Exists(folderPath))
        //{
        //    Directory.CreateDirectory(folderPath);
        //}

        ////// 写入 JSON 数据到文件
        //File.WriteAllText(filePath, a);

        //Debug.Log($"JSON 文件已保存到: {filePath}");

    }
    [Button]
    public async Task getsong()
    {
        data=await netease.search.GetSongMessage("2142598645");
        Debug.Log("is get");
    }
    [Button]
    void cater()
    {
        var data1 = NeteaseCrypto.DecryptAES(vas, "aaaabbbbccccdddd");
        var data2 = NeteaseCrypto.DecryptAES(data1, "0CoJUm6Qyw8W8jud");
        Debug.Log(data2);
    }
    public async void PostGetAsync(NeteaseCrypto crypt)
    {
        using UnityWebRequest webRequest = UnityWebRequest.Post(
            //"https://music.163.com/weapi/cloudsearch/get/web?csrf_token=d90cf0865a613764d0d7e88e76bb4e48",
            //"https://music.163.com/weapi/song/enhance/player/url/v1",
            "https://music.163.com/weapi/login/qrcode/unikey",
            new Dictionary<string, string>
            {
            { "encSecKey", crypt.EncSecKey },
            { "params", crypt.Parame },
            { "csrf_token", "a1f0fb45eba57a6f4ef6c397e95fb3f9" }
            });

        SetRequestHeaders(webRequest, true);

        // 使用 SendWebRequest 的异步操作
        var operation = webRequest.SendWebRequest();
        while (!operation.isDone)
        {
            await Task.Yield(); // 等待一帧
        }
        if (webRequest.result != UnityWebRequest.Result.Success)
            throw new UnityException(webRequest.error);
        var a = Encoding.UTF8.GetString(webRequest.downloadHandler.data);
    }
    #region post请求头，所有post请求必须经过这个
    private void SetRequestHeaders(UnityWebRequest unityWebRequest, bool needCookies)
    {
        unityWebRequest.SetRequestHeader("User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36");
        unityWebRequest.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
        unityWebRequest.SetRequestHeader("Accept", "*/*");
        unityWebRequest.SetRequestHeader("Accept-Language",
            "zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7,zh-TW;q=0.6,or;q=0.5");
        if (needCookies)
            unityWebRequest.SetRequestHeader("Cookie", "__csrf=a1f0fb45eba57a6f4ef6c397e95fb3f9;" +
                "MUSIC_U=000C27F0DAA27CBF67A90F39DCADC07CA78CF7F39504D8E052C2A8E5A7E2EB898DF676B1E8C506CB025F6BC1A1FF0DF24A743F6DF412D20239A71539D69EE31F587C15D451B9606B8B74AC676A5782F22745DF6C8C5F104C8D5F4F97A111002D396BDB8A6521E0228B90C933F18706A6922860D55C4EB964E9BB7432AF50DDA8A97B2C34D41A7CCC847C9C852723EE4DA59DF1C9D0A5FF8BE097A0FECD00A54178B33C8D5B617D87085EB7FD362D98867EE6294A363AABC2D43A32A84E93BEE6E76424416D85466C761D5B26DFC91B051C215BB3F5224C822931EE5B19B1675EF7D457FF185AC316F799FF45CC01944A6FED44346B36EDBB55D6E499B9D0AB06B7CFFB3250307899F1656DCB8856939721;" +
                "NMTID=YpG1fbxNTQXjldXE;__remember_me=True");
    }
    #endregion
}

/*
mp3音乐请求
请求示例
{"ids":"[2142598645]","level":"standard","encodeType":"mp3","csrf_token":"c6307937dc5fa1b82b157700ed97e801"}
standard	标准音质	128 kbps	l (low)	免费用户默认音质
higher	较高音质	192 kbps	m (medium)	
exhigh	极高音质	320 kbps	h (high)	普通VIP可用
lossless	无损音质	≈900 kbps	sq (super)	需要VIP或单独购买
hires	高清无损	>900 kbps	无	特殊版权歌曲，需高级VIP
其中搜索时会包含有哪些音质
lossless及以上为flac
但似乎并不支持wav的请求方法
请求位置
"https://music.163.com/weapi/song/enhance/player/url/v1"
*/
/*
个性化推荐歌单
https://music.163.com/weapi/discovery/recommend/resource?csrf_token=d90cf0865a613764d0d7e88e76bb4e48
传入：
{"csrf_token":"d90cf0865a613764d0d7e88e76bb4e48"}
*/

/*
歌词获取
https://music.163.com/weapi/song/lyric?csrf_token=d90cf0865a613764d0d7e88e76bb4e48
POST传入
{"id":1449668970,"lv":-1,"tv":-1,"csrf_token":"d90cf0865a613764d0d7e88e76bb4e48"}
*/

/*
榜单
https://music.163.com/discover/toplist
是一个get方法，来获取一个html文档来获得特色榜单其中包含了各个榜单的id，可以使用？id=来进行id
*/
/*
网易云首页，其中包含热门推荐
https://music.163.com/discover
GET，其中包含了推荐热门
*/
/*
https://music.163.com/weapi/v3/song/detail?csrf_token=e36506133e5846380fc5038c2acbf02c
除了歌曲本身的所有信息，除了歌曲的url
*/