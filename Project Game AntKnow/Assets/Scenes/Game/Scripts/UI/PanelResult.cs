using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace AntKnow.Game
{
    /// <summary>
    /// Panel kết quả cuối game
    /// </summary>
    public class PanelResult : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI textTop1Name;
        [SerializeField] private TextMeshProUGUI textTop1Money;
        [SerializeField] private TextMeshProUGUI textTop1Reward; // AntCoin + EXP
        
        [SerializeField] private TextMeshProUGUI textTop2Name;
        [SerializeField] private TextMeshProUGUI textTop2Money;
        [SerializeField] private TextMeshProUGUI textTop2Reward;
        
        [SerializeField] private TextMeshProUGUI textTop3Name;
        [SerializeField] private TextMeshProUGUI textTop3Money;
        [SerializeField] private TextMeshProUGUI textTop3Reward;
        
        [SerializeField] private TextMeshProUGUI textTop4Name;
        [SerializeField] private TextMeshProUGUI textTop4Money;
        [SerializeField] private TextMeshProUGUI textTop4Reward;
        
        [SerializeField] private Button btnOK;
        
        [Header("Rewards")]
        [SerializeField] private int[] antCoinRewards = new int[] { 100, 50, 25, 10 }; // Top 1-4
        [SerializeField] private int[] expRewards = new int[] { 50, 30, 15, 5 }; // Top 1-4
        
        private void Awake()
        {
            if (btnOK != null)
            {
                btnOK.onClick.AddListener(OnOKClicked);
            }
        }
        
        /// <summary>
        /// Show result
        /// </summary>
        public void Show(List<PlayerResult> results)
        {
            // Sort by money (descending)
            results.Sort((a, b) => b.money.CompareTo(a.money));
            
            // Show top 4
            if (results.Count > 0) ShowPlayerResult(0, results[0], textTop1Name, textTop1Money, textTop1Reward);
            if (results.Count > 1) ShowPlayerResult(1, results[1], textTop2Name, textTop2Money, textTop2Reward);
            if (results.Count > 2) ShowPlayerResult(2, results[2], textTop3Name, textTop3Money, textTop3Reward);
            if (results.Count > 3) ShowPlayerResult(3, results[3], textTop4Name, textTop4Money, textTop4Reward);
            
            gameObject.SetActive(true);
        }
        
        /// <summary>
        /// Show player result
        /// </summary>
        private void ShowPlayerResult(int rank, PlayerResult result, TextMeshProUGUI nameText, TextMeshProUGUI moneyText, TextMeshProUGUI rewardText)
        {
            if (nameText != null)
            {
                nameText.text = result.playerName;
            }
            
            if (moneyText != null)
            {
                moneyText.text = $"{result.money}";
            }
            
            if (rewardText != null)
            {
                int antCoin = rank < antCoinRewards.Length ? antCoinRewards[rank] : 0;
                int exp = rank < expRewards.Length ? expRewards[rank] : 0;
                rewardText.text = $"+{antCoin} AntCoin, +{exp} EXP";
            }
        }
        
        /// <summary>
        /// On OK clicked
        /// </summary>
        private void OnOKClicked()
        {
            // TODO: Save rewards to Firebase
            
            // Return to menu scene
            SceneManager.LoadScene("MenuScene");
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
    /// Player result data
    /// </summary>
    [System.Serializable]
    public class PlayerResult
    {
        public string playerId;
        public string playerName;
        public int money;
        public int propertyValue; // Tổng giá trị nhà đất
    }
}

