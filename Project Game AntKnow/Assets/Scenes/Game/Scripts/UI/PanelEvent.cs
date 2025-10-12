using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace AntKnow.Game
{
    /// <summary>
    /// Panel hiển thị event card - random events từ bộ bài event
    /// </summary>
    public class PanelEvent : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI textEvent;
        [SerializeField] private Button btnOK;
        
        [Header("Settings")]
        [SerializeField] private float autoCloseTime = 3f; // Tự động đóng sau 3 giây
        
        [Header("Event Database")]
        [SerializeField] private EventCardData[] eventCards = {
            new EventCardData("Bạn nhận được tiền thưởng từ công ty!", 200),
            new EventCardData("Bạn trúng xổ số!", 500),
            new EventCardData("Bạn phải trả thuế!", -150),
            new EventCardData("Bạn bị mất ví!", -100),
            new EventCardData("Bạn nhận được tiền từ người thân!", 300),
            new EventCardData("Bạn phải sửa xe!", -200),
            new EventCardData("Bạn nhận được tiền hoàn thuế!", 250),
            new EventCardData("Bạn phải trả tiền bảo hiểm!", -180)
        };

        private System.Action<int> onCloseCallback; // ⭐ Callback with money change
        private int currentMoneyChange = 0;
        
        private void Awake()
        {
            if (btnOK != null)
            {
                btnOK.onClick.AddListener(OnOKClicked);
            }
        }
        
        /// <summary>
        /// Show random event
        /// </summary>
        public void ShowRandomEvent(System.Action<int> onClose = null)
        {
            onCloseCallback = onClose;

            // Get random event
            EventCardData randomEvent = GetRandomEvent();
            currentMoneyChange = randomEvent.moneyChange;

            // Format message with money change
            string message = randomEvent.message;
            if (randomEvent.moneyChange > 0)
            {
                message += $"\n+{randomEvent.moneyChange} 💰";
            }
            else if (randomEvent.moneyChange < 0)
            {
                message += $"\n{randomEvent.moneyChange} 💰";
            }

            if (textEvent != null)
            {
                textEvent.text = message;
            }

            gameObject.SetActive(true);

            // Auto close after 3 seconds
            StartCoroutine(AutoCloseCoroutine());
        }
        
        /// <summary>
        /// Show specific event
        /// </summary>
        public void Show(string eventText, int moneyChange, System.Action<int> onClose = null)
        {
            onCloseCallback = onClose;
            currentMoneyChange = moneyChange;

            if (textEvent != null)
            {
                textEvent.text = eventText;
            }

            gameObject.SetActive(true);

            // Auto close after 3 seconds
            StartCoroutine(AutoCloseCoroutine());
        }
        
        /// <summary>
        /// Get random event from database
        /// </summary>
        private EventCardData GetRandomEvent()
        {
            if (eventCards == null || eventCards.Length == 0)
            {
                return new EventCardData("Không có event nào!", 0);
            }

            int randomIndex = Random.Range(0, eventCards.Length);
            return eventCards[randomIndex];
        }
        
        /// <summary>
        /// Auto close coroutine
        /// </summary>
        private IEnumerator AutoCloseCoroutine()
        {
            yield return new WaitForSeconds(autoCloseTime);
            OnOKClicked();
        }
        
        /// <summary>
        /// On OK clicked
        /// </summary>
        private void OnOKClicked()
        {
            StopAllCoroutines();
            onCloseCallback?.Invoke(currentMoneyChange); // ⭐ Pass money change to callback
            Hide();
        }
        
        /// <summary>
        /// Hide panel
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Event card data
    /// </summary>
    [System.Serializable]
    public struct EventCardData
    {
        public string message;
        public int moneyChange; // Positive = gain, Negative = lose

        public EventCardData(string msg, int money)
        {
            message = msg;
            moneyChange = money;
        }
    }
}

