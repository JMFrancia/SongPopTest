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

    private void OnEnable()
    {
        if (GameManager.isDataLoaded)
        {
            PopulateButtons();
        }
        else {
            GameManager.dataLoaded -= PopulateButtons; //Failsafe to ensure no case where callback added twice
            GameManager.dataLoaded += PopulateButtons;
        }

        quitButtonGO = Instantiate(quitButtonPrefab, playlistButtonGroup.transform);
        quitButtonGO.GetComponent<Button>().onClick.AddListener(Quit);

        GameManager.Data.mediaReady -= OnMediaReady;
        GameManager.Data.mediaReady += OnMediaReady;

        sceneLoadOp = SceneManager.LoadSceneAsync("Game");
    }

    private void OnDisable()
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
        sceneLoadOp.allowSceneActivation = true;
    }

    private void Quit()
    {
        Application.Quit();
    }
}
