using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class searchInput : MonoBehaviour
{
    public SearchUi ui;
    public Text searchtext;
    public async void search(string searchData)
    {
        searchtext.text = searchData;
        var a = await NewBehaviourScript._instanc.netease.search.searchSong(searchData);
        ui.Generate(a.Songs);
    }
}
