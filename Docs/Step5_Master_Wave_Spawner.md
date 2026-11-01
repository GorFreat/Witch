# Hướng dẫn Nâng cấp: Master Wave Spawner & Multi-Object Pool

Tài liệu này ghi lại toàn bộ kế hoạch, tư duy kiến trúc và hướng dẫn từng bước để bạn tự thực hiện việc nâng cấp `EnemySpawner` thành **Bộ điều phối Sóng quái trung tâm (Master Wave Spawner)**, tích hợp **Multi-Object Pool** và **Hệ thống Sóng quái hướng dữ liệu (WaveData ScriptableObject)**.

---

## 1. Bản chất vấn đề & Tư duy kiến trúc

### Vấn đề của Spawner cũ:
1. Chỉ nhận vào đúng **1 Prefab duy nhất** (`_enemyPrefab`). Không thể sinh nhiều loại quái khác nhau.
2. Nếu nhét nhiều loại quái vào chung 1 cái `List<EnemyCore>`, khi cần sinh con Skeleton nó sẽ bốc nhầm con Dơi cũ đang ngủ trong Pool ra!
3. Chưa có cơ chế **Độ khó tăng dần theo thời gian (Wave Progression)**.

### Kiến trúc giải pháp:
```
[Wave 1 Asset] ──┐
[Wave 2 Asset] ──┼──> [EnemySpawner] ──> Quản lý [Tủ Multi-Pool]
[Wave 3 Asset] ──┘         │                     ├── Ngăn chứa Dơi (Bat Pool)
                           │                     └── Ngăn chứa Xương (Skeleton Pool)
                           ▼
                 Sinh quái ngẫu nhiên theo tỉ lệ % ngoài rìa Camera
```

1. **`WaveData` (ScriptableObject)**: Lưu cấu hình từng đợt (Thời gian kéo dài, nhịp sinh quái, danh sách các loại quái kèm **tỉ lệ xuất hiện**).
2. **Multi-Object Pool (Tủ nhiều ngăn kéo)**: 
   Sử dụng cấu trúc `Dictionary<GameObject, List<EnemyCore>>`:
   * Khi cần Dơi $\rightarrow$ Mở ngăn kéo Dơi lấy Dơi cũ ra.
   * Khi cần Skeleton $\rightarrow$ Mở ngăn kéo Skeleton lấy Skeleton cũ ra.
   * Hoàn toàn không bao giờ lấy nhầm loại quái!
3. **Thuật toán Chọn ngẫu nhiên có trọng số (Weighted Random Selection)**:
   Ví dụ trong Wave 2: Dơi (Trọng số 70), Skeleton (Trọng số 30) $\rightarrow$ Tỉ lệ 70% ra Dơi, 30% ra Skeleton.

---

## 2. Chi tiết các file cần sửa & tạo mới

---

### File 1: Tạo ScriptableObject `WaveData.cs` (Mới)
Tạo file mới tại: `Assets/Game/Scripts/Data/WaveData.cs`:

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemySpawnConfig
{
    [Tooltip("Prefab của loại quái này")]
    public GameObject enemyPrefab;

    [Tooltip("Tỉ lệ xuất hiện (trọng số). Càng cao thì quái này xuất hiện càng nhiều")]
    [Range(1, 100)]
    public int spawnWeight = 10;
}

[CreateAssetMenu(fileName = "NewWaveData", menuName = "Game/Waves/Wave Data")]
public class WaveData : ScriptableObject
{
    [Header("Wave Info")]
    [Tooltip("Tên của đợt quái (dùng để debug hoặc hiển thị UI)")]
    public string waveName = "Wave 1";

    [Header("Timing")]
    [Tooltip("Thời gian kéo dài của đợt này (tính bằng giây)")]
    public float duration = 60f;

    [Tooltip("Thời gian cách nhau giữa mỗi lần sinh quái trong đợt này (giây)")]
    public float spawnInterval = 1.5f;

    [Header("Enemies In This Wave")]
    [Tooltip("Danh sách các loại quái và tỉ lệ xuất hiện trong đợt này")]
    public List<EnemySpawnConfig> enemies = new List<EnemySpawnConfig>();

    // Thuật toán chọn ngẫu nhiên có trọng số (Weighted Random)
    public GameObject GetRandomEnemyPrefab()
    {
        if (enemies == null || enemies.Count == 0) return null;
        if (enemies.Count == 1) return enemies[0].enemyPrefab;

        // 1. Tính tổng tất cả trọng số (ví dụ: 70 + 30 = 100)
        int totalWeight = 0;
        foreach (var config in enemies)
        {
            if (config.enemyPrefab != null)
            {
                totalWeight += config.spawnWeight;
            }
        }

        if (totalWeight <= 0) return null;

        // 2. Quay số ngẫu nhiên từ 0 đến totalWeight - 1
        int randomValue = UnityEngine.Random.Range(0, totalWeight);
        int currentWeightSum = 0;

        // 3. Tìm xem lá phiếu ngẫu nhiên rơi vào con quái nào
        foreach (var config in enemies)
        {
            if (config.enemyPrefab == null) continue;

            currentWeightSum += config.spawnWeight;
            if (randomValue < currentWeightSum)
            {
                return config.enemyPrefab;
            }
        }

        return enemies[0].enemyPrefab;
    }
}
```

---

### File 2: Nâng cấp `EnemySpawner.cs`
Mở file [`Assets/Game/Scripts/Mechanism/EnemySpawner.cs`](file:///d:/UnityLearn/MyGame/Witch/Assets/Game/Scripts/Mechanism/EnemySpawner.cs).

Thay thế toàn bộ nội dung bằng code mới dưới đây (Đã xóa `using NUnit.Framework;`, thêm Multi-Pool và quản lý Wave):

```csharp
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Player Reference")]
    [Tooltip("Đọc vị trí Player qua Scriptable Anchor từ Bước 2")]
    [SerializeField] private TransformAnchor _playerAnchor;

    [Header("Wave Settings")]
    [Tooltip("Danh sách các đợt quái diễn ra theo thứ tự")]
    [SerializeField] private List<WaveData> _waves = new List<WaveData>();

    [Header("Spawn Settings")]
    [Tooltip("Khoảng cách sinh quái so với Player (nên để ngoài màn hình Camera)")]
    [SerializeField] private float _spawnRadius = 12f;

    [Header("Global Cap")]
    [Tooltip("Số lượng quái tối đa cùng xuất hiện trên màn hình để bảo toàn 60 FPS")]
    [SerializeField] private int _maxEnemyAlive = 30;

    // Tủ Multi-Pool: Mỗi Prefab sở hữu một danh sách riêng biệt
    private Dictionary<GameObject, List<EnemyCore>> _enemyPools = new Dictionary<GameObject, List<EnemyCore>>();

    private int _currentWaveIndex = 0;
    private float _waveTimer = 0f;
    private float _nextSpawnTime = 0f;

    private void Update()
    {
        // 1. Kiểm tra điều kiện tiên quyết
        if (_playerAnchor == null || !_playerAnchor.HasValue) return;
        if (_waves == null || _waves.Count == 0) return;

        WaveData currentWave = _waves[_currentWaveIndex];

        // 2. Quản lý thời gian của Wave hiện tại
        _waveTimer += Time.deltaTime;
        if (_waveTimer >= currentWave.duration)
        {
            AdvanceToNextWave();
            return;
        }

        // 3. Quản lý nhịp sinh quái
        if (Time.time >= _nextSpawnTime)
        {
            _nextSpawnTime = Time.time + currentWave.spawnInterval;
            TrySpawnEnemy(currentWave);
        }
    }

    private void AdvanceToNextWave()
    {
        if (_currentWaveIndex + 1 < _waves.Count)
        {
            _currentWaveIndex++;
            _waveTimer = 0f;
            Debug.Log($"[Spawner] Chuyển sang: {_waves[_currentWaveIndex].waveName}");
        }
        else
        {
            // Nếu đã hết các Wave cài đặt, giữ nguyên Wave cuối cùng để người chơi sinh tồn tiếp tục
            _waveTimer = 0f;
        }
    }

    private void TrySpawnEnemy(WaveData wave)
    {
        // Kiểm tra tổng số quái đang sống trên toàn bản đồ
        if (GetTotalActiveEnemies() >= _maxEnemyAlive) return;

        // Bốc ngẫu nhiên một loại quái từ cấu hình của Wave hiện tại
        GameObject prefabToSpawn = wave.GetRandomEnemyPrefab();
        if (prefabToSpawn == null) return;

        // Tính vị trí ngẫu nhiên ngoài rìa camera
        Vector2 randomCircle = Random.insideUnitCircle.normalized * _spawnRadius;
        Vector3 spawnPos = _playerAnchor.Value.position + new Vector3(randomCircle.x, randomCircle.y, 0f);

        // Lấy đúng loại quái tương ứng từ Multi-Pool và kích hoạt
        EnemyCore enemy = GetEnemyFromPool(prefabToSpawn);
        if (enemy != null)
        {
            enemy.ResetEnemy(spawnPos);
        }
    }

    private EnemyCore GetEnemyFromPool(GameObject prefab)
    {
        // 1. Nếu chưa có ngăn chứa cho Prefab này trong tủ, tạo ngăn mới
        if (!_enemyPools.ContainsKey(prefab))
        {
            _enemyPools[prefab] = new List<EnemyCore>();
        }

        List<EnemyCore> specificPool = _enemyPools[prefab];

        // 2. Tìm xem trong ngăn này có con quái nào đã chết (đang tắt) không
        foreach (var enemy in specificPool)
        {
            if (!enemy.gameObject.activeSelf)
            {
                return enemy; // Tái sử dụng!
            }
        }

        // 3. Nếu trong ngăn hết hàng, Instantiate thêm 1 con mới và cất vào ngăn này
        GameObject newEnemyObj = Instantiate(prefab, transform);
        EnemyCore newEnemy = newEnemyObj.GetComponent<EnemyCore>();
        specificPool.Add(newEnemy);
        return newEnemy;
    }

    private int GetTotalActiveEnemies()
    {
        int count = 0;
        foreach (var pool in _enemyPools.Values)
        {
            foreach (var enemy in pool)
            {
                if (enemy.gameObject.activeSelf) count++;
            }
        }
        return count;
    }
}
```

---

## 3. Các bước thiết lập trong Unity Editor

### Bước 1: Tạo thư mục chứa Wave
1. Trong cửa sổ Project, vào `Assets/Game/Data`.
2. Tạo thư mục mới tên là: `Waves` (Đường dẫn: `Assets/Game/Data/Waves`).

### Bước 2: Tạo các Asset Wave
1. Chuột phải vào thư mục `Waves` $\rightarrow$ **Create** $\rightarrow$ **Game** $\rightarrow$ **Waves** $\rightarrow$ **Wave Data**.
2. Đặt tên file là: `Wave1_BatSwarm`.
   * `Wave Name`: Wave 1 - Bầy Dơi
   * `Duration`: `20` (Để test nhanh trong 20 giây đầu)
   * `Spawn Interval`: `2.0`
   * Bấm dấu `+` ở `Enemies`:
     * Kéo Prefab `DemonBat` vào.
     * `Spawn Weight`: `10`
3. Tạo tiếp file thứ hai: `Wave2_Frenzy`.
   * `Wave Name`: Wave 2 - Cuồng Nộ
   * `Duration`: `30`
   * `Spawn Interval`: `0.8` (Quái ra dồn dập hơn)
   * Kéo Prefab `DemonBat` vào (sau này có thêm quái Skeleton thì bấm `+` thêm Skeleton vào đây).

### Bước 3: Gắn Wave vào Spawner
1. Chọn GameObject `EnemySpawner` trong Scene.
2. Tại trường **Waves**, bấm số lượng Size = 2.
3. Kéo `Wave1_BatSwarm` vào ô số 0.
4. Kéo `Wave2_Frenzy` vào ô số 1.

---

## 4. Kiểm tra thành quả (Play Mode)

1. Bấm **Play Game**:
   * Trong 20 giây đầu tiên, dơi xuất hiện tà tà (mỗi 2 giây 1 con).
   * Đến giây thứ 20, nhìn vào cửa sổ Console, bạn sẽ thấy thông báo: `[Spawner] Chuyển sang: Wave 2 - Cuồng Nộ`.
   * Dơi bắt đầu ùa ra dồn dập với tốc độ 0.8 giây/con!
2. Quan sát cửa sổ Hierarchy: Dù tốc độ sinh tăng lên, nhưng số lượng GameObject sinh ra không hề bị tràn lan vì những con dơi chết lập tức được tái sử dụng qua hệ thống **Multi-Pool**.
