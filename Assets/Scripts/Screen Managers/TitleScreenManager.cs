using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/*
 * Manager script for the title screen
 */
public class TitleScreenManager : MonoBehaviour
{
    [Header("Audio clip references")]
    [SerializeField] AudioClip shushSound;
    [SerializeField] AudioClip classroomBackgroundSound;

    [Header("Prefab references")]
    [SerializeField] GameObject playlistButtonPrefab;
    [SerializeField] GameObject quitButtonPrefab;

    [Header("Scene object references")]
    [SerializeField] VerticalLayoutGroup playlistButtonGroup;
    [SerializeField] GameObject loadingPanel;

    GameObject quitButtonGO;
    AsyncOperation sceneLoadOp;
    BlurPanelManager blurPanelManager;
    AudioSource backgroundAudiosource;
    AudioSource shushAudiosource;

    private void Awake()
    {
        AudioSource[] audioSources = GetComponents<AudioSource>();
        backgroundAudiosource = audioSources[0];
        shushAudiosource = audioSources[1];

        quitButtonGO = Instantiate(quitButtonPrefab, playlistButtonGroup.transform);
        quitButtonGO.GetComponent<Button>().onClick.AddListener(Quit);

        //Populate buttons once playlist data loaded
        if (GameManager.isDataLoaded)
        {
            PopulateButtons();
        }
        else {
            GameManager.dataLoaded += PopulateButtons;
        }

        GameManager.Data.mediaReady += OnMediaReady;

        blurPanelManager = GameObject.FindWithTag("BlurPanel").GetComponent<BlurPanelManager>();
        blurPanelManager.BlurIn();
    }

    private void Start()
    {
        //Async pre-load for game screen
        sceneLoadOp = SceneManager.LoadSceneAsync(SceneNames.GAME_SCENE);
        sceneLoadOp.allowSceneActivation = false;

        backgroundAudiosource.clip = classroomBackgroundSound;
        backgroundAudiosource.loop = true;
        backgroundAudiosource.volume = .25f;
        backgroundAudiosource.Play();
    }

    private void Destroy()
    {
        loadingPanel.SetActive(false);
        GameManager.dataLoaded -= PopulateButtons;
    }

    /*
     * Generates playlist buttons
     */
    void PopulateButtons() {
        foreach (Playlist playlist in GameManager.Data.Playlists.Values)
        {
            GameObject buttonGO = Instantiate(playlistButtonPrefab, playlistButtonGroup.transform);
            buttonGO.GetComponentInChildren<Text>().text = playlist.playlist;
            buttonGO.GetComponent<Button>().onClick.AddListener(() => LoadPlaylist(playlist));
        }
        quitButtonGO.transform.SetAsLastSibling();
        GameManager.dataLoaded -= PopulateButtons;
    }

    /*
     * Loads playlist to begin game
     */
    void LoadPlaylist(Playlist playlist)
    {
        shushAudiosource.clip = shushSound;
        shushAudiosource.loop = false;
        shushAudiosource.Play();

        loadingPanel.SetActive(true);
        GameManager.ActivePlaylist = playlist;
    }

    /*
     * When a playlist is loaded, mediaReady event is called as soon as media is downloaded.
     * That's our cue to begin transition to the next scene ASAP.    
     */
    void OnMediaReady()
    {
        GameManager.Data.mediaReady -= OnMediaReady;
        Debug.Log("Media ready");
        FadeOutBackgroundSound();
        StartCoroutine(LoadSceneWhenShushComplete());
    }

    void FadeOutBackgroundSound()
    {
        float fadeOutTime = shushSound.length * 1.5f;
        StartCoroutine(Utilities.FadeOutAudio(backgroundAudiosource, fadeOutTime));
    }

    /*
     * Wait until "shush" sfx complete before loading next scene
     */
    IEnumerator LoadSceneWhenShushComplete() { 
        while(shushAudiosource.isPlaying) {
            yield return new WaitForSeconds(shushSound.length - shushAudiosource.time);
        }
        sceneLoadOp.allowSceneActivation = true;
    }

    private void Quit()
    {
        Debug.Log("Quitting game. Goodbye!");
        Application.Quit();
    }
}
