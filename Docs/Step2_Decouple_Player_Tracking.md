# Bước 2: Tách rời Cơ chế Theo dõi Player (Player Tracking Decoupling)

Tài liệu này ghi lại toàn bộ kế hoạch, tư duy kiến trúc và hướng dẫn từng bước để bạn tự thực hiện **Bước 2**: Loại bỏ `FindAnyObjectByType` và cơ chế kéo thả cứng `Transform` giữa Scene và Prefab.

---

## 1. Vấn đề hiện tại: "Cả thế giới đi tìm Player"

Trong game hiện tại của bạn, có ít nhất 3 hệ thống cần biết vị trí của Player:
1. **[`MapManager.cs`](file:///d:/UnityLearn/MyGame/Witch/Assets/Scripts/Map/MapManager.cs#L35)**: Dùng `FindAnyObjectByType<PlayerController>()` để tìm Player và lấy tọa độ load Map chunk.
2. **[`CameraFollow.cs`](file:///d:/UnityLearn/MyGame/Witch/Assets/Scripts/Util/CameraFollow.cs#L5)**: Kéo thả cứng `[SerializeField] private Transform _playerTransform`.
3. **[`EnemyCore.cs`](file:///d:/UnityLearn/MyGame/Witch/Assets/Scripts/Core/EnemyCore.cs#L7)**: Kéo thả cứng `[field: SerializeField] public Transform targetTransform`.

### Hậu quả của cách làm cũ:
* **Hiệu năng kém**: `FindAnyObjectByType` phải duyệt qua toàn bộ cây GameObject trong Scene. Khi Scene có hàng trăm quái vật và tilemap, hàm này sẽ gây khựng giật.
* **Lỗi Prefab (Rào cản lớn nhất khi làm Spawner)**: 
  * Khi bạn tạo một **Enemy Spawner** để sinh quái tự động, Prefab con quái nằm trong thư mục `Project Assets`, nó **không thể kéo thả** GameObject `Player` nằm trong Scene vào được!
  * Nếu Player chết rồi hồi sinh (hoặc đổi Scene), tất cả các biến kéo thả `playerTransform` trước đó sẽ bị biến thành `Missing / NullReferenceException`.

---

## 2. Giải pháp chuẩn công nghiệp: *ScriptableObject Transform Anchor*

Đây là kiến trúc nổi tiếng được giới thiệu bởi **Ryan Hipple (Unite 2017)** - hiện là tiêu chuẩn vàng của cộng đồng Unity để giải quyết bài toán chia sẻ dữ liệu giữa các thực thể mà không gây phụ thuộc chéo.

```mermaid
graph TD
    subgraph Asset Data
        Anchor[PlayerTransformAnchor.asset<br/>(ScriptableObject)]
    end

    subgraph Runtime Scene
        Player[PlayerController] -->|Awake/OnEnable: Đăng ký tọa độ| Anchor
        Map[MapManager] -->|Đọc tọa độ để load Chunk| Anchor
        Cam[CameraFollow] -->|Đọc tọa độ để bám theo| Anchor
        Enemy[Enemy / Spawner] -->|Đọc tọa độ để dí theo| Anchor
    end
```

### Nguyên lý hoạt động:
1. Tạo một file Asset trong Project gọi là `PlayerTransformAnchor.asset`.
2. **Player** nắm giữ Asset này: Khi sinh ra, Player đăng ký chính mình vào: `Anchor.Set(transform)`. Khi bị hủy/tắt: `Anchor.Clear()`.
3. **MapManager** (và cả Camera, Quái vật): Chỉ cần giữ Asset này trong Inspector. Khi cần vị trí, nó đọc từ `Anchor.Value`.
4. **Lợi ích vượt trội**:
   - Zero `FindAnyObjectByType`.
   - Các Prefab quái vật / spawner nằm trong thư mục Asset có thể kéo thả `PlayerTransformAnchor.asset` vào một cách dễ dàng.
   - Không lo Player bị tiêu diệt hay hồi sinh.

---

## 3. Các bước thực hiện chi tiết

---

### Bước 2.1: Tạo Script `TransformAnchor.cs` (Mới)
Tạo một file mới tại: `Assets/Scripts/Core/TransformAnchor.cs` (hoặc trong thư mục `Util` tùy bạn sắp xếp).

```csharp
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTransformAnchor", menuName = "Game/Anchors/Transform Anchor")]
public class TransformAnchor : ScriptableObject
{
    // Lưu Transform của đối tượng được theo dõi (ví dụ: Player)
    public Transform Value { get; private set; }

    // Kiểm tra xem hiện tại đã có đối tượng nào đăng ký chưa
    public bool HasValue => Value != null;

    // Sự kiện bắn ra khi có đối tượng mới đăng ký (giúp MapManager khởi tạo ngay khi Player xuất hiện)
    public event Action<Transform> OnTransformSet;

    public void Set(Transform target)
    {
        Value = target;
        OnTransformSet?.Invoke(target);
    }

    public void Clear()
    {
        Value = null;
    }
}
```

---

### Bước 2.2: Tạo file Asset trong Unity Editor
1. Quay lại Unity Editor.
2. Tạo thư mục `Assets/Data` (nếu chưa có).
3. Nhấp chuột phải vào cửa sổ Project $\rightarrow$ **Create** $\rightarrow$ **Game** $\rightarrow$ **Anchors** $\rightarrow$ **Transform Anchor**.
4. Đặt tên file là: `PlayerTransformAnchor`.

---

### Bước 2.3: Cập nhật `PlayerController.cs` để đăng ký bản thân

Mở file [`Assets/Scripts/Entity/PlayerController.cs`](file:///d:/UnityLearn/MyGame/Witch/Assets/Scripts/Entity/PlayerController.cs):

1. **Thêm biến tham chiếu đến Anchor:**
   ```csharp
   [Header("Anchor")]
   [SerializeField] private TransformAnchor _playerAnchor;
   ```

2. **Đăng ký và hủy đăng ký trong `OnEnable` / `OnDisable`:**
   ```csharp
   private void OnEnable()
   {
       // Đăng ký transform của bản thân vào Anchor
       if (_playerAnchor != null)
       {
           _playerAnchor.Set(transform);
       }

       health.OnHurt.AddListener(Hurt);
       health.OnDie.AddListener(Die);
   }

   private void OnDisable()
   {
       // Hủy đăng ký khi Player bị tắt hoặc chuyển scene
       if (_playerAnchor != null)
       {
           _playerAnchor.Clear();
       }

       health.OnHurt.RemoveListener(Hurt);
       health.OnDie.RemoveListener(Die);
   }
   ```

---

### Bước 2.4: Cập nhật `MapManager.cs` để đọc vị trí từ Anchor

Mở file [`Assets/Scripts/Map/MapManager.cs`](file:///d:/UnityLearn/MyGame/Witch/Assets/Scripts/Map/MapManager.cs):

1. **Thay thế biến `playerTransform` bằng `_playerAnchor`:**
   ```csharp
   [Header("Player Tracking")]
   [Tooltip("Player transform anchor for map tracking")]
   [SerializeField] private TransformAnchor _playerAnchor;
   ```

2. **Cập nhật hàm `Start()`:**
   Xóa bỏ hoàn toàn đoạn code `FindAnyObjectByType<PlayerController>()`. Thay bằng việc kiểm tra Anchor:
   ```csharp
   private void Start()
   {
       if (_playerAnchor != null && _playerAnchor.HasValue)
       {
           InitializeMap(_playerAnchor.Value.position);
       }
       else if (_playerAnchor != null)
       {
           // Nếu Player chưa kịp Awake, chờ sự kiện khi Player được đăng ký
           _playerAnchor.OnTransformSet += HandlePlayerRegistered;
       }
   }

   private void OnDestroy()
   {
       if (_playerAnchor != null)
       {
           _playerAnchor.OnTransformSet -= HandlePlayerRegistered;
       }
   }

   private void HandlePlayerRegistered(Transform player)
   {
       InitializeMap(player.position);
   }

   private void InitializeMap(Vector3 playerPosition)
   {
       currentChunkCoord = GetChunkCoordinateFromPosition(playerPosition);
       UpdateLoadedChunks();
   }
   ```

3. **Cập nhật hàm `Update()`:**
   Thay vì đọc `playerTransform.position`, đọc từ `_playerAnchor.Value.position`:
   ```csharp
   private void Update()
   {
       if (_playerAnchor == null || !_playerAnchor.HasValue) return;

       Vector2Int playerCoord = GetChunkCoordinateFromPosition(_playerAnchor.Value.position);
       if (playerCoord != currentChunkCoord)
       {
           currentChunkCoord = playerCoord;
           UpdateLoadedChunks();
       }
   }
   ```

---

## 4. Các bước kiểm tra trong Unity Editor

1. **Gắn Asset vào Inspector:**
   - Chọn GameObject `Player`: Kéo file asset `PlayerTransformAnchor` vào ô **Player Anchor** của `PlayerController`.
   - Chọn GameObject `MapManager`: Kéo file asset `PlayerTransformAnchor` vào ô **Player Anchor** của `MapManager`.
2. **Chạy Game (Play Mode):**
   - Bản đồ Chunk vẫn tải xung quanh Player hoàn hảo như trước.
   - Khi nhân vật di chuyển qua ô mới, Chunk mới vẫn load bình thường.
   - Không còn bất kỳ dòng `FindAnyObjectByType` nào chạy ngầm trong game!

---

## 5. Câu hỏi thường gặp (FAQ)

### Q1: Tại sao không dùng `public static Transform PlayerTransform;` trong `PlayerController`?
- Dùng biến `static` trong code là cách làm đơn giản nhất nhưng có các hạn chế sau:
  1. **Không hiển thị trên Inspector**: Bạn không thể nhìn thấy biến `static` trên Inspector để biết Player có đang tồn tại hay không.
  2. **Không hoạt động với Domain Reload tắt**: Khi bạn tắt Domain Reload trong Unity để vào Play Mode siêu tốc, các biến static C# sẽ không tự reset, rất dễ sinh bug.
  3. **Không dùng được cho Spawner Prefab**: ScriptableObject là một file Asset thực sự, kéo thả được vào bất kỳ Prefab nào trong Project.

### Q2: Script `CameraFollow` có áp dụng được cách này không?
- **Hoàn toàn được!** Bạn chỉ cần thay `[SerializeField] private Transform _playerTransform;` trong `CameraFollow` thành `[SerializeField] private TransformAnchor _playerAnchor;`. Vậy là cả Camera lẫn Map đều độc lập 100% với Player.
