using UnityEngine;
using TMPro;
using System.Collections;

namespace AntKnow.Game
{
    /// <summary>
    /// Panel hiển thị thông báo nhanh
    /// </summary>
    public class PanelNotification : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI textNotification;
        
        [Header("Settings")]
        [SerializeField] private float displayDuration = 1f; // Hiển thị 1 giây
        
        private Coroutine notificationCoroutine;
        
        private void Awake()
        {
            // Initially hidden
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Show notification
        /// </summary>
        public void ShowNotification(string message)
        {
            if (textNotification != null)
            {
                textNotification.text = message;
            }
            
            gameObject.SetActive(true);
            
            // Stop previous coroutine if running
            if (notificationCoroutine != null)
            {
                StopCoroutine(notificationCoroutine);
            }
            
            // Start new notification coroutine
            notificationCoroutine = StartCoroutine(NotificationCoroutine());
        }
        
        /// <summary>
        /// Notification coroutine
        /// </summary>
        private IEnumerator NotificationCoroutine()
        {
            yield return new WaitForSeconds(displayDuration);
            Hide();
        }
        
        /// <summary>
        /// Hide notification
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
            
            if (notificationCoroutine != null)
            {
                StopCoroutine(notificationCoroutine);
                notificationCoroutine = null;
            }
        }
        
        /// <summary>
        /// Check if notification is showing
        /// </summary>
        public bool IsShowing()
        {
            return gameObject.activeInHierarchy;
        }
        
        /// <summary>
        /// Show specific notifications
        /// </summary>
        public void ShowTurnOrderNotification(string playerName, int position)
        {
            ShowNotification($"{playerName} đi thứ {position}");
        }
        
        public void ShowGameEndNotification(bool isWin, string winnerName = "")
        {
            if (isWin)
            {
                ShowNotification($"Chúc mừng {winnerName} đã chiến thắng!");
            }
            else
            {
                ShowNotification("Trận đấu kết thúc sau 25 turn!");
            }
        }
        
        public void ShowPlayerJoinedNotification(string playerName)
        {
            ShowNotification($"{playerName} đã tham gia trận đấu");
        }
        
        public void ShowPlayerLeftNotification(string playerName)
        {
            ShowNotification($"{playerName} đã rời khỏi trận đấu");
        }
        
        public void ShowQuizRoundNotification(int roundNumber)
        {
            ShowNotification($"Quiz Round {roundNumber} - Tất cả người chơi trả lời câu hỏi!");
        }
        
        public void ShowSkillActivatedNotification(string playerName, string skillName)
        {
            ShowNotification($"{playerName} sử dụng skill: {skillName}");
        }
        
        public void ShowPropertyPurchasedNotification(string playerName, string propertyName)
        {
            ShowNotification($"{playerName} đã mua {propertyName}");
        }
        
        public void ShowBankruptcyNotification(string playerName)
        {
            ShowNotification($"{playerName} đã phá sản!");
        }
    }
}
