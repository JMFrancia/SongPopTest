using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayScreenManager : MonoBehaviour
{
    [SerializeField] GameObject choiceButtonPrefab;
    [SerializeField] VerticalLayoutGroup choicesLayoutGroup;

    AudioSource audiosource;
    Question[] questions;
    bool[] results;
    int currentQuestionIndex = -1;

    private void Start()
    {
        audiosource = GetComponent<AudioSource>();
        questions = GameManager.ActivePlaylist.questions;
        results = new bool[questions.Length];

        LoadNextQuestion();
    }

    void LoadQuestion(Question question) {
        audiosource.clip = GameManager.Data.SongSamples[question.song];
        audiosource.Play();

        //Refactor to re-use same buttons
        foreach(Transform choice in choicesLayoutGroup.transform) {
            Destroy(choice.gameObject);
        }

        for (int n = 0; n < question.choices.Length; n++) {
            GameObject choiceGO = Instantiate(choiceButtonPrefab, choicesLayoutGroup.transform);
            choiceGO.GetComponentInChildren<Text>().text = $"\"{question.choices[n].title}\" by {question.choices[n].artist}";

            if (n == question.answerIndex) 
            {
                choiceGO.GetComponent<Button>().onClick.AddListener(OnCorrectButtonPress);
            } 
            else
            {
                choiceGO.GetComponent<Button>().onClick.AddListener(OnIncorrectButtonPress);
            }
        } 
    }

    void OnCorrectButtonPress() {
        Debug.Log("Correct!");
        results[currentQuestionIndex] = true;
        LoadNextQuestion();
    }

    void OnIncorrectButtonPress() {
        Debug.Log("Incorrect!");
        results[currentQuestionIndex] = false;
        LoadNextQuestion();
    }

    void LoadNextQuestion()
    {
        currentQuestionIndex++;
        if(currentQuestionIndex < questions.Length)
        {
            LoadQuestion(questions[currentQuestionIndex]);
        } else {
            GameManager.results = results;
            SceneManager.LoadScene("Results");
        }
    }
}
