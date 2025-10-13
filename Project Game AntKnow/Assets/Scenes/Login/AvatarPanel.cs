using System;
using UnityEngine;
using UnityEngine.UI;
using AntKnow.Auth;
using Firebase.Auth;

namespace AntKnow.Auth
{
    public class AvatarPanel : MonoBehaviour
    {
        [Header("Avatar Panel Components")]
        [SerializeField] private GameObject avatarPanel;
        [SerializeField] private Image avatarImage;
        [SerializeField] private Text textIngameName;
        [SerializeField] private Text textOnlineStatus;

        [Header("Gender Sprites")]
        [SerializeField] private Sprite maleAvatarSprite;      // Male avatar
        [SerializeField] private Sprite femaleAvatarSprite;    // Female avatar
        [SerializeField] private Sprite defaultAvatarSprite;   // Default (if no gender set)

        private UserData currentUserData;
        private FirebaseAuthService firebaseAuthService;


        private void Start()
        {
            InitializePanel();
            SetupEventListeners();
            HidePanel();
        }

        private void InitializePanel()
        {
            // Tìm FirebaseAuthService nếu chưa được gán
            if (firebaseAuthService == null)
            {
                firebaseAuthService = FindObjectOfType<FirebaseAuthService>();
            }

            // Set default avatar if no sprite assigned
            if (avatarImage != null && defaultAvatarSprite != null)
            {
                avatarImage.sprite = defaultAvatarSprite;
            }

            // Set default online status
            if (textOnlineStatus != null)
            {
                textOnlineStatus.text = "Online";
            }
        }

        private void SetupEventListeners()
        {
            // Subscribe to Firebase auth events
            if (firebaseAuthService != null)
            {
                firebaseAuthService.OnUserSignedIn += OnUserSignedIn;
                firebaseAuthService.OnUserSignedOut += OnUserSignedOut;
            }
        }

        /// <summary>
        /// Hiển thị panel với thông tin user
        /// </summary>
        public void ShowPanel(UserData userData)
        {
            currentUserData = userData;
            
            if (avatarPanel != null)
            {
                avatarPanel.SetActive(true);
            }

            UpdateUserInfo(userData);
        }

        /// <summary>
        /// Ẩn panel
        /// </summary>
        public void HidePanel()
        {
            if (avatarPanel != null)
            {
                avatarPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Cập nhật thông tin user trên panel
        /// </summary>
        public void UpdateUserInfo(UserData userData)
        {
            currentUserData = userData;

            // Update ingame name
            if (textIngameName != null)
            {
                if (!string.IsNullOrEmpty(userData.ingameName))
                {
                    textIngameName.text = userData.ingameName;
                }
                else
                {
                    // Default name if not set
                    textIngameName.text = "New Player";
                }
            }

            // Update avatar image based on gender
            if (avatarImage != null)
            {
                Sprite avatarToUse = GetAvatarSpriteByGender(userData.gender);
                avatarImage.sprite = avatarToUse;
            }

            Debug.Log($"Avatar Panel updated for user: {userData.username}, gender: {userData.gender}");
        }

        /// <summary>
        /// Get avatar sprite based on gender
        /// </summary>
        private Sprite GetAvatarSpriteByGender(string gender)
        {
            if (string.IsNullOrEmpty(gender))
            {
                // No gender set, use default
                return defaultAvatarSprite != null ? defaultAvatarSprite : null;
            }

            switch (gender.ToLower())
            {
                case "male":
                case "nam":
                    return maleAvatarSprite != null ? maleAvatarSprite : defaultAvatarSprite;

                case "female":
                case "nữ":
                case "nu":
                    return femaleAvatarSprite != null ? femaleAvatarSprite : defaultAvatarSprite;

                default:
                    return defaultAvatarSprite;
            }
        }

        /// <summary>
        /// Cập nhật ingame name (gọi từ MenuScene)
        /// </summary>
        public void UpdateIngameName(string ingameName)
        {
            if (currentUserData != null)
            {
                currentUserData.ingameName = ingameName;
                UpdateUserInfo(currentUserData);
            }
        }

        private void OnUserSignedIn(FirebaseUser user)
        {
            Debug.Log($"Avatar Panel: User signed in - {user.Email}");
        }

        private void OnUserSignedOut()
        {
            Debug.Log("Avatar Panel: User signed out");
            HidePanel();
        }


        private void OnDestroy()
        {
            // Unsubscribe from events
            if (firebaseAuthService != null)
            {
                firebaseAuthService.OnUserSignedIn -= OnUserSignedIn;
                firebaseAuthService.OnUserSignedOut -= OnUserSignedOut;
            }
        }


    }
}
