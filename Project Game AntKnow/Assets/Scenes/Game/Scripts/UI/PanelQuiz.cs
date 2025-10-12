using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        /// Load random question from Firebase using randomValue field
        /// ✅ OPTIMIZED: Only loads 1 document instead of all
        /// Logic: Random anchor → Query randomValue >= anchor → Fallback to min if not found
        /// </summary>
        private async void LoadRandomQuestion()
        {
            try
            {
                FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

                // ✅ Random anchor point (0.0 - 1.0)
                float anchor = Random.Range(0f, 1f);

                Debug.Log($"[PanelQuiz] Querying quiz with randomValue >= {anchor:F3}");

                // ✅ Query 1 quiz with randomValue >= anchor
                Query query = db.Collection("quizzes")
                    .OrderBy("randomValue")
                    .StartAt(anchor)
                    .Limit(1);

                QuerySnapshot snapshot = await query.GetSnapshotAsync();

                DocumentSnapshot doc = null;

                // If no quiz found (anchor too high), fallback to smallest randomValue
                if (snapshot.Count == 0)
                {
                    Debug.Log("[PanelQuiz] No quiz found with anchor, getting smallest randomValue");
                    query = db.Collection("quizzes")
                        .OrderBy("randomValue")
                        .Limit(1);

                    snapshot = await query.GetSnapshotAsync();
                }

                if (snapshot.Count > 0)
                {
                    // ✅ FIX: Use First() instead of [0]
                    doc = snapshot.Documents.First();

                    // Parse question data
                    Dictionary<string, object> data = doc.ToDictionary();

                    string question = data.ContainsKey("question") && data["question"] != null
                        ? data["question"].ToString()
                        : "Question not found";

                    string difficulty = data.ContainsKey("difficulty") && data["difficulty"] != null
                        ? data["difficulty"].ToString()
                        : "Easy";

                    // ✅ FIX: Parse options array safely
                    string answer1 = "";
                    string answer2 = "";
                    string answer3 = "";
                    string answer4 = "";

                    if (data.ContainsKey("options") && data["options"] != null)
                    {
                        // Try as List<object> first (Firestore array)
                        if (data["options"] is List<object> optionsList)
                        {
                            answer1 = optionsList.Count > 0 && optionsList[0] != null ? optionsList[0].ToString() : "";
                            answer2 = optionsList.Count > 1 && optionsList[1] != null ? optionsList[1].ToString() : "";
                            answer3 = optionsList.Count > 2 && optionsList[2] != null ? optionsList[2].ToString() : "";
                            answer4 = optionsList.Count > 3 && optionsList[3] != null ? optionsList[3].ToString() : "";
                        }
                        // Fallback to object[]
                        else if (data["options"] is object[] optionsArray)
                        {
                            answer1 = optionsArray.Length > 0 && optionsArray[0] != null ? optionsArray[0].ToString() : "";
                            answer2 = optionsArray.Length > 1 && optionsArray[1] != null ? optionsArray[1].ToString() : "";
                            answer3 = optionsArray.Length > 2 && optionsArray[2] != null ? optionsArray[2].ToString() : "";
                            answer4 = optionsArray.Length > 3 && optionsArray[3] != null ? optionsArray[3].ToString() : "";
                        }
                    }

                    int correctIndex = data.ContainsKey("correctAnswer") && data["correctAnswer"] != null
                        ? System.Convert.ToInt32(data["correctAnswer"])
                        : 0;

                    Debug.Log($"[PanelQuiz] ✅ Loaded quiz: '{question}' (difficulty: {difficulty})");

                    // Set question
                    SetQuestion(question, difficulty, answer1, answer2, answer3, answer4, correctIndex);
                }
                else
                {
                    Debug.LogWarning("[PanelQuiz] No questions found in Firebase, using demo");
                    LoadDemoQuestion();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[PanelQuiz] Error loading question: {e.Message}\n{e.StackTrace}");
                LoadDemoQuestion();
            }
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

