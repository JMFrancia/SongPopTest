using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainScreenManager : MonoBehaviour
{
    [SerializeField] AudioClip shushSound;
    [SerializeField] AudioClip classroomBackgroundSound;
    [SerializeField] GameObject playlistButtonPrefab;
    [SerializeField] GameObject quitButtonPrefab;
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
        sceneLoadOp = SceneManager.LoadSceneAsync("Game");
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
     * Once data loaded in GameManager, populate playlist choice buttons
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

    void LoadPlaylist(Playlist playlist)
    {
        shushAudiosource.clip = shushSound;
        shushAudiosource.loop = false;
        shushAudiosource.Play();
        StartCoroutine(FadeOutBackground(shushSound.length * 1.5f));

        loadingPanel.SetActive(true);
        GameManager.ActivePlaylist = playlist;
    }

    IEnumerator FadeOutBackground(float time)
    {
        float timePassed = 0f;
        float originalVal = backgroundAudiosource.volume;
        while (backgroundAudiosource.volume > 0f)
        {
            backgroundAudiosource.volume = Mathf.Lerp(originalVal, 0f, timePassed / time);
            timePassed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        backgroundAudiosource.Stop();
        backgroundAudiosource.volume = originalVal;
    }

    void OnMediaReady()
    {
        GameManager.Data.mediaReady -= OnMediaReady;
        Debug.Log("Media ready");
        StartCoroutine(LoadSceneWhenShushComplete());
    }

    IEnumerator LoadSceneWhenShushComplete() { 
        while(shushAudiosource.isPlaying) {
            yield return new WaitForSeconds(shushSound.length - shushAudiosource.time);
        }
        sceneLoadOp.allowSceneActivation = true;
    }

    private void Quit()
    {
        Application.Quit();
    }
}
