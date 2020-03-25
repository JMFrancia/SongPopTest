using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/*
 * Manager for the results scene
 */
public class ResultsScreenManager : MonoBehaviour
{
    [Serializable]
    struct ScoreGrade
    {
        public float minScore;
        public char grade;

        public ScoreGrade(float min, char grade)
        {
            this.minScore = min;
            this.grade = grade;
        }
    }

    [Tooltip("The weight accuracy score will be given against speed scores when calculating final score")]
    [SerializeField] float accuracyToSpeedWeight = .7f;

    [Tooltip("Minimum grades (0 -> 1) required to achieve given letter grades")]
    [SerializeField]
    ScoreGrade[] scoreGrades = new ScoreGrade[] {
        new ScoreGrade(0f, 'F'),
        new ScoreGrade(.3f, 'D'),
        new ScoreGrade(.5f, 'C'),
        new ScoreGrade(.8f, 'B'),
        new ScoreGrade(.9f, 'A')
    };
    [Header("Image references")]
    [SerializeField] Image gradeCircle;
    [Header("Scene object references")]
    [SerializeField] GameObject background;
    [SerializeField] Text speedGradeText;
    [SerializeField] Text speedRatingText;
    [SerializeField] Text accuracyScoreText;
    [SerializeField] VerticalLayoutGroup resultsGroup;
    [SerializeField] GameObject resultPrefab;
    [SerializeField] Button nextButton;

    //Animation settings
    float offscreenBackgroundYPos = 425f;
    float offscreenNextbuttonXPos = 250f;
    float sceneTransitionDelay = 1.5f;
    float backgroundAnimationSpeed = .75f;
    float gradeCircleAnimationSpeed = .33f;
    float gradeCircleAnimationDelay = .75f;
    float correctScoreAnimationDelay = .3f;

    float gradePlusMargin = .2f;
    float gradeMinusMargin = .7f;

    AsyncOperation sceneLoadOp;
    BlurPanelManager blurPanel;
    Vector3 nextButtonOriginalPos;
    Vector3 backgroundOriginalPos;

    private void Start()
    {
        //Starting animation
        gradeCircle.fillAmount = 0f;
        backgroundOriginalPos = background.transform.position;
        background.transform.position = new Vector3(0f, offscreenBackgroundYPos, 0f);
        blurPanel = GameObject.FindWithTag(TagNames.BLUR_PANEL_TAG).GetComponent<BlurPanelManager>();
        blurPanel.onBlurInComplete += () =>
        {
            LeanTween.move(background, backgroundOriginalPos, backgroundAnimationSpeed).setEase(LeanTweenType.easeOutCirc).setOnComplete(PopulateResults);
            foreach (AudioSource source in GetComponents<AudioSource>())
            {
                source.Play();
            }
        };
        blurPanel.BlurIn();

        sceneLoadOp = SceneManager.LoadSceneAsync(SceneNames.TITLE_SCENE);
        sceneLoadOp.allowSceneActivation = false;

        nextButton.onClick.AddListener(() => StartCoroutine(ReturnToTitleScreenAfterDelay(sceneTransitionDelay)));
        nextButtonOriginalPos = nextButton.transform.position;
        nextButton.transform.position += new Vector3(offscreenNextbuttonXPos, 0f, 0f);
    }

    void CircleGrade(float delay = 0f)
    {
        StartCoroutine(FillImage(gradeCircle, gradeCircleAnimationSpeed, delay));
    }

    /*
     * Calculates results and places them on the page
     */
    void PopulateResults()
    {
        //Populate result rows
        int totalCorrect = 0;
        float delay = 0f;
        for (int n = 0; n < GameManager.correctAnswers.Length; n++)
        {
            int correctChoiceIndex = GameManager.ActivePlaylist.questions[n].answerIndex;
            Choice correctChoice = GameManager.ActivePlaylist.questions[n].choices[correctChoiceIndex];
            ResultRow row = Instantiate(resultPrefab, resultsGroup.transform).GetComponent<ResultRow>();

            //Set song value
            string song = $"\"{correctChoice.title}\" by {correctChoice.artist}";
            row.SetSong(song);

            //Set speed value
            if (GameManager.correctAnswers[n])
            {
                float speed = GameManager.Data.SongSamples[GameManager.ActivePlaylist.questions[n].song].length + 1 - GameManager.speedScores[n];
                row.SetSpeed($"{speed.ToString("0.0#")}s");
            }
            else
            {
                row.SetSpeed("--");
            }

            //Set correct/incorrect value with delays for animation
            delay += correctScoreAnimationDelay;
            row.SetCorrect(GameManager.correctAnswers[n], delay);

            //Goes toward final score calculations
            if (GameManager.correctAnswers[n])
            {
                totalCorrect++;
            }

            //Finish animations by circling the letter grade
            if (n == GameManager.correctAnswers.Length - 1)
            {
                CircleGrade(delay + gradeCircleAnimationDelay);
            }
        }

        //Calculate scores
        float perfectSpeedScore = GameManager.ActivePlaylist.questions.Sum(q => GameManager.Data.SongSamples[q.song].length + 1f); //+1f for extra time to guess
        float speedScore = GameManager.speedScores.Sum() / perfectSpeedScore;
        float accuracyScore = ((float)totalCorrect / (float)GameManager.correctAnswers.Length);
        //Final score not displayed, but used to determine letter grade
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
                //Determines if '+' or '-' should be added to letter grade. Do not add for an F (in American schools at least, no such thing as an F+ or F-)
                if (n > 0)
                {
                    float gradeMargin = (scoreGrades[n].minScore - finalScore) - scoreGrades[n - 1].minScore;
                    if (gradeMargin > gradeMinusMargin)
                    {
                        grade += "-";
                    }
                    else if (gradeMargin < gradePlusMargin)
                    {
                        grade += "+";
                    }
                }
                break;
            }
        }

        accuracyScoreText.text = FloatToPercentageString(accuracyScore);
        speedRatingText.text = FloatToPercentageString(speedScore);
        speedGradeText.text = grade;

        //Animate Next button
        LeanTween.move(nextButton.gameObject, nextButtonOriginalPos, 1f).setEase(LeanTweenType.easeOutCirc);
    }

    string FloatToPercentageString(float input)
    {
        return $"{Mathf.RoundToInt(input * 100)}%";
    }

    IEnumerator ReturnToTitleScreenAfterDelay(float delay)
    {
        blurPanel.BlurOut();
        nextButton.interactable = false;
        yield return new WaitForSeconds(delay);
        sceneLoadOp.allowSceneActivation = true;
    }

    IEnumerator FillImage(Image image, float time, float delay = 0f)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }
        float originalFill = image.fillAmount;
        float timePassed = 0f;
        while (image.fillAmount < 1f)
        {
            image.fillAmount = Mathf.Lerp(originalFill, 1f, timePassed / time);
            timePassed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
}
