using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public GameObject panel;
    public Text textField;
    public GameManager gm;
    public Image waterFillUI;
    bool isShown = false;
    private void Awake()
    {
        Instance = this;
    }
    public void GetManager()
    {
        gm = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
        Debug.Log($"{gm == null}");
    }
    private void Update()
    {
        if (gm == null) return;
        waterFillUI.fillAmount = gm.WaterProgress;
        if (gm.isOver && !isShown)
        {
            isShown = true;
            ShowWindow(gm.gameResult == GameManager.GameResult.Win ? "You won!" : "You lost!");
        }
    }
    public void ShowWindow(string message)
    {
        textField.text = message;
        panel.SetActive(true);
    }
    public void Exit()
    {
        gm.Exit();
    }
}
