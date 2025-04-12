# nasetMusicForUnity

## 目录
- [主要功能](#主要功能)
- [其他类](#可视化)
- [使用示例](#使用示例)
- [安装与依赖](#安装与依赖)
- [贡献指南](#贡献指南)

## 主要功能
主要功能大多数由NeteaseMusic来承担，包括但不限于登陆以及信息获取
但只会返回各种URL
返回的大多数由nasetMusic\Assets\script\mymakethepluging\NeteaseMusic\Songsdata.cs
中的Song来接受大部分的音乐信息
并且所有的搜索请求都要使用NeteaseMusic.search属性来进行搜索，也可以自己写，或者添加到search，但由于请求需要配套的login与Network类所以各位见仁见智
预计会做目前没做完的有
//歌单
//歌手信息
//用户详细信息

## 其他类
''NewBehaviourScript'' 这个类仅用于试做使用，其他类使用此处也仅仅是因为其中保存与自动实例的NeteaseMusic类
''PorjectAudioPlayer'' 该类用于播放指定网络地址的声音，通过song的信息来交给search来进行请求得到的音乐地址，但由于MP3不支持流式加载，所以会检测flac还是mp3
''WindowUtility''该类用于控制窗口，包括软边框、去窗口头、拖动、最小化、最大化//预计未来会做窗口缩放（header中包含了使用案例，butterbutton只是生成了一个按下时就报告的事件）
''SearchUi''用于获取


所有功能均来自网页API，并且实际效果运行小于直接用APP或者EXE，所以本项目仅供学习，如有侵权立刻删库跑路
