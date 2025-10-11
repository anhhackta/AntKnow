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
        [SerializeField] private TextMeshProUGUI textDifficulty;
        [SerializeField] private Button btnAnswer1;
        [SerializeField] private Button btnAnswer2;
        [SerializeField] private Button btnAnswer3;
        [SerializeField] private Button btnAnswer4;
        [SerializeField] private TextMeshProUGUI textTimer;
        
        [Header("Fortune Wheel")]
        [SerializeField] private GameObject fortuneWheelObject;
        [SerializeField] private TextMeshProUGUI textWheelResult;
        
        [Header("Settings")]
        [SerializeField] private float answerTime = 15f; // 15 giây để trả lời
        [SerializeField] private bool isAnnualQuiz = false; // Quiz thường niên 8 turn
        
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
        public void Show(System.Action<bool> onAnswer, bool annualQuiz = false)
        {
            onAnswerCallback = onAnswer;
            isAnswered = false;
            timeRemaining = answerTime;
            isAnnualQuiz = annualQuiz;
            
            // Hide fortune wheel initially
            if (fortuneWheelObject != null)
            {
                fortuneWheelObject.SetActive(false);
            }
            
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
                    string difficulty = data.ContainsKey("difficulty") ? data["difficulty"].ToString() : "Easy";
                    
                    // Parse options array
                    var options = data.ContainsKey("options") ? data["options"] as object[] : new object[4];
                    string answer1 = options.Length > 0 ? options[0].ToString() : "";
                    string answer2 = options.Length > 1 ? options[1].ToString() : "";
                    string answer3 = options.Length > 2 ? options[2].ToString() : "";
                    string answer4 = options.Length > 3 ? options[3].ToString() : "";
                    
                    int correctIndex = data.ContainsKey("correctAnswer") ? System.Convert.ToInt32(data["correctAnswer"]) : 0;

                    // Set question
                    SetQuestion(question, difficulty, answer1, answer2, answer3, answer4, correctIndex);
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
                "Easy",
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
        private void SetQuestion(string question, string difficulty, string ans1, string ans2, string ans3, string ans4, int correctIndex)
        {
            if (textQuestion != null)
            {
                textQuestion.text = question;
            }
            
            if (textDifficulty != null)
            {
                textDifficulty.text = difficulty;
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
            // Show result on difficulty text
            if (textDifficulty != null)
            {
                textDifficulty.text = isCorrect ? "Trả lời đúng" : "Trả lời sai";
            }
            
            // Highlight correct/wrong buttons
            HighlightAnswers(isCorrect);
            
            yield return new WaitForSeconds(3f);
            
            // If wrong answer and annual quiz, show fortune wheel
            if (!isCorrect && isAnnualQuiz)
            {
                yield return StartCoroutine(ShowFortuneWheel());
            }
            
            // Callback
            onAnswerCallback?.Invoke(isCorrect);
            
            // Hide
            Hide();
        }
        
        /// <summary>
        /// Highlight correct/wrong answers
        /// </summary>
        private void HighlightAnswers(bool isCorrect)
        {
            Button[] buttons = { btnAnswer1, btnAnswer2, btnAnswer3, btnAnswer4 };
            
            // Highlight correct answer in green
            if (correctAnswerIndex >= 0 && correctAnswerIndex < buttons.Length)
            {
                var correctBtn = buttons[correctAnswerIndex];
                var colors = correctBtn.colors;
                colors.normalColor = Color.green;
                correctBtn.colors = colors;
            }
            
            // If wrong, highlight selected answer in red
            if (!isCorrect)
            {
                // Find which button was clicked (this would need to be tracked)
                // For now, just show the correct answer
            }
        }
        
        /// <summary>
        /// Show fortune wheel for wrong answer in annual quiz
        /// </summary>
        private IEnumerator ShowFortuneWheel()
        {
            if (fortuneWheelObject != null)
            {
                fortuneWheelObject.SetActive(true);
                
                // Spin animation (simplified)
                float spinTime = 2f;
                float elapsed = 0f;
                
                while (elapsed < spinTime)
                {
                    elapsed += Time.deltaTime;
                    // Add rotation animation here
                    yield return null;
                }
                
                // Random penalty (1/3 chance for each)
                int penaltyType = Random.Range(0, 3);
                string penaltyText = "";
                
                switch (penaltyType)
                {
                    case 0:
                        penaltyText = "Trừ tiền random";
                        break;
                    case 1:
                        penaltyText = "Hạ 1 nhà bất kì";
                        break;
                    case 2:
                        penaltyText = "Không làm gì cả";
                        break;
                }
                
                if (textWheelResult != null)
                {
                    textWheelResult.text = penaltyText;
                }
                
                yield return new WaitForSeconds(2f);
                
                fortuneWheelObject.SetActive(false);
            }
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

