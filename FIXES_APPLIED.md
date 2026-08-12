# 🔧 FIXES APPLIED - 3 CRITICAL ISSUES

**Date:** 2026-08-06  
**Issues Fixed:** AdultMia duplicate, SoundMixermanager persistence, Back button troubleshooting

---

## ✅ **VẤN ĐỀ 1: AdultMia tồn tại ở Main Menu (FIXED)**

### **Triệu chứng:**
- Quit về Main Menu → AdultMia vẫn hiển thị
- Play lại → 2 Mia cùng tồn tại (duplicate)

### **Nguyên nhân:**
PlayerManager có `DontDestroyOnLoad` → AdultMia sống mãi mãi kể cả ở Main Menu

### **Fix áp dụng:**
**File:** `Assets/Game/Scripts/PlayerManager.cs`

```csharp
private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
{
    if (!isInitialized) return;
    Debug.Log($"[PlayerManager] Scene loaded: {scene.name}");
    
    // ➕ THÊM: Destroy player khi về Main Menu
    if (scene.name == "MainMenuScene")
    {
        Debug.Log("[PlayerManager] Returned to Main Menu - destroying player");
        Destroy(gameObject);
        return;
    }
    
    // ... rest of code
}
```

### **Testing:**
1. Start game → Play
2. Trong game, mở menu → Quit
3. Verify: Chỉ còn UI, không có AdultMia ở Main Menu
4. Click Play lại → Chỉ có 1 Mia duy nhất

---

## ✅ **VẤN ĐỀ 2: Volume chỉ chỉnh được trong 1 scene (FIXED)**

### **Triệu chứng:**
- Volume sliders hoạt động ở Faycrest scene
- Chuyển sang MainMenu/HospitalScene → Sliders không hoạt động

### **Nguyên nhân:**
- `SoundMixermanager` KHÔNG có `DontDestroyOnLoad`
- KHÔNG có Singleton pattern
- Mỗi scene load → Instance bị destroy

### **Fix áp dụng:**
**File:** `Assets/Game/Scripts/Manager/SoundMixermanager.cs`

```csharp
public class SoundMixermanager : MonoBehaviour
{   
    // ➕ THÊM: Singleton instance
    public static SoundMixermanager Instance { get; private set; }
    
    [SerializeField] private AudioMixer mixer;
    
    void Awake()  // ➕ THÊM method Awake
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[SoundMixermanager] Duplicate instance - destroying duplicate");
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);  // ➕ Persist across scenes
        Debug.Log("[SoundMixermanager] Instance created and will persist");
    }
    
    // ➕ THÊM: Null checks cho mixer
    public void SetMasterVolume(float volume)
    {
        if (mixer == null)
        {
            Debug.LogError("[SoundMixermanager] AudioMixer reference is null!");
            return;
        }
        mixer.SetFloat("Master", Mathf.Log10(volume) * 20);
    }
    
    // ... similar null checks for Music and SFX
}
```

### **Testing:**
1. Open any scene (MainMenu, HospitalScene, etc.)
2. Open Settings panel
3. Adjust volume sliders
4. Verify: Volume changes immediately
5. Change scene → Adjust volume again
6. Verify: Still works in new scene

---

## ⚠️ **VẤN ĐỀ 3: Nút Back ở NightmarePuzzle không hoạt động (NEEDS CHECKING)**

### **Triệu chứng:**
Click nút Back → Không quay về NightmareScene

### **Possible Causes:**

#### **A. Scene name không khớp**
**File:** `Assets/BackToNightmare.cs` line 9:
```csharp
[SerializeField] private string targetSceneName = "Nightmare";
```

**Kiểm tra:**
1. Mở Unity → File > Build Settings
2. Check scene name có phải là **"Nightmare"** hay **"NightmareScene"**?
3. Hoặc check trong folder `Assets/Game/Scenes/`

**Fix nếu sai:**
- Option 1: Đổi trong code: `targetSceneName = "NightmareScene"`
- Option 2: Đổi trong Unity Inspector (chọn Back button → Inspector → Target Scene Name)

#### **B. Button không có Event Listener**

**Kiểm tra trong Unity:**
1. Mở scene `NightmarePuzzle`
2. Hierarchy → Tìm Back button (GameObject `Back` hoặc `BackButton`)
3. Inspector → Scroll xuống `Button (Script)` component
4. Check section `On Click ()`:
   - Có event listener không?
   - Event nào được gọi?

**Fix nếu thiếu:**
1. Click dấu `+` trong `On Click ()`
2. Kéo GameObject chứa `BackToNightmare` script vào ô Object
3. Dropdown → Chọn `BackToNightmare` → `BackToNightmareScene()`

#### **C. SceneTransition reference null**

**Kiểm tra:**
1. Chọn GameObject có `BackToNightmare` script
2. Inspector → `BackToNightmare (Script)`
3. Check field `Scene Transition`:
   - Có được assign không?
   - Nếu để null → OK (dùng direct load)

**Expected behavior:**
- Nếu `sceneTransition == null` → Load scene trực tiếp (không fade)
- Nếu có assign → Dùng fade effect

#### **D. Scene chưa được add vào Build Settings**

**Kiểm tra:**
1. Unity → File > Build Settings
2. Verify scene "Nightmare" (hoặc "NightmareScene") có trong list `Scenes In Build`
3. Checkbox phải được tick ✅

**Fix nếu thiếu:**
1. Kéo scene file từ Project window vào Build Settings
2. Hoặc click `Add Open Scenes`

---

## 🧪 **TESTING CHECKLIST**

### **Test 1: AdultMia Duplicate Fix**
- [ ] Start game from Main Menu
- [ ] Play game → Enter HospitalScene
- [ ] Open pause menu → Quit to Main Menu
- [ ] Verify: Only UI visible, no AdultMia
- [ ] Click Play again
- [ ] Verify: Only 1 Mia spawns

### **Test 2: Volume Persistence Fix**
- [ ] Start game → Open Settings
- [ ] Adjust Master volume
- [ ] Verify: Volume changes
- [ ] Play game → Enter HospitalScene
- [ ] Open Settings → Adjust Music volume
- [ ] Verify: Volume changes
- [ ] Change to another scene (Nightmare, etc.)
- [ ] Open Settings → Adjust SFX volume
- [ ] Verify: Volume still works

### **Test 3: Back Button (Manual Check Required)**
- [ ] Play until NightmarePuzzle scene
- [ ] Click Back button
- [ ] Expected: Return to NightmareScene
- [ ] If not working:
  - [ ] Check Console for errors
  - [ ] Verify scene name matches
  - [ ] Check Button event listener
  - [ ] Verify scene in Build Settings

---

## 🔍 **DEBUG COMMANDS**

Thêm vào code để debug Back button:

### **Option 1: Log trong BackToNightmare.cs**

```csharp
public void BackToNightmareScene()
{
    Debug.Log($"[BackToNightmare] Button clicked! Target: {targetSceneName}");
    Debug.Log($"[BackToNightmare] SceneTransition null? {sceneTransition == null}");
    
    if (sceneTransition != null)
    {
        Debug.Log("[BackToNightmare] Using SceneTransition...");
        sceneTransition.TransitionToScene(targetSceneName);
    }
    else
    {
        Debug.Log("[BackToNightmare] Using direct SceneManager.LoadScene...");
        SceneManager.LoadScene(targetSceneName);
    }
}
```

### **Option 2: Test direct scene load**

Tạm thời bypass SceneTransition để test:

```csharp
public void BackToNightmareScene()
{
    // Test: Force direct load
    Debug.Log("[BackToNightmare] TESTING: Direct load to Nightmare");
    SceneManager.LoadScene("NightmareScene");  // Thử cả 2 tên
}
```

---

## 📝 **NOTES**

1. **PlayerManager Fix:**
   - Destroy player khi về Main Menu
   - Ngăn duplicate khi Play lại
   - Clean solution không ảnh hưởng gameplay

2. **SoundMixermanager Fix:**
   - Singleton + DontDestroyOnLoad
   - Null checks cho mixer
   - Hoạt động xuyên suốt tất cả scenes

3. **Back Button:**
   - Cần kiểm tra Unity Inspector setup
   - Có thể là scene name mismatch
   - Code logic đúng, vấn đề nằm ở configuration

---

## 🚀 **READY FOR GITHUB PUSH?**

**Fixed:** ✅ AdultMia duplicate  
**Fixed:** ✅ Volume slider persistence  
**Needs Check:** ⚠️ Back button (Unity Inspector setup)

**Recommendation:**
1. Test cả 3 issues trong Unity Editor
2. Fix Back button configuration nếu cần
3. Commit với message: "fix: destroy player on main menu, add SoundMixermanager persistence"
4. Push to GitHub

---

**End of fixes document.**
