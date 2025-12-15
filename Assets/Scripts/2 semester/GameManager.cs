using UnityEngine;
using Fusion;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    [Header("Scene References")]
    public Transform hemisphere;
    public NetworkPrefabRef holePrefab;
    public NetworkPrefabRef plugPrefab;
    public Transform plugSpawnArea;

    [Header("UI Water Timer")]
    public Image waterFillUI;
    public float gameDuration = 60f;
    public float totalTime = 0f;

    [Header("Gameplay")]
    public int holesToSpawn = 5;

    public static GameManager Instance { get; private set; }
    [Networked] private TickTimer gameTimer { get; set; }
    [Networked] private int closedHolesCount { get; set; }
    private float baseRadius;


    private void Awake()
    {
        Instance = this;
    }

    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;

        hemisphere = GameObject.FindWithTag("Hemisphere").transform;
        plugSpawnArea = GameObject.FindWithTag("PlugSpawn").transform;
        waterFillUI = GameObject.FindWithTag("WaterUI").GetComponent<Image>();

        gameTimer = TickTimer.CreateFromSeconds(Runner, gameDuration);

        LoadHemisphereRadius();
        SpawnHoles();
        SpawnPlugs();

    }

    private void Update()
    {
        if (gameTimer.Expired(Runner))
            return;

        float remaining = gameTimer.RemainingTime(Runner) ?? 0f;
        waterFillUI.fillAmount = 1f - (remaining / gameDuration);
        if (closedHolesCount < holesToSpawn && !gameTimer.Expired(Runner)) totalTime += Time.deltaTime;

        if (Object.HasStateAuthority && gameTimer.Expired(Runner))
        {
            LoseGame();
        }
    }

    // Вызывается дыркой, когда она закрыта игроком
    public void RegisterHoleClosed()
    {
        if (!Object.HasStateAuthority) return;

        closedHolesCount++;

        if (closedHolesCount >= holesToSpawn)
        {
            WinGame();
        }
    }

    // === Генерация дыр ===
    private void LoadHemisphereRadius()
    {
        MeshFilter mf = hemisphere.GetComponent<MeshFilter>();
        baseRadius = mf.sharedMesh.bounds.extents.x;
    }

    private void SpawnHoles()
    {
        for (int i = 0; i < holesToSpawn; i++)
        {
            Vector3 pos;
            Quaternion rot;
            GeneratePointOnInsideSurface(out pos, out rot);
            Runner.Spawn(holePrefab, pos, rot, Object.InputAuthority);
        }
    }

    // === Генерация затычек ===
    private void SpawnPlugs()
    {
        for (int i = 0; i < holesToSpawn; i++)
        {
            Vector3 spawnPos = plugSpawnArea.transform.position + Random.insideUnitSphere * 0.7f;

            Runner.Spawn(plugPrefab, spawnPos, Quaternion.identity, Object.InputAuthority,
                (runner, obj) =>
                {
                    var rb = obj.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.isKinematic = true;
                    }
                }
            );

        }
    }

    // === Геометрия спавна дыр ===
    private void GeneratePointOnInsideSurface(out Vector3 worldPos, out Quaternion worldRot)
    {
        // Берём точку ВНУТРИ полусферы
        Bounds b = hemisphere.GetComponent<Renderer>().bounds;
        baseRadius = b.size.x / 2f; //радиус полусферы

        for (int i = 0; i < 20; i++)
        {
            /*Vector3 randomPoint = new Vector3(
                Random.Range(b.min.x, b.max.x),
                Random.Range(b.center.y, b.max.y),
                Random.Range(b.min.z, b.max.z)
            );*/
            float u = Random.value;
            float v = Random.value;

            float theta = u * 2 * Mathf.PI;
            float phi = v * Mathf.PI / 2;

            float x = baseRadius * Mathf.Cos(theta) * Mathf.Sin(phi);
            float y = baseRadius * Mathf.Cos(phi);
            float z = baseRadius * Mathf.Sin(theta) * Mathf.Sin(phi);
            Vector3 randomPoint = new Vector3(x, y, z);
            Vector3 localPos = new Vector3(x, y, z);
            worldPos = hemisphere.TransformPoint(localPos);
            Vector3 normal = (worldPos - hemisphere.position).normalized;
            worldRot = Quaternion.LookRotation(normal, Vector3.up);

            /*Vector3 dir = (randomPoint - b.center).normalized;

            if (Physics.Raycast(b.center, dir, out RaycastHit hit, b.extents.magnitude))
            {
                if (hit.transform == hemisphere)
                {
                    worldPos = hit.point;
                    worldRot = Quaternion.LookRotation(-hit.normal);
                    return;
                }
            }*/
        }

        // Фолбэк (чтобы не крашилось)
        worldPos = hemisphere.position;
        worldRot = Quaternion.identity;
    }

    // === Завершение игры ===
    private void WinGame()
    {
        Debug.Log($"WIN! Все дыры закрыты. Время прохождения - {totalTime}");
        Runner.Shutdown();
    }

    private void LoseGame()
    {
        Debug.Log("LOSE! Вода поднялась.");
        Runner.Shutdown();
    }
}
