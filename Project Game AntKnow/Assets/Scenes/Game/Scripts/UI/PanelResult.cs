using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Firebase.Functions;
using Firebase.Extensions;
using AntKnow.Auth;

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
        /// Show result with Cloud Function rewards
        /// </summary>
        public void Show(List<PlayerResult> results, float gameDuration)
        {
            // Sort by total value (money + property value)
            results.Sort((a, b) => (b.money + b.propertyValue).CompareTo(a.money + a.propertyValue));
            
            // Show top players (2-4 players)
            int playerCount = results.Count;
            
            if (results.Count > 0) ShowPlayerResult(1, results[0], textTop1Name, textTop1Money, textTop1Reward);
            if (results.Count > 1) ShowPlayerResult(2, results[1], textTop2Name, textTop2Money, textTop2Reward);
            if (results.Count > 2) ShowPlayerResult(3, results[2], textTop3Name, textTop3Money, textTop3Reward);
            if (results.Count > 3) ShowPlayerResult(4, results[3], textTop4Name, textTop4Money, textTop4Reward);
            
            // Award rewards via Cloud Function
            StartCoroutine(AwardRewardsCoroutine(results, gameDuration));
            
            gameObject.SetActive(true);
        }
        
        /// <summary>
        /// Show player result
        /// </summary>
        private void ShowPlayerResult(int rank, PlayerResult result, TextMeshProUGUI nameText, TextMeshProUGUI moneyText, TextMeshProUGUI rewardText)
        {
            if (result == null) return;
            
            if (nameText != null)
            {
                nameText.text = result.playerName;
            }
            
            if (moneyText != null)
            {
                int totalValue = result.money + result.propertyValue;
                moneyText.text = $"{totalValue}";
            }
            
            if (rewardText != null)
            {
                int antCoin = rank <= antCoinRewards.Length ? antCoinRewards[rank - 1] : 0;
                int exp = rank <= expRewards.Length ? expRewards[rank - 1] : 0;
                rewardText.text = $"+{antCoin} AntCoin, +{exp} EXP";
            }
        }
        
        /// <summary>
        /// Award rewards via Cloud Function
        /// </summary>
        private System.Collections.IEnumerator AwardRewardsCoroutine(List<PlayerResult> results, float gameDuration)
        {
            // Award rewards for each player
            for (int i = 0; i < results.Count; i++)
            {
                var result = results[i];
                int rank = i + 1;
                
                // Call Cloud Function
                yield return StartCoroutine(CallAwardMatchFunction(result.playerId, rank, gameDuration));
            }
        }
        
        /// <summary>
        /// Call awardMatch Cloud Function
        /// </summary>
        private System.Collections.IEnumerator CallAwardMatchFunction(string playerId, int rank, float gameDuration)
        {
            bool completed = false;
            bool success = false;
            
            var functions = FirebaseFunctions.DefaultInstance;
            var callable = functions.GetHttpsCallable("awardMatch");
            
            var data = new Dictionary<string, object>
            {
                { "rank", rank },
                { "durationSec", Mathf.RoundToInt(gameDuration) }
            };
            
            callable.CallAsync(data).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError($"[PanelResult] Award failed for player {playerId}: {task.Exception}");
                }
                else if (task.IsCompleted)
                {
                    var result = task.Result.Data as Dictionary<string, object>;
                    if (result != null && result.ContainsKey("antCoin") && result.ContainsKey("xp"))
                    {
                        int antCoin = System.Convert.ToInt32(result["antCoin"]);
                        int xp = System.Convert.ToInt32(result["xp"]);
                        Debug.Log($"[PanelResult] Awarded {antCoin} AntCoin and {xp} XP to player {playerId}");
                        success = true;
                    }
                }
                completed = true;
            });
            
            // Wait for completion
            while (!completed)
            {
                yield return null;
            }
            
            if (!success)
            {
                Debug.LogWarning($"[PanelResult] Failed to award rewards to player {playerId}");
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

