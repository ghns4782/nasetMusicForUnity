using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class searchInput : MonoBehaviour
{
    public SearchUi ui; 
    public async void search(string searchData)
    {
        var a = await NewBehaviourScript._instanc.netease.search.searchSong(searchData);
        ui.Generate(a.Songs);
    }
}
