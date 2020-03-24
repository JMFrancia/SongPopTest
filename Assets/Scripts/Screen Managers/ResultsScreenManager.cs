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

    [SerializeField] Image gradeCircle;
    [SerializeField] GameObject background;
    [SerializeField] Text speedGradeText;
    [SerializeField] Text speedRatingText;
    [SerializeField] Text accuracyScoreText;
    [SerializeField] VerticalLayoutGroup resultsGroup;
    [SerializeField] GameObject resultPrefab;
    [SerializeField] Button nextButton;

    AsyncOperation sceneLoadOp;
    BlurPanelManager blurPanel;
    Vector3 nextButtonOriginalPos;
    Vector3 backgroundOriginalPos;

    private void Start()
    {
        backgroundOriginalPos = background.transform.position;
        background.transform.position -= new Vector3(0f, 450f, 0f);

        blurPanel = GameObject.FindWithTag("BlurPanel").GetComponent<BlurPanelManager>();
        blurPanel.onBlurInComplete += () =>
        {
            LeanTween.move(background, backgroundOriginalPos, .75f).setEase(LeanTweenType.easeOutCirc).setOnComplete(PopulateResults);
            foreach (AudioSource source in GetComponents<AudioSource>())
            {
                source.Play();
            }
        };
        blurPanel.BlurIn();

        sceneLoadOp = SceneManager.LoadSceneAsync("Title");
        sceneLoadOp.allowSceneActivation = false;
        nextButton.onClick.AddListener(() => StartCoroutine(ReturnToTitleScreen(1.5f)));
        nextButtonOriginalPos = nextButton.transform.position;
        nextButton.transform.position -= new Vector3(0f, 78f, 0f);
        gradeCircle.fillAmount = 0f;
    }

    void CircleGrade(float delay = 0f) {
        StartCoroutine(FillImage(gradeCircle, .33f, delay));
    }

    IEnumerator FillImage(Image image, float time, float delay = 0f) {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }
        float originalFill = image.fillAmount;
        float timePassed = 0f;
        while(image.fillAmount < 1f)
        {
            image.fillAmount = Mathf.Lerp(originalFill, 1f, timePassed / time);
            timePassed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
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
            ResultRow row = Instantiate(resultPrefab, resultsGroup.transform).GetComponent<ResultRow>();

            //Set song value
            string song = $"\"{correctChoice.title}\" by {correctChoice.artist}";
            row.SetSong(song);

            //Set speed value
            if (GameManager.results[n])
            {
                float speed = GameManager.Data.SongSamples[GameManager.ActivePlaylist.questions[n].song].length + 1 - GameManager.scores[n];
                row.SetSpeed($"{speed.ToString("0.0#")}s");
            } else {
                row.SetSpeed("--");
            }

            //Set correct/incorrect value
            delay += .3f;
            row.SetCorrect(GameManager.results[n], delay);

            //Goes toward final score calculations
            if(GameManager.results[n]) {
                totalCorrect++;
            }

            if(n == GameManager.results.Length - 1) {
                CircleGrade(delay + 1f);
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

        LeanTween.move(nextButton.gameObject, nextButtonOriginalPos, 1f).setEase(LeanTweenType.easeOutCirc);
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
