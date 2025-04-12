# nasetMusicForUnity

## 目录
- [主要功能](#主要功能)
- [其他类](#其他类)
- [安装与依赖](#安装与依赖)
- [免责声明](#免责声明)

## 主要功能

主要功能大多数由 `NeteaseMusic` 类实现，包括但不限于：
- 用户登录
- 音乐信息获取
- 返回各种URL

音乐信息由 `nasetMusic\Assets\script\mymakethepluging\NeteaseMusic\Songsdata.cs` 中的 `Song` 类接收。

### 搜索功能
- 所有搜索请求需使用 `NeteaseMusic.search` 属性
- 支持自定义搜索实现
- 需要配套的 `login` 与 `Network` 类

### 计划功能
- 歌单功能
- 歌手信息
- 用户详细信息

## 其他类

### `NewBehaviourScript`
- 仅用于测试目的
- 包含自动实例化的 `NeteaseMusic` 类

### `PorjectAudioPlayer`
- 播放指定网络地址的音频
- 通过 `song` 信息请求音乐地址
- 自动检测 `flac` 或 `mp3` 格式
- 注意：MP3不支持流式加载

### `WindowUtility`
- 窗口控制功能：
  - 软边框
  - 去除窗口标题栏
  - 窗口拖动
  - 最小化/最大化
- 未来计划：窗口缩放功能
- `/header` 中包含使用案例
- `butterbutton` 仅生成按下事件报告

### `SearchUi`
- 图片获取功能
- 实现 `IUnlimitedScroller` 接口的无限滚动列表
- 使用快速实例化/删除机制
- 采用 `action` 处理图片加载，防止快速滚动时对象被删除

## 安装与依赖

### 必需依赖
- `Newtonsoft Json`：用于JSON序列化/反序列化
- `QRCode`：生成登录二维码
- `System.Drawing`：QRCode包的依赖项
- `BouncyCastle.Cryptography`：请求加密使用

## 免责声明

⚠️ **重要声明**：
- 所有功能均基于网页API实现
- 实际运行效果可能不如官方APP或EXE
- 本项目仅供学习研究使用
- 如涉及侵权，将立即删除仓库
