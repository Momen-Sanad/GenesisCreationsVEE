using System;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

[CreateAssetMenu(fileName = "QuizManager", menuName = "Scriptable Objects/QuizManager")]
public class QuizManager : ScriptableObject
{
    [Range(0, 3)]
    public int correctAnswerIndex = 0;

    Button[] answerButtons;
    Canvas quizCanvas;
    Color defaultButtonColor;
    AudioSource audioSource;

    public void ActivateQuiz(Canvas canvas, Button[] buttons, AudioSource source)
    {
        quizCanvas = canvas;
        answerButtons = buttons;
        audioSource = source;

        if (quizCanvas == null || answerButtons == null || answerButtons.Length != 4)
        {
            Debug.LogError("QuizManager: Missing Canvas or Buttons references!");
            return;
        }

        defaultButtonColor = answerButtons[0].image.color;
        quizCanvas.gameObject.SetActive(true);

        // Reset & wire up buttons
        for (var i = 0; i < answerButtons.Length; i++)
        {
            var idx = i;
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => OnAnswerSelected(idx));
            answerButtons[i].image.color = defaultButtonColor;
        }
    }

    void OnAnswerSelected(int index)
    {
        // Change colors
        for (var i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].image.color = (i == correctAnswerIndex) ? Color.green : Color.red;
        }

        // Play audio if correct
        if (index == correctAnswerIndex && audioSource != null)
        {
            audioSource.Play();
        }
    }
}