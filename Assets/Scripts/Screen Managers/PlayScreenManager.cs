using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/*
 * Playscreen manager handles logic for the game scene
 */
public class PlayScreenManager : MonoBehaviour
{
    [Tooltip("Additional time beyond end of song clip to answer")]
    [SerializeField] float extraTimeToGuess = 1f;
    [Tooltip("Wait time between answering question and next question")]
    [SerializeField] float waitBetweenQuestions = 1.5f;
    [Tooltip("Fadeout time for a song clip")]
    [SerializeField] float songFadeout = 1f;

    [Header("Audio references")]
    [SerializeField] AudioClip wrongAnswerSound;
    [SerializeField] AudioClip rightAnswerSound;

    [Header("Scene object references")]
    [SerializeField] GameObject choiceButtonPrefab;
    [SerializeField] VerticalLayoutGroup choicesLayoutGroup;
    [SerializeField] Text headerText;
    [SerializeField] Timer timer;

    const string CORRECT_ANSWER = "Correct!";
    const string WRONG_ANSWER = "Wrong!";
    const string CALL_TO_ACTION = "Name that song!";

    AudioSource songAudioSource;
    AudioSource sfxAudioSource;
    BlurPanelManager blurPanel;
    Question[] questions;
    Button[] choiceButtons;
    Button correctChoice;
    bool[] results;
    float[] scores;
    int currentQuestionIndex = -1;
    bool isFadingOut = false;

    private void Start()
    {
        AudioSource[] audioSources = GetComponents<AudioSource>();
        songAudioSource = audioSources[0];
        sfxAudioSource = audioSources[1];

        blurPanel = GameObject.FindWithTag(TagNames.BLUR_PANEL_TAG).GetComponent<BlurPanelManager>();
        questions = GameManager.ActivePlaylist.questions;
        results = new bool[questions.Length];
        scores = new float[questions.Length];

        //If no choice made before timer up, no answer given = wrong
        timer.onComplete += () => OnChoiceButtonPress(null);

        blurPanel.onBlurInComplete += LoadNextQuestion;
        blurPanel.BlurIn();
    }

    private void Update()
    {
        //Update hook only used to determining when to fade out a songclip
        if (songAudioSource.isPlaying &&
            !isFadingOut &&
            songAudioSource.time >= (songAudioSource.clip.length - songFadeout))
        {
            FadeOutSong();
        }
    }

    /*
     * Begins a question, resetting timer, playing soundclip and populating buttons
     */
    void LoadQuestion(Question question)
    {
        songAudioSource.clip = GameManager.Data.SongSamples[question.song];
        timer.Set(songAudioSource.clip.length + extraTimeToGuess);
        songAudioSource.Play();

        //Generate buttons if not already done
        if (choiceButtons == null)
        {
            //Operates on assumption that all questions have same # of choices
            choiceButtons = new Button[question.choices.Length];
            for (int n = 0; n < choiceButtons.Length; n++)
            {
                Button button = Instantiate(choiceButtonPrefab, choicesLayoutGroup.transform).GetComponent<Button>();
                button.onClick.AddListener(() => OnChoiceButtonPress(button));
                choiceButtons[n] = button;
            }
        }

        //Set button text + callback
        for (int n = 0; n < question.choices.Length; n++)
        {
            choiceButtons[n].interactable = true;
            choiceButtons[n].image.color = Color.white;
            choiceButtons[n].GetComponentInChildren<Text>().text = $"\"{question.choices[n].title}\" by {question.choices[n].artist}";
            if (n == question.answerIndex)
            {
                correctChoice = choiceButtons[n];
            }
        }

        //Shuffle buttons to prevent players from memorizing answer positions if re-playing same playlist
        ShuffleLayoutChilden(choicesLayoutGroup);
    }

    /*
     * Load next question on the playlist
     */
    void LoadNextQuestion()
    {
        headerText.text = CALL_TO_ACTION;
        currentQuestionIndex++;
        if (currentQuestionIndex < questions.Length)
        {
            LoadQuestion(questions[currentQuestionIndex]);
        }
        else
        {
            GameManager.correctAnswers = results;
            GameManager.speedScores = scores;
            StartCoroutine(GoToResultsScreenAfterDelay(1.5f));
        }
    }

    /*
     * Callback for pressing a choice button
     */
    void OnChoiceButtonPress(Button button)
    {
        bool correct = (button == correctChoice);

        timer.Stop();
        scores[currentQuestionIndex] = timer.time;

        headerText.text = correct ? CORRECT_ANSWER : WRONG_ANSWER;
        results[currentQuestionIndex] = correct;
        PlayAnswerSound(correct);

        for (int n = 0; n < choicesLayoutGroup.transform.childCount; n++)
        {
            Button b = choicesLayoutGroup.transform.GetChild(n).GetComponent<Button>();
            b.interactable = false;
            if (b == correctChoice)
            {
                b.image.color = Color.green;
            }
        }
        if (!correct && button != null)
        {
            button.image.color = Color.red;
        }

        StartCoroutine(LoadNextQuestionAfterDelay(waitBetweenQuestions));
    }

    void ShuffleLayoutChilden(VerticalLayoutGroup group)
    {
        for (int n = 0; n < group.transform.childCount; n++)
        {
            int index = Random.Range(0, group.transform.childCount - 1);
            group.transform.GetChild(n).SetSiblingIndex(index);
        }
    }

    void PlayAnswerSound(bool correct)
    {
        FadeOutSong();
        sfxAudioSource.clip = correct ? rightAnswerSound : wrongAnswerSound;
        sfxAudioSource.Play();
    }

    void FadeOutSong()
    {
        isFadingOut = true;
        StartCoroutine(
            Utilities.FadeOutAudio(
                songAudioSource,
                songFadeout,
                () => { isFadingOut = false; }
            )
        );
    }

    IEnumerator LoadNextQuestionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        LoadNextQuestion();
    }

    IEnumerator GoToResultsScreenAfterDelay(float delay)
    {
        blurPanel.BlurOut();
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneNames.RESULTS_SCENE);
    }
}
