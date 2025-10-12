using UnityEngine;
using UnityEditor;

namespace AntKnow.Game.Editor
{
    /// <summary>
    /// Editor tool để tự động tạo House/Hotel markers cho tất cả tiles
    /// Menu: Tools → AntKnow → Generate Tile Markers
    /// </summary>
    public class TileMarkerGenerator : EditorWindow
    {
        private bool createForAllTiles = true;
        private GameObject selectedTile = null;
        
        [MenuItem("Tools/AntKnow/Generate Tile Markers")]
        public static void ShowWindow()
        {
            GetWindow<TileMarkerGenerator>("Tile Marker Generator");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Tile Marker Generator", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            EditorGUILayout.HelpBox(
                "Tool này sẽ tạo 5 empty GameObjects (HouseMarker1-4, HotelMarker) cho mỗi tile.\n" +
                "Markers sẽ được đặt tại vị trí mặc định (4 góc + center).\n" +
                "Sau đó bạn có thể điều chỉnh positions trong Scene view.",
                MessageType.Info
            );
            
            GUILayout.Space(10);
            
            createForAllTiles = EditorGUILayout.Toggle("Create for ALL tiles", createForAllTiles);
            
            if (!createForAllTiles)
            {
                selectedTile = (GameObject)EditorGUILayout.ObjectField(
                    "Selected Tile",
                    selectedTile,
                    typeof(GameObject),
                    true
                );
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Generate Markers", GUILayout.Height(40)))
            {
                if (createForAllTiles)
                {
                    GenerateMarkersForAllTiles();
                }
                else
                {
                    if (selectedTile != null)
                    {
                        GenerateMarkersForTile(selectedTile);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Error", "Please select a tile GameObject!", "OK");
                    }
                }
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Clear All Markers", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog(
                    "Confirm",
                    "Are you sure you want to delete all markers from all tiles?",
                    "Yes", "No"))
                {
                    ClearAllMarkers();
                }
            }
        }
        
        /// <summary>
        /// Generate markers for all tiles in scene
        /// </summary>
        private void GenerateMarkersForAllTiles()
        {
            // Find all TileVisual components
            TileVisual[] tiles = FindObjectsOfType<TileVisual>();
            
            if (tiles.Length == 0)
            {
                EditorUtility.DisplayDialog("Error", "No tiles found in scene!", "OK");
                return;
            }
            
            int count = 0;
            foreach (var tile in tiles)
            {
                if (GenerateMarkersForTile(tile.gameObject))
                {
                    count++;
                }
            }
            
            EditorUtility.DisplayDialog(
                "Success",
                $"Generated markers for {count} tiles!",
                "OK"
            );
        }
        
        /// <summary>
        /// Generate markers for specific tile
        /// </summary>
        private bool GenerateMarkersForTile(GameObject tileObj)
        {
            if (tileObj == null)
            {
                return false;
            }
            
            TileVisual tileVisual = tileObj.GetComponent<TileVisual>();
            if (tileVisual == null)
            {
                Debug.LogWarning($"[TileMarkerGenerator] {tileObj.name} does not have TileVisual component!");
                return false;
            }
            
            // Find or create Platform
            Transform platform = tileObj.transform.Find("Platform");
            if (platform == null)
            {
                Debug.LogWarning($"[TileMarkerGenerator] {tileObj.name} does not have Platform child!");
                return false;
            }
            
            // Create Markers container (if not exists)
            Transform markersContainer = platform.Find("Markers");
            if (markersContainer == null)
            {
                GameObject markersObj = new GameObject("Markers");
                markersObj.transform.SetParent(platform);
                markersObj.transform.localPosition = Vector3.zero;
                markersObj.transform.localRotation = Quaternion.identity;
                markersObj.transform.localScale = Vector3.one;
                markersContainer = markersObj.transform;
            }
            
            // Default positions (local space of Platform)
            Vector3[] housePositions = new Vector3[]
            {
                new Vector3(-0.15f, 0.1f, -0.15f),  // HouseMarker1 (Top-left)
                new Vector3(0.15f, 0.1f, -0.15f),   // HouseMarker2 (Top-right)
                new Vector3(-0.15f, 0.1f, 0.15f),   // HouseMarker3 (Bottom-left)
                new Vector3(0.15f, 0.1f, 0.15f)     // HouseMarker4 (Bottom-right)
            };
            
            Vector3 hotelPosition = new Vector3(0f, 0.15f, 0f); // Center
            
            // Create House Markers
            Transform[] houseMarkers = new Transform[4];
            for (int i = 0; i < 4; i++)
            {
                string markerName = $"HouseMarker{i + 1}";
                Transform marker = markersContainer.Find(markerName);
                
                if (marker == null)
                {
                    GameObject markerObj = new GameObject(markerName);
                    markerObj.transform.SetParent(markersContainer);
                    markerObj.transform.localPosition = housePositions[i];
                    markerObj.transform.localRotation = Quaternion.Euler(0f, -90f, 0f); // Default rotation
                    markerObj.transform.localScale = Vector3.one;
                    marker = markerObj.transform;
                }
                
                houseMarkers[i] = marker;
            }
            
            // Create Hotel Marker
            Transform hotelMarker = markersContainer.Find("HotelMarker");
            if (hotelMarker == null)
            {
                GameObject markerObj = new GameObject("HotelMarker");
                markerObj.transform.SetParent(markersContainer);
                markerObj.transform.localPosition = hotelPosition;
                markerObj.transform.localRotation = Quaternion.Euler(90f, 180f, 0f); // Default rotation
                markerObj.transform.localScale = Vector3.one;
                hotelMarker = markerObj.transform;
            }
            
            // Assign markers to TileVisual component
            SerializedObject so = new SerializedObject(tileVisual);
            
            SerializedProperty houseMarkersProperty = so.FindProperty("houseMarkers");
            if (houseMarkersProperty != null && houseMarkersProperty.isArray)
            {
                houseMarkersProperty.arraySize = 4;
                for (int i = 0; i < 4; i++)
                {
                    houseMarkersProperty.GetArrayElementAtIndex(i).objectReferenceValue = houseMarkers[i];
                }
            }
            
            SerializedProperty hotelMarkerProperty = so.FindProperty("hotelMarker");
            if (hotelMarkerProperty != null)
            {
                hotelMarkerProperty.objectReferenceValue = hotelMarker;
            }
            
            so.ApplyModifiedProperties();
            
            Debug.Log($"[TileMarkerGenerator] Generated markers for {tileObj.name}");
            return true;
        }
        
        /// <summary>
        /// Clear all markers from all tiles
        /// </summary>
        private void ClearAllMarkers()
        {
            TileVisual[] tiles = FindObjectsOfType<TileVisual>();
            
            int count = 0;
            foreach (var tile in tiles)
            {
                Transform platform = tile.transform.Find("Platform");
                if (platform != null)
                {
                    Transform markers = platform.Find("Markers");
                    if (markers != null)
                    {
                        DestroyImmediate(markers.gameObject);
                        count++;
                    }
                }
            }
            
            EditorUtility.DisplayDialog(
                "Success",
                $"Cleared markers from {count} tiles!",
                "OK"
            );
        }
    }
}

