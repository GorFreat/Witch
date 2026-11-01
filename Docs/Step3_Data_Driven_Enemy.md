# Bước 3: Kiến trúc Quái vật Hướng Dữ liệu (Data-Driven Enemy Design)

Tài liệu này ghi lại toàn bộ kế hoạch, tư duy kiến trúc và hướng dẫn từng bước để bạn tự thực hiện **Bước 3**: Ứng dụng sức mạnh của **ScriptableObject** để tạo ra hệ thống quái vật hướng dữ liệu (Data-Driven), giúp bạn có thể tạo ra hàng chục loại quái vật mới (Skeleton, Ghost, Zombie, Slime...) chỉ trong vài giây mà **không cần phải viết thêm một dòng code C# nào**.

---

## 1. Vấn đề hiện tại: Bẫy "Copy - Paste" khi làm nhiều quái

Hãy nhìn lại cấu trúc quái hiện tại của bạn:
* Bạn có [`EnemyCore.cs`](file:///d:/UnityLearn/MyGame/Witch/Assets/Scripts/Core/EnemyCore.cs) chứa các biến: `moveSpeed`, `attackRange`, `attackCooldown`...
* Bạn có [`DemonBatController.cs`](file:///d:/UnityLearn/MyGame/Witch/Assets/Scripts/Entity/DemonBatController.cs) kế thừa `EnemyCore`, trong đó khai báo 5 biến Animation Clips (`idleClip`, `chaseClip`, `attackClip`, `hurtClip`, `dieClip`) và hàm `Awake()` khởi tạo State Machine.

### Chuyện gì sẽ xảy ra khi game của bạn có 10 con quái?
1. Bạn muốn tạo con **Skeleton** $\rightarrow$ Bạn phải tạo file `SkeletonController.cs`.
2. Bạn phải **copy - paste** lại đúng 5 biến Animation Clips đó.
3. Bạn phải **copy - paste** lại toàn bộ hàm `Awake()` để khởi tạo State Machine.
4. Máu thì chỉnh ở component `Health`, tốc độ thì chỉnh ở `EnemyCore`, hoạt ảnh thì kéo ở `Controller` $\rightarrow$ Thông số một con quái bị xé lẻ ra 3 nơi khác nhau!

```
❌ CÁCH LÀM CŨ (Trùng lặp code, khó quản lý):
[DemonBatController.cs]  ──> Hardcode 5 Clips + Copy hàm khởi tạo State
[SkeletonController.cs]  ──> Hardcode 5 Clips + Copy hàm khởi tạo State
[GhostController.cs]     ──> Hardcode 5 Clips + Copy hàm khởi tạo State
```

---

## 2. Giải pháp kiến trúc: Data-Driven Design với ScriptableObject

Thay vì mỗi con quái là một file code C#, ta tách biệt hoàn toàn **Dữ liệu (Data)** ra khỏi **Hành vi (Behavior)**:

```
🌟 KIẾN TRÚC MỚI (Data-Driven):
[DemonBatData.asset] ──┐
[SkeletonData.asset] ──┼──> Dùng chung duy nhất 1 Controller: [EnemyController]
[GhostData.asset]    ──┘    (Tự động đọc dữ liệu và nạp State Machine)
```

### Lợi ích vượt trội:
1. **Tạo quái siêu tốc**: Muốn tạo một con quái mới? Chỉ cần chuột phải tạo một file Asset `ZombieData.asset`, điền chỉ số và kéo animation clip vào. **Xong! 0 dòng code mới**.
2. **Quản lý tập trung (Single Source of Truth)**: Máu, tốc độ, sát thương, tầm đánh, animation clips đều nằm gọn gàng trong **đúng 1 file asset**.
3. **Cân bằng game (Game Balancing) lúc Play Mode**: Bạn có thể vừa chơi game vừa chỉnh tốc độ, sát thương trong file Asset mà **không sợ bị mất thông số khi tắt Play Mode**.

---

## 3. Các bước thực hiện chi tiết

---

### Bước 3.1: Tạo ScriptableObject `EnemyData.cs` (Mới)
Tạo file mới tại: `Assets/Scripts/Core/EnemyData.cs` (hoặc trong thư mục `SO` tùy bạn):

```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Game/Enemies/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("General Info")]
    public string enemyName = "Enemy";

    [Header("Base Stats")]
    [Tooltip("Máu tối đa")]
    public float maxHealth = 50f;

    [Tooltip("Tốc độ di chuyển")]
    public float moveSpeed = 4f;

    [Tooltip("Sát thương gây ra")]
    public float damage = 10f;

    [Header("Combat Settings")]
    [Tooltip("Tầm phát hiện / Tấn công")]
    public float attackRange = 1.5f;

    [Tooltip("Thời gian hồi giữa các đòn đánh")]
    public float attackCooldown = 1.5f;

    [Tooltip("Lực đẩy lùi gây ra cho Player")]
    public float knockBackForce = 5f;

    [Header("Animation Clips")]
    public AnimationClip idleClip;
    public AnimationClip chaseClip;
    public AnimationClip attackClip;
    public AnimationClip hurtClip;
    public AnimationClip dieClip;
}
```

---

### Bước 3.2: Bổ sung hàm `SetMaxHealth` trong `Health.cs`

Mở file [`Assets/Scripts/Combat/Health.cs`](file:///d:/UnityLearn/MyGame/Witch/Assets/Scripts/Combat/Health.cs), thêm một hàm đơn giản để quái có thể nạp lượng máu từ file Data:

```csharp
// Thêm vào cuối file Health.cs:
public virtual void SetMaxHealth(float maxHealth)
{
    _maxHealth = maxHealth;
    _currentHealth = maxHealth;
    OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
}
```

---

### Bước 3.3: Tinh chỉnh `EnemyCore.cs` để nhận `EnemyData`

Mở file [`Assets/Scripts/Core/EnemyCore.cs`](file:///d:/UnityLearn/MyGame/Witch/Assets/Scripts/Core/EnemyCore.cs):

1. **Thêm biến tham chiếu đến `EnemyData`:**
   ```csharp
   [field: SerializeField] public EnemyData enemyData { get; private set; }
   ```

2. **Chuyển các biến chỉ số thành thuộc tính đọc từ `EnemyData`:**
   Thay vì khai báo các biến riêng lẻ `moveSpeed`, `attackRange`, `attackCooldown`, bạn cho chúng đọc trực tiếp từ `enemyData`:
   ```csharp
   public float moveSpeed => enemyData != null ? enemyData.moveSpeed : 4f;
   public float attackRange => enemyData != null ? enemyData.attackRange : 1.5f;
   public float attackCooldown => enemyData != null ? enemyData.attackCooldown : 1.5f;
   public float _moveSpeed => moveSpeed;
   ```

3. **Cập nhật hàm `Awake()`:**
   Đồng bộ máu của quái với `enemyData.maxHealth`:
   ```csharp
   protected override void Awake()
   {
       base.Awake();
       _machine = new StateMachine();
       health = GetComponent<Health>();

       if (enemyData != null && health != null)
       {
           health.SetMaxHealth(enemyData.maxHealth);
       }
   }
   ```

---

### Bước 3.4: Tinh gọn `DemonBatController.cs`

Mở file [`Assets/Scripts/Entity/DemonBatController.cs`](file:///d:/UnityLearn/MyGame/Witch/Assets/Scripts/Entity/DemonBatController.cs):

* **Xóa bỏ hoàn toàn** vùng `#region DEMON BAT ANIMATION CLIPS` (5 biến `idleClip`, `chaseClip`, `attackClip`, `hurtClip`, `dieClip`).
* Trong hàm `Awake()`, lấy các clips trực tiếp từ `enemyData`:

```csharp
using UnityEngine;

public class DemonBatController : EnemyCore
{
    [field: SerializeField] public Collider2D hitBox { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        // Lấy animation clips trực tiếp từ enemyData thay vì khai báo riêng lẻ
        idleState = new EnemyIdleState(_animator, enemyData.idleClip, this);
        chaseState = new EnemyChaseState(_animator, enemyData.chaseClip, this);
        attackState = new EnemyCloseAttack(_animator, enemyData.attackClip, hitBox, this);
        hurtState = new HurtState(_animator, enemyData.hurtClip, health, _rb, () =>
        {
            _machine.ChangeState(idleState);
        });
        dieState = new DieState(_animator, enemyData.dieClip, health, _rb);
    }

    protected override void Start()
    {
        base.Start();
        hitBox.enabled = false;
        _machine.Initialize(chaseState);
    }

    public void Update()
    {
        _machine.currentState.LogicUpdate();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        _machine.currentState.PhysicalUpdate();
    }
}
```

---

## 4. Các bước thao tác trong Unity Editor

1. **Tạo file Asset dữ liệu cho con dơi:**
   - Trong thư mục `Assets/Data`, chuột phải $\rightarrow$ **Create** $\rightarrow$ **Game** $\rightarrow$ **Enemies** $\rightarrow$ **Enemy Data**.
   - Đặt tên file là: `DemonBatData`.
   - Chọn file này và điền các thông số:
     - `Enemy Name`: Demon Bat
     - `Max Health`: 50
     - `Move Speed`: 4.5
     - `Attack Range`: 1.5
     - Kéo 5 Animation Clips của DemonBat vào các ô tương ứng.
2. **Gắn Asset vào Quái vật:**
   - Chọn Prefab hoặc GameObject `DemonBat`: Kéo file `DemonBatData` vào ô **Enemy Data** của component `DemonBatController`.
3. **Chạy thử Game (Play Mode):**
   - Con dơi vẫn bay đuổi theo và tấn công Player như bình thường.
   - Thử nghiệm tính năng "thần thánh": Khi game đang chạy, bấm vào file `DemonBatData`, chỉnh `Move Speed` từ `4.5` lên `10` $\rightarrow$ Con dơi lập tức bay nhanh như tên bắn ngay trước mắt bạn!

---

## 5. Câu hỏi thường gặp (FAQ)

### Q1: Sau này muốn tạo quái Skeleton hoặc Zombie thì làm thế nào?
- Bạn chỉ cần nhân bản (Ctrl+D) file `DemonBatData` $\rightarrow$ đổi tên thành `SkeletonData` $\rightarrow$ đổi số máu, đổi animation clip của Skeleton.
- Tạo một GameObject/Prefab Skeleton có gắn `DemonBatController` (hoặc đổi tên thành `EnemyMeleeController`), gắn `SkeletonData` vào là bạn đã có một con quái hoàn toàn mới! Không cần viết thêm code.

### Q2: Nếu có quái đánh xa (Bắn cung / Bắn phép) thì sao?
- Các chỉ số (máu, tốc độ) vẫn nằm trong `EnemyData`.
- Bạn chỉ cần tạo thêm 1 State mới gọi là `EnemyRangedAttackState` thay vì `EnemyCloseAttack`.
