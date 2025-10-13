using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AntKnow.Auth;
using Unity.Services.Vivox;
using Unity.Services.Core;

namespace AntKnow.Chat
{
    /// <summary>
    /// Simple Chat Manager - Singleton chat manager using Vivox
    /// Persists across scenes and panel switches
    /// </summary>
    public class SimpleChatManager : MonoBehaviour
    {
        // Singleton instance
        public static SimpleChatManager Instance { get; private set; }

        [Header("Simple UI")]
        [SerializeField] private InputField chatInput;
        [SerializeField] private TextMeshProUGUI chatDisplay;
        [SerializeField] private ScrollRect chatScrollRect;
        [SerializeField] private Button sendButton;
        [SerializeField] private Button toggleButton;
        [SerializeField] private GameObject chatPanel;

        [Header("Settings")]
        [SerializeField] private bool autoConnect = false; // Don't auto-connect, join when user opens chat
        [SerializeField] private bool showPanelByDefault = false; // Hide chat panel by default
        [SerializeField] private bool useMockChat = false; // Use real Vivox chat

        [Header("Vivox Settings")]
        [SerializeField] private string server = "https://unity.vivox.com/appconfig/18968-proje-59535-udash";
        [SerializeField] private string domain = "mtu1xp.vivox.com";
        [SerializeField] private string issuer = "18968-proje-59535-udash";
        [SerializeField] private string key = "9diWIL6eBlHhlQCQzlu5dRJDIIwyQb2x";

        // State
        private bool isConnected = false;
        private bool isPanelVisible = false;
        private List<string> messages = new List<string>();
        private string currentUserId;
        private string currentDisplayName;
        private string currentChannelName = "GlobalChat"; // Current channel name (can be changed for game rooms)
        
        private void Awake()
        {
            // Singleton pattern with DontDestroyOnLoad
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log("[SimpleChat] Singleton instance created and persisted");
            }
            else
            {
                Debug.LogWarning("[SimpleChat] Duplicate instance detected, destroying...");
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            SetupUI();
            SetupVivoxEvents();

            // Hide panel by default
            if (chatPanel != null)
            {
                chatPanel.SetActive(showPanelByDefault);
                isPanelVisible = showPanelByDefault;
            }

            // Don't auto-connect, wait for user to open chat
            // if (autoConnect)
            // {
            //     ConnectToChat();
            // }
        }
        
        /// <summary>
        /// Setup UI components
        /// </summary>
        private void SetupUI()
        {
            // Setup send button
            if (sendButton != null)
            {
                sendButton.onClick.AddListener(SendMessage);
            }
            
            // Setup toggle button
            if (toggleButton != null)
            {
                toggleButton.onClick.AddListener(ToggleChat);
            }
            
            // Setup input field
            if (chatInput != null)
            {
                chatInput.onEndEdit.AddListener(OnInputEndEdit);
                chatInput.placeholder.GetComponent<TextMeshProUGUI>().text = "Nhập tin nhắn...";
            }

            // Update toggle button text
            UpdateToggleButton();
        }
        
        /// <summary>
        /// Setup Vivox event listeners
        /// </summary>
        private void SetupVivoxEvents()
        {
            // Subscribe to Vivox events
            VivoxService.Instance.LoggedIn += OnVivoxLoggedIn;
            VivoxService.Instance.LoggedOut += OnVivoxLoggedOut;
            VivoxService.Instance.ChannelJoined += OnVivoxChannelJoined;
            VivoxService.Instance.ChannelMessageReceived += OnVivoxMessageReceived;
        }
        
        /// <summary>
        /// Connect to chat (mock or real)
        /// </summary>
        public async Task ConnectToChat()
        {
            try
            {
                Debug.Log("[SimpleChat] Connecting to chat...");
                
                // Get user data from GameDataManager
                var gameDataManager = FindObjectOfType<GameDataManager>();
                if (gameDataManager != null)
                {
                    currentUserId = gameDataManager.currentUserId;
                    currentDisplayName = gameDataManager.currentIngameName ?? gameDataManager.currentUsername;
                }
                else
                {
                    // Fallback to test data
                    currentUserId = "test_user_" + UnityEngine.Random.Range(1000, 9999);
                    currentDisplayName = "TestUser";
                }
                
                if (useMockChat)
                {
                    await ConnectToMockChat();
                }
                else
                {
                    await ConnectToRealChat();
                }
                
                isConnected = true;
                AddSystemMessage("Đã kết nối chat thành công!");
                
                Debug.Log("[SimpleChat] Connected successfully!");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SimpleChat] Connection failed: {e.Message}");
                AddSystemMessage($"Lỗi kết nối: {e.Message}");
            }
        }
        
        /// <summary>
        /// Connect to mock chat (works without Vivox package)
        /// </summary>
        private async Task ConnectToMockChat()
        {
            Debug.Log("[SimpleChat] Using Mock Chat (no Vivox package needed)");

            // Simulate connection delay
            await Task.Delay(1000);

            // No welcome messages, just connect silently
            // User will see messages when others chat

            Debug.Log("[SimpleChat] Mock chat connected");
        }
        
        /// <summary>
        /// Connect to real Vivox chat (requires Vivox package)
        /// </summary>
        private async Task ConnectToRealChat()
        {
            Debug.Log("[SimpleChat] Attempting to connect to Vivox...");
            
            try
            {
                // Check if Vivox package is available
                var vivoxType = System.Type.GetType("Unity.Services.Vivox.VivoxService, Unity.Services.Vivox");
                if (vivoxType == null)
                {
                    throw new Exception("Vivox package not found. Please import Vivox Unity package.");
                }
                
                // Initialize Vivox if available
                await InitializeVivox();
                
                // Login to Vivox
                await LoginToVivox();
                
                // Join channel
                await JoinChannel();
                
                Debug.Log("[SimpleChat] Vivox connected successfully");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SimpleChat] Vivox connection failed: {e.Message}. Falling back to mock chat.");
                useMockChat = true;
                await ConnectToMockChat();
            }
        }
        
        /// <summary>
        /// Initialize Vivox Service
        /// </summary>
        private async Task InitializeVivox()
        {
            try
            {
                Debug.Log("[SimpleChat] Initializing Vivox Service...");
                
                // Initialize Unity Services with Vivox credentials
                var options = new InitializationOptions();
                options.SetVivoxCredentials(server, domain, issuer, key);
                await UnityServices.InitializeAsync(options);
                
                // Initialize Vivox Service
                await VivoxService.Instance.InitializeAsync();
                
                Debug.Log("[SimpleChat] Vivox Service initialized successfully");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SimpleChat] Vivox initialization failed: {e.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Login to Vivox
        /// </summary>
        private async Task LoginToVivox()
        {
            try
            {
                Debug.Log($"[SimpleChat] Logging in to Vivox as {currentDisplayName}...");
                
                var loginOptions = new LoginOptions { DisplayName = currentDisplayName };
                await VivoxService.Instance.LoginAsync(loginOptions);
                
                Debug.Log("[SimpleChat] Successfully logged in to Vivox");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SimpleChat] Vivox login failed: {e.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Join channel
        /// </summary>
        private async Task JoinChannel()
        {
            try
            {
                Debug.Log($"[SimpleChat] Joining channel: {currentChannelName}...");

                await VivoxService.Instance.JoinGroupChannelAsync(currentChannelName, ChatCapability.TextOnly);

                Debug.Log($"[SimpleChat] Successfully joined channel: {currentChannelName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SimpleChat] Vivox join channel failed: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Join a specific channel (for game rooms)
        /// Example: JoinChannelByName("GameRoom_12345")
        /// </summary>
        public async void JoinChannelByName(string channelName)
        {
            if (string.IsNullOrEmpty(channelName))
            {
                Debug.LogError("[SimpleChat] Channel name is null or empty");
                return;
            }

            try
            {
                // Leave current channel if connected
                if (isConnected)
                {
                    await LeaveChannel();
                }

                // Update current channel name
                currentChannelName = channelName;
                Debug.Log($"[SimpleChat] Switching to channel: {currentChannelName}");

                // Connect and join new channel
                await ConnectToChat();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SimpleChat] Failed to join channel {channelName}: {e.Message}");
            }
        }

        /// <summary>
        /// Leave current channel
        /// </summary>
        private async Task LeaveChannel()
        {
            try
            {
                if (VivoxService.Instance != null)
                {
                    Debug.Log($"[SimpleChat] Leaving channel: {currentChannelName}...");
                    await VivoxService.Instance.LeaveChannelAsync(currentChannelName);
                    Debug.Log($"[SimpleChat] Left channel: {currentChannelName}");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SimpleChat] Failed to leave channel: {e.Message}");
            }
        }
        
        /// <summary>
        /// Send message
        /// </summary>
        public async void SendMessage()
        {
            if (chatInput == null)
            {
                Debug.LogWarning("[SimpleChat] Input field is null - setup UI references");
                return;
            }
            
            string message = chatInput.text.Trim();
            if (string.IsNullOrEmpty(message))
            {
                return;
            }
            
            // Clear input first
            chatInput.text = "";
            
            try
            {
                if (!isConnected)
                {
                    // Try to connect first
                    ConnectToChat(); // Don't await void method
                    await Task.Delay(3000); // Wait for connection
                }
                
                if (isConnected && !useMockChat)
                {
                    await SendRealMessage(message);
                }
                else
                {
                    // Fallback to mock for testing
                    await SendMockMessage(message);
                }
                
                // Add message to display
                AddMessage(currentDisplayName ?? "You", message);
                
                Debug.Log($"[SimpleChat] Sent message: {message}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SimpleChat] Send message failed: {e.Message}");
                AddSystemMessage($"Lỗi gửi tin nhắn: {e.Message}");
            }
        }
        
        /// <summary>
        /// Send mock message (simulates network delay)
        /// </summary>
        private async Task SendMockMessage(string message)
        {
            await Task.Delay(100); // Simulate network delay
            
            // Simulate receiving message back (echo)
            await Task.Delay(500);
            AddMessage("Echo", $"Echo: {message}");
        }
        
        /// <summary>
        /// Send real message via Vivox
        /// </summary>
        private async Task SendRealMessage(string message)
        {
            try
            {
                Debug.Log($"[SimpleChat] Sending message to {currentChannelName}: {message}");

                await VivoxService.Instance.SendChannelTextMessageAsync(currentChannelName, message);

                Debug.Log("[SimpleChat] Message sent successfully");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SimpleChat] Failed to send message: {e.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Handle input field end edit (Enter key)
        /// </summary>
        private void OnInputEndEdit(string text)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                SendMessage();
            }
        }
        
        /// <summary>
        /// Add message to display (simple format: time + sender + message)
        /// </summary>
        private void AddMessage(string sender, string message)
        {
            // Simple format: [HH:mm] Sender: Message
            // No "System" prefix, just natural chat
            string formattedMessage = $"[{DateTime.Now:HH:mm}] {sender}: {message}";
            messages.Add(formattedMessage);

            UpdateChatDisplay();

            // Auto-scroll to bottom
            if (chatScrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                chatScrollRect.verticalNormalizedPosition = 0f;
            }
        }
        
        /// <summary>
        /// Add system message
        /// </summary>
        private void AddSystemMessage(string message)
        {
            string formattedMessage = $"[SYSTEM] {message}";
            messages.Add(formattedMessage);
            
            UpdateChatDisplay();
        }
        
        /// <summary>
        /// Update chat display text
        /// </summary>
        private void UpdateChatDisplay()
        {
            if (chatDisplay == null) return;
            
            // Keep only last 20 messages
            if (messages.Count > 20)
            {
                messages.RemoveAt(0);
            }
            
            // Join all messages
            chatDisplay.text = string.Join("\n", messages);
        }
        
        /// <summary>
        /// Toggle chat panel visibility
        /// </summary>
        public void ToggleChat()
        {
            isPanelVisible = !isPanelVisible;

            if (chatPanel != null)
            {
                chatPanel.SetActive(isPanelVisible);
            }

            // Join chat when user opens panel for the first time
            if (isPanelVisible && !isConnected)
            {
                Debug.Log("[SimpleChat] User opened chat, connecting and joining global chat...");
                ConnectToChat();
            }

            UpdateToggleButton();

            Debug.Log($"[SimpleChat] Chat panel {(isPanelVisible ? "opened" : "closed")}");
        }
        
        /// <summary>
        /// Update toggle button text
        /// </summary>
        private void UpdateToggleButton()
        {
            if (toggleButton != null)
            {
                var buttonText = toggleButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = isPanelVisible ? "Ẩn Chat" : "Chat";
                }
            }
        }
        
        /// <summary>
        /// Disconnect from chat
        /// </summary>
        public async void DisconnectFromChat()
        {
            try
            {
                if (isConnected && !useMockChat)
                {
                    Debug.Log("[SimpleChat] Disconnecting from Vivox...");
                    
                    // Leave all channels
                    await VivoxService.Instance.LeaveAllChannelsAsync();
                    
                    // Logout from Vivox
                    await VivoxService.Instance.LogoutAsync();
                }
                
                isConnected = false;
                AddSystemMessage("Đã ngắt kết nối chat");
                
                Debug.Log("[SimpleChat] Disconnected from chat");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SimpleChat] Disconnect failed: {e.Message}");
            }
        }
        
        /// <summary>
        /// Vivox event handlers
        /// </summary>
        private void OnVivoxLoggedIn()
        {
            Debug.Log("[SimpleChat] Vivox logged in successfully");
            AddSystemMessage("Đã kết nối Vivox thành công!");
        }
        
        private void OnVivoxLoggedOut()
        {
            Debug.Log("[SimpleChat] Vivox logged out");
            AddSystemMessage("Đã ngắt kết nối Vivox");
        }
        
        private void OnVivoxChannelJoined(string channelName)
        {
            Debug.Log($"[SimpleChat] Joined channel: {channelName}");
            AddSystemMessage($"Đã tham gia kênh: {channelName}");
        }
        
        private void OnVivoxMessageReceived(VivoxMessage message)
        {
            Debug.Log($"[SimpleChat] Received message from {message.SenderDisplayName}: {message.MessageText}");
            AddMessage(message.SenderDisplayName, message.MessageText);
        }
        
        private void OnDestroy()
        {
            // Only cleanup if this is the singleton instance being destroyed
            if (Instance == this)
            {
                Debug.Log("[SimpleChat] Singleton instance being destroyed, cleaning up...");

                // Unsubscribe from Vivox events
                if (VivoxService.Instance != null)
                {
                    VivoxService.Instance.LoggedIn -= OnVivoxLoggedIn;
                    VivoxService.Instance.LoggedOut -= OnVivoxLoggedOut;
                    VivoxService.Instance.ChannelJoined -= OnVivoxChannelJoined;
                    VivoxService.Instance.ChannelMessageReceived -= OnVivoxMessageReceived;
                }

                // Disconnect from chat when singleton is destroyed (app quit or scene unload)
                DisconnectFromChat();
            }
        }

        /// <summary>
        /// Manually disconnect from chat (call when user logs out)
        /// </summary>
        public void ManualDisconnect()
        {
            Debug.Log("[SimpleChat] Manual disconnect requested");
            DisconnectFromChat();
            isConnected = false;
            messages.Clear();
            UpdateChatDisplay();
        }
    }
}