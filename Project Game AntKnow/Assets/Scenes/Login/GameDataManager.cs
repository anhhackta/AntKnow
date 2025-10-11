using System;
using UnityEngine;

namespace AntKnow.Auth
{
    /// <summary>
    /// Quản lý dữ liệu game và truyền giữa các scene
    /// </summary>
    public class GameDataManager : MonoBehaviour
    {
        private static GameDataManager _instance;
        public static GameDataManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameDataManager");
                    _instance = go.AddComponent<GameDataManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("Current User Data")]
        public string currentUserId;
        public string currentUsername;
        public string currentEmail;
        public string currentIngameName;
        public string currentGender;
        public int currentLevel = 1;
        public int currentXp = 0;
        public int currentAntCoin = 0;
        public int currentDCoin = 0;

        [Header("Game State")]
        public bool isUserLoggedIn = false;
        public bool hasInventory = false;
        public bool hasLoadout = false;

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
        /// Set user data sau khi login thành công
        /// </summary>
        public void SetUserData(string userId, string username, string email, string ingameName = null, string gender = null, int level = 1, int xp = 0, int antCoin = 0, int dCoin = 0)
        {
            currentUserId = userId;
            currentUsername = username;
            currentEmail = email;
            currentIngameName = ingameName;
            currentGender = gender;
            currentLevel = level;
            currentXp = xp;
            currentAntCoin = antCoin;
            currentDCoin = dCoin;
            isUserLoggedIn = true;
            
            Debug.Log($"GameDataManager: User data set - {username} ({userId}) - Level: {level}, AntCoin: {antCoin}, DCoin: {dCoin}");
        }

        /// <summary>
        /// Clear user data khi logout
        /// </summary>
        public void ClearUserData()
        {
            currentUserId = null;
            currentUsername = null;
            currentEmail = null;
            currentIngameName = null;
            currentGender = null;
            currentLevel = 1;
            currentXp = 0;
            currentAntCoin = 0;
            currentDCoin = 0;
            isUserLoggedIn = false;
            hasInventory = false;
            hasLoadout = false;
            
            Debug.Log("GameDataManager: User data cleared");
        }

        /// <summary>
        /// Update ingame name
        /// </summary>
        public void UpdateIngameName(string ingameName)
        {
            currentIngameName = ingameName;
            Debug.Log($"GameDataManager: Ingame name updated to {ingameName}");
        }

        /// <summary>
        /// Update gender
        /// </summary>
        public void UpdateGender(string gender)
        {
            currentGender = gender;
            Debug.Log($"GameDataManager: Gender updated to {gender}");
        }

        /// <summary>
        /// Mark inventory as loaded
        /// </summary>
        public void SetInventoryLoaded(bool loaded)
        {
            hasInventory = loaded;
            Debug.Log($"GameDataManager: Inventory loaded = {loaded}");
        }

        /// <summary>
        /// Mark loadout as loaded
        /// </summary>
        public void SetLoadoutLoaded(bool loaded)
        {
            hasLoadout = loaded;
            Debug.Log($"GameDataManager: Loadout loaded = {loaded}");
        }

        /// <summary>
        /// Check if user needs ingame name setup
        /// </summary>
        public bool NeedsIngameNameSetup()
        {
            return isUserLoggedIn && string.IsNullOrEmpty(currentIngameName);
        }

        /// <summary>
        /// Get current user info for display
        /// </summary>
        public string GetDisplayName()
        {
            if (!string.IsNullOrEmpty(currentIngameName))
                return currentIngameName;
            return currentUsername ?? "Unknown User";
        }
    }
}
