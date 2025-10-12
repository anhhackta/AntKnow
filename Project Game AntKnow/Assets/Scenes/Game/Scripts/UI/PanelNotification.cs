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
            // ⭐ KHÔNG set inactive trong Awake()
            // Để Unity Inspector quyết định initial state
            // ShowNotification() sẽ tự activate khi cần
        }
        
        /// <summary>
        /// Show notification
        /// </summary>
        public void ShowNotification(string message)
        {
            Debug.Log($"[PanelNotification] ShowNotification: {message}");

            if (textNotification != null)
            {
                textNotification.text = message;
            }

            // ⭐ Check and activate ALL parents in hierarchy
            Transform current = transform.parent;
            while (current != null)
            {
                if (!current.gameObject.activeSelf)
                {
                    Debug.LogWarning($"[PanelNotification] Parent '{current.name}' is inactive! Activating...");
                    current.gameObject.SetActive(true);
                }
                current = current.parent;
            }

            // ⭐ Activate this GameObject
            Debug.Log($"[PanelNotification] Before SetActive: activeSelf={gameObject.activeSelf}");
            gameObject.SetActive(true);
            Debug.Log($"[PanelNotification] After SetActive: activeSelf={gameObject.activeSelf}");

            Debug.Log($"[PanelNotification] Panel is now active: {gameObject.activeInHierarchy}");

            // ⭐ If still not active, log error
            if (!gameObject.activeInHierarchy)
            {
                Debug.LogError("[PanelNotification] Panel still not active! Cannot start coroutine!");
                Debug.LogError("[PanelNotification] Checking hierarchy...");
                Transform node = transform;
                while (node != null)
                {
                    Debug.LogError($"  - {node.name}: activeSelf={node.gameObject.activeSelf}, activeInHierarchy={node.gameObject.activeInHierarchy}");
                    node = node.parent;
                }
                return; // ⭐ Don't start coroutine if inactive
            }

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
