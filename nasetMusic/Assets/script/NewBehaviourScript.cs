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
    [Button("����vas")]
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
    [Button("����cookie")]
    public void SaveCookiesToFile()
    {
        if (netease.GetCookies(out string cookie))
        {
            // ���л�Ϊ JSON �ַ���
            string json = netease.GetCookieJson();

            // д���ļ�
            File.WriteAllText(Application.persistentDataPath + "/cookies.json", json);

            Debug.Log("Cookies �ѱ��浽: " + Application.persistentDataPath + "/cookies.json");
        }
    }
    [Button("����cookie")]
    public void LoadCookiesFromFile()
    { 
        if (!File.Exists(Application.persistentDataPath + "/cookies.json"))
        {
            Debug.LogError("�ļ�������: " + Application.persistentDataPath + "/cookies.json");
            return ;
        }

        // ��ȡ JSON ����
        string json = File.ReadAllText(Application.persistentDataPath + "/cookies.json");


        netease.SetloginJeson(json);
        Debug.Log("Cookies �Ѽ���: " + json);
    }

    [Button("������ʾ")]
    void CryptoVas()
    {
        netease.willbe(
            (pngBytes) =>
            {
                Texture2D texture = new Texture2D(2, 2); // ��ʼ�ߴ粻��Ҫ��LoadImage�Ḳ��
                texture.LoadImage(pngBytes); // �Զ�����PNG�ֽ�����
                texture.Apply(); // Ӧ���������

                // ��Texture2Dת��ΪSprite
                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f) // ���ĵ����
                );
                texture.filterMode = FilterMode.Point;
                // ���õ�Unity UI Image
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

        //// ���Ŀ¼�����ڣ��򴴽�
        //if (!Directory.Exists(folderPath))
        //{
        //    Directory.CreateDirectory(folderPath);
        //}

        ////// д�� JSON ���ݵ��ļ�
        //File.WriteAllText(filePath, a);

        //Debug.Log($"JSON �ļ��ѱ��浽: {filePath}");

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

        // ʹ�� SendWebRequest ���첽����
        var operation = webRequest.SendWebRequest();
        while (!operation.isDone)
        {
            await Task.Yield(); // �ȴ�һ֡
        }
        if (webRequest.result != UnityWebRequest.Result.Success)
            throw new UnityException(webRequest.error);
        var a = Encoding.UTF8.GetString(webRequest.downloadHandler.data);
    }
    #region post����ͷ������post������뾭�����
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
mp3��������
����ʾ��
{"ids":"[2142598645]","level":"standard","encodeType":"mp3","csrf_token":"c6307937dc5fa1b82b157700ed97e801"}
standard	��׼����	128 kbps	l (low)	����û�Ĭ������
higher	�ϸ�����	192 kbps	m (medium)	
exhigh	��������	320 kbps	h (high)	��ͨVIP����
lossless	��������	��900 kbps	sq (super)	��ҪVIP�򵥶�����
hires	��������	>900 kbps	��	�����Ȩ��������߼�VIP
��������ʱ���������Щ����
lossless������Ϊflac
���ƺ�����֧��wav�����󷽷�
����λ��
"https://music.163.com/weapi/song/enhance/player/url/v1"
*/
/*
���Ի��Ƽ��赥
https://music.163.com/weapi/discovery/recommend/resource?csrf_token=d90cf0865a613764d0d7e88e76bb4e48
���룺
{"csrf_token":"d90cf0865a613764d0d7e88e76bb4e48"}
*/

/*
��ʻ�ȡ
https://music.163.com/weapi/song/lyric?csrf_token=d90cf0865a613764d0d7e88e76bb4e48
POST����
{"id":1449668970,"lv":-1,"tv":-1,"csrf_token":"d90cf0865a613764d0d7e88e76bb4e48"}
*/

/*
��
https://music.163.com/discover/toplist
��һ��get����������ȡһ��html�ĵ��������ɫ�����а����˸����񵥵�id������ʹ�ã�id=������id
*/
/*
��������ҳ�����а��������Ƽ�
https://music.163.com/discover
GET�����а������Ƽ�����
*/
/*
https://music.163.com/weapi/v3/song/detail?csrf_token=e36506133e5846380fc5038c2acbf02c
���˸��������������Ϣ�����˸�����url
*/