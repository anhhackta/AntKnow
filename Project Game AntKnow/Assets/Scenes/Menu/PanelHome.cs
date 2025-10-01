using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AntKnow.Auth;

namespace AntKnow.Auth
{
    /// <summary>
    /// Panel Home - hiển thị nhân vật đã chọn và các chức năng chính
    /// </summary>
    public class PanelHome : MonoBehaviour
    {
        [Header("Character Display")]
        [SerializeField] private Image characterImage; // 1 Image component duy nhất

        [Header("Character Sprites")]
        [SerializeField] private Sprite maleCharacterSprite;
        [SerializeField] private Sprite femaleCharacterSprite;

        // Loại bỏ xúc xắc để đơn giản hóa

        private GameDataManager gameDataManager;

        private void Start()
        {
            InitializePanelHome();
        }

        private void InitializePanelHome()
        {
            gameDataManager = GameDataManager.Instance;
            SetupEventListeners();
            
            // Không cập nhật sprite ngay lập tức vì data chưa load
            // Sẽ được gọi từ MenuSceneManager sau khi load data
            Debug.Log("PanelHome: Initialized, waiting for data...");
        }

        private void SetupEventListeners()
        {
            // Action buttons sẽ được xử lý trong panel con khác
            // Không cần setup event listeners ở đây
        }

        public void UpdateCharacterDisplay()
        {
            // Cập nhật sprite dựa trên gender từ database
            UpdateCharacterSprite();
        }

        public void ForceUpdateCharacterSprite()
        {
            Debug.Log("PanelHome: Force updating character sprite...");
            
            // Kiểm tra xem có data chưa
            if (gameDataManager == null)
            {
                Debug.LogError("PanelHome: GameDataManager is null!");
                return;
            }
            
            if (string.IsNullOrEmpty(gameDataManager.currentGender))
            {
                Debug.LogWarning("PanelHome: Gender data not loaded yet, retrying in 0.5s...");
                Invoke(nameof(ForceUpdateCharacterSprite), 0.5f);
                return;
            }
            
            UpdateCharacterSprite();
        }

        private void UpdateCharacterSprite()
        {
            Debug.Log("=== PANELHOME DEBUG ===");
            
            if (characterImage == null) 
            {
                Debug.LogError("PanelHome: CharacterImage is null! Please assign it in the inspector.");
                return;
            }
            Debug.Log("✓ CharacterImage component found");

            if (gameDataManager == null)
            {
                Debug.LogError("PanelHome: GameDataManager is null!");
                return;
            }
            Debug.Log("✓ GameDataManager found");

            // Lấy gender từ database và cập nhật sprite
            string gender = gameDataManager.currentGender;
            Debug.Log($"PanelHome: Current gender from database: '{gender}'");
            Debug.Log($"PanelHome: Male sprite assigned: {maleCharacterSprite != null}");
            Debug.Log($"PanelHome: Female sprite assigned: {femaleCharacterSprite != null}");

            Sprite spriteToUse = null;

            if (gender == "male" && maleCharacterSprite != null)
            {
                spriteToUse = maleCharacterSprite;
                Debug.Log("PanelHome: Using MALE sprite");
            }
            else if (gender == "female" && femaleCharacterSprite != null)
            {
                spriteToUse = femaleCharacterSprite;
                Debug.Log("PanelHome: Using FEMALE sprite");
            }
            else
            {
                Debug.LogWarning($"PanelHome: No sprite found for gender '{gender}'");
                Debug.LogWarning($"PanelHome: Male sprite null: {maleCharacterSprite == null}");
                Debug.LogWarning($"PanelHome: Female sprite null: {femaleCharacterSprite == null}");
            }

            if (spriteToUse != null)
            {
                characterImage.sprite = spriteToUse;
                characterImage.enabled = true;
                Debug.Log($"PanelHome: ✓ SUCCESS - Updated character sprite to {gender}");
                Debug.Log($"PanelHome: Image enabled: {characterImage.enabled}");
                Debug.Log($"PanelHome: Image sprite: {characterImage.sprite != null}");
            }
            else
            {
                Debug.LogError($"PanelHome: ✗ FAILED - Could not set sprite for gender '{gender}'");
            }
            
            Debug.Log("=== END PANELHOME DEBUG ===");
        }

        // Không cần Update() nữa vì không có 3D model để xoay
        // Action buttons sẽ được xử lý trong panel con khác

        public void SetCharacterImage(Sprite sprite)
        {
            if (characterImage != null)
            {
                characterImage.sprite = sprite;
            }
        }

        // Loại bỏ dice animation để đơn giản hóa

        private void OnDestroy()
        {
            // Không cần clean up event listeners vì không có action buttons ở đây
        }
    }
}
