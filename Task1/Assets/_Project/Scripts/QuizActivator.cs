using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class QuizActivator : MonoBehaviour
{
    public enum QuizMode
    {
        PlayAll,
        SingleQuestion
    }

    [Header("Quiz Data")]
    public QuizzerSO quizData;

    [Header("UI References")]
    public GameObject quizCanvas;
    public TMP_Text questionText;
    public Button[] answerButtons;

    [Header("Visual Feedback")]
    public Color correctColor = Color.green;
    public Color wrongColor = Color.red;
    public Color normalColor = Color.white;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip correctSound;

    [Header("Quiz Flow")]
    public QuizMode quizMode = QuizMode.PlayAll;
    public int specificQuestionIndex = 0; // Used if quizMode == SingleQuestion
    public float nextQuestionDelay = 2f; // Seconds before auto advancing
    public bool loopQuestions = false;   // Restart after finishing?

    int currentQuestionIndex;
    bool questionActive;

    void Awake()
    {
        if (quizCanvas)
            quizCanvas.SetActive(false);
    }

    public void StartQuiz()
    {
        if (quizCanvas)
            quizCanvas.SetActive(true);

        if (quizMode == QuizMode.SingleQuestion)
        {
            currentQuestionIndex = specificQuestionIndex;
            ShowQuestion(currentQuestionIndex);
        }
        else
        {
            currentQuestionIndex = 0;
            ShowQuestion(currentQuestionIndex);
        }
    }

    void ShowQuestion(int index)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        var question = quizData.GetQuestion(index);
        if (question == null) return;

        questionText.text = question.questionText;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            // Reset visuals
            answerButtons[i].image.color = normalColor;
            answerButtons[i].onClick.RemoveAllListeners();

            // Assign text if available
            TMP_Text btnText = answerButtons[i].GetComponentInChildren<TMP_Text>();
            if (i < question.answers.Length && btnText != null)
            {
                btnText.text = question.answers[i];
                answerButtons[i].gameObject.SetActive(true);
            }
            else
            {
                answerButtons[i].gameObject.SetActive(false);
            }

            int capturedIndex = i;
            answerButtons[i].onClick.AddListener(() => OnAnswerSelected(capturedIndex));
        }

        questionActive = true;
    }

    void OnAnswerSelected(int chosenIndex)
    {
        if (!questionActive) return;

        var question = quizData.GetQuestion(currentQuestionIndex);
        int correctIndex = question.correctAnswerIndex;

        if (chosenIndex == correctIndex)
        {
            // Correct Answer
            answerButtons[chosenIndex].image.color = correctColor;

            if (audioSource && correctSound)
                audioSource.PlayOneShot(correctSound);

            questionActive = false;

            if (quizMode == QuizMode.PlayAll)
                StartCoroutine(NextQuestionDelay());
            else
                Debug.Log("Correct! Single-question mode ends here.");
        }
        else
        {
            // Wrong Answer – stay on the same question
            answerButtons[chosenIndex].image.color = wrongColor;
        }
    }

    IEnumerator NextQuestionDelay()
    {
        yield return new WaitForSeconds(nextQuestionDelay);

        currentQuestionIndex++;

        if (currentQuestionIndex < quizData.questions.Length)
        {
            ShowQuestion(currentQuestionIndex);
        }
        else
        {
            if (loopQuestions)
            {
                currentQuestionIndex = 0;
                ShowQuestion(currentQuestionIndex);
            }
            else
            {
                Debug.Log("Quiz Finished!");
                if (quizCanvas) quizCanvas.SetActive(false);
            }
        }
    }
}
