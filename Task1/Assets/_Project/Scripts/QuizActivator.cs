using UnityEngine;
using UnityEngine.UI;

public class QuizActivator : MonoBehaviour
{
    public QuizManager quizManager; 
    public Canvas quizCanvas;       
    public Button[] answerButtons;  
    public AudioSource audioSource;

    public void StartQuiz()
    {
        // Stop crosshair behaviour to allow button selection
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        quizManager.ActivateQuiz(quizCanvas, answerButtons, audioSource);
    }
}