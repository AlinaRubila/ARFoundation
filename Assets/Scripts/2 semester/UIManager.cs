using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public GameObject panel;
    public Text textField;
    public GameManager gm;
    private void Awake()
    {
        Instance = this;
    }
    public void ShowWindow(string message)
    {
        textField.text = message;
        panel.SetActive(true);
    }
    public void Exit()
    {
        gm = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
        gm.Exit();
    }
}
