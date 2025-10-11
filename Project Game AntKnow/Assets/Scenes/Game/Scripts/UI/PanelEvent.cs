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
        [SerializeField] private string[] eventCards = {
            "Bạn nhận được tiền thưởng từ công ty: +200",
            "Bạn trúng xổ số: +500",
            "Bạn phải trả thuế: -150",
            "Bạn bị mất ví: -100",
            "Bạn nhận được tiền từ người thân: +300",
            "Bạn phải sửa xe: -200",
            "Bạn nhận được tiền hoàn thuế: +250",
            "Bạn phải trả tiền bảo hiểm: -180"
        };
        
        private System.Action onCloseCallback;
        
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
        public void ShowRandomEvent(System.Action onClose = null)
        {
            onCloseCallback = onClose;
            
            // Get random event
            string randomEvent = GetRandomEvent();
            
            if (textEvent != null)
            {
                textEvent.text = randomEvent;
            }
            
            gameObject.SetActive(true);
            
            // Auto close after 3 seconds
            StartCoroutine(AutoCloseCoroutine());
        }
        
        /// <summary>
        /// Show specific event
        /// </summary>
        public void Show(string eventText, System.Action onClose = null)
        {
            onCloseCallback = onClose;
            
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
        private string GetRandomEvent()
        {
            if (eventCards == null || eventCards.Length == 0)
            {
                return "Không có event nào!";
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
            onCloseCallback?.Invoke();
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
}

