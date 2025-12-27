using Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
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
    float totalTime = 0f;

    [Header("Gameplay")]
    public int holesToSpawn = 5;
    public static GameManager Instance { get; private set; }
    [Networked] private TickTimer gameTimer { get; set; }
    [Networked] private int closedHolesCount { get; set; }
    [Networked] private bool isOver { get; set; }
    private bool uiShown = false;
    enum GameResult {None, Win, Lose}
    [Networked] private GameResult gameResult { get; set; }
    private float baseRadius;
    [Networked, Capacity(5)]
    //List<Vector3> spawnedHoles => default;
    NetworkArray<Vector3> spawnedHoles => default;
    public UIManager ui;
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

        // добавляем тач, если нужно
        pressAction.AddBinding("<Touchscreen>/primaryTouch/press");
        positionAction.AddBinding("<Touchscreen>/touch*/position");
        pressAction.Enable();
        positionAction.Enable();
    }

    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        waterFillUI = GameObject.FindWithTag("WaterUI").GetComponent<Image>();
        hemisphere = GameObject.FindWithTag("Hemisphere").transform;
        plugSpawnArea = GameObject.FindWithTag("PlugSpawn").transform;
        ui = GameObject.FindWithTag("UIManager").GetComponent<UIManager>();
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
        if (isOver && !uiShown)
        {
            switch (gameResult)
            {
                case GameResult.Win:
                    WinGame();
                    break;
                case GameResult.Lose:
                    LoseGame(); 
                    break;
            }
            uiShown = true;
        }
        if (!gameTimer.IsRunning || isOver)
            return;

        float remaining = gameTimer.RemainingTime(Runner) ?? 0f;
        waterFillUI.fillAmount = Mathf.Clamp01(1f - remaining / gameDuration); 
        if (closedHolesCount < holesToSpawn && !gameTimer.Expired(Runner)) totalTime += Time.deltaTime;

        if (Object.HasStateAuthority && gameTimer.Expired(Runner))
        {
            //LoseGame();
            isOver = true;
            gameResult = GameResult.Lose;
        }
    }

    // Вызывается дыркой, когда она закрыта игроком
    public void RegisterHoleClosed()
    {
        if (!Object.HasStateAuthority) return;

        closedHolesCount++;

        if (closedHolesCount >= holesToSpawn)
        {
            //WinGame();
            isOver = true;
            gameResult = GameResult.Win;
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
            Runner.Spawn(holePrefab, pos, rot, PlayerRef.None, (runner, obj) =>
            {
                obj.transform.SetParent(hemisphere.transform, true); 
            });
            //spawnedHoles.Add(pos);
            spawnedHoles.Set(i, pos);
        }
    }

    // === Генерация затычек ===
    private void SpawnPlugs()
    {
        MeshFilter mf = hemisphere.GetComponent<MeshFilter>();
        baseRadius = mf.sharedMesh.bounds.extents.x;
        for (int i = 0; i < holesToSpawn; i++)
        {
            Vector3 randVector = new Vector3((float)rng.NextDouble(), 0f, (float)rng.NextDouble()).normalized;
            Vector3 spawnPos = plugSpawnArea.transform.position + randVector * 5f * baseRadius;
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

    private void GeneratePointOnInsideSurface(out Vector3 worldPos, out Quaternion worldRot)
    {
        MeshFilter mf = hemisphere.GetComponent<MeshFilter>();
        baseRadius = mf.sharedMesh.bounds.extents.x;

        for (int i = 0; i < 20; i++)
        {
            /*float u = Random.value;
            float v = Random.value;*/
            float u = (float)rng.NextDouble();
            float v = (float)rng.NextDouble();

            float theta = u * 2 * Mathf.PI;
            float minPhi = 0.4f / baseRadius;
            //float phi = Random.Range(minPhi, Mathf.PI / 2f - minPhi);
             float phi = Mathf.Lerp(minPhi, Mathf.PI / 2f - minPhi, v);
          
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
                if (spawnedHoles[i] == Vector3.zero)
                    continue;
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
        worldPos = hemisphere.position;
        worldRot = Quaternion.identity;
    }

    private void WinGame()
    {
        waterFillUI.fillAmount = 0;
        Debug.Log($"WIN! Все дыры закрыты. Время прохождения - {totalTime}");
        ui.ShowWindow("You won!");
        isOver = true;
    }

    private void LoseGame()
    {
        Debug.Log("LOSE! Вода поднялась.");
        ui.ShowWindow("You lost!");
        isOver = true;
    }
    public void Exit()
    {
        Runner.Shutdown();
        SceneManager.LoadScene("StartMultiplayer");
    }
}
