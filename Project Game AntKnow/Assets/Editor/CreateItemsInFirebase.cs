using UnityEngine;
using UnityEditor;
using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AntKnow.Editor
{
    /// <summary>
    /// Unity Editor Tool để tạo items collection trong Firebase
    /// </summary>
    public class CreateItemsInFirebase : EditorWindow
    {
        private FirebaseFirestore firestore;
        private bool isInitialized = false;
        private Vector2 scrollPosition;
        private string statusMessage = "";
        
        [MenuItem("AntKnow/Create Items in Firebase")]
        public static void ShowWindow()
        {
            GetWindow<CreateItemsInFirebase>("Create Items in Firebase");
        }
        
        private void OnEnable()
        {
            InitializeFirebase();
        }
        
        private async void InitializeFirebase()
        {
            try
            {
                await Firebase.FirebaseApp.CheckAndFixDependenciesAsync();
                firestore = FirebaseFirestore.DefaultInstance;
                isInitialized = true;
                statusMessage = "✅ Firebase initialized successfully!";
            }
            catch (System.Exception e)
            {
                statusMessage = $"❌ Firebase initialization failed: {e.Message}";
                Debug.LogError(statusMessage);
            }
        }
        
        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            GUILayout.Label("Create Items in Firebase", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            // Status
            EditorGUILayout.HelpBox(statusMessage, MessageType.Info);
            GUILayout.Space(10);
            
            if (!isInitialized)
            {
                if (GUILayout.Button("Retry Initialize Firebase"))
                {
                    InitializeFirebase();
                }
                EditorGUILayout.EndScrollView();
                return;
            }
            
            // Equipment
            GUILayout.Label("Equipment Items", EditorStyles.boldLabel);
            if (GUILayout.Button("Create All Equipment (5 items)"))
            {
                CreateAllEquipment();
            }
            GUILayout.Space(5);
            
            // Skill Cards
            GUILayout.Label("Skill Cards", EditorStyles.boldLabel);
            if (GUILayout.Button("Create All Skill Cards (4 cards)"))
            {
                CreateAllSkillCards();
            }
            GUILayout.Space(5);
            
            // EXP Cards
            GUILayout.Label("EXP Cards", EditorStyles.boldLabel);
            if (GUILayout.Button("Create EXP Card (1 card)"))
            {
                CreateExpCard();
            }
            GUILayout.Space(10);
            
            // Create All
            EditorGUILayout.HelpBox("This will create all 10 items in Firebase", MessageType.Warning);
            if (GUILayout.Button("CREATE ALL ITEMS (10 items)", GUILayout.Height(40)))
            {
                CreateAllItems();
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        private async void CreateAllItems()
        {
            statusMessage = "Creating all items...";
            Repaint();
            
            await CreateAllEquipmentAsync();
            await CreateAllSkillCardsAsync();
            await CreateExpCardAsync();
            
            statusMessage = "✅ All items created successfully!";
            Repaint();
        }
        
        private async void CreateAllEquipment()
        {
            statusMessage = "Creating equipment...";
            Repaint();
            await CreateAllEquipmentAsync();
            statusMessage = "✅ Equipment created successfully!";
            Repaint();
        }
        
        private async void CreateAllSkillCards()
        {
            statusMessage = "Creating skill cards...";
            Repaint();
            await CreateAllSkillCardsAsync();
            statusMessage = "✅ Skill cards created successfully!";
            Repaint();
        }
        
        private async void CreateExpCard()
        {
            statusMessage = "Creating exp card...";
            Repaint();
            await CreateExpCardAsync();
            statusMessage = "✅ EXP card created successfully!";
            Repaint();
        }
        
        // ===== CREATE METHODS =====
        
        private async Task CreateAllEquipmentAsync()
        {
            // Hat
            await CreateItemAsync("equip.hat.basic", new Dictionary<string, object>
            {
                { "name", "Hat Basic" },
                { "type", "equipment" },
                { "rarity", "common" },
                { "status", "active" },
                { "icon", "equip.hat.basic" },
                { "attributes", new Dictionary<string, object>
                    {
                        { "health", 5 },
                        { "agility", 0 },
                        { "intelligence", 0 },
                        { "luck", 0 },
                        { "resistance", 0 }
                    }
                },
                { "equipment", new Dictionary<string, object>
                    {
                        { "slot", "hat" },
                        { "durabilityMax", 100 }
                    }
                }
            });
            
            // Shirt
            await CreateItemAsync("equip.shirt.basic", new Dictionary<string, object>
            {
                { "name", "Shirt Basic" },
                { "type", "equipment" },
                { "rarity", "common" },
                { "status", "active" },
                { "icon", "equip.shirt.basic" },
                { "attributes", new Dictionary<string, object>
                    {
                        { "health", 10 },
                        { "agility", 0 },
                        { "intelligence", 0 },
                        { "luck", 0 },
                        { "resistance", 5 }
                    }
                },
                { "equipment", new Dictionary<string, object>
                    {
                        { "slot", "shirt" },
                        { "durabilityMax", 100 }
                    }
                }
            });
            
            // Wings
            await CreateItemAsync("equip.wings.basic", new Dictionary<string, object>
            {
                { "name", "Wings Basic" },
                { "type", "equipment" },
                { "rarity", "common" },
                { "status", "active" },
                { "icon", "equip.wings.basic" },
                { "attributes", new Dictionary<string, object>
                    {
                        { "health", 0 },
                        { "agility", 10 },
                        { "intelligence", 0 },
                        { "luck", 5 },
                        { "resistance", 0 }
                    }
                },
                { "equipment", new Dictionary<string, object>
                    {
                        { "slot", "wings" },
                        { "durabilityMax", 100 }
                    }
                }
            });
            
            // Shoes
            await CreateItemAsync("equip.shoes.basic", new Dictionary<string, object>
            {
                { "name", "Shoes Basic" },
                { "type", "equipment" },
                { "rarity", "common" },
                { "status", "active" },
                { "icon", "equip.shoes.basic" },
                { "attributes", new Dictionary<string, object>
                    {
                        { "health", 0 },
                        { "agility", 8 },
                        { "intelligence", 0 },
                        { "luck", 0 },
                        { "resistance", 0 }
                    }
                },
                { "equipment", new Dictionary<string, object>
                    {
                        { "slot", "shoes" },
                        { "durabilityMax", 100 }
                    }
                }
            });
            
            // Mask
            await CreateItemAsync("equip.mask.basic", new Dictionary<string, object>
            {
                { "name", "Mask Basic" },
                { "type", "equipment" },
                { "rarity", "common" },
                { "status", "active" },
                { "icon", "equip.mask.basic" },
                { "attributes", new Dictionary<string, object>
                    {
                        { "health", 0 },
                        { "agility", 0 },
                        { "intelligence", 5 },
                        { "luck", 0 },
                        { "resistance", 3 }
                    }
                },
                { "equipment", new Dictionary<string, object>
                    {
                        { "slot", "mask" },
                        { "durabilityMax", 100 }
                    }
                }
            });
        }
        
        private async Task CreateAllSkillCardsAsync()
        {
            // Bảo Kê
            await CreateItemAsync("skill.bao-ke", new Dictionary<string, object>
            {
                { "name", "Bảo Kê" },
                { "type", "skill_card" },
                { "rarity", "rare" },
                { "status", "active" },
                { "icon", "skill.bao-ke" },
                { "attributes", new Dictionary<string, object>
                    {
                        { "health", 0 },
                        { "agility", 0 },
                        { "intelligence", 0 },
                        { "luck", 0 },
                        { "resistance", 10 },
                        { "primaryStat", "resistance" },
                        { "attributePerLevel", 2 }
                    }
                },
                { "skill", new Dictionary<string, object>
                    {
                        { "mode", "passive" },
                        { "effect", "Giảm 20% tiền thuê nhà" },
                        { "effectId", "rentReduction" },
                        { "cooldownBaseTurns", 0 }
                    }
                }
            });
            
            // Chậm Chỉ
            await CreateItemAsync("skill.cham-chi", new Dictionary<string, object>
            {
                { "name", "Chậm Chỉ" },
                { "type", "skill_card" },
                { "rarity", "common" },
                { "status", "active" },
                { "icon", "skill.cham-chi" },
                { "attributes", new Dictionary<string, object>
                    {
                        { "health", 5 },
                        { "agility", 0 },
                        { "intelligence", 0 },
                        { "luck", 0 },
                        { "resistance", 0 },
                        { "primaryStat", "health" },
                        { "attributePerLevel", 1 }
                    }
                },
                { "skill", new Dictionary<string, object>
                    {
                        { "mode", "passive" },
                        { "effect", "Tăng 10% HP" },
                        { "effectId", "healthBoost" },
                        { "cooldownBaseTurns", 0 }
                    }
                }
            });
            
            // Lận Trộn
            await CreateItemAsync("skill.lan-tron", new Dictionary<string, object>
            {
                { "name", "Lận Trộn" },
                { "type", "skill_card" },
                { "rarity", "epic" },
                { "status", "active" },
                { "icon", "skill.lan-tron" },
                { "attributes", new Dictionary<string, object>
                    {
                        { "health", 0 },
                        { "agility", 15 },
                        { "intelligence", 0 },
                        { "luck", 5 },
                        { "resistance", 0 },
                        { "primaryStat", "agility" },
                        { "attributePerLevel", 3 }
                    }
                },
                { "skill", new Dictionary<string, object>
                    {
                        { "mode", "active" },
                        { "effect", "Tránh 1 lần thuê nhà" },
                        { "effectId", "avoidRent" },
                        { "cooldownBaseTurns", 5 }
                    }
                }
            });
            
            // Siêu Sale
            await CreateItemAsync("skill.sieu-sale", new Dictionary<string, object>
            {
                { "name", "Siêu Sale" },
                { "type", "skill_card" },
                { "rarity", "legendary" },
                { "status", "active" },
                { "icon", "skill.sieu-sale" },
                { "attributes", new Dictionary<string, object>
                    {
                        { "health", 0 },
                        { "agility", 0 },
                        { "intelligence", 20 },
                        { "luck", 10 },
                        { "resistance", 0 },
                        { "primaryStat", "intelligence" },
                        { "attributePerLevel", 4 }
                    }
                },
                { "skill", new Dictionary<string, object>
                    {
                        { "mode", "active" },
                        { "effect", "Giảm 50% giá mua nhà" },
                        { "effectId", "buyDiscount" },
                        { "cooldownBaseTurns", 3 }
                    }
                }
            });
        }
        
        private async Task CreateExpCardAsync()
        {
            await CreateItemAsync("exp.small", new Dictionary<string, object>
            {
                { "name", "EXP Small" },
                { "type", "exp_card" },
                { "rarity", "common" },
                { "status", "active" },
                { "icon", "exp.small" },
                { "exp", new Dictionary<string, object>
                    {
                        { "xpValue", 100 }
                    }
                }
            });
        }
        
        private async Task CreateItemAsync(string itemId, Dictionary<string, object> data)
        {
            try
            {
                var itemRef = firestore.Collection("items").Document(itemId);
                await itemRef.SetAsync(data);
                Debug.Log($"✅ Created item: {itemId}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Failed to create item {itemId}: {e.Message}");
            }
        }
    }
}

