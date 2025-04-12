using NeteaseMusicAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
[System.Serializable]
public partial class Artist
{
    /// <summary>
    /// 艺人id
    /// </summary>
    [JsonProperty("id")]
    public int Id;
    /// <summary>
    /// 艺人名称
    /// </summary>
    [JsonProperty("name")]
    public string Name;
    /// <summary>
    /// 暂时未知
    /// </summary>
    [JsonProperty("tns")]
    public string[] Tns ;
    /// <summary>
    /// 暂时未知
    /// </summary>
    [JsonProperty("alias")]
    public string[] Alias;
}
[System.Serializable]
public partial class Album
{
    /// <summary>
    /// 专辑id
    /// </summary>
    [JsonProperty("id")]
    public int Id;
    /// <summary>
    /// 专辑名称
    /// </summary>
    [JsonProperty("name")]
    public string Name;
    /// <summary>
    /// 专辑封面
    /// </summary>
    [JsonProperty("picUrl")]
    public string PicUrl;
    /// <summary>
    /// 别名
    /// </summary>
    [JsonProperty("tns")]
    public string[] Tns;
    /// <summary>
    /// 暂时不知道
    /// </summary>
    [JsonProperty("pic_str")]
    public string PicStr;
    /// <summary>
    /// 暂时不知道
    /// </summary>
    [JsonProperty("pic")]
    public long Pic;

}
[System.Serializable]
public partial class Song
{
    /// <summary>
    /// 歌名
    /// </summary>
    [JsonProperty("name")]
    public string Name;
    /// <summary>
    /// 歌曲id
    /// </summary>
    [JsonProperty("id")]
    public string Id;
    /// <summary>
    /// 专辑信息
    /// </summary>
    [JsonProperty("al")]
    public Album Album;
    /// <summary>
    /// 艺人
    /// </summary>
    [JsonProperty("ar")]
    public Artist[] Artists;
    /// <summary>
    /// mv id
    /// </summary>
    [JsonProperty("mv")]
    public int Mv;
    /// <summary>
    /// 是否付费
    /// </summary>
    [JsonProperty("payed")]
    public bool Payed;
    /// <summary>
    /// 时长
    /// </summary>
    [JsonProperty("dt")]
    public int Duration;
    /// <summary>
    /// 加密
    /// </summary>
    [JsonProperty("privilege")]
    public Privilege privilege;

    private NeteaseMusic api;
    public void setapi(NeteaseMusic Api)
    {
        api = Api;
    }
    public string GetArtNames()
    {
        return string.Join(" / ", this.Artists.Select(a => a.Name));
    }
    public string GetSongleght()
    {
        // 将毫秒转换为TimeSpan
        TimeSpan timeSpan = TimeSpan.FromMilliseconds(Duration);

        // 提取分钟和秒
        int minutes = timeSpan.Minutes;
        int seconds = timeSpan.Seconds;

        // 格式化为"分钟:秒"
        return $"{minutes}:{seconds:D2}";

    }
    public AudioLevel ClampToMax(AudioLevel value)
    {
        if (value != AudioLevel.Unknown)
        {
            return (int)value > (int)this.privilege.MaxBrLevel ? this.privilege.MaxBrLevel : value;
        }
        else
        {
            return AudioLevel.Standard;
        }
    }
}
[System.Serializable]
public partial class Privilege
{
    /// <summary>
    /// 是否付费
    /// </summary>
    [JsonProperty("payed")]
    public int Payed;
    /// <summary>
    /// 最大等级
    /// </summary>
    [JsonProperty("maxBrLevel")]
    [JsonConverter(typeof(StringEnumConverter))]
    public AudioLevel MaxBrLevel;
    /// <summary>
    /// 最大播放等级
    /// </summary>
    [JsonProperty("playMaxBrLevel")]
    [JsonConverter(typeof(StringEnumConverter))]
    public AudioLevel PlayMaxBrLevel;
    /// <summary>
    /// 最大下载等级
    /// </summary>
    [JsonProperty("downloadMaxBrLevel")]
    [JsonConverter(typeof(StringEnumConverter))]
    public AudioLevel downloadMaxBrLevel;
}
[System.Serializable]
public partial class SongData
{
    [JsonProperty("songs")]
    public List<Song> Songs;
}
[System.Serializable]
public partial class MusicApiResponse
{
    [JsonProperty("result")]
    public Result Result;

    [JsonProperty("code")]
    public string Code;
}
[System.Serializable]
public class Result
{

    [JsonProperty("songs")]
    public List<Song> Songs;

    [JsonProperty("songCount")]
    public int SongCount;
}

[System.Serializable]
public enum AudioLevel
{
    [JsonProperty("standard")]
    Standard,

    [JsonProperty("higher")]
    Higher,

    [JsonProperty("exhigh")]
    ExHigh,

    [JsonProperty("lossless")]
    Lossless,

    [JsonProperty("hires")]
    HiRes,

    // 处理未知值
    [JsonProperty("unknown")]
    Unknown
}