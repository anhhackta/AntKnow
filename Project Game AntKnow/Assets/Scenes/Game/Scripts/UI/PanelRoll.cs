using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace AntKnow.Game
{
    /// <summary>
    /// Panel hiển thị xúc xắc và nút roll
    /// </summary>
    public class PanelRoll : MonoBehaviour
    {
        [Header("Dice Components")]
        [SerializeField] private Image dice1Image;
        [SerializeField] private Image dice2Image;
        [SerializeField] private Sprite[] diceSprites; // 6 sprites (1-6)
        
        [Header("Result Display")]
        [SerializeField] private TextMeshProUGUI textResult;
        
        [Header("Roll Button")]
        [SerializeField] private Button btnRoll;
        
        [Header("Animation Settings")]
        [SerializeField] private float rollDuration = 1.5f;
        [SerializeField] private float frameInterval = 0.1f;
        
        private int lastDice1 = 1;
        private int lastDice2 = 1;
        private bool isRolling = false;
        
        public bool IsRolling => isRolling;
        
        private void Awake()
        {
            // Initialize display
            SetDiceDisplay(1, 1);
            UpdateResultDisplay(2, false);
            SetRollButtonEnabled(false);
        }
        
        /// <summary>
        /// Roll dice animation
        /// </summary>
        public IEnumerator RollDice(int dice1, int dice2, bool isDouble, bool wasLuckyDouble = false)
        {
            if (isRolling) yield break;
            
            isRolling = true;
            SetRollButtonEnabled(false);
            
            // Store final values
            lastDice1 = dice1;
            lastDice2 = dice2;
            
            float elapsed = 0f;
            
            // Roll animation
            while (elapsed < rollDuration)
            {
                // Random dice during animation
                int animDice1 = Random.Range(1, 7);
                int animDice2 = Random.Range(1, 7);
                
                SetDiceDisplay(animDice1, animDice2);
                
                elapsed += frameInterval;
                yield return new WaitForSeconds(frameInterval);
            }
            
            // Set final result
            SetDiceDisplay(dice1, dice2);
            UpdateResultDisplay(dice1 + dice2, isDouble, wasLuckyDouble);
            
            isRolling = false;
        }
        
        /// <summary>
        /// Set dice display
        /// </summary>
        private void SetDiceDisplay(int dice1, int dice2)
        {
            if (dice1Image != null && diceSprites != null && dice1 >= 1 && dice1 <= 6)
            {
                dice1Image.sprite = diceSprites[dice1 - 1];
            }
            
            if (dice2Image != null && diceSprites != null && dice2 >= 1 && dice2 <= 6)
            {
                dice2Image.sprite = diceSprites[dice2 - 1];
            }
        }
        
        /// <summary>
        /// Update result display
        /// </summary>
        private void UpdateResultDisplay(int total, bool isDouble, bool wasLuckyDouble = false)
        {
            if (textResult != null)
            {
                if (wasLuckyDouble)
                {
                    textResult.text = $"{total} ⭐ LUCK! ⭐";
                }
                else if (isDouble)
                {
                    textResult.text = $"{total} (Đôi)";
                }
                else
                {
                    textResult.text = $"{total}";
                }
            }
        }
        
        /// <summary>
        /// Set roll button enabled state
        /// </summary>
        public void SetRollButtonEnabled(bool enabled)
        {
            if (btnRoll != null)
            {
                btnRoll.interactable = enabled && !isRolling;
                
                // Visual feedback - make button dimmer when disabled
                var colors = btnRoll.colors;
                if (!enabled || isRolling)
                {
                    colors.normalColor = new Color(0.7f, 0.7f, 0.7f, 1f);
                    colors.highlightedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
                }
                else
                {
                    colors.normalColor = Color.white;
                    colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
                }
                btnRoll.colors = colors;
            }
        }
        
        /// <summary>
        /// Set roll button click handler
        /// </summary>
        public void SetRollButtonHandler(System.Action onClick)
        {
            if (btnRoll != null)
            {
                btnRoll.onClick.RemoveAllListeners();
                btnRoll.onClick.AddListener(() => onClick?.Invoke());
            }
        }
        
        /// <summary>
        /// Show/hide panel
        /// </summary>
        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
        
        /// <summary>
        /// Reset to initial state
        /// </summary>
        public void Reset()
        {
            StopAllCoroutines();
            isRolling = false;
            SetDiceDisplay(1, 1);
            UpdateResultDisplay(2, false);
            SetRollButtonEnabled(false);
        }
        
        /// <summary>
        /// Get last dice values
        /// </summary>
        public (int dice1, int dice2, int total) GetLastRoll()
        {
            return (lastDice1, lastDice2, lastDice1 + lastDice2);
        }
    }
}
