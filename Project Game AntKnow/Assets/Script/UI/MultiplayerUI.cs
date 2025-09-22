using UnityEngine;
using UnityEngine.UI;

public class MultiplayerUI : MonoBehaviour {
  [Header("UI Elements")]
  [SerializeField] Button createLobbyButton;
  [SerializeField] Button quickJoinButton;
  [SerializeField] Button joinByCodeButton;
  [SerializeField] Button startGameButton;
  [SerializeField] Button leaveLobbyButton;
  [SerializeField] InputField lobbyCodeInput;
  [SerializeField] InputField playerNameInput;
  [SerializeField] Text lobbyStatusText;
  [SerializeField] Text playerListText;

  void Start() {
    // Find MultiplayerManager and assign UI references
    var multiplayerManager = FindObjectOfType<MultiplayerManager>();
    if (multiplayerManager != null) {
      // Use reflection to assign UI references
      var managerType = typeof(MultiplayerManager);
      
      var createLobbyField = managerType.GetField("createLobbyButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
      createLobbyField?.SetValue(multiplayerManager, createLobbyButton);
      
      var quickJoinField = managerType.GetField("quickJoinButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
      quickJoinField?.SetValue(multiplayerManager, quickJoinButton);
      
      var joinByCodeField = managerType.GetField("joinByCodeButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
      joinByCodeField?.SetValue(multiplayerManager, joinByCodeButton);
      
      var startGameField = managerType.GetField("startGameButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
      startGameField?.SetValue(multiplayerManager, startGameButton);
      
      var leaveLobbyField = managerType.GetField("leaveLobbyButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
      leaveLobbyField?.SetValue(multiplayerManager, leaveLobbyButton);
      
      var lobbyCodeField = managerType.GetField("lobbyCodeInput", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
      lobbyCodeField?.SetValue(multiplayerManager, lobbyCodeInput);
      
      var playerNameField = managerType.GetField("playerNameInput", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
      playerNameField?.SetValue(multiplayerManager, playerNameInput);
      
      var lobbyStatusField = managerType.GetField("lobbyStatusText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
      lobbyStatusField?.SetValue(multiplayerManager, lobbyStatusText);
      
      var playerListField = managerType.GetField("playerListText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
      playerListField?.SetValue(multiplayerManager, playerListText);
    }
  }
}
