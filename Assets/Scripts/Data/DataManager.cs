using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DataManager : MonoBehaviour
{
    //Necessary to import top-level JSON array
    private class SPData
    {
        public List<Playlist> playlists;
    }

    [SerializeField] TextAsset rawDataJSON;
    [SerializeField] bool cacheDataBetweenRounds = false;

    public Action mediaReady;

    public Dictionary<string, Playlist> Playlists { get; private set; }
    public Dictionary<Song, AudioClip> SongSamples { get; private set; }
    public Dictionary<string, Texture> Images { get; private set; }
    public bool fetchingMedia { get; private set; } = false;


    SPData data;

    int audioClipsFetched;
    int audioRequestsInProgress;
    int totalAudioRequests;

    int imagesFetched;
    int imageRequestsInProgress;
    int totalImageRequests;


    /*
     * Deserializes JSON data and sets up Playlist dict
     */
    public void Initialize()
    {
        string formattedData = $"{{\"playlists\":{rawDataJSON.text}}}";
        data = JsonUtility.FromJson<SPData>( formattedData);

        Playlists = new Dictionary<string, Playlist>();
        for(int n = 0; n < data.playlists.Count; n++) {
            Playlists[data.playlists[n].playlist] = data.playlists[n];
        }

        Images = new Dictionary<string, Texture>();
        SongSamples = new Dictionary<Song, AudioClip>();
    }

    /*
     * Fetches / caches all images and audio clips for a given playlist
     */
    public void FetchPlaylistMedia(Playlist playlist)
    {
        fetchingMedia = true;
        FetchAllImages(playlist);
        FetchAllAudioClips(playlist);
    }

    /*
     * Fetches / caches all audio clips for a given playlist
     */
    void FetchAllAudioClips(Playlist playlist)
    {
        if(!cacheDataBetweenRounds)
            SongSamples.Clear();
        audioClipsFetched = 0;
        audioRequestsInProgress = 0;
        totalAudioRequests = 0;
        for (int n = 0; n < playlist.questions.Length; n++)
        {
            totalAudioRequests++;
            StartCoroutine(FetchSongClip(playlist.questions[n].song));
        }
    }

    /*
     * Fetches / caches all images for a given playlist
     */
    void FetchAllImages(Playlist playlist)
    {
        if (!cacheDataBetweenRounds)
            Images.Clear();
        imagesFetched = 0;
        imageRequestsInProgress = 0;
        totalImageRequests = 0;
        for (int n = 0; n < playlist.questions.Length; n++)
        {
            totalImageRequests++;
            StartCoroutine(FetchImage(playlist.questions[n].song.picture));
        }
    }

    /*
     * Fetches / caches audio clip from given URL, stores in audio clip dictionary with URL as key
     */
    IEnumerator FetchImage(string url)
    {
        Images[url] = null;
        imageRequestsInProgress++;
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.isNetworkError)
            {
                Debug.Log($"Error while sending request: {request.error}");
            }
            else
            {
                Images[url] = ((DownloadHandlerTexture)request.downloadHandler).texture;
            }
        }
        if (Images[url] == null)
        {
            Images.Remove(url);
        }
        else
        {
            imagesFetched++;
        }
        imageRequestsInProgress--;
        if (imageRequestsInProgress == 0)
        {
            Debug.Log($"All images fetched. {imagesFetched} / {totalImageRequests} received successfully");
            if(audioRequestsInProgress == 0) {
                fetchingMedia = false;
                mediaReady.Invoke();
            }
        }
    }

    /*
     * Fetches / caches audio clip from given URL, stores in audio clip dictionary with URL as key
     */
    IEnumerator FetchSongClip(Song song)
    {
        SongSamples[song] = null;
        audioRequestsInProgress++;
        using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(song.sample, AudioType.WAV))
        {
            yield return request.SendWebRequest();

            if (request.isNetworkError)
            {
                Debug.Log($"Error while sending request: {request.error}");
            }
            else
            {
                SongSamples[song] = DownloadHandlerAudioClip.GetContent(request);
            }
        }
        if (SongSamples[song] == null)
        {
            SongSamples.Remove(song);
        }
        else
        {
            audioClipsFetched++;
        }
        audioRequestsInProgress--;
        if (audioRequestsInProgress == 0)
        {
            Debug.Log($"All audio clips fetched. {audioClipsFetched} / {totalAudioRequests} received successfully");
            if (imageRequestsInProgress == 0)
            {
                fetchingMedia = false;
                mediaReady.Invoke();
            }
        }
    }


}
