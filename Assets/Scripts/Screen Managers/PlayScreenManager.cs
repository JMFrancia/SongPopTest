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
    [SerializeField] GameObject choiceButtonPrefab;
    [SerializeField] VerticalLayoutGroup choicesLayoutGroup;
    [SerializeField] Text headerText;
    [SerializeField] Timer timer;

    AudioSource audiosource;
    Question[] questions;
    bool[] results;
    float[] scores;
    int currentQuestionIndex = -1;
    bool isFadingOut = false;

    private void Start()
    {
        audiosource = GetComponent<AudioSource>();
        questions = GameManager.ActivePlaylist.questions;
        results = new bool[questions.Length];
        scores = new float[questions.Length];

        LoadNextQuestion();
    }

    void LoadQuestion(Question question) {
        audiosource.clip = GameManager.Data.SongSamples[question.song];
        timer.Set(audiosource.clip.length + extraTimeToGuess);
        audiosource.Play();

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

    private void Update()
    {
        if( audiosource.isPlaying && 
            !isFadingOut && 
            audiosource.time >= (audiosource.clip.length - songFadeout)) 
        {
            StartCoroutine(FadeOutAudio());
        }
    }

    IEnumerator FadeOutAudio() {
        isFadingOut = true;
        float timePassed = 0f;
        while(audiosource.volume > 0f) {
            audiosource.volume = Mathf.Max(0f , 1f - timePassed / songFadeout);
            timePassed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        isFadingOut = false;
        audiosource.Stop();
        audiosource.volume = 1f;
    }

    void OnChoiceButtonPress(bool correct) {
        timer.Stop();
        scores[currentQuestionIndex] = timer.time;
        if(correct)
        {
            headerText.text = "Correct!";
            results[currentQuestionIndex] = true;

        } else
        {
            headerText.text = "Wrong!";
            results[currentQuestionIndex] = false;
        }

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
        } else {
            GameManager.results = results;
            GameManager.scores = scores;
            SceneManager.LoadScene("Results");
        }
    }
}
