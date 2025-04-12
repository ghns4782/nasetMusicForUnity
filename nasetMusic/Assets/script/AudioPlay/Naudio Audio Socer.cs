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
    #region ����������ֲ���
    // ��ʼ���ز�������Ƶ
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

    // ��ʽ������Ƶ��Э��
    private IEnumerator StreamAudio(string url, AudioType audioType)
    {
        OnloadSonging?.Invoke(playList[Index]);
        switch (audioType)
        {
            //MP3ģʽ
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

                // ������Ƶ����
                audioWebRequest = UnityWebRequestMultimedia.GetAudioClip(url, audioType);
                ((DownloadHandlerAudioClip)audioWebRequest.downloadHandler).streamAudio = true;
                // ��ʼ����
                audioWebRequest.SendWebRequest();
                while (!audioWebRequest.isDone )
                {
                    downloadProgress = audioWebRequest.downloadProgress;
                    receivedBytes = (long)(totalBytes * downloadProgress);
                    yield return null;
                }

                // ��ȡ��Ƶ����
                streamingClip = ((DownloadHandlerAudioClip)audioWebRequest.downloadHandler).audioClip;
                streamingClip.name = Path.GetFileName(url);
                PlaylenghtCall?.Invoke(streamingClip.length);
                // ��ʼ����
                audioSource.clip = streamingClip;
                audioSource.Play();
                downloadProgress = 1;
                isDownloading = false;
                break;
            case AudioType.UNKNOWN:
                //��MP3ģʽ
                isDownloading = true;
                downloadProgress = 0f;
                receivedBytes = 0;
                // �ȷ���HEAD�����ȡ�ļ���С
                UnityWebRequest headRequest = UnityWebRequest.Head(url);
                yield return headRequest.SendWebRequest();
                
                if (headRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Failed to get audio info: " + headRequest.error);
                    yield break;
                }

                totalBytes = long.Parse(headRequest.GetResponseHeader("Content-Length"));

                // ������Ƶ����
                audioWebRequest = UnityWebRequestMultimedia.GetAudioClip(url, audioType);
                ((DownloadHandlerAudioClip)audioWebRequest.downloadHandler).streamAudio = true;

                // ��ʼ����
                audioWebRequest.SendWebRequest();

                // �ȴ���ʼ���ݵ���
                while (!audioWebRequest.isDone && downloadProgress < 0.2)
                {
                    downloadProgress = audioWebRequest.downloadProgress;
                    receivedBytes = (long)(totalBytes * downloadProgress);
                    yield return null;
                }


                // ��ȡ��Ƶ����
                streamingClip = ((DownloadHandlerAudioClip)audioWebRequest.downloadHandler).audioClip;
                streamingClip.name = Path.GetFileName(url);
                PlaylenghtCall?.Invoke(streamingClip.length);
                // ��ʼ����
                audioSource.clip = streamingClip;
                audioSource.Play();
                bool isBuffering = false;
                float bufferThreshold = 0.2f; // ������ֵ20%

                // ��������ʣ�ಿ��
                while (!audioWebRequest.isDone)
                {
                    downloadProgress = audioWebRequest.downloadProgress;
                    receivedBytes = (long)(totalBytes * downloadProgress);

                    float playedProgress = audioSource.time / streamingClip.length;
                    float remainingBuffer = downloadProgress - playedProgress;

                    // ���岻��ʱ��ͣ����
                    if (remainingBuffer < bufferThreshold && !isBuffering)
                    {
                        isBuffering = true;
                        audioSource.Pause();
                        Debug.Log("���岻�㣬��ͣ���ŵȴ�����...");
                    }
                    // �����㹻ʱ�ָ�����
                    else if (remainingBuffer >= bufferThreshold && isBuffering)
                    {
                        isBuffering = false;
                        audioSource.Play();
                        Debug.Log("�����㹻���ָ�����");
                    }
                    yield return null;
                }
                downloadProgress = 1;
                isDownloading = false;
                break;
        }
    }
    #endregion
    #region ��ת���֣�seek��
    // ��ת��ָ��ʱ�䲥��
    public void Seek(float time)
    {
        if (streamingClip != null)
        {
            // �����Ӧ������λ��
            int targetSample = (int)(time * streamingClip.frequency);

            // ����Ƿ������ص���λ��
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
                // �����ת��δ�����������´Ӹ�λ�ÿ�ʼ����
                StartCoroutine(SeekAndContinue(time));
            }
        }
    }

    // ��ת��δ��������Ĵ���
    private IEnumerator SeekAndContinue(float time)
    {
        // ֹͣ��ǰ����
        if (isDownloading)
        {
            audioWebRequest.Abort();
        }

        // �����ֽ�ƫ����
        float seekRatio = time / streamingClip.length;
        long byteOffset = (long)(totalBytes * seekRatio);

        // ������Rangeͷ������
        UnityWebRequest newRequest = UnityWebRequestMultimedia.GetAudioClip(currentUrl, AudioType.UNKNOWN);
        newRequest.SetRequestHeader("Range", $"bytes={byteOffset}-");
        ((DownloadHandlerAudioClip)newRequest.downloadHandler).streamAudio = true;

        audioWebRequest = newRequest;
        audioWebRequest.SendWebRequest();

        // �ȴ���ʼ���ݵ���
        while (!audioWebRequest.isDone && streamingClip == null)
        {
            yield return null;
        }

        if (audioWebRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Seek download failed: " + audioWebRequest.error);
            yield break;
        }

        // ��ȡ�µ���Ƶ����������
        AudioClip newClip = DownloadHandlerAudioClip.GetContent(audioWebRequest);
        audioSource.clip = newClip;
        audioSource.time = 0; // ��Ϊ�Ǵ��м俪ʼ���صģ�����ʱ����Ϊ0
        audioSource.Play();

        streamingClip = newClip;
    }
    #endregion
    #region ���߷���
    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        StartCoroutine(PlayInvoke());
    }
    // ��ȡ���ؽ���
    public float GetDownloadProgress()
    {
        return downloadProgress;
    }

    // ��ȡ�������ֽ���
    public long GetReceivedBytes()
    {
        return receivedBytes;
    }

    // ��ȡ���ֽ���
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
    #region ��ť��slider���� 
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