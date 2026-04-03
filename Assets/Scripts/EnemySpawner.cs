using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Danh sách Enemy Prefab (Kéo 6 Prefab vào đây)")]
    public GameObject[] enemyPrefabs; // Kéo 6 loại enemy vào đây trong Inspector

    [Header("Vị trí Spawn")]
    public float spawnXLeft  = -9f;   // Mé trái nơi enemy xuất hiện
    public float spawnXRight =  9f;   // Mé phải nơi enemy xuất hiện
    public float spawnYMin   = -2.8f; // Y thấp nhất (đồng bộ với PlayerController)
    public float spawnYMax   = -0.5f; // Y cao nhất (đồng bộ với PlayerController)

    [Header("Kích hoạt Spawn")]
    public float activationDistance = 2f; // Player đi xa trung tâm bao nhiêu thì kích hoạt

    [Header("Tốc độ Spawn")]
    public float spawnInterval = 3f;     // Thời gian giữa 2 đợt spawn (giây)
    public int   spawnPerWave  = 1;      // Số lượng enemy mỗi đợt
    public int   maxEnemies    = 15;     // Số lượng enemy tối đa cùng lúc trên màn

    // --- Private ---
    private Transform player;
    private float     spawnTimer = 0f;
    private bool      isActive   = false;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player == null || enemyPrefabs.Length == 0) return;

        // Kích hoạt khi player đi ra khỏi vùng giữa
        float distFromCenter = Mathf.Abs(player.position.x);
        if (distFromCenter >= activationDistance)
        {
            isActive = true;
        }

        if (!isActive) return;

        // Đếm giờ spawn
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            TrySpawnWave();
        }
    }

    void TrySpawnWave()
    {
        // Đếm số enemy còn sống trên màn (theo layer hoặc tag "Enemy")
        int currentEnemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
        if (currentEnemyCount >= maxEnemies) return;

        int canSpawn = Mathf.Min(spawnPerWave, maxEnemies - currentEnemyCount);

        for (int i = 0; i < canSpawn; i++)
        {
            SpawnOneEnemy();
        }
    }

    void SpawnOneEnemy()
    {
        // Chọn ngẫu nhiên loại enemy
        int randomIndex = Random.Range(0, enemyPrefabs.Length);
        GameObject prefab = enemyPrefabs[randomIndex];
        if (prefab == null) return;

        // Chọn ngẫu nhiên bên trái hoặc phải
        float spawnX = Random.value > 0.5f ? spawnXRight : spawnXLeft;

        // Chọn ngẫu nhiên tọa độ Y trong dải cho phép
        float spawnY = Random.Range(spawnYMin, spawnYMax);

        Vector3 spawnPos = new Vector3(spawnX, spawnY, 0f);

        Instantiate(prefab, spawnPos, Quaternion.identity);
    }

    // Vẽ vùng kích hoạt trong Unity Editor để dễ hình dung
    void OnDrawGizmosSelected()
    {
        // Vùng giữa (không spawn)
        Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
        Gizmos.DrawCube(Vector3.zero, new Vector3(activationDistance * 2f, 3f, 0f));

        // Điểm spawn trái
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(spawnXLeft, spawnYMin, 0), new Vector3(spawnXLeft, spawnYMax, 0));

        // Điểm spawn phải
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(new Vector3(spawnXRight, spawnYMin, 0), new Vector3(spawnXRight, spawnYMax, 0));
    }
}
