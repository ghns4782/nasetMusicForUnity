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
    /// 搜索歌曲
    /// </summary>
    /// <param name="searchLong">获取长度</param>
    /// <param name="searchoffset">获取多少个歌曲之后</param>
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
            Debug.Log("您并未登录");
#endif
            throw new TimeoutException("并未登陆");
        }
        return ConvertJsonToSongsData(json);
    }
    /// <summary>
    /// 默认搜索歌曲
    /// </summary>
    /// <param name="searchName"></param>
    /// <returns></returns>
    /// <exception cref="TimeoutException"></exception>
    public async Task<Result> searchSong(string searchName)
    {
        var json = await Api.netWork.PostAsync("https://music.163.com/weapi/cloudsearch/get/web?csrf_token="+ Api.login.TryGetCsrf(),
        new SearchData(searchName, 1, 0, 30, Api.login.TryGetCsrf()) ,needCookies:true);
        var josnD = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        Debug.Log("成功");
        if (josnD["code"].ToString() == "50000005")
        {
#if UNITY_EDITOR
            Debug.Log("您并未登录");
#endif
            throw new TimeoutException("并未登陆");
        }
        return ConvertJsonToSongsData(json);
    }
    /// <summary>
    /// 搜索歌单
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
            Debug.Log("您并未登录");
#endif
            throw new TimeoutException("并未登陆");
        }
        return json;
    }
    /// <summary>
    /// 获取歌曲播放信息
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
    /// 获取歌单信息
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
    /// 获取歌曲详细信息
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

        // 直接获取 privileges 数组
        var privileges = jsonObj["privileges"][0];
            int payed = (int)privileges["payed"];  // 获取 payed 值（1 或 0）
            string maxBrLevel = (string)privileges["maxBrLevel"];  // 获取音质等级字符串
            string playMaxBrLevel = (string)privileges["playMaxBrLevel"];  // 获取音质等级字符串
            string downloadMaxBrLevel = (string)privileges["downloadMaxBrLevel"];  // 获取音质等级字符串
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
            Debug.Log($"JSON 解析失败: {ex.Message}");
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
搜索
https://music.163.com/weapi/cloudsearch/get/web?csrf_token=
传入 
{"hlpretag":"<span class=\"s-fc7\">","hlposttag":"</span>","#/discover":"","s":"TRUE","type":"1","offset":"0","total":"true","limit":"30","csrf_token":"d90cf0865a613764d0d7e88e76bb4e48"}
数据拆解
"s"搜索内容
”offset“查看第多少个之后
”limit“搜索多少个，但是长度始终限制最高30
“TYPE”搜索类型（”0“歌曲）（”10“专辑）（”100“歌手）（“1006”歌词）（“1000”歌单）
*/
/*
https://music.163.com/weapi/v3/song/detail?csrf_token=e36506133e5846380fc5038c2acbf02c
除了歌曲本身的所有信息，除了歌曲的url
*/
/*
用户的创建与收藏的歌单
https://music.163.com/weapi/user/playlist?csrf_token=
传入
{"offset":"0","limit":"1001","uid":"1408203055","csrf_token":""}
*/
/*
指定歌单的歌曲信息

https://music.163.com/weapi/v6/playlist/detail?csrf_token=
https://music.163.com/weapi/v1/play/record?csrf_token=
传入
{"id":"2154103709","offset":"0","total":"true","limit":"1000","n":"1000","csrf_token":""}
非用户创建
{"id":"4971145080","offset":"0","total":"true","limit":"1000","n":"1000","csrf_token":""}
（目前并不知道具体可以请求到多少，用户喜欢可以一次性请求到92条全部的歌曲，但是非用户喜欢一次性只能得到20条，所以并不是很知道原理，但又有的时候可以一次性请求到280多条以上）
*/
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