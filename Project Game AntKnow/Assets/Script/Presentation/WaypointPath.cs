using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class WaypointPath : MonoBehaviour {
  [SerializeField] Transform root;
  [SerializeField] bool autoCollectFromChildren = true;
  [SerializeField] Transform[] nodes = Array.Empty<Transform>();

  public IReadOnlyList<Transform> Nodes => nodes ?? Array.Empty<Transform>();
  public int Count => nodes != null ? nodes.Length : 0;

  void Awake() {
    if (autoCollectFromChildren && NeedsRefresh()) {
      Refresh();
    }
  }

  void OnValidate() {
    if (autoCollectFromChildren) {
      Refresh();
    }
  }

  void Reset() {
    autoCollectFromChildren = true;
    Refresh();
  }

  public void Refresh() {
    Transform source = root != null ? root : transform;
    int childCount = source.childCount;
    if (childCount == 0) {
      nodes = Array.Empty<Transform>();
      return;
    }

    var collected = new List<Transform>(childCount);
    for (int i = 0; i < childCount; i++) {
      collected.Add(source.GetChild(i));
    }

    nodes = collected.ToArray();
  }

  public Transform[] GetNodes() => nodes ?? Array.Empty<Transform>();

  bool NeedsRefresh() => nodes == null || nodes.Length == 0;
}
