using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainScreenManager : MonoBehaviour
{
    [SerializeField] GameObject playlistButtonPrefab;
    [SerializeField] GameObject quitButtonPrefab;
    [SerializeField] VerticalLayoutGroup playlistButtonGroup;
    [SerializeField] GameObject loadingPanel;

    GameObject quitButtonGO;
    AsyncOperation sceneLoadOp;

    private void Awake()
    {
        quitButtonGO = Instantiate(quitButtonPrefab, playlistButtonGroup.transform);
        quitButtonGO.GetComponent<Button>().onClick.AddListener(Quit);

        if (GameManager.isDataLoaded)
        {
            PopulateButtons();
        }
        else {
            GameManager.dataLoaded -= PopulateButtons;
            GameManager.dataLoaded += PopulateButtons;
        }

        GameManager.Data.mediaReady -= OnMediaReady;
        GameManager.Data.mediaReady += OnMediaReady;

    }

    private void Start()
    {
        sceneLoadOp = SceneManager.LoadSceneAsync("Game");
        sceneLoadOp.allowSceneActivation = false;
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
    }

    void LoadPlaylist(Playlist playlist)
    {
        loadingPanel.SetActive(true);
        GameManager.ActivePlaylist = playlist;
    }

    void OnMediaReady()
    {
        Debug.Log("Media ready");
        sceneLoadOp.allowSceneActivation = true;
    }

    private void Quit()
    {
        Application.Quit();
    }
}
