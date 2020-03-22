using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultsScreenManager : MonoBehaviour
{
    [SerializeField] Text percentageCorrectText;
    [SerializeField] VerticalLayoutGroup resultsGroup;
    [SerializeField] GameObject resultPanelPrefab;
    [SerializeField] Button nextButton;

    AsyncOperation sceneLoadOp;

    private void Start()
    {
        sceneLoadOp = SceneManager.LoadSceneAsync("Title");
        sceneLoadOp.allowSceneActivation = false;

        PopulateResults();

        nextButton.onClick.AddListener(ReturnToTitleScreen);
    }

    void PopulateResults()
    {
        int totalCorrect = 0;
        for (int n = 0; n < GameManager.results.Length; n++)
        {
            int correctChoiceIndex = GameManager.ActivePlaylist.questions[n].answerIndex;
            Choice correctChoice = GameManager.ActivePlaylist.questions[n].choices[correctChoiceIndex];
            GameObject resultObj = Instantiate(resultPanelPrefab, resultsGroup.transform);
            resultObj.GetComponentInChildren<Text>().text = $"\"{correctChoice.title}\" by {correctChoice.artist}";
            resultObj.GetComponent<Image>().color = GameManager.results[n] ? Color.green : Color.red;
            if(GameManager.results[n]) {
                totalCorrect++;
            }
        }

        float finalScore = ((float)totalCorrect / (float)GameManager.results.Length) * 100;
        percentageCorrectText.text = $"{finalScore.ToString("0.##\\%")} Correct";
    }

    void ReturnToTitleScreen() {
        sceneLoadOp.allowSceneActivation = true;
    }
}
