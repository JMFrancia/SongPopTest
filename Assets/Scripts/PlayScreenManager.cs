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
    bool[] correctAnswers;
    int currentQuestionIndex = -1;

    private void Awake()
    {
        audiosource = GetComponent<AudioSource>();
        questions = GameManager.ActivePlaylist.questions;
        correctAnswers = new bool[questions.Length];

        LoadNextQuestion();
    }

    void LoadQuestionAtIndex(int n) {
        LoadQuestion(questions[n]);
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
        correctAnswers[currentQuestionIndex] = true;
        LoadNextQuestion();
    }

    void OnIncorrectButtonPress() {
        Debug.Log("Incorrect!");
        correctAnswers[currentQuestionIndex] = false;
        LoadNextQuestion();
    }

    void LoadNextQuestion()
    {
        currentQuestionIndex++;
        if(currentQuestionIndex < questions.Length - 1)
        {
            LoadQuestion(questions[currentQuestionIndex]);
        } else {
            SceneManager.LoadScene("Title"); //Replace w/ results screen
        }
    }
}
