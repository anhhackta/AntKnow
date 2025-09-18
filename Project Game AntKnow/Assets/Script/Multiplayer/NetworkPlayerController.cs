using System.Collections;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class NetworkPlayerController : NetworkBehaviour {
  [SerializeField] PlayerController playerController;

  public PlayerController LocalController => playerController;

  public void ServerInit(int playerId, int startNode = 0) {
    if (!IsServer) return;
    if (playerController == null) playerController = GetComponent<PlayerController>();
    playerController?.Init(playerId, startNode);
  }

  public IEnumerator ServerMoveBySteps(int steps) {
    if (playerController == null) yield break;
    yield return playerController.MoveBySteps(steps);
  }

  public void ServerSetNode(int nodeIndex) {
    if (!IsServer) return;
    playerController?.SetNode(nodeIndex);
  }
}
