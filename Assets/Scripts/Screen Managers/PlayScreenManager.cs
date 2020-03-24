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

        timer.onComplete += () => OnChoiceButtonPress(false);

        blurPanel.onBlurInComplete += LoadNextQuestion;
        blurPanel.BlurIn();
    }

    void LoadQuestion(Question question) {
        songAudioSource.clip = GameManager.Data.SongSamples[question.song];
        timer.Set(songAudioSource.clip.length + extraTimeToGuess);
        songAudioSource.Play();

        //Refactor to re-use same buttons
        foreach(Transform choice in choicesLayoutGroup.transform) {
            Destroy(choice.gameObject);
        }

        for (int n = 0; n < question.choices.Length; n++) {
            GameObject choiceGO = Instantiate(choiceButtonPrefab, choicesLayoutGroup.transform);
            choiceGO.GetComponentInChildren<Text>().text = $"\"{question.choices[n].title}\" by {question.choices[n].artist}";
            bool correct = (n == question.answerIndex);
            choiceGO.GetComponent<Button>().onClick.AddListener(() => OnChoiceButtonPress(correct));
        } 
    }

    void PlayAnswerSound(bool correct)
    {
        StartCoroutine(FadeOutAudio());
        sfxAudioSource.clip = correct ? rightAnswerSound : wrongAnswerSound;
        sfxAudioSource.Play();
    }

    private void Update()
    {
        if( songAudioSource.isPlaying && 
            !isFadingOut && 
            songAudioSource.time >= (songAudioSource.clip.length - songFadeout)) 
        {
            StartCoroutine(FadeOutAudio());
        }
    }

    IEnumerator FadeOutAudio() {
        isFadingOut = true;
        float timePassed = 0f;
        while(songAudioSource.volume > 0f) {
            songAudioSource.volume = Mathf.Max(0f , 1f - timePassed / songFadeout);
            timePassed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        isFadingOut = false;
        songAudioSource.Stop();
        songAudioSource.volume = 1f;
    }

    void OnChoiceButtonPress(bool correct) {
        timer.Stop();
        scores[currentQuestionIndex] = timer.time;

        headerText.text = correct ? "Correct!" : "Wrong!";
        results[currentQuestionIndex] = correct;
        PlayAnswerSound(correct);

        for(int n = 0; n < choicesLayoutGroup.transform.childCount; n++) {
            Button b = choicesLayoutGroup.transform.GetChild(n).GetComponent<Button>();
            b.interactable = false;
            if (n == questions[currentQuestionIndex].answerIndex) {
                b.image.color = Color.green;
            }
        }

        StartCoroutine(LoadNextQuestionInSeconds(waitBetweenQuestions));
    }

    IEnumerator LoadNextQuestionInSeconds(float seconds) {
        yield return new WaitForSeconds(seconds);
        LoadNextQuestion();
    }

    void LoadNextQuestion()
    {
        headerText.text = "Name that song!";
        currentQuestionIndex++;
        if(currentQuestionIndex < questions.Length)
        {
            LoadQuestion(questions[currentQuestionIndex]);
        } 
        else
        {
            GameManager.results = results;
            GameManager.scores = scores;
            StartCoroutine(GoToResultsScreen(1.5f));
        }
    }

    IEnumerator GoToResultsScreen(float delay) {
        blurPanel.BlurOut();
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("Results");
    }
}
