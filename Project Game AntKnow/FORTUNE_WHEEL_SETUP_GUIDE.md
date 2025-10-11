# 🎡 **HƯỚNG DẪN TẠO FORTUNE WHEEL ĐƠN GIẢN**

## **📋 TỔNG QUAN**

Fortune Wheel đơn giản chỉ cần 2 sprites: Wheel và Pointer (kim). Wheel có 3 phần: House, Skip, Money -1.

---

## **🎯 BƯỚC 1: TẠO FORTUNE WHEEL PREFAB ĐƠN GIẢN**

### **1.1 Tạo Wheel GameObject Structure**

```
FortuneWheelPrefab (GameObject)
├── Wheel (GameObject với SpriteRenderer)
│   ├── FortuneWheel.png (Sprite - hình tròn có 3 phần)
│   └── WheelPivot (Empty GameObject - center point)
├── Pointer (GameObject với SpriteRenderer)
│   └── PointerSprite (Sprite - mũi tên)
└── FortuneWheelController (Script)
```

### **1.2 Wheel Sprite Design**

**FortuneWheel.png (512x512):**
- **Phần 1**: House icon - Màu teal/turquoise
- **Phần 2**: "SKIP" text - Màu yellow/orange
- **Phần 3**: Money -1 icon - Màu coral/red

**Pointer Sprite (32x64):**
- Hình mũi tên nhọn đơn giản
- Màu đen với border trắng

### **1.3 FortuneWheelController Script (Đơn Giản)**

```csharp
using UnityEngine;
using System.Collections;

namespace AntKnow.Game
{
    public class FortuneWheelController : MonoBehaviour
    {
        [Header("Wheel Components")]
        [SerializeField] private Transform wheel;
        [SerializeField] private Transform pointer;
        
        [Header("Animation Settings")]
        [SerializeField] private float spinDuration = 2f;
        [SerializeField] private int spinRotations = 5;
        [SerializeField] private AnimationCurve spinCurve = AnimationCurve.EaseOut(0, 1, 1, 0);
        
        [Header("Wheel Sections")]
        [SerializeField] private float[] sectionAngles = { 0f, 120f, 240f }; // 3 sections
        
        private bool isSpinning = false;
        private System.Action<int> onResultCallback; // 0=house, 1=skip, 2=money
        
        public void Spin(System.Action<int> onResult)
        {
            if (isSpinning) return;
            
            onResultCallback = onResult;
            StartCoroutine(SpinCoroutine());
        }
        
        private IEnumerator SpinCoroutine()
        {
            isSpinning = true;
            
            // Calculate final rotation
            float randomAngle = Random.Range(0f, 360f);
            float totalRotation = (spinRotations * 360f) + randomAngle;
            
            float elapsed = 0f;
            Vector3 startRotation = wheel.eulerAngles;
            Vector3 targetRotation = startRotation + Vector3.forward * totalRotation;
            
            // Spin animation
            while (elapsed < spinDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / spinDuration;
                float curveValue = spinCurve.Evaluate(t);
                
                Vector3 currentRotation = Vector3.Lerp(startRotation, targetRotation, curveValue);
                wheel.eulerAngles = currentRotation;
                
                yield return null;
            }
            
            // Determine result
            float finalAngle = wheel.eulerAngles.z;
            int result = GetSectionFromAngle(finalAngle);
            
            isSpinning = false;
            onResultCallback?.Invoke(result);
        }
        
        private int GetSectionFromAngle(float angle)
        {
            // Normalize angle to 0-360
            while (angle < 0) angle += 360;
            while (angle >= 360) angle -= 360;
            
            // Check which section the pointer is in
            for (int i = 0; i < sectionAngles.Length; i++)
            {
                float sectionStart = sectionAngles[i];
                float sectionEnd = sectionAngles[(i + 1) % sectionAngles.Length];
                
                if (sectionEnd < sectionStart) sectionEnd += 360;
                
                if (angle >= sectionStart && angle < sectionEnd)
                {
                    return i;
                }
            }
            
            return 0; // Default to first section
        }
        
        public void ResetWheel()
        {
            wheel.eulerAngles = Vector3.zero;
        }
    }
}
```

---

## **🎯 BƯỚC 2: SETUP PREFAB TRONG UNITY (ĐƠN GIẢN)**

### **2.1 Tạo Fortune Wheel Prefab**

1. **Tạo Empty GameObject** tên "FortuneWheelPrefab"
2. **Tạo Wheel GameObject:**
   - Add Component: SpriteRenderer
   - Assign: FortuneWheel.png sprite
   - Position: (0, 0, 0)
   - Scale: (1, 1, 1)
3. **Tạo Pointer GameObject:**
   - Add Component: SpriteRenderer  
   - Assign: Pointer sprite
   - Position: (0, 1.5, -0.1) - Above wheel
   - Rotation: (0, 0, 0)
4. **Thêm FortuneWheelController script** vào root GameObject

### **2.2 Setup Animation Curve**

1. **Chọn FortuneWheelController**
2. **Trong Inspector, click vào Spin Curve**
3. **Setup curve:**
   - Key 0: Value = 1, Tangent = 0
   - Key 1: Value = 0, Tangent = 0
   - Curve type: Ease Out

### **2.3 Setup Wheel Sections**

```
Section Angles (dựa trên FortuneWheel.png):
- Index 0: 0° (House - Teal)
- Index 1: 120° (SKIP - Yellow)  
- Index 2: 240° (Money -1 - Coral)
```

---

## **🎯 BƯỚC 3: INTEGRATE VÀO PANEL QUIZ**

### **3.1 PanelQuiz Hierarchy (Đơn Giản)**

```
PanelQuiz (GameObject)
├── PanelQuizController (PanelQuiz Script)
├── QuizPanel (GameObject)
└── FortuneWheelPanel (GameObject)
    ├── FortuneWheel (FortuneWheelController)
    ├── WheelResult (TextMeshProUGUI)
    └── WheelBackground (Image)
```

### **3.2 Inspector Setup**

**PanelQuiz Script:**
```
Fortune Wheel:
- Fortune Wheel: [Drag FortuneWheelController]
- Fortune Wheel Panel: [Drag FortuneWheelPanel GameObject]
```

**FortuneWheelController Script:**
```
Wheel Components:
- Wheel: [Drag Wheel Transform]
- Pointer: [Drag Pointer Transform]

Animation Settings:
- Spin Duration: 2
- Spin Rotations: 5
- Spin Curve: [Setup Ease Out curve]

Wheel Sections:
- Section Angles: [0, 120, 240]
```

### **3.3 Cập nhật PanelQuiz.cs**

```csharp
// Thêm vào PanelQuiz.cs
[Header("Fortune Wheel")]
[SerializeField] private FortuneWheelController fortuneWheel;
[SerializeField] private GameObject fortuneWheelPanel;

private void ShowFortuneWheel()
{
    if (fortuneWheel == null || fortuneWheelPanel == null) return;
    
    fortuneWheelPanel.SetActive(true);
    fortuneWheel.ResetWheel();
    
    fortuneWheel.Spin((result) => {
        StartCoroutine(ShowWheelResultCoroutine(result));
    });
}

private IEnumerator ShowWheelResultCoroutine(int result)
{
    yield return new WaitForSeconds(1f);
    
    string resultText = "";
    switch (result)
    {
        case 0: resultText = "Hạ 1 nhà!"; break;
        case 1: resultText = "Bỏ qua!"; break;
        case 2: resultText = "Mất tiền!"; break;
    }
    
    textWheelResult.text = resultText;
    yield return new WaitForSeconds(2f);
    
    fortuneWheelPanel.SetActive(false);
    onAnswerCallback?.Invoke(false); // Always false for penalties
}
```

---

## **🎯 BƯỚC 4: TESTING**

### **4.1 Test Checklist**

- [ ] Wheel spins smoothly
- [ ] Pointer points to correct section
- [ ] Result callback works
- [ ] Integration với PanelQuiz works

### **4.2 Common Issues & Solutions**

**Wheel không quay:**
- Check wheel Transform assignment
- Verify AnimationCurve setup

**Pointer không đúng vị trí:**
- Check pointer Transform assignment
- Verify section angles array

**Callback không gọi:**
- Check onResultCallback assignment
- Verify GetSectionFromAngle logic

---

## **✅ HOÀN THÀNH**

Sau khi hoàn thành, Fortune Wheel đơn giản sẽ:
- ✅ Quay mượt mà với animation curve
- ✅ Chọn random penalty section (House/Skip/Money)
- ✅ Hiển thị kết quả đúng
- ✅ Tích hợp hoàn hảo với PanelQuiz
- ✅ Chỉ cần 2 sprites đơn giản

**Fortune Wheel đơn giản sẵn sàng cho Annual Quiz Rounds!** 🎡✨
