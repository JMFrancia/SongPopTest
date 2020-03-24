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
    [SerializeField] Text accuracyScoreText;
    [SerializeField] VerticalLayoutGroup resultsGroup;
    [SerializeField] GameObject resultPrefab;
    [SerializeField] Button nextButton;

    AsyncOperation sceneLoadOp;
    BlurPanelManager blurPanel;

    private void Start()
    {
        sceneLoadOp = SceneManager.LoadSceneAsync("Title");
        sceneLoadOp.allowSceneActivation = false;
        nextButton.onClick.AddListener(() => StartCoroutine(ReturnToTitleScreen(1.5f)));

        blurPanel = GameObject.FindWithTag("BlurPanel").GetComponent<BlurPanelManager>();
        blurPanel.onBlurInComplete += PopulateResults;
        blurPanel.BlurIn();
    }

    void PopulateResults()
    {

        //Calculate speed scores first
        float perfectSpeedScore = GameManager.ActivePlaylist.questions.Sum(q => GameManager.Data.SongSamples[q.song].length + 1f); //+1f for extra time to guess
        float speedScore = GameManager.scores.Sum() / perfectSpeedScore;

        //Populate result rows
        int totalCorrect = 0;
        float delay = 0f;
        for (int n = 0; n < GameManager.results.Length; n++)
        {
            int correctChoiceIndex = GameManager.ActivePlaylist.questions[n].answerIndex;
            Choice correctChoice = GameManager.ActivePlaylist.questions[n].choices[correctChoiceIndex];
            GameObject resultObj = Instantiate(resultPrefab, resultsGroup.transform);

            string song = $"\"{correctChoice.title}\" by {correctChoice.artist}";
            float speed = GameManager.Data.SongSamples[GameManager.ActivePlaylist.questions[n].song].length + 1 - GameManager.scores[n];

            ResultRow row = resultObj.GetComponent<ResultRow>();
            row.SetSong(song);
            delay += .3f;
            row.SetCorrect(GameManager.results[n], delay);
            row.SetSpeed($"{speed.ToString("0.0#")}s");

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

        accuracyScoreText.text = FloatToPercentage(accuracyScore);
        speedRatingText.text = FloatToPercentage(speedScore);
        speedGradeText.text = grade;
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
