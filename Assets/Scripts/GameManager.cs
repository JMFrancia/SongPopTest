using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static DataManager Data {
        get {
            return _instance._data;
        }
    }

    public static event Action dataLoaded;

    public static Playlist ActivePlaylist {
        set {
            _instance.SetActivePlaylist(value);
        }
        get {
            return _instance._activePlaylist;
        }
    }
    public static bool isDataLoaded = false;

    public static bool[] results;
    public static float[] scores;

    static GameManager _instance;
    Playlist _activePlaylist;
    DataManager _data;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this);
        }

        DontDestroyOnLoad(gameObject);

        _data = GetComponent<DataManager>();
        _data.Initialize();
        isDataLoaded = true;
        if(dataLoaded != null)
            dataLoaded.Invoke(); 
    }

    void SetActivePlaylist(Playlist playlist)
    {
        _activePlaylist = playlist;
        _data.FetchPlaylistMedia(playlist);
    }
}
