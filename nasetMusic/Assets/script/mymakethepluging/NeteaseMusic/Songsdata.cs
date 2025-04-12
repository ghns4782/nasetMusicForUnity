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
    /// ����id
    /// </summary>
    [JsonProperty("id")]
    public int Id;
    /// <summary>
    /// ��������
    /// </summary>
    [JsonProperty("name")]
    public string Name;
    /// <summary>
    /// ��ʱδ֪
    /// </summary>
    [JsonProperty("tns")]
    public string[] Tns ;
    /// <summary>
    /// ��ʱδ֪
    /// </summary>
    [JsonProperty("alias")]
    public string[] Alias;
}
[System.Serializable]
public partial class Album
{
    /// <summary>
    /// ר��id
    /// </summary>
    [JsonProperty("id")]
    public int Id;
    /// <summary>
    /// ר������
    /// </summary>
    [JsonProperty("name")]
    public string Name;
    /// <summary>
    /// ר������
    /// </summary>
    [JsonProperty("picUrl")]
    public string PicUrl;
    /// <summary>
    /// ����
    /// </summary>
    [JsonProperty("tns")]
    public string[] Tns;
    /// <summary>
    /// ��ʱ��֪��
    /// </summary>
    [JsonProperty("pic_str")]
    public string PicStr;
    /// <summary>
    /// ��ʱ��֪��
    /// </summary>
    [JsonProperty("pic")]
    public long Pic;

}
[System.Serializable]
public partial class Song
{
    /// <summary>
    /// ����
    /// </summary>
    [JsonProperty("name")]
    public string Name;
    /// <summary>
    /// ����id
    /// </summary>
    [JsonProperty("id")]
    public string Id;
    /// <summary>
    /// ר����Ϣ
    /// </summary>
    [JsonProperty("al")]
    public Album Album;
    /// <summary>
    /// ����
    /// </summary>
    [JsonProperty("ar")]
    public Artist[] Artists;
    /// <summary>
    /// mv id
    /// </summary>
    [JsonProperty("mv")]
    public int Mv;
    /// <summary>
    /// �Ƿ񸶷�
    /// </summary>
    [JsonProperty("payed")]
    public bool Payed;
    /// <summary>
    /// ʱ��
    /// </summary>
    [JsonProperty("dt")]
    public int Duration;
    /// <summary>
    /// ����
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
        // ������ת��ΪTimeSpan
        TimeSpan timeSpan = TimeSpan.FromMilliseconds(Duration);

        // ��ȡ���Ӻ���
        int minutes = timeSpan.Minutes;
        int seconds = timeSpan.Seconds;

        // ��ʽ��Ϊ"����:��"
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
    /// �Ƿ񸶷�
    /// </summary>
    [JsonProperty("payed")]
    public int Payed;
    /// <summary>
    /// ���ȼ�
    /// </summary>
    [JsonProperty("maxBrLevel")]
    [JsonConverter(typeof(StringEnumConverter))]
    public AudioLevel MaxBrLevel;
    /// <summary>
    /// ��󲥷ŵȼ�
    /// </summary>
    [JsonProperty("playMaxBrLevel")]
    [JsonConverter(typeof(StringEnumConverter))]
    public AudioLevel PlayMaxBrLevel;
    /// <summary>
    /// ������صȼ�
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

    // ����δֵ֪
    [JsonProperty("unknown")]
    Unknown
}