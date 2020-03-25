using UnityEngine;
using System;

/*
 * GameManager class is a permenant singleton class in charge of keeping track of 
 * game state. 
 * 
 * Eventually wound up being not much more than a glorified gatekeeper
 * for DataManager, but also stores scores / results, so kept it as is.
 */
public class GameManager : MonoBehaviour
{
    public static DataManager Data
    {
        get
        {
            return _instance._data;
        }
    }

    public static event Action dataLoaded;

    //The playlist being used for the current game
    public static Playlist ActivePlaylist
    {
        set
        {
            _instance.SetActivePlaylist(value);
        }
        get
        {
            return _instance._activePlaylist;
        }
    }
    public static bool isDataLoaded = false;

    public static bool[] correctAnswers;
    public static float[] speedScores;

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
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(gameObject);

        _data = GetComponent<DataManager>();
        _data.Initialize();
        isDataLoaded = true;
        if (dataLoaded != null)
        {
            dataLoaded.Invoke();
        }
    }

    void SetActivePlaylist(Playlist playlist)
    {
        _activePlaylist = playlist;
        _data.FetchPlaylistMedia(playlist);
    }
}
