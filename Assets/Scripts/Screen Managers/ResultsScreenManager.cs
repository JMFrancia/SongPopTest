using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultsScreenManager : MonoBehaviour
{
    [Serializable]
    struct ScoreGrade {
        public float minScore;
        public char grade;

        public ScoreGrade(float min, char grade) {
            this.minScore = min;
            this.grade = grade;
        }
    }

    [SerializeField] float accuracyToSpeedWeight = .7f;
    [SerializeField] ScoreGrade[] scoreGrades = new ScoreGrade[] {
        new ScoreGrade(0f, 'F'),
        new ScoreGrade(.3f, 'D'),
        new ScoreGrade(.5f, 'C'),
        new ScoreGrade(.8f, 'B'),
        new ScoreGrade(.9f, 'A')
    };

    [SerializeField] Text speedGradeText;
    [SerializeField] Text speedRatingText;
    [SerializeField] Text finalScoreText;
    [SerializeField] Text accuracyScoreText;
    [SerializeField] VerticalLayoutGroup resultsGroup;
    [SerializeField] GameObject resultPanelPrefab;
    [SerializeField] Button nextButton;

    AsyncOperation sceneLoadOp;
    BlurPanelManager blurPanel;

    private void Start()
    {
        sceneLoadOp = SceneManager.LoadSceneAsync("Title");
        sceneLoadOp.allowSceneActivation = false;
        blurPanel = GameObject.FindWithTag("BlurPanel").GetComponent<BlurPanelManager>();
        nextButton.onClick.AddListener(() => StartCoroutine(ReturnToTitleScreen(1.5f)));
        PopulateResults();
        blurPanel.BlurIn();
    }

    void PopulateSpeedScore() {
        float perfectSpeedScore = GameManager.ActivePlaylist.questions.Sum(q => GameManager.Data.SongSamples[q.song].length + 1f); //+1f for extra time to guess
        float speedScore = GameManager.scores.Sum() / perfectSpeedScore;
        Array.Sort<ScoreGrade>(scoreGrades, (a, b) => a.minScore.CompareTo(b.minScore));
        string grade = "F";
        for (int n = 0; n < scoreGrades.Length; n++)
        {
            if (speedScore > scoreGrades[n].minScore)
            {
                grade = scoreGrades[n].grade.ToString();
            }
            else
            {
                if (n > 0)
                {
                    float gradeMargin = (scoreGrades[n].minScore - speedScore) - scoreGrades[n - 1].minScore;
                    if (gradeMargin > .7f)
                    {
                        grade += "-";
                    }
                    else if (gradeMargin < .2f)
                    {
                        grade += "+";
                    }
                }
                break;
            }
        }

    }

    void PopulateResults()
    {

        //Calculate speed scores first
        float perfectSpeedScore = GameManager.ActivePlaylist.questions.Sum(q => GameManager.Data.SongSamples[q.song].length + 1f); //+1f for extra time to guess
        float speedScore = GameManager.scores.Sum() / perfectSpeedScore;

        //Calculate accuracy scores
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
        float accuracyScore = ((float)totalCorrect / (float)GameManager.results.Length);

        float finalScore = accuracyScore * accuracyToSpeedWeight + speedScore * (1 - accuracyToSpeedWeight);

        //Determine letter grade
        Array.Sort<ScoreGrade>(scoreGrades, (a, b) => a.minScore.CompareTo(b.minScore));
        string grade = "F";
        for (int n = 0; n < scoreGrades.Length; n++)
        {
            if (finalScore > scoreGrades[n].minScore)
            {
                grade = scoreGrades[n].grade.ToString();
            }
            else
            {
                if (n > 0)
                {
                    float gradeMargin = (scoreGrades[n].minScore - finalScore) - scoreGrades[n - 1].minScore;
                    if (gradeMargin > .7f)
                    {
                        grade += "-";
                    }
                    else if (gradeMargin < .2f)
                    {
                        grade += "+";
                    }
                }
                break;
            }
        }

        accuracyScoreText.text = $"Accuracy: {FloatToPercentage(accuracyScore)}";
        speedRatingText.text = $"Speed: {FloatToPercentage(speedScore)}";
        finalScoreText.text = $"Total: {FloatToPercentage(finalScore)}";
        speedGradeText.text = $"Grade: {grade}";
    }

    string FloatToPercentage(float input) {
        return $"{Mathf.RoundToInt(input * 100)}%";
    }

    IEnumerator ReturnToTitleScreen(float delay)
    {
        blurPanel.BlurOut();
        nextButton.interactable = false;
        yield return new WaitForSeconds(delay); 
        sceneLoadOp.allowSceneActivation = true;
    }
}
