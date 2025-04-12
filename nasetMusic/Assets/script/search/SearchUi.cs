using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnlimitedScrollUI;

public class SearchUi : MonoBehaviour
{
    public GameObject cell;
    public List<Song> Songs;
    public Dictionary<string, Sprite> maps=new Dictionary<string, Sprite>();
    private IUnlimitedScroller unlimitedScroller;

    public void Generate(List<Song> Songs)
    {
        unlimitedScroller.Clear();
        this.Songs = Songs;
        unlimitedScroller.Generate(cell, Songs.Count, (index, iCell) => {
            var regularCell = iCell as BetterICellButton;
            regularCell.SetSong(Songs[index]);
            regularCell.songIndex = index;
            regularCell.mapset(this);
        });
        var rec = GetComponent<RectTransform>();
        rec.anchorMax = new Vector2(1, 1);
        rec.anchorMin = new Vector2(0, 1);

    }

    private void Start()
    {
        unlimitedScroller = GetComponent<IUnlimitedScroller>();
        // Wait until the scroller size was set by other layout controllers.

    }

    private IEnumerator DelayGenerate()
    {
        yield return new WaitForEndOfFrame();
        unlimitedScroller.Generate(cell, Songs.Count, (index, iCell) => {
            var regularCell = iCell as BetterICellButton;
            regularCell.SetSong(Songs[index]);
        });
    }
    public void GetSprite(string Url, Action<Sprite> OutText)
    {
        StartCoroutine(GetUrlPicter(Url, OutText));
    }
    public IEnumerator GetUrlPicter(string Url,Action<Sprite> OutText)
    {
        if(maps.TryGetValue(Url,out Sprite sp))
        {
            OutText?.Invoke(sp);
        }
        else
        {
            using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(Url))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("图片加载失败: " + webRequest.error);
                }
                else
                {
                    // 获取下载的纹理
                    Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);

                    // 创建Sprite
                    Sprite sprite = Sprite.Create(
                        texture,
                        new Rect(0, 0, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f) // 轴心点居中
                    );
                    maps[Url] = sprite;
                    OutText?.Invoke(maps[Url]);
                }
            }
        }
    }
}
