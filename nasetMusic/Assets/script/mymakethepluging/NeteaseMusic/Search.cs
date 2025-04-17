using NeteaseMusicAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Search
{
    private NeteaseMusic Api;
    public Search(NeteaseMusic api)
    {
        Api = api;
    }
    /// <summary>
    /// ��������
    /// </summary>
    /// <param name="searchLong">��ȡ����</param>
    /// <param name="searchoffset">��ȡ���ٸ�����֮��</param>
    /// <returns></returns>
    /// <exception cref="TimeoutException"></exception>
    public async Task<Result> searchSong(string searchName, int searchLong = 30, int searchoffset = 0)
    {
        var json = await Api.netWork.PostAsync("https://music.163.com/weapi/cloudsearch/get/web?csrf_token=" + Api.login.TryGetCsrf(),
        new SearchData(searchName, 1, searchoffset, searchLong, Api.login.TryGetCsrf()));
        var josnD = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        if (josnD["code"].ToString() == "50000005")
        {
#if UNITY_EDITOR
            Debug.Log("����δ��¼");
#endif
            throw new TimeoutException("��δ��½");
        }
        return ConvertJsonToSongsData(json);
    }
    /// <summary>
    /// Ĭ����������
    /// </summary>
    /// <param name="searchName"></param>
    /// <returns></returns>
    /// <exception cref="TimeoutException"></exception>
    public async Task<Result> searchSong(string searchName)
    {
        var json = await Api.netWork.PostAsync("https://music.163.com/weapi/cloudsearch/get/web?csrf_token="+ Api.login.TryGetCsrf(),
        new SearchData(searchName, 1, 0, 30, Api.login.TryGetCsrf()) ,needCookies:true);
        var josnD = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        Debug.Log("�ɹ�");
        if (josnD["code"].ToString() == "50000005")
        {
#if UNITY_EDITOR
            Debug.Log("����δ��¼");
#endif
            throw new TimeoutException("��δ��½");
        }
        return ConvertJsonToSongsData(json);
    }
    /// <summary>
    /// �����赥
    /// </summary>
    /// <param name="searchName"></param>
    /// <param name="searchLong"></param>
    /// <param name="searchoffset"></param>
    /// <returns></returns>
    /// <exception cref="TimeoutException"></exception>
    public async Task<string> searchPlaylists(string searchName, int searchLong = 30, int searchoffset = 0)
    {
        var json = await Api.netWork.PostAsync("https://music.163.com/weapi/cloudsearch/get/web?csrf_token="+ Api.login.TryGetCsrf(),
        new SearchData(searchName, 1000, searchoffset, searchLong, Api.login.TryGetCsrf()));
        var josnD = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        if (josnD["code"].ToString() == "50000005")
        {
#if UNITY_EDITOR
            Debug.Log("����δ��¼");
#endif
            throw new TimeoutException("��δ��½");
        }
        return json;
    }
    /// <summary>
    /// ��ȡ����������Ϣ
    /// </summary>
    /// <returns></returns>
    public async Task<string> GetSongAsync(string id, AudioLevel Level)
    {
        var json = await Api.netWork.PostAsync("https://music.163.com/weapi/song/enhance/player/url/v1?csrf_token="+ Api.login.TryGetCsrf(),
               new
               {
                   ids = $"[{id}]",
                   level = Level.ToString().ToLower(),
                   encodeType = "mp3",
                   csrf_token = Api.login.TryGetCsrf()
               },
           needCookies:true
           );
        var jsonObj = JObject.Parse(json);
        string maxBrLevel = (string)jsonObj["data"][0]["url"];
        return maxBrLevel;
    }
    /// <summary>
    /// ��ȡ�赥��Ϣ
    /// </summary>
    /// <param name="ListId"></param>
    /// <returns></returns>
    public async Task<PlayList> GetplayList(string ListId)
    {
        string json = await Api.netWork.PostAsync("https://music.163.com/weapi/v6/playlist/detail?csrf_token=" + Api.login.TryGetCsrf(),
               new
               {
                   id = ListId,
                   offset = 0,
                   total = true,
                   limit = 1000,
                   n=1000,
                   csrf_token = Api.login.TryGetCsrf()
               },
               needCookies: true
            );
        playLsitCallback ascas = JsonConvert.DeserializeObject<playLsitCallback>(json);
        if (ascas.code == "200")
        {
            ascas.MergePrivileges();
            return ascas.plsylist;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// ��ȡ������ϸ��Ϣ
    /// </summary>
    /// <param name="songId"></param>
    /// <returns></returns>
    public async Task<Song> GetSongMessage(string songId)
    {
        var json = await Api.netWork.PostAsync("https://music.163.com/weapi/v3/song/detail?csrf_token=" + Api.login.TryGetCsrf(),
               new
               {
                   id = songId,
                   c = $"[{{\"id\":\"{songId}\"}}]",
                   csrf_token = Api.login.TryGetCsrf()
               },
               needCookies: true
               );

        return ConvertJsonToSongData(json);
    }
    public async Task<likeCallBack> GetUserLike(string UserId)
    {
        var json = await Api.netWork.PostAsync("https://music.163.com/weapi/user/playlist?csrf_token=" + Api.login.TryGetCsrf(),
            new
            {
                offset = 0,
                limit = 1001,
                uid = UserId,
                csrf_token = Api.login.TryGetCsrf()
            },

            needCookies: true
            );
        likeCallBack ascas = JsonConvert.DeserializeObject<likeCallBack>(json);
        if (ascas.code == "200")
        {
            return ascas;
        }
        else
        {
            return null;
        }
    }
    //{"offset":"0","limit":"1001","uid":"1408203055","csrf_token":""}
    public async Task<string> GetSongUrl(Song songData, AudioLevel level = AudioLevel.Standard)
    {
        var json = await Api.search.GetSongAsync(songData.Id.ToString(), songData.ClampToMax(level));
        return json;
    }
    public Song ConvertJsonToSongData(string jsonString)
    {
        // Configure JsonSerializerSettings if needed
        var settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore
        };

        // Add StringEnumConverter to properly handle enum conversion
        settings.Converters.Add(new StringEnumConverter());

        // Deserialize the JSON string into SongData object
        SongData songData = JsonConvert.DeserializeObject<SongData>(jsonString, settings);
        var jsonObj = JObject.Parse(jsonString);

        // ֱ�ӻ�ȡ privileges ����
        var privileges = jsonObj["privileges"][0];
            int payed = (int)privileges["payed"];  // ��ȡ payed ֵ��1 �� 0��
            string maxBrLevel = (string)privileges["maxBrLevel"];  // ��ȡ���ʵȼ��ַ���
            string playMaxBrLevel = (string)privileges["playMaxBrLevel"];  // ��ȡ���ʵȼ��ַ���
            string downloadMaxBrLevel = (string)privileges["downloadMaxBrLevel"];  // ��ȡ���ʵȼ��ַ���
        songData.Songs[0].privilege.Payed = payed;
        songData.Songs[0].privilege = new Privilege();
        songData.Songs[0].privilege.MaxBrLevel = JsonConvert.DeserializeObject<AudioLevel>($"\"{maxBrLevel}\"", new StringEnumConverter());
        songData.Songs[0].privilege.PlayMaxBrLevel = JsonConvert.DeserializeObject<AudioLevel>($"\"{playMaxBrLevel}\"", new StringEnumConverter());
        songData.Songs[0].privilege.downloadMaxBrLevel = JsonConvert.DeserializeObject<AudioLevel>($"\"{downloadMaxBrLevel}\"", new StringEnumConverter());
        songData.Songs[0].setapi(Api: Api);
        return songData.Songs[0];
    }

    public Result ConvertJsonToSongsData(string json)
    {
        // Configure JsonSerializerSettings if needed
        MusicApiResponse response = null;
        try
        {
            response = JsonConvert.DeserializeObject<MusicApiResponse>(json);
            Console.WriteLine($"Code: {response.Code}, Song count: {response.Result.SongCount}");
        }
        catch (JsonException ex)
        {
            Debug.Log($"JSON ����ʧ��: {ex.Message}");
        }
        if(response.Code == "200")
        {
            return response.Result;
        }
        else
        {
            return null;
        }
    }

    public class SearchData {
        public string hlpretag = "<span class=\"s-fc7\">";
        public string hlposttag = "</span>";
        public string s;
        public int type;
        public int offset;
        public bool total=true;
        public int limit;
        public string csrf_token="";
        public SearchData(string content,int seachType,int Offset,int Limit,string token)
        {
            s = content;
            type = seachType;
            offset = Offset;
            limit = Limit;
            csrf_token = token;
        }
        public SearchData(string content, int seachType, int Offset, int Limit)
        {
            s = content;
            type = seachType;
            offset = Offset;
            limit = Limit;
        }
    }
}

/*
����
https://music.163.com/weapi/cloudsearch/get/web?csrf_token=
���� 
{"hlpretag":"<span class=\"s-fc7\">","hlposttag":"</span>","#/discover":"","s":"TRUE","type":"1","offset":"0","total":"true","limit":"30","csrf_token":"d90cf0865a613764d0d7e88e76bb4e48"}
���ݲ��
"s"��������
��offset���鿴�ڶ��ٸ�֮��
��limit���������ٸ������ǳ���ʼ���������30
��TYPE���������ͣ���0������������10��ר��������100�����֣�����1006����ʣ�����1000���赥��
*/
/*
https://music.163.com/weapi/v3/song/detail?csrf_token=e36506133e5846380fc5038c2acbf02c
���˸��������������Ϣ�����˸�����url
*/
/*
�û��Ĵ������ղصĸ赥
https://music.163.com/weapi/user/playlist?csrf_token=
����
{"offset":"0","limit":"1001","uid":"1408203055","csrf_token":""}
*/
/*
ָ���赥�ĸ�����Ϣ

https://music.163.com/weapi/v6/playlist/detail?csrf_token=
https://music.163.com/weapi/v1/play/record?csrf_token=
����
{"id":"2154103709","offset":"0","total":"true","limit":"1000","n":"1000","csrf_token":""}
���û�����
{"id":"4971145080","offset":"0","total":"true","limit":"1000","n":"1000","csrf_token":""}
��Ŀǰ����֪������������󵽶��٣��û�ϲ������һ��������92��ȫ���ĸ��������Ƿ��û�ϲ��һ����ֻ�ܵõ�20�������Բ����Ǻ�֪��ԭ�������е�ʱ�����һ��������280�������ϣ�
*/
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