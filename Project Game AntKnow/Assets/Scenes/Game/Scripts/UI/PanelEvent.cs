using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace AntKnow.Game
{
    /// <summary>
    /// Panel hiển thị event card
    /// </summary>
    public class PanelEvent : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI textEvent;
        [SerializeField] private Button btnOK;
        
        [Header("Settings")]
        [SerializeField] private float autoCloseTime = 3f; // Tự động đóng sau 3 giây
        
        private System.Action onCloseCallback;
        
        private void Awake()
        {
            if (btnOK != null)
            {
                btnOK.onClick.AddListener(OnOKClicked);
            }
        }
        
        /// <summary>
        /// Show event
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

