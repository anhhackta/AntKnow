using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Firebase.Firestore;
using Firebase.Extensions;

namespace AntKnow.Game
{
    /// <summary>
    /// Panel quiz - lấy câu hỏi từ Firebase
    /// </summary>
    public class PanelQuiz : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI textQuestion;
        [SerializeField] private Button btnAnswer1;
        [SerializeField] private Button btnAnswer2;
        [SerializeField] private Button btnAnswer3;
        [SerializeField] private Button btnAnswer4;
        [SerializeField] private TextMeshProUGUI textTimer;
        
        [Header("Settings")]
        [SerializeField] private float answerTime = 30f; // 30 giây để trả lời
        
        private TextMeshProUGUI[] answerTexts = new TextMeshProUGUI[4];
        private int correctAnswerIndex = -1;
        private float timeRemaining = 0f;
        private bool isAnswered = false;
        
        private System.Action<bool> onAnswerCallback; // true = correct, false = wrong
        
        private void Awake()
        {
            // Get answer texts
            answerTexts[0] = btnAnswer1.GetComponentInChildren<TextMeshProUGUI>();
            answerTexts[1] = btnAnswer2.GetComponentInChildren<TextMeshProUGUI>();
            answerTexts[2] = btnAnswer3.GetComponentInChildren<TextMeshProUGUI>();
            answerTexts[3] = btnAnswer4.GetComponentInChildren<TextMeshProUGUI>();
            
            // Setup button listeners
            btnAnswer1.onClick.AddListener(() => OnAnswerClicked(0));
            btnAnswer2.onClick.AddListener(() => OnAnswerClicked(1));
            btnAnswer3.onClick.AddListener(() => OnAnswerClicked(2));
            btnAnswer4.onClick.AddListener(() => OnAnswerClicked(3));
        }
        
        /// <summary>
        /// Show quiz panel
        /// </summary>
        public void Show(System.Action<bool> onAnswer)
        {
            onAnswerCallback = onAnswer;
            isAnswered = false;
            timeRemaining = answerTime;
            
            // Load random question from Firebase
            LoadRandomQuestion();
            
            gameObject.SetActive(true);
            StartCoroutine(TimerCoroutine());
        }
        
        /// <summary>
        /// Load random question from Firebase
        /// </summary>
        private void LoadRandomQuestion()
        {
            FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
            
            db.Collection("quizzes").GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError($"[PanelQuiz] Error loading questions: {task.Exception}");
                    LoadDemoQuestion();
                    return;
                }
                
                if (task.IsCompleted)
                {
                    QuerySnapshot snapshot = task.Result;

                    if (snapshot.Count == 0)
                    {
                        Debug.LogWarning("[PanelQuiz] No questions found in Firebase");
                        LoadDemoQuestion();
                        return;
                    }

                    // Convert to list to support indexing
                    var documentsList = new List<DocumentSnapshot>(snapshot.Documents);

                    // Get random question
                    int randomIndex = Random.Range(0, documentsList.Count);
                    DocumentSnapshot doc = documentsList[randomIndex];

                    // Parse question data
                    Dictionary<string, object> data = doc.ToDictionary();

                    string question = data.ContainsKey("question") ? data["question"].ToString() : "Question not found";
                    string answer1 = data.ContainsKey("answer1") ? data["answer1"].ToString() : "";
                    string answer2 = data.ContainsKey("answer2") ? data["answer2"].ToString() : "";
                    string answer3 = data.ContainsKey("answer3") ? data["answer3"].ToString() : "";
                    string answer4 = data.ContainsKey("answer4") ? data["answer4"].ToString() : "";
                    int correctIndex = data.ContainsKey("valueRandom") ? System.Convert.ToInt32(data["valueRandom"]) : 0;

                    // Set question
                    SetQuestion(question, answer1, answer2, answer3, answer4, correctIndex);
                }
            });
        }
        
        /// <summary>
        /// Load demo question (fallback)
        /// </summary>
        private void LoadDemoQuestion()
        {
            SetQuestion(
                "2 + 2 = ?",
                "3",
                "4",
                "5",
                "6",
                1 // Index 1 = "4" is correct
            );
        }
        
        /// <summary>
        /// Set question data
        /// </summary>
        private void SetQuestion(string question, string ans1, string ans2, string ans3, string ans4, int correctIndex)
        {
            if (textQuestion != null)
            {
                textQuestion.text = question;
            }
            
            answerTexts[0].text = ans1;
            answerTexts[1].text = ans2;
            answerTexts[2].text = ans3;
            answerTexts[3].text = ans4;
            
            correctAnswerIndex = correctIndex;
            
            // Enable all buttons
            btnAnswer1.interactable = true;
            btnAnswer2.interactable = true;
            btnAnswer3.interactable = true;
            btnAnswer4.interactable = true;
        }
        
        /// <summary>
        /// Timer coroutine
        /// </summary>
        private IEnumerator TimerCoroutine()
        {
            while (timeRemaining > 0 && !isAnswered)
            {
                timeRemaining -= Time.deltaTime;
                
                if (textTimer != null)
                {
                    textTimer.text = $"{Mathf.CeilToInt(timeRemaining)}s";
                }
                
                yield return null;
            }
            
            // Time's up
            if (!isAnswered)
            {
                OnTimeUp();
            }
        }
        
        /// <summary>
        /// On answer clicked
        /// </summary>
        private void OnAnswerClicked(int answerIndex)
        {
            if (isAnswered) return;
            
            isAnswered = true;
            
            // Disable all buttons
            btnAnswer1.interactable = false;
            btnAnswer2.interactable = false;
            btnAnswer3.interactable = false;
            btnAnswer4.interactable = false;
            
            // Check if correct
            bool isCorrect = (answerIndex == correctAnswerIndex);
            
            Debug.Log($"[PanelQuiz] Answer {answerIndex} clicked. Correct: {isCorrect}");
            
            // Callback
            StartCoroutine(ShowResultAndClose(isCorrect));
        }
        
        /// <summary>
        /// On time up
        /// </summary>
        private void OnTimeUp()
        {
            Debug.Log("[PanelQuiz] Time's up!");
            isAnswered = true;
            
            // Treat as wrong answer
            StartCoroutine(ShowResultAndClose(false));
        }
        
        /// <summary>
        /// Show result and close
        /// </summary>
        private IEnumerator ShowResultAndClose(bool isCorrect)
        {
            // Show result (optional: highlight correct/wrong answer)
            if (textQuestion != null)
            {
                textQuestion.text = isCorrect ? "Đúng rồi! ✓" : "Sai rồi! ✗";
            }
            
            yield return new WaitForSeconds(1.5f);
            
            // Callback
            onAnswerCallback?.Invoke(isCorrect);
            
            // Hide
            Hide();
        }
        
        /// <summary>
        /// Hide panel
        /// </summary>
        public void Hide()
        {
            StopAllCoroutines();
            gameObject.SetActive(false);
        }
    }
}

