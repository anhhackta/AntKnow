namespace AntKnow
{
    /// <summary>
    /// Global game configuration - Centralized settings
    /// </summary>
    public static class GameConfig
    {
        // ===== MULTIPLAYER SETTINGS =====
        
        /// <summary>
        /// Maximum players per game (cố định cho game cờ tỷ phú)
        /// </summary>
        public const int MAX_PLAYERS = 4;
        
        /// <summary>
        /// Minimum players to start game
        /// </summary>
        public const int MIN_PLAYERS = 2;
        
        // ===== MATCHMAKING SETTINGS =====
        
        /// <summary>
        /// Thời gian timeout khi tìm trận (giây)
        /// </summary>
        public const float MATCHMAKING_TIMEOUT = 60f;
        
        /// <summary>
        /// Interval giữa các lần retry tìm trận (giây)
        /// </summary>
        public const float MATCHMAKING_RETRY_INTERVAL = 5f;
        
        // ===== LOBBY SETTINGS =====
        
        /// <summary>
        /// Interval gửi heartbeat để giữ lobby sống (giây)
        /// </summary>
        public const float LOBBY_HEARTBEAT_INTERVAL = 15f;
        
        /// <summary>
        /// Interval cập nhật thông tin lobby (giây)
        /// </summary>
        public const float LOBBY_UPDATE_INTERVAL = 2f;
        
        /// <summary>
        /// Số lượng lobby tối đa khi query
        /// </summary>
        public const int LOBBY_QUERY_COUNT = 25;
        
        // ===== RELAY SETTINGS =====
        
        /// <summary>
        /// Max connections cho Relay (MAX_PLAYERS - 1 vì host không tính)
        /// </summary>
        public const int RELAY_MAX_CONNECTIONS = MAX_PLAYERS - 1;
        
        // ===== GAME SETTINGS =====
        
        /// <summary>
        /// Delay trước khi start game (giây)
        /// </summary>
        public const float GAME_START_DELAY = 3f;
        
        /// <summary>
        /// Scene name cho game
        /// </summary>
        public const string GAME_SCENE_NAME = "SceneGame";
        
        /// <summary>
        /// Scene name cho menu
        /// </summary>
        public const string MENU_SCENE_NAME = "MenuScene";
    }
}

