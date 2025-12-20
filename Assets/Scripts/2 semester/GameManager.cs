using Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
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
    List<Vector3> spawnedHoles = new List<Vector3>();

    public static InputAction positionAction;
    public static InputAction pressAction;
    public InputActionAsset inputActionsAsset;

    private void Awake()
    {
        Instance = this;
        pressAction = inputActionsAsset.FindAction("Gameplay/Press");
        positionAction = inputActionsAsset.FindAction("Gameplay/PointerPosition");

        // добавляем тач, если нужно
        pressAction.AddBinding("<Touchscreen>/primaryTouch/press");
        positionAction.AddBinding("<Touchscreen>/touch*/position");
        pressAction.Enable();
        positionAction.Enable();
    }

    public override void Spawned()
    {
       
        waterFillUI = GameObject.FindWithTag("WaterUI").GetComponent<Image>();
        if (!Object.HasStateAuthority) return;

        hemisphere = GameObject.FindWithTag("Hemisphere").transform;
        plugSpawnArea = GameObject.FindWithTag("PlugSpawn").transform;

        gameTimer = TickTimer.CreateFromSeconds(Runner, gameDuration);

        LoadHemisphereRadius();
        SpawnHoles();
        SpawnPlugs();

    }

    private void Update()
    {
        if (!gameTimer.IsRunning)
            return;
        /*if (gameTimer.Expired(Runner))
            return;*/

        float remaining = gameTimer.RemainingTime(Runner) ?? 0f;
        waterFillUI.fillAmount = Mathf.Clamp01(1f - remaining / gameDuration); 
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
            spawnedHoles.Add(pos);
        }
    }

    // === Генерация затычек ===
    private void SpawnPlugs()
    {
        MeshFilter mf = hemisphere.GetComponent<MeshFilter>();
        baseRadius = mf.sharedMesh.bounds.extents.x;
        for (int i = 0; i < holesToSpawn; i++)
        {
            Vector3 spawnPos = plugSpawnArea.transform.position + Random.insideUnitSphere * 5f * baseRadius;
            Vector3 pos = new Vector3(spawnPos.x, 0f, spawnPos.z);
            Runner.Spawn(plugPrefab, pos, Quaternion.identity, Object.InputAuthority,
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
        MeshFilter mf = hemisphere.GetComponent<MeshFilter>();
        baseRadius = mf.sharedMesh.bounds.extents.x;

        for (int i = 0; i < 20; i++)
        {
            float u = Random.value;
            float v = Random.value;

            float theta = u * 2 * Mathf.PI;
            float minPhi = 0.4f / baseRadius;
            float phi = Random.Range(minPhi, Mathf.PI / 2f - minPhi);
            //float phi = v * Mathf.PI / 2;

            float x = baseRadius * Mathf.Cos(theta) * Mathf.Sin(phi);
            float y = baseRadius * Mathf.Cos(phi);
            float z = baseRadius * Mathf.Sin(theta) * Mathf.Sin(phi);
            Vector3 randomPoint = new Vector3(x, y, z);
            Vector3 localPos = new Vector3(x, y, z);
            worldPos = hemisphere.TransformPoint(localPos);
            Vector3 normal = (worldPos - hemisphere.position).normalized;
            worldPos -= normal * 0.5f;
            bool tooClose = false;
            foreach (var p in spawnedHoles)
            {
                if (Vector3.Distance(worldPos, p) < 2f)
                {
                    tooClose = true;
                    break;
                }
            }
            if (tooClose)
                continue;
            worldRot = Quaternion.LookRotation(-normal, Vector3.up);
            return;

        }

        // Фолбэк
        worldPos = hemisphere.position;
        worldRot = Quaternion.identity;
    }

    private void WinGame()
    {
        waterFillUI.fillAmount = 0;
        Debug.Log($"WIN! Все дыры закрыты. Время прохождения - {totalTime}");
        Runner.Shutdown();
    }

    private void LoseGame()
    {
        Debug.Log("LOSE! Вода поднялась.");
        Runner.Shutdown();
    }
}
