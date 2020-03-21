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
    int currentQuestionIndex = 0;

    private void Awake()
    {
        audiosource = GetComponent<AudioSource>();
        questions = GameManager.ActivePlaylist.questions;
        correctAnswers = new bool[questions.Length];

        LoadQuestion(questions[0]);
    }

    void LoadQuestion(Question question) {
        for (int n = 0; n < question.choices.Length; n++) {
            GameObject choiceGO = Instantiate(choiceButtonPrefab, choicesLayoutGroup.transform);
            choiceGO.GetComponentInChildren<Text>().text = $"\"{question.choices[n].title}\" by {question.choices[n].artist}";
            if(n == question.answerIndex) {
                choiceGO.GetComponent<Button>().onClick.AddListener(OnCorrectButtonPress);
            } else {
                choiceGO.GetComponent<Button>().onClick.AddListener(OnIncorrectButtonPress);
            }
        } 
    }

    void OnCorrectButtonPress() {
        Debug.Log("Correct!");
        correctAnswers[currentQuestionIndex] = true;
    }

    void OnIncorrectButtonPress() {
        Debug.Log("Incorrect!");
        correctAnswers[currentQuestionIndex] = false;
    }
}
