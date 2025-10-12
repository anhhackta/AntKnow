using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using AntKnow.Auth;

namespace AntKnow.Game
{
    /// <summary>
    /// Panel chính quản lý UI game - PanelMe và PanelPlayer
    /// PanelMe: Hiển thị thông tin người chơi chính
    /// PanelPlayer: Container với VerticalLayoutGroup chứa các PanelPlayerPrefab
    /// </summary>
    public class PanelGame : MonoBehaviour
    {
        [Header("Panel Components")]
        [SerializeField] private PanelPlayerMe panelMe;
        [SerializeField] private Transform panelPlayerContainer; // Parent cho PanelPlayerPrefab
        [SerializeField] private GameObject panelPlayerPrefab; // Prefab cho mỗi player khác
        
        [Header("Settings")]
        [SerializeField] private int maxPlayers = 4;
        
        private List<PanelPlayer> panelPlayers = new List<PanelPlayer>();
        private PlayerGameController localPlayer;
        
        private void Awake()
        {
            // Ensure container has VerticalLayoutGroup
            if (panelPlayerContainer != null)
            {
                var layoutGroup = panelPlayerContainer.GetComponent<VerticalLayoutGroup>();
                if (layoutGroup == null)
                {
                    layoutGroup = panelPlayerContainer.gameObject.AddComponent<VerticalLayoutGroup>();
                    layoutGroup.spacing = 10f;
                    layoutGroup.childControlHeight = false;
                    layoutGroup.childControlWidth = true;
                    layoutGroup.childForceExpandHeight = false;
                    layoutGroup.childForceExpandWidth = true;
                }
            }
        }
        
        /// <summary>
        /// Initialize với local player
        /// </summary>
        public void Initialize(PlayerGameController localPlayerController)
        {
            localPlayer = localPlayerController;
            
            // Initialize PanelMe
            if (panelMe != null)
            {
                panelMe.Initialize(localPlayer);
                
                // Add click handler để mở PanelInfo
                var button = panelMe.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.AddListener(OnPanelMeClicked);
                }
                else
                {
                    // Add Button component if not exists
                    button = panelMe.gameObject.AddComponent<Button>();
                    button.onClick.AddListener(OnPanelMeClicked);
                }
            }
        }
        
        /// <summary>
        /// Add player panel (for other players)
        /// </summary>
        public void AddPlayerPanel(PlayerGameController player)
        {
            if (panelPlayerPrefab == null || panelPlayerContainer == null)
            {
                Debug.LogError("[PanelGame] PanelPlayerPrefab or Container not assigned!");
                return;
            }
            
            // Create new panel
            GameObject panelObj = Instantiate(panelPlayerPrefab, panelPlayerContainer);
            PanelPlayer panelPlayer = panelObj.GetComponent<PanelPlayer>();
            
            if (panelPlayer != null)
            {
                panelPlayer.Initialize(player);
                panelPlayers.Add(panelPlayer);
                
                // Add click handler để mở PanelInfo
                var button = panelObj.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.AddListener(() => OnPanelPlayerClicked(player));
                }
                else
                {
                    button = panelObj.AddComponent<Button>();
                    button.onClick.AddListener(() => OnPanelPlayerClicked(player));
                }
                
                Debug.Log($"[PanelGame] Added player panel for {player.PlayerName}");
            }
        }
        
        /// <summary>
        /// Remove player panel
        /// </summary>
        public void RemovePlayerPanel(PlayerGameController player)
        {
            for (int i = panelPlayers.Count - 1; i >= 0; i--)
            {
                if (panelPlayers[i] != null && panelPlayers[i].GetPlayer() == player)
                {
                    Destroy(panelPlayers[i].gameObject);
                    panelPlayers.RemoveAt(i);
                    Debug.Log($"[PanelGame] Removed player panel for {player.PlayerName}");
                    break;
                }
            }
        }
        
        /// <summary>
        /// Update all player panels
        /// </summary>
        public void UpdateAllPanels()
        {
            // Update PanelMe
            if (panelMe != null)
            {
                panelMe.UpdateDisplayPublic();
            }
            
            // Update all PanelPlayers
            foreach (var panel in panelPlayers)
            {
                if (panel != null)
                {
                    panel.UpdateDisplayPublic();
                }
            }
        }
        
        /// <summary>
        /// PanelMe clicked - show PanelInfo
        /// PUBLIC để có thể assign trong Inspector nếu cần
        /// </summary>
        public void OnPanelMeClicked()
        {
            if (localPlayer != null)
            {
                ShowPlayerInfo(localPlayer);
            }
        }

        /// <summary>
        /// PanelPlayer clicked - show PanelInfo
        /// PUBLIC để có thể assign trong Inspector nếu cần
        /// </summary>
        public void OnPanelPlayerClicked(PlayerGameController player)
        {
            ShowPlayerInfo(player);
        }

        /// <summary>
        /// Show PanelInfo for player
        /// </summary>
        public void ShowPlayerInfo(PlayerGameController player)
        {
            // Find PanelInfo in scene
            var panelInfo = FindObjectOfType<PanelInfo>();
            if (panelInfo != null)
            {
                panelInfo.Show(player);
            }
            else
            {
                Debug.LogWarning("[PanelGame] PanelInfo not found in scene!");
            }
        }
        
        /// <summary>
        /// Clear all player panels
        /// </summary>
        public void ClearAllPlayerPanels()
        {
            foreach (var panel in panelPlayers)
            {
                if (panel != null)
                {
                    Destroy(panel.gameObject);
                }
            }
            panelPlayers.Clear();
        }
        
        /// <summary>
        /// Get PanelMe reference
        /// </summary>
        public PanelPlayerMe GetPanelMe()
        {
            return panelMe;
        }
        
        /// <summary>
        /// Get all PanelPlayer references
        /// </summary>
        public List<PanelPlayer> GetPanelPlayers()
        {
            return panelPlayers;
        }
    }
}
