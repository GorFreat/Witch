# Bước 1: Tách rời UI khỏi Combat Logic (Decouple Health & HealthUI)

Tài liệu này được tạo riêng để bạn theo dõi chi tiết toàn bộ kế hoạch, tư duy kiến trúc và mã nguồn cần sửa cho **Bước 1**. Bạn có thể đọc, tham chiếu và đặt câu hỏi về bất kỳ dòng code hoặc khái niệm nào chưa rõ.

---

## 1. Bản chất vấn đề & Tư duy kiến trúc

### Vấn đề hiện tại: Phụ thuộc chặt (Tight Coupling)
```
[PlayerHealth] ──(nắm giữ tham chiếu cứng)──> [HealthUI]
```
- Trong code hiện tại của bạn: `PlayerHealth` có biến `[SerializeField] private HealthUI _healthUI;`.
- Khi `PlayerHealth` nhận sát thương, nó gọi trực tiếp `_healthUI.UpdateHeart(...)`.
- **Hệ quả tiêu cực**:
  1. `PlayerHealth` (logic chiến đấu) bị "dính chặt" vào `HealthUI` (giao diện người dùng).
  2. Nếu bạn tắt UI, tạo một màn chơi không có UI, hoặc spawn Player từ Prefab ở một Scene khác chưa kịp kéo thả UI vào Inspector $\rightarrow$ Code sẽ văng lỗi `NullReferenceException`.
  3. Vi phạm nguyên tắc **Single Responsibility** (S trong SOLID): `PlayerHealth` chỉ nên lo việc tính toán máu, không nên lo việc vẽ giao diện.

### Kiến trúc giải pháp: Observer / Event-Driven Pattern
```
[PlayerHealth (Model)] ──(bắn tín hiệu: OnPlayerHealthChanged)──> [Event Channel]
                                                                        │
                                                    ┌───────────────────┴───────────────────┐
                                                    ▼                                       ▼
                                           [HealthUI (View)]                        [Sound/VFX (sau này)]
```
- **Model (`PlayerHealth`)**: Khi máu thay đổi, chỉ phát ra tín hiệu (Event). Nó hoàn toàn không quan tâm ai đang nghe.
- **View (`HealthUI`)**: Tự chủ động lắng nghe tín hiệu từ Player. Khi nhận được tín hiệu, nó tự vẽ lại số tim.
- Cả hai script độc lập với nhau. Bạn có thể xóa `HealthUI`, thay thế bằng thanh máu Slider, hoặc thêm hiệu ứng âm thanh mà không cần sửa 1 dòng code nào trong `PlayerHealth`.

---

## 2. Chi tiết các file cần sửa

---

### File 1: `Assets/Scripts/Combat/Health.cs`

#### A. Mục tiêu
Thêm sự kiện `OnHealthChanged` truyền đi 2 tham số: `float currentHealth` và `float maxHealth`. Sự kiện này sẽ kích hoạt khi game bắt đầu (`Start`) và mỗi khi bị trừ máu (`TakeDamage`).

#### B. So sánh Code Trước & Sau

```csharp
// ===== TRƯỚC KHI SỬA =====
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour, IDamageable
{
    [field: SerializeField] protected float _maxHealth = 100f;
    [SerializeField] protected float _currentHealth;

    public UnityEvent OnHurt;
    public UnityEvent OnDie;
    // ...
```

```csharp
// ===== SAU KHI SỬA =====
using System; // [MỚI] Dùng cho Action
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour, IDamageable
{
    [field: SerializeField] protected float _maxHealth = 100f;
    [SerializeField] protected float _currentHealth;

    public UnityEvent OnHurt;
    public UnityEvent OnDie;

    // [MỚI] Event C# chuẩn hiệu năng cao, truyền (máu hiện tại, máu tối đa)
    public event Action<float, float> OnHealthChanged;

    public Vector2 knockBackVelocity { get; private set; }
    protected bool isDead = false;

    void Start()
    {
        _currentHealth = _maxHealth;
        // [MỚI] Bắn sự kiện lúc bắt đầu game để UI cập nhật số tim ban đầu
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }

    public virtual void TakeDamage(float damage, Vector2 knockBackDir, float knockBackForce)
    {
        if (isDead) return;

        _currentHealth -= damage;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        knockBackVelocity = knockBackDir.normalized * knockBackForce;

        OnHurt?.Invoke();

        // [MỚI] Bắn sự kiện thông báo máu đã thay đổi
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

        Debug.Log("Taking damage: " + damage);
        if(_currentHealth <= 0)
        {
            Die();
        }
    }

    // [MỚI - TÙY CHỌN] Hàm hồi máu chuẩn bị sẵn cho tương lai (nhặt bình máu, phép hồi máu)
    public virtual void Heal(float amount)
    {
        if (isDead) return;
        _currentHealth = Mathf.Clamp(_currentHealth + amount, 0, _maxHealth);
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }

    protected void Die()
    {
        if(isDead) return;
        isDead = true;
        OnDie?.Invoke();
    }

    public float GetMaxHealth() => _maxHealth;
    public float GetCurrentHealth() => _currentHealth;
}
```

---

### File 2: `Assets/Scripts/Combat/PlayerHealth.cs`

#### A. Mục tiêu
- Xóa bỏ hoàn toàn biến `_healthUI`.
- Tạo một sự kiện tĩnh (`static event`) mang tên `OnPlayerHealthChanged`. Nhờ đó, bất kỳ đâu trong game (kể cả UI nằm ở Canvas riêng) đều có thể đăng ký nghe mà không cần tìm kiếm hay kéo thả GameObject `Player`.

#### B. So sánh Code Trước & Sau

```csharp
// ===== TRƯỚC KHI SỬA =====
using UnityEngine;

public class PlayerHealth : Health
{
    [SerializeField] private HealthUI _healthUI;

    private void Awake()
    {
        _healthUI.SetMaxHeart((int)_maxHealth);
    }

    public override void TakeDamage(float damage, Vector2 knockBackDir, float knockBackForce)
    {
        base.TakeDamage(damage, knockBackDir, knockBackForce);
        _healthUI.UpdateHeart((int)_currentHealth);
    }
}
```

```csharp
// ===== SAU KHI SỬA (Thay thế toàn bộ file) =====
using System;
using UnityEngine;

public class PlayerHealth : Health
{
    // Event tĩnh toàn cục: UI có thể lắng nghe trực tiếp thông qua tên class PlayerHealth
    public static event Action<float, float> OnPlayerHealthChanged;

    private void OnEnable()
    {
        // Đăng ký nhận sự kiện từ chính bản thân (lớp cha Health)
        OnHealthChanged += ForwardHealthChanged;
    }

    private void OnDisable()
    {
        // Hủy đăng ký khi Player bị tắt hoặc chuyển scene để tránh rò rỉ bộ nhớ
        OnHealthChanged -= ForwardHealthChanged;
    }

    // Chuyển tiếp sự kiện từ instance nội bộ ra sự kiện static toàn cục
    private void ForwardHealthChanged(float current, float max)
    {
        OnPlayerHealthChanged?.Invoke(current, max);
    }
}
```

---

### File 3: `Assets/Scripts/UI/HealthUI.cs`

#### A. Mục tiêu
- `HealthUI` tự động đăng ký nghe sự kiện `PlayerHealth.OnPlayerHealthChanged` khi component được bật (`OnEnable`).
- Khi người chơi bị thương hoặc vào game, `HealthUI` tự động cập nhật số lượng tim và hiển thị tim đầy/rỗng.

#### B. So sánh Code Trước & Sau

```csharp
// ===== CODE MỚI CỦA HealthUI.cs =====
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private Image _heartImage;
    [SerializeField] private Sprite _fullHeartSprite;
    [SerializeField] private Sprite _emptyHeartSprite;

    private List<Image> Hearts = new List<Image>();

    // [MỚI] Đăng ký lắng nghe sự kiện khi UI được kích hoạt
    private void OnEnable()
    {
        PlayerHealth.OnPlayerHealthChanged += HandlePlayerHealthChanged;
    }

    // [MỚI] Luôn luôn hủy đăng ký khi UI bị tắt hoặc hủy để tránh lỗi bộ nhớ
    private void OnDisable()
    {
        PlayerHealth.OnPlayerHealthChanged -= HandlePlayerHealthChanged;
    }

    // [MỚI] Hàm xử lý khi nhận được tín hiệu máu thay đổi từ Player
    private void HandlePlayerHealthChanged(float currentHealth, float maxHealth)
    {
        int maxHeartInt = (int)maxHealth;

        // Nếu số tim được tạo ra chưa khớp với máu tối đa (ví dụ mới vào game hoặc tăng max HP)
        if (Hearts.Count != maxHeartInt)
        {
            SetMaxHeart(maxHeartInt);
        }

        // Cập nhật trạng thái từng trái tim
        UpdateHeart((int)currentHealth);
    }

    // (Giữ nguyên các hàm bên dưới của bạn)
    public void SetMaxHeart(int maxHeart)
    {
        foreach(Image heart in Hearts)
        {
            Destroy(heart.gameObject);
        }
        Hearts.Clear();

        for(int i = 0; i < maxHeart; i++)
        {
            Image newHeart = Instantiate(_heartImage, transform);
            newHeart.sprite = _fullHeartSprite;
            Hearts.Add(newHeart);
        }
    }

    public void UpdateHeart(int currentHealth)
    {
        for (int i = 0; i < Hearts.Count; i++)
        {
            if( i < currentHealth)
            {
                Hearts[i].sprite = _fullHeartSprite;
            }
            else
            {
                Hearts[i].sprite = _emptyHeartSprite;
            }
        }
    }
}
```

---

## 3. Các bước kiểm tra trong Unity Editor

1. **Lưu code** của cả 3 file và quay lại Unity Editor để compile.
2. **Kiểm tra Inspector của Player**:
   - Chọn GameObject `Player` trong Hierarchy.
   - Nhìn vào component `PlayerHealth`, bạn sẽ thấy ô `_healthUI` đã biến mất hoàn toàn.
3. **Chạy game (Play Mode)**:
   - Các trái tim trên góc màn hình sẽ tự động hiển thị đầy đủ theo `_maxHealth`.
   - Cho quái (`DemonBat`) chạm vào Player: Máu trừ và số tim rỗng tương ứng.
   - Game chạy bình thường mà không hề có bất kỳ liên kết trực tiếp nào giữa Player và UI.

---

## 4. Giải đáp các câu hỏi kiến trúc chuyên sâu

### Q1: Tại sao lại dùng C# `event Action<float, float>` thay vì `UnityEvent`?
- **Hiệu năng**: `UnityEvent` có chi phí gọi và cấp phát bộ nhớ (GC alloc) lớn hơn C# `Action` thuần từ 2-4 lần. Trong game, các sự kiện gameplay gọi liên tục nên ưu tiên C# `event Action`.
- **An toàn**: Từ khóa `event` ngăn các class bên ngoài vô tình ghi đè (`= null`) sự kiện, họ chỉ có thể dùng toán tử `+=` (đăng ký) hoặc `-=` (hủy đăng ký).

### Q2: Tại sao BẮT BUỘC phải hủy đăng ký trong `OnDisable`?
- Nếu bạn đăng ký `PlayerHealth.OnPlayerHealthChanged += HandlePlayerHealthChanged` mà không hủy khi UI bị tắt/hủy, C# Garbage Collector sẽ **không thể thu hồi bộ nhớ** của UI vì `PlayerHealth` (static) vẫn đang giữ tham chiếu ngầm tới UI.
- Đây chính là nguyên nhân số 1 gây rò rỉ bộ nhớ (Memory Leak) và lỗi `MissingReferenceException` trong Unity. Luôn nhớ nguyên tắc: **Có `+=` ở `OnEnable` thì phải có `-=` ở `OnDisable`**.

### Q3: `static event` có nhược điểm gì khi game làm Multiplayer hoặc Co-op 2 người không?
- `static event` cực kỳ tuyệt vời cho game **Single-player** (như Vampire Survivors, Roguelike).
- Nếu sau này bạn làm game 2 người chơi (Co-op): sự kiện static sẽ không phân biệt được máu này là của Player 1 hay Player 2. Khi đó, chúng ta sẽ nâng cấp lên bước cao hơn là **Event Bus có ID** (`PlayerDamagedEvent(playerId, currentHp, maxHp)`) hoặc **ScriptableObject Channel**.
