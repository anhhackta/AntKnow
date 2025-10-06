using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

namespace AntKnow.Game
{
    /// <summary>
    /// Panel hiển thị active cards để player chọn sử dụng
    /// </summary>
    public class PanelCard : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Transform cardContainer; // Parent cho card buttons
        [SerializeField] private GameObject cardButtonPrefab; // Prefab cho mỗi card button
        [SerializeField] private TextMeshProUGUI textTimer; // Timer countdown
        [SerializeField] private Button btnSkip; // Button bỏ qua
        
        [Header("Settings")]
        [SerializeField] private float choiceTime = 10f; // 10 giây để chọn
        
        private List<CardButton> cardButtons = new List<CardButton>();
        private float timeRemaining = 0f;
        private bool isChosen = false;
        
        private System.Action<int> onCardChosenCallback; // cardId, -1 = skip
        
        private void Awake()
        {
            if (btnSkip != null)
            {
                btnSkip.onClick.AddListener(OnSkipClicked);
            }
        }
        
        /// <summary>
        /// Show panel với list active cards
        /// </summary>
        public void Show(List<CardData> cards, System.Action<int> onCardChosen)
        {
            onCardChosenCallback = onCardChosen;
            isChosen = false;
            timeRemaining = choiceTime;
            
            // Clear old cards
            ClearCards();
            
            // Create card buttons
            foreach (var card in cards)
            {
                CreateCardButton(card);
            }
            
            gameObject.SetActive(true);
            StartCoroutine(TimerCoroutine());
        }
        
        /// <summary>
        /// Clear cards
        /// </summary>
        private void ClearCards()
        {
            foreach (var btn in cardButtons)
            {
                if (btn != null && btn.gameObject != null)
                {
                    Destroy(btn.gameObject);
                }
            }
            cardButtons.Clear();
        }
        
        /// <summary>
        /// Create card button
        /// </summary>
        private void CreateCardButton(CardData card)
        {
            if (cardButtonPrefab == null || cardContainer == null) return;
            
            GameObject btnObj = Instantiate(cardButtonPrefab, cardContainer);
            CardButton cardBtn = btnObj.GetComponent<CardButton>();
            
            if (cardBtn != null)
            {
                cardBtn.Initialize(card, OnCardClicked);
                cardButtons.Add(cardBtn);
            }
        }
        
        /// <summary>
        /// Timer coroutine
        /// </summary>
        private IEnumerator TimerCoroutine()
        {
            while (timeRemaining > 0 && !isChosen)
            {
                timeRemaining -= Time.deltaTime;
                
                if (textTimer != null)
                {
                    textTimer.text = $"{Mathf.CeilToInt(timeRemaining)}s";
                }
                
                yield return null;
            }
            
            // Time's up
            if (!isChosen)
            {
                OnSkipClicked();
            }
        }
        
        /// <summary>
        /// On card clicked
        /// </summary>
        private void OnCardClicked(int cardId)
        {
            if (isChosen) return;
            
            isChosen = true;
            StopAllCoroutines();
            
            Debug.Log($"[PanelCard] Card {cardId} chosen");
            
            onCardChosenCallback?.Invoke(cardId);
            Hide();
        }
        
        /// <summary>
        /// On skip clicked
        /// </summary>
        private void OnSkipClicked()
        {
            if (isChosen) return;
            
            isChosen = true;
            StopAllCoroutines();
            
            Debug.Log("[PanelCard] Skipped");
            
            onCardChosenCallback?.Invoke(-1); // -1 = skip
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
    
    /// <summary>
    /// Card data
    /// </summary>
    [System.Serializable]
    public class CardData
    {
        public int cardId;
        public string cardName;
        public string cardDescription;
        public Sprite cardSprite;
        public int cooldownRemaining; // Turns remaining
    }
}

