using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

[DisallowMultipleComponent]
public class PlayerController : MonoBehaviour {
  [Header("Path")]
  [SerializeField] WaypointPath waypointPath;
  [SerializeField] bool autoAssignPath = true;
  [SerializeField, FormerlySerializedAs("waypoints")] Transform[] fallbackWaypoints = Array.Empty<Transform>();
  [Header("Movement")]
  [SerializeField] float moveSpeed = 3f;

  public int PlayerId { get; private set; }
  public int NodeIndex { get; private set; }
  public int PathLength => HasPath ? _nodes.Length : waypointPath != null ? waypointPath.Count : fallbackWaypoints?.Length ?? 0;

  Transform[] _nodes = Array.Empty<Transform>();

  void Awake() {
    ResolvePath();
  }

  void OnValidate() {
    if (!Application.isPlaying) {
      ResolvePath();
    }
  }

  public void Init(int id, int start = 0) {
    PlayerId = id;
    EnsurePath();
    if (!HasPath) return;

    int index = NormalizeIndex(start);
    NodeIndex = index;
    transform.position = _nodes[index].position;
  }

  public void SetNode(int nodeIndex) {
    EnsurePath();
    if (!HasPath) return;

    int idx = NormalizeIndex(nodeIndex);
    NodeIndex = idx;
    transform.position = _nodes[idx].position;
  }

  public IEnumerator MoveBySteps(int steps) {
    EnsurePath();
    if (!HasPath || steps == 0) yield break;

    int direction = steps > 0 ? 1 : -1;
    int iterations = Mathf.Abs(steps);

    for (int i = 0; i < iterations; i++) {
      int next = NormalizeIndex(NodeIndex + direction);
      Vector3 dest = _nodes[next].position;
      while ((transform.position - dest).sqrMagnitude > 1e-4f) {
        transform.position = Vector3.MoveTowards(transform.position, dest, moveSpeed * Time.deltaTime);
        yield return null;
      }
      NodeIndex = next;
      yield return null;
    }
  }

  bool HasPath => _nodes != null && _nodes.Length > 0;

  void EnsurePath() {
    if (!HasPath) {
      ResolvePath();
      if (!HasPath && fallbackWaypoints != null && fallbackWaypoints.Length > 0) {
        _nodes = fallbackWaypoints;
      }
    }
  }

  void ResolvePath() {
    if (waypointPath == null && autoAssignPath) {
      waypointPath = FindObjectOfType<WaypointPath>();
    }

    if (waypointPath != null) {
      if (autoAssignPath && waypointPath.Count == 0) {
        waypointPath.Refresh();
      }
      var nodes = waypointPath.GetNodes();
      _nodes = nodes != null && nodes.Length > 0 ? nodes : Array.Empty<Transform>();
    } else if (fallbackWaypoints != null && fallbackWaypoints.Length > 0) {
      _nodes = fallbackWaypoints;
    } else {
      _nodes = Array.Empty<Transform>();
    }
  }

  int NormalizeIndex(int value) {
    if (!HasPath) return 0;
    int length = _nodes.Length;
    int result = value % length;
    if (result < 0) {
      result += length;
    }
    return result;
  }
}
