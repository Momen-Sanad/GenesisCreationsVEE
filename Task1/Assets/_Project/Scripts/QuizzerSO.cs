using UnityEngine;

[CreateAssetMenu(fileName = "QuizzerSO", menuName = "Scriptable Objects/QuizzerSO")]
public class QuizzerSO : ScriptableObject
{

    [System.Serializable]
    public class Question
    {
        [TextArea] public string questionText;
        public string[] answers = new string[4];

        [Tooltip("From 0 to 3")]
        public int correctAnswerIndex; // 0–3
    }

    public Question[] questions;

    public Question GetQuestion(int index)
    {
        if (index < 0 || index >= questions.Length)
        {
            Debug.LogError("Question index out of range");
            return null;
        }
        return questions[index];
    }
}