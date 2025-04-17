using System.Collections;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnlimitedScrollUI;

public class BetterICellButton : MonoBehaviour,ICell
{
    Song song;
    public int songIndex;
    public Text Index;
    public Text songname;
    public Text songArName;
    public Image Alimage;
    private SearchUi map;
    public Shadow shadow;
    public betterButton button;
    public void OnBecomeInvisible(ScrollerPanelSide side)
    {
    }
    public void mapset(SearchUi minmap)
    {
        map = minmap;
    }
    public void OnBecomeVisible(ScrollerPanelSide side)
    {
        shadow.enabled = false;
        button.OnClickEnter.AddListener( () =>
        {
            shadow.enabled = true;
        });
        button.OnClickExit.AddListener(() =>
        {
            shadow.enabled = false;
        });
        Index.text = (songIndex+1).ToString();
        songArName.text = song.GetArtNames();
        songname.text = song.Name;

        map.GetSprite(song.Album.PicUrl, (Sprite) =>
        {
            if (Alimage != null||Alimage == null)
            {
                Alimage.sprite = Sprite;
            }
        });
    }
    public void SetSong(Song inSong)
    {
        song = inSong;
    }
    public void addsong()
    {
        Debug.Log(song.privilege.MaxBrLevel);
        PorjectAudioPlayer.instanc.AddenderPlayList(song);
    }
}
