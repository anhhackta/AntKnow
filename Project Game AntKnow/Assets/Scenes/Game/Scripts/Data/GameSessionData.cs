using System.Collections.Generic;
using UnityEngine;
using AntKnow.Auth;

namespace AntKnow.Game
{
    /// <summary>
    /// Singleton để lưu trữ dữ liệu game session
    /// Truyền dữ liệu từ MenuScene sang GameScene
    /// </summary>
    public class GameSessionData : MonoBehaviour
    {
        private static GameSessionData _instance;
        public static GameSessionData Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameSessionData");
                    _instance = go.AddComponent<GameSessionData>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("Network Info")]
        public string relayJoinCode;
        public bool isHost;
        public string lobbyId;

        [Header("Player Info")]
        public string firebaseUID;
        public string unityPlayerId;
        public string playerName;
        public int level;
        public string gender;

        [Header("Currency")]
        public int antCoin;
        public int dCoin;

        [Header("Loadout - Skill Cards")]
        public List<string> skillCardIds = new List<string>();
        public List<SkillCardData> skillCards = new List<SkillCardData>();

        [Header("Loadout - Equipment")]
        public Dictionary<string, string> equipmentIds = new Dictionary<string, string>();
        public EquipmentSetData equipmentSet = new EquipmentSetData();

        [Header("Calculated Stats")]
        public int totalHealth;
        public int totalAgility;
        public int totalIntelligence;
        public int totalLuck;
        public int totalResistance;

        [Header("Debug")]
        public bool enableDebugLogs = true;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Load dữ liệu từ GameDataManager (Firebase)
        /// </summary>
        public void SetFromGameDataManager()
        {
            var gdm = GameDataManager.Instance;
            if (gdm == null)
            {
                DebugLogError("GameDataManager not found!");
                return;
            }

            // Basic info
            firebaseUID = gdm.currentUserId;
            playerName = gdm.currentIngameName ?? gdm.currentUsername;
            level = gdm.currentLevel;
            gender = gdm.currentGender;
            antCoin = gdm.currentAntCoin;
            dCoin = gdm.currentDCoin;

            DebugLog($"Loaded player data: {playerName} (Level {level})");
            DebugLog($"Firebase UID: {firebaseUID}");
            DebugLog($"Currency: AntCoin={antCoin}, DCoin={dCoin}");

            // ⚠️ CRITICAL: Load loadout data from GameDataManager
            // For now, use placeholder data
            // TODO: Implement Firebase loadout loading
            
            // ⚠️ Calculate total stats from loadout
            // Hiện tại chưa có loadout system hoàn chỉnh, dùng stats mặc định
            totalHealth = 100;
            totalAgility = 50;
            totalIntelligence = 50;
            totalLuck = 50;
            totalResistance = 50;
            
            DebugLog($"Stats set to default: HP={totalHealth} AGI={totalAgility} INT={totalIntelligence} LUCK={totalLuck} RES={totalResistance}");
            DebugLog("⚠️ TODO: Implement real loadout loading from Firebase!");
        }

        /// <summary>
        /// Set network info khi vào game
        /// </summary>
        public void SetNetworkInfo(string relayCode, bool host, string lobby = null)
        {
            relayJoinCode = relayCode;
            isHost = host;
            lobbyId = lobby;

            DebugLog($"Network info set - Relay: {relayCode}, IsHost: {host}");
        }

        /// <summary>
        /// Set Unity Player ID sau khi đăng nhập UGS
        /// </summary>
        public void SetUnityPlayerId(string playerId)
        {
            unityPlayerId = playerId;
            DebugLog($"Unity Player ID set: {playerId}");
        }

        /// <summary>
        /// Load loadout từ Firebase (implement sau)
        /// </summary>
        public async void LoadLoadoutFromFirebase()
        {
            // TODO: Implement
            // 1. Get loadout từ Firebase: users/{uid}/loadouts/slot1
            // 2. Load skill cards data
            // 3. Load equipment data
            // 4. Calculate total stats

            DebugLog("LoadLoadoutFromFirebase - Not implemented yet");
        }

        /// <summary>
        /// Calculate total stats từ cards và equipment
        /// </summary>
        public void CalculateTotalStats()
        {
            totalHealth = 0;
            totalAgility = 0;
            totalIntelligence = 0;
            totalLuck = 0;
            totalResistance = 0;

            // Add stats from skill cards
            foreach (var card in skillCards)
            {
                totalHealth += card.health;
                totalAgility += card.agility;
                totalIntelligence += card.intelligence;
                totalLuck += card.luck;
                totalResistance += card.resistance;
            }

            // Add stats from equipment
            totalHealth += equipmentSet.totalHealth;
            totalAgility += equipmentSet.totalAgility;
            totalIntelligence += equipmentSet.totalIntelligence;
            totalLuck += equipmentSet.totalLuck;
            totalResistance += equipmentSet.totalResistance;

            DebugLog($"Total Stats - HP:{totalHealth} AGI:{totalAgility} INT:{totalIntelligence} LUCK:{totalLuck} RES:{totalResistance}");
        }

        /// <summary>
        /// Clear session data
        /// </summary>
        public void Clear()
        {
            relayJoinCode = null;
            isHost = false;
            lobbyId = null;
            skillCardIds.Clear();
            skillCards.Clear();
            equipmentIds.Clear();

            DebugLog("Session data cleared");
        }

        /// <summary>
        /// Get summary string for debugging
        /// </summary>
        public string GetSummary()
        {
            return $"Player: {playerName} (Lv.{level})\n" +
                   $"Firebase UID: {firebaseUID}\n" +
                   $"Unity Player ID: {unityPlayerId}\n" +
                   $"Is Host: {isHost}\n" +
                   $"Relay Code: {relayJoinCode}\n" +
                   $"Stats: HP={totalHealth} AGI={totalAgility} INT={totalIntelligence} LUCK={totalLuck} RES={totalResistance}";
        }

        #region Debug Helpers

        private void DebugLog(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[GameSessionData] {message}");
            }
        }

        private void DebugLogError(string message)
        {
            if (enableDebugLogs)
            {
                Debug.LogError($"[GameSessionData] {message}");
            }
        }

        #endregion
    }

    /// <summary>
    /// Skill card data structure
    /// </summary>
    [System.Serializable]
    public class SkillCardData
    {
        public string cardId;
        public string itemId;
        public int level;
        public int stars;
        
        // Base attributes
        public int health;
        public int agility;
        public int intelligence;
        public int luck;
        public int resistance;
        
        // Skill info
        public string skillMode; // passive/active
        public string primaryStat;
        public int cooldownBaseTurns;
        public string triggerId;
        public string effectId;
    }

    /// <summary>
    /// Equipment set data structure
    /// </summary>
    [System.Serializable]
    public class EquipmentSetData
    {
        public string hatId;
        public string shirtId;
        public string wingsId;
        public string shoesId;
        public string maskId;
        
        // Total stats from all equipment
        public int totalHealth;
        public int totalAgility;
        public int totalIntelligence;
        public int totalLuck;
        public int totalResistance;
    }
}
