using Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    [Header("Scene References")]
    public NetworkPrefabRef holePrefab;
    public NetworkPrefabRef plugPrefab;
    UIManager uiManager;
    [SerializeField] private Transform hemisphere;
    [SerializeField] private Transform plugSpawnArea;

    [Header("UI Water Timer")]
    public float gameDuration = 60f;
    float totalTime = 0f;
    [Networked] public float WaterProgress { get; set; }

    [Header("Gameplay")]
    public int holesToSpawn = 5;
    public static GameManager Instance { get; private set; }
    [Networked] private TickTimer gameTimer { get; set; }
    [Networked] private int closedHolesCount { get; set; }
    [Networked] public bool isOver { get; set; }
    public enum GameResult {None, Win, Lose}
    [Networked] public GameResult gameResult { get; set; }
    private float baseRadius;
    [Networked, Capacity(5)]
    NetworkArray<Vector3> spawnedHoles => default;
    [Networked] private int HoleSeed { get; set; }
    [Networked] bool isSpawned { get; set; }
    System.Random rng { get; set; }
    [Header("Input Assets")]
    public static InputAction positionAction;
    public static InputAction pressAction;
    public InputActionAsset inputActionsAsset;
    private void Awake()
    {
        Instance = this;
        pressAction = inputActionsAsset.FindAction("Gameplay/Press");
        positionAction = inputActionsAsset.FindAction("Gameplay/PointerPosition");

        //pressAction.AddBinding("<Touchscreen>/primaryTouch/press");
        //positionAction.AddBinding("<Touchscreen>/touch*/position");
        pressAction.Enable();
        positionAction.Enable(); //как сказал GPT, момент с вводом может быть проблемным - типа он включается у всех. Надо проверить
    }
    public override void Spawned()
    {
        uiManager = GameObject.FindWithTag("UIManager")?.GetComponent<UIManager>();
        if (uiManager != null) uiManager.GetManager();
        if (!Object.HasStateAuthority) return;
        gameTimer = TickTimer.CreateFromSeconds(Runner, gameDuration);
        if (!Runner.IsSharedModeMasterClient || isSpawned)
            return;
        HoleSeed = Runner.Tick;
        rng = new System.Random(HoleSeed);
        isSpawned = true;
        LoadHemisphereRadius();
        SpawnHoles();
        SpawnPlugs();
    }
    private void Update()
    {
        if (!gameTimer.IsRunning || isOver)
            return;

        float remaining = gameTimer.RemainingTime(Runner) ?? 0f;
        if (Object.HasStateAuthority) 
        WaterProgress = Mathf.Clamp01(1f - remaining / gameDuration);
        if (closedHolesCount < holesToSpawn && !gameTimer.Expired(Runner)) totalTime += Time.deltaTime;

        if (Object.HasStateAuthority && gameTimer.Expired(Runner))
        {
            LoseGame();
        }
    }

    public void RegisterHoleClosed()
    {
        if (!Object.HasStateAuthority) return;

        closedHolesCount++;

        if (closedHolesCount >= holesToSpawn)
        {
            WinGame();
        }
    }

    private void LoadHemisphereRadius()
    {
        MeshFilter mf = hemisphere.GetComponent<MeshFilter>();
        baseRadius = mf.sharedMesh.bounds.extents.x;
    }

    private void SpawnHoles()
    {
        List<Vector3> localPositions = new List<Vector3>();
        for (int i = 0; i < holesToSpawn; i++)
            {
                Vector3 pos;
                Quaternion rot;
                GeneratePointOnInsideSurface(localPositions, out pos, out rot);
                Runner.Spawn(holePrefab, hemisphere.TransformPoint(pos), hemisphere.rotation * rot, Object.InputAuthority,
                    (runner, obj) =>
                    {
                        obj.transform.SetParent(hemisphere, true);
                    }
                    );
            localPositions.Add(pos);
            spawnedHoles.Set(i, pos);
            }
    }

    private void SpawnPlugs()
    {
        MeshFilter mf = hemisphere.GetComponent<MeshFilter>();
        baseRadius = mf.sharedMesh.bounds.extents.x;
        for (int i = 0; i < holesToSpawn; i++)
        {
            Vector3 randVector = new Vector3((float)rng.NextDouble(), 0f, (float)rng.NextDouble()).normalized;
            Vector3 spawnPos = plugSpawnArea.transform.position + randVector * 2f * baseRadius;
            Vector3 pos = new Vector3(spawnPos.x, 0f, spawnPos.z);
            Runner.Spawn(plugPrefab, pos, Quaternion.identity, Object.InputAuthority,
                (runner, obj) =>
                {
                    var rb = obj.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.isKinematic = true;
                    }
                    obj.transform.SetParent(hemisphere.transform, true);
                    obj.transform.localRotation = Quaternion.identity;
                }
            );
        }
    }

    private void GeneratePointOnInsideSurface(List<Vector3> existingPositions, out Vector3 worldPos, out Quaternion worldRot)
    {
        MeshFilter mf = hemisphere.GetComponent<MeshFilter>();
        baseRadius = mf.sharedMesh.bounds.extents.x;

        for (int i = 0; i < 40; i++)
        {
            float u = (float)rng.NextDouble();
            float v = (float)rng.NextDouble();

            float theta = u * 2 * Mathf.PI;
            float minPhi = 0.4f / baseRadius;
             float phi = Mathf.Lerp(minPhi, Mathf.PI / 2f - minPhi, v);
          
            float x = baseRadius * Mathf.Cos(theta) * Mathf.Sin(phi);
            float y = baseRadius * Mathf.Cos(phi);
            float z = baseRadius * Mathf.Sin(theta) * Mathf.Sin(phi);
            Vector3 localPos = new Vector3(x, y, z);
            worldPos = hemisphere.TransformPoint(localPos);
            Vector3 normal = (worldPos - hemisphere.position).normalized;
            worldPos -= normal * 0.5f;
            bool tooClose = false;
            foreach (var p in existingPositions)
            {
                /*if (p == Vector3.zero)
                    continue;*/
                if (Vector3.Distance(worldPos, p) < 0.4f)
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
        worldPos = hemisphere.position;
        worldRot = Quaternion.identity;
    }

    private void WinGame()
    {
        WaterProgress = 0;
        Debug.Log($"WIN! Все дыры закрыты. Время прохождения - {totalTime}");
        isOver = true;
        gameResult = GameResult.Win;
    }

    private void LoseGame()
    {
        Debug.Log("LOSE! Вода поднялась.");
        isOver = true;
        gameResult = GameResult.Lose;
    }
    public void Exit()
    {
        Runner.Shutdown();
        SceneManager.LoadScene("StartMultiplayer");
    }
}
