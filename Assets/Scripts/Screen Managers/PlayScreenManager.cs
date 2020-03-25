using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayScreenManager : MonoBehaviour
{
    [SerializeField] float extraTimeToGuess = 1f; //Time to guess beyond the length of the audio clip
    [SerializeField] float waitBetweenQuestions = 1.5f;
    [SerializeField] float songFadeout = 1f;

    [SerializeField] AudioClip wrongAnswerSound;
    [SerializeField] AudioClip rightAnswerSound;

    [SerializeField] GameObject choiceButtonPrefab;
    [SerializeField] VerticalLayoutGroup choicesLayoutGroup;
    [SerializeField] Text headerText;
    [SerializeField] Timer timer;

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

        blurPanel = GameObject.FindWithTag("BlurPanel").GetComponent<BlurPanelManager>();
        questions = GameManager.ActivePlaylist.questions;
        results = new bool[questions.Length];
        scores = new float[questions.Length];


        //If no choice made before timer up
        timer.onComplete += () => OnChoiceButtonPress(null);

        blurPanel.onBlurInComplete += LoadNextQuestion;
        blurPanel.BlurIn();
    }

    void LoadQuestion(Question question)
    {
        songAudioSource.clip = GameManager.Data.SongSamples[question.song];
        timer.Set(songAudioSource.clip.length + extraTimeToGuess);
        songAudioSource.Play();

        //Generate buttons if not already done
        if(choiceButtons == null) {
            //Operates on assumption that all questions have same # of choices
            choiceButtons = new Button[question.choices.Length];
            for(int n = 0; n < choiceButtons.Length; n++) {
                Button button = Instantiate(choiceButtonPrefab, choicesLayoutGroup.transform).GetComponent<Button>();
                button.onClick.AddListener(() => OnChoiceButtonPress(button));
                choiceButtons[n] = button;
            }
        }

        //Set button text + callback
        for (int n = 0; n < question.choices.Length; n++)
        {
            choiceButtons[n].interactable = true;
            choiceButtons[n].image.color = Color.gray;
            choiceButtons[n].GetComponentInChildren<Text>().text = $"\"{question.choices[n].title}\" by {question.choices[n].artist}";
            //button.onClick.RemoveAllListeners();
            //button.onClick.AddListener(() => OnChoiceButtonPress(button));
            //Debug.Log("Listeners: " + button.onClick.GetPersistentEventCount());
            if (n == question.answerIndex)
            {
                correctChoice = choiceButtons[n];
            }
        }

        //Shuffle buttons here
    }

    void PlayAnswerSound(bool correct)
    {
        StartCoroutine(FadeOutAudio());
        sfxAudioSource.clip = correct ? rightAnswerSound : wrongAnswerSound;
        sfxAudioSource.Play();
    }

    private void Update()
    {
        if (songAudioSource.isPlaying &&
            !isFadingOut &&
            songAudioSource.time >= (songAudioSource.clip.length - songFadeout))
        {
            StartCoroutine(FadeOutAudio());
        }
    }

    //Move to static utilities class
    IEnumerator FadeOutAudio()
    {
        isFadingOut = true;
        float timePassed = 0f;
        while (songAudioSource.volume > 0f)
        {
            songAudioSource.volume = Mathf.Max(0f, 1f - timePassed / songFadeout);
            timePassed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        isFadingOut = false;
        songAudioSource.Stop();
        songAudioSource.volume = 1f;
    }

    void OnChoiceButtonPress(Button button)
    {
        bool correct = (button == correctChoice);

        timer.Stop();
        scores[currentQuestionIndex] = timer.time;

        headerText.text = correct ? "Correct!" : "Wrong!";
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
        if(!correct && button != null) {
            button.image.color = Color.red;
        }

        StartCoroutine(LoadNextQuestionInSeconds(waitBetweenQuestions));
    }

    void LoadNextQuestion()
    {
        headerText.text = "Name that song!";
        currentQuestionIndex++;
        if (currentQuestionIndex < questions.Length)
        {
            LoadQuestion(questions[currentQuestionIndex]);
        }
        else
        {
            GameManager.results = results;
            GameManager.scores = scores;
            StartCoroutine(GoToResultsScreenAfterDelay(1.5f));
        }
    }

    IEnumerator LoadNextQuestionInSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        LoadNextQuestion();
    }

    IEnumerator GoToResultsScreenAfterDelay(float delay)
    {
        blurPanel.BlurOut();
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("Results");
    }
}
