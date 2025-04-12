using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SongsButtonMessgae : MonoBehaviour
{
    public Slider Leightslider;
    public Text leight;
    public Text Nowplaying;
    public Image buttonMage;
    public Sprite isplaying;
    public Sprite isDown;
    public void Start()
    {
        PorjectAudioPlayer.instanc.PlayTimeCall += playTimecall;
        PorjectAudioPlayer.instanc.PlaylenghtCall += playleightcall;
        PorjectAudioPlayer.instanc.OnloadSonging += IamgeRedSong;
    }
    private float thisSongsleight;
    #region 中间部分
    public void playTimecall(float Time)
    {
        Nowplaying.text = Togetmuine(Time);
        Leightslider.value = Time/ thisSongsleight;
    }
    public void playleightcall(float time)
    {
        leight.text = Togetmuine(time);
        thisSongsleight = time;
    }
    public void Update()
    {
        if (PorjectAudioPlayer.instanc.ISOplay)
        {
            buttonMage.sprite = isplaying;
        }
        else
        {
            buttonMage.sprite = isDown;
        }
    }
    public string Togetmuine(float time)
    {
        float totalSeconds = time;
        int minutes = Mathf.FloorToInt(totalSeconds / 60);
        int seconds = Mathf.FloorToInt(totalSeconds % 60);
        return $"{minutes}:{seconds}";
    }
    #endregion

    public SearchUi UI;
    public Image SongAL;
    public Text ARtext;
    public void IamgeRedSong(Song song)
    {
        UI.GetSprite(song.Album.PicUrl, (Sprite) =>
        {
            Debug.Log("变更图片");
            SongAL.sprite = Sprite;
            SongAL.color = new Color(1, 1, 1, 1);
        });
        ARtext.text = song.Name + " - " + song.GetArtNames();
    }
}
