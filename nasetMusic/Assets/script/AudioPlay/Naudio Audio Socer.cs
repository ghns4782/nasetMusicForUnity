using Org.BouncyCastle.Bcpg;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static QRCoder.PayloadGenerator;
public class PorjectAudioPlayer : MonoBehaviour
{
    private static PorjectAudioPlayer _instanc;
    public static PorjectAudioPlayer instanc
    {
        get
        {
            if (_instanc == null)
            {
                _instanc = FindFirstObjectByType<PorjectAudioPlayer>();
                if (_instanc == null)
                {
                    _instanc = new GameObject("audioplayerobject").AddComponent<PorjectAudioPlayer>();
                }
            }
            return _instanc;
        }
    }
    private UnityWebRequest audioWebRequest;
    private AudioSource audioSource;
    private AudioClip streamingClip;
    private string currentUrl;
    private bool isDownloading;
    public float downloadProgress;
    private long totalBytes;
    private long receivedBytes;
    public List<Song> playList;
    private int Index;
    public Action<float> PlayTimeCall;
    public Action<float> PlaylenghtCall;
    public Action<Song> OnloadSonging;
    public AudioLevel level;
    public int GetIndex => Index;
    public bool ISOplay => audioSource.isPlaying;
    public Song GetNowPlayingSong
    {
        get
        {
            if (audioSource.clip != null|| playList.Count<=0)
            {
                return playList[Index];
            }else
            {
                return null;
            }
        }
    }
    public async void AddenderPlayList(Song addsong)
    {
        if (playList == null)
        {
            playList = new List<Song>();
        }
        playList.Add(addsong);
        if (audioSource.clip == null)
        {
            PlayAudio(await NewBehaviourScript._instanc.netease.search.GetSongUrl(playList[Index], level));
        }
    }
    #region 网络加载音乐部分
    // 开始加载并播放音频
    Coroutine downloadcor;
    public void PlayAudio(string url)
    {
        if (isDownloading)
        {
            StopCoroutine(downloadcor);
            if (audioWebRequest != null && !audioWebRequest.isDone)
            {
                audioWebRequest.Abort();
            }
        }
        Uri uri = new Uri(url);
        string baseUrl = uri.GetLeftPart(UriPartial.Path);
        currentUrl = baseUrl;
        bool isMp3 = currentUrl.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase);
        if (isMp3)
        {
            downloadcor = StartCoroutine(StreamAudio(currentUrl, AudioType.MPEG));
        }
        else
        {
            downloadcor = StartCoroutine(StreamAudio(currentUrl, AudioType.UNKNOWN));
        }
    }

    // 流式加载音频的协程
    private IEnumerator StreamAudio(string url, AudioType audioType)
    {
        OnloadSonging?.Invoke(playList[Index]);
        switch (audioType)
        {
            //MP3模式
            case AudioType.MPEG:
                isDownloading = true;
                downloadProgress = 0f;
                receivedBytes = 0;
                UnityWebRequest mp3headRequest = UnityWebRequest.Head(url);
                yield return mp3headRequest.SendWebRequest();
                if (mp3headRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Failed to get audio info: " + mp3headRequest.error);
                    yield break;
                }

                totalBytes = long.Parse(mp3headRequest.GetResponseHeader("Content-Length"));

                // 创建音频请求
                audioWebRequest = UnityWebRequestMultimedia.GetAudioClip(url, audioType);
                ((DownloadHandlerAudioClip)audioWebRequest.downloadHandler).streamAudio = true;
                // 开始下载
                audioWebRequest.SendWebRequest();
                while (!audioWebRequest.isDone )
                {
                    downloadProgress = audioWebRequest.downloadProgress;
                    receivedBytes = (long)(totalBytes * downloadProgress);
                    yield return null;
                }

                // 获取音频剪辑
                streamingClip = ((DownloadHandlerAudioClip)audioWebRequest.downloadHandler).audioClip;
                streamingClip.name = Path.GetFileName(url);
                PlaylenghtCall?.Invoke(streamingClip.length);
                // 开始播放
                audioSource.clip = streamingClip;
                audioSource.Play();
                downloadProgress = 1;
                isDownloading = false;
                break;
            case AudioType.UNKNOWN:
                //非MP3模式
                isDownloading = true;
                downloadProgress = 0f;
                receivedBytes = 0;
                // 先发送HEAD请求获取文件大小
                UnityWebRequest headRequest = UnityWebRequest.Head(url);
                yield return headRequest.SendWebRequest();
                
                if (headRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Failed to get audio info: " + headRequest.error);
                    yield break;
                }

                totalBytes = long.Parse(headRequest.GetResponseHeader("Content-Length"));

                // 创建音频请求
                audioWebRequest = UnityWebRequestMultimedia.GetAudioClip(url, audioType);
                ((DownloadHandlerAudioClip)audioWebRequest.downloadHandler).streamAudio = true;

                // 开始下载
                audioWebRequest.SendWebRequest();

                // 等待初始数据到达
                while (!audioWebRequest.isDone && downloadProgress < 0.2)
                {
                    downloadProgress = audioWebRequest.downloadProgress;
                    receivedBytes = (long)(totalBytes * downloadProgress);
                    yield return null;
                }


                // 获取音频剪辑
                streamingClip = ((DownloadHandlerAudioClip)audioWebRequest.downloadHandler).audioClip;
                streamingClip.name = Path.GetFileName(url);
                PlaylenghtCall?.Invoke(streamingClip.length);
                // 开始播放
                audioSource.clip = streamingClip;
                audioSource.Play();
                bool isBuffering = false;
                float bufferThreshold = 0.2f; // 缓冲阈值20%

                // 继续下载剩余部分
                while (!audioWebRequest.isDone)
                {
                    downloadProgress = audioWebRequest.downloadProgress;
                    receivedBytes = (long)(totalBytes * downloadProgress);

                    float playedProgress = audioSource.time / streamingClip.length;
                    float remainingBuffer = downloadProgress - playedProgress;

                    // 缓冲不足时暂停播放
                    if (remainingBuffer < bufferThreshold && !isBuffering)
                    {
                        isBuffering = true;
                        audioSource.Pause();
                        Debug.Log("缓冲不足，暂停播放等待缓冲...");
                    }
                    // 缓冲足够时恢复播放
                    else if (remainingBuffer >= bufferThreshold && isBuffering)
                    {
                        isBuffering = false;
                        audioSource.Play();
                        Debug.Log("缓冲足够，恢复播放");
                    }
                    yield return null;
                }
                downloadProgress = 1;
                isDownloading = false;
                break;
        }
    }
    #endregion
    #region 跳转部分（seek）
    // 跳转到指定时间播放
    public void Seek(float time)
    {
        if (streamingClip != null)
        {
            // 计算对应的样本位置
            int targetSample = (int)(time * streamingClip.frequency);

            // 检查是否已下载到该位置
            float downloadedRatio = (float)receivedBytes / totalBytes;
            float totalTime = streamingClip.length;
            float downloadedTime = totalTime * downloadedRatio;

            if (time <= downloadedTime)
            {
                audioSource.time = time;
                if (!audioSource.isPlaying) audioSource.Play();
            }
            else
            {
                // 如果跳转到未下载区域，重新从该位置开始加载
                StartCoroutine(SeekAndContinue(time));
            }
        }
    }

    // 跳转到未下载区域的处理
    private IEnumerator SeekAndContinue(float time)
    {
        // 停止当前下载
        if (isDownloading)
        {
            audioWebRequest.Abort();
        }

        // 计算字节偏移量
        float seekRatio = time / streamingClip.length;
        long byteOffset = (long)(totalBytes * seekRatio);

        // 创建带Range头的请求
        UnityWebRequest newRequest = UnityWebRequestMultimedia.GetAudioClip(currentUrl, AudioType.UNKNOWN);
        newRequest.SetRequestHeader("Range", $"bytes={byteOffset}-");
        ((DownloadHandlerAudioClip)newRequest.downloadHandler).streamAudio = true;

        audioWebRequest = newRequest;
        audioWebRequest.SendWebRequest();

        // 等待初始数据到达
        while (!audioWebRequest.isDone && streamingClip == null)
        {
            yield return null;
        }

        if (audioWebRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Seek download failed: " + audioWebRequest.error);
            yield break;
        }

        // 获取新的音频剪辑并播放
        AudioClip newClip = DownloadHandlerAudioClip.GetContent(audioWebRequest);
        audioSource.clip = newClip;
        audioSource.time = 0; // 因为是从中间开始加载的，所以时间设为0
        audioSource.Play();

        streamingClip = newClip;
    }
    #endregion
    #region 工具方法
    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        StartCoroutine(PlayInvoke());
    }
    // 获取下载进度
    public float GetDownloadProgress()
    {
        return downloadProgress;
    }

    // 获取已下载字节数
    public long GetReceivedBytes()
    {
        return receivedBytes;
    }

    // 获取总字节数
    public long GetTotalBytes()
    {
        return totalBytes;
    }

    private void OnDestroy()
    {
        if (audioWebRequest != null && !audioWebRequest.isDone)
        {
            audioWebRequest.Abort();
        }
    }
    #endregion
    #region 按钮与slider方法 
    public float GetCurrentPlayTime()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            return audioSource.time;
        }
        return 0f;
    }
    IEnumerator PlayInvoke()
    {
        while (true)
        {
            while (audioSource.isPlaying)
            {
                PlayTimeCall?.Invoke(GetCurrentPlayTime());
                yield return new WaitForSecondsRealtime(1f);
            }
            yield return null;
        }
    }
    public void REcodeaudioplay()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        else
        {
            audioSource.Play();
        }
    }
    public async void NEXTSong()
    {
        Index += 1;
        if (Index > playList.Count)
        {
            Index = 0;
        }
        PlayAudio(await NewBehaviourScript._instanc.netease.search.GetSongUrl(playList[Index], AudioLevel.Standard));
    }
    public async void lastSong()
    {
        Index -= 1;
        if (Index < 0)
        {
            Index = playList.Count-1;
        }
        PlayAudio(await NewBehaviourScript._instanc.netease.search.GetSongUrl(playList[Index], AudioLevel.Standard));
    }
    public async void ToindexSong(int index)
    {
        Index = index;
        if (Index > playList.Count)
        {
            Index = playList.Count - 1;
        }
        PlayAudio(await NewBehaviourScript._instanc.netease.search.GetSongUrl(playList[Index], AudioLevel.Standard));
    }
    #endregion
}