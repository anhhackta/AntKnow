using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour {
  [SerializeField] Transform[] waypoints;
  [SerializeField] float moveSpeed = 3f;
  public int PlayerId { get; private set; }
  public int NodeIndex { get; private set; }

  public void Init(int id, int start = 0) {
    PlayerId = id; NodeIndex = start;
    if (waypoints != null && waypoints.Length > start)
      transform.position = waypoints[start].position;
  }

  public IEnumerator MoveBySteps(int steps) {
    if (waypoints == null || waypoints.Length == 0) yield break;
    int target = (NodeIndex + steps) % waypoints.Length;
    while (NodeIndex != target) {
      int next = (NodeIndex + 1) % waypoints.Length;
      Vector3 dest = waypoints[next].position;
      while ((transform.position - dest).sqrMagnitude > 1e-4f) {
        transform.position = Vector3.MoveTowards(transform.position, dest, moveSpeed * Time.deltaTime);
        yield return null;
      }
      NodeIndex = next;
      yield return null;
    }
  }
}

