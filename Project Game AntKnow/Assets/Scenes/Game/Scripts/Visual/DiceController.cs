using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace AntKnow.Game
{
    /// <summary>
    /// Controller cho 2D dice roll
    /// Sử dụng sprites để animate
    /// </summary>
    public class DiceController : MonoBehaviour
    {
        [Header("Dice Sprites")]
        [SerializeField] private Sprite[] diceSprites; // 6 sprites (1-6)
        
        [Header("Dice Images")]
        [SerializeField] private Image dice1Image;
        [SerializeField] private Image dice2Image;
        
        [Header("Animation")]
        [SerializeField] private float rollDuration = 1f;
        [SerializeField] private float frameInterval = 0.1f;
        
        [Header("Result")]
        [SerializeField] private TMPro.TextMeshProUGUI resultText;
        
        private int lastDice1 = 1;
        private int lastDice2 = 1;
        private int lastSum = 2;
        private bool isDouble = false;
        
        public int LastDice1 => lastDice1;
        public int LastDice2 => lastDice2;
        public int LastSum => lastSum;
        public bool IsDouble => isDouble;
        
        private bool isRolling = false;
        
        private void Awake()
        {
            // Validate sprites
            if (diceSprites == null || diceSprites.Length != 6)
            {
                Debug.LogError("[DiceController] Need exactly 6 dice sprites!");
            }
        }
        
        /// <summary>
        /// Roll dice with luck stat
        /// </summary>
        public IEnumerator RollDice(int luckStat = 0)
        {
            if (isRolling)
            {
                Debug.LogWarning("[DiceController] Already rolling!");
                yield break;
            }
            
            isRolling = true;
            
            // Animate rolling
            float elapsed = 0f;
            while (elapsed < rollDuration)
            {
                // Random sprites during animation
                int random1 = Random.Range(0, 6);
                int random2 = Random.Range(0, 6);
                
                dice1Image.sprite = diceSprites[random1];
                dice2Image.sprite = diceSprites[random2];
                
                elapsed += frameInterval;
                yield return new WaitForSeconds(frameInterval);
            }
            
            // Calculate final result
            lastDice1 = Random.Range(1, 7);
            lastDice2 = Random.Range(1, 7);
            
            // Check luck for double
            if (luckStat > 0)
            {
                float doubleChance = luckStat / 100f; // 1 luck = 1% chance
                float roll = Random.value;
                
                if (roll < doubleChance)
                {
                    // Force double!
                    lastDice2 = lastDice1;
                    Debug.Log($"[DiceController] Luck triggered! Double {lastDice1}!");
                }
            }
            
            lastSum = lastDice1 + lastDice2;
            isDouble = (lastDice1 == lastDice2);
            
            // Show final result
            dice1Image.sprite = diceSprites[lastDice1 - 1];
            dice2Image.sprite = diceSprites[lastDice2 - 1];
            
            if (resultText != null)
            {
                string doubleText = isDouble ? " (ĐÔI!)" : "";
                resultText.text = $"{lastSum}{doubleText}";
            }
            
            Debug.Log($"[DiceController] Rolled: {lastDice1} + {lastDice2} = {lastSum} (Double: {isDouble})");
            
            isRolling = false;
        }
        
        /// <summary>
        /// Roll dice without luck (for testing)
        /// </summary>
        public IEnumerator RollDiceSimple()
        {
            yield return RollDice(0);
        }
    }
}

