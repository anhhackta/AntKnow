using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace AntKnow.Auth
{
    /// <summary>
    /// Panel thông báo matchmaking - hiện khi tìm thấy trận, không thể join/tạo phòng
    /// Chỉ có 1 panel và 1 text
    /// </summary>
    public class PanelMatchNotification : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TMP_Text notificationText; // Text thông báo (UI.Text)

        [Header("Settings")]
        [SerializeField] private float autoHideDuration = 3f; // Tự động ẩn sau 3 giây

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;

        private Coroutine autoHideCoroutine;

        private void Awake()
        {
            // Ẩn panel ban đầu
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Hiện thông báo với message
        /// </summary>
        public void ShowNotification(string message, float duration = -1f)
        {
            DebugLog($"Showing notification: {message}");

            // Hiện CẢ PANEL
            gameObject.SetActive(true);

            // Set text
            if (notificationText != null)
            {
                notificationText.text = message;
            }

            // Auto hide
            if (autoHideCoroutine != null)
            {
                StopCoroutine(autoHideCoroutine);
            }

            float hideDuration = duration > 0 ? duration : autoHideDuration;
            autoHideCoroutine = StartCoroutine(AutoHideCoroutine(hideDuration));
        }

        /// <summary>
        /// Ẩn thông báo
        /// </summary>
        public void HideNotification()
        {
            DebugLog("Hiding notification");

            // Ẩn CẢ PANEL
            gameObject.SetActive(false);

            if (autoHideCoroutine != null)
            {
                StopCoroutine(autoHideCoroutine);
                autoHideCoroutine = null;
            }
        }

        /// <summary>
        /// Tự động ẩn sau duration giây
        /// </summary>
        private IEnumerator AutoHideCoroutine(float duration)
        {
            yield return new WaitForSeconds(duration);
            HideNotification();
        }

        /// <summary>
        /// Hiện thông báo "Match Found" rồi ẩn ngay (2s)
        /// </summary>
        public void ShowMatchFoundNotification()
        {
            ShowNotification("Match Found", 2f);
        }

        /// <summary>
        /// Hiện thông báo "Đang tìm trận..."
        /// </summary>
        public void ShowSearchingNotification()
        {
            ShowNotification("🔍 Đang tìm trận...", 2f);
        }

        /// <summary>
        /// Hiện thông báo "Hủy tìm trận"
        /// </summary>
        public void ShowCancelledNotification()
        {
            ShowNotification("❌ Đã hủy tìm trận", 2f);
        }

        /// <summary>
        /// Hiện thông báo lỗi
        /// </summary>
        public void ShowErrorNotification(string error)
        {
            ShowNotification($"⚠️ Lỗi: {error}", 3f);
        }

        #region Debug Helpers

        private void DebugLog(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[PanelMatchNotification] {message}");
            }
        }

        #endregion
    }
}

