using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARCore;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Android;
using UnityEngine.UI;

public class SceneLoad : MonoBehaviour
{
    bool arSupport = false;
    bool cameraPermission = false;
    [SerializeField] GameObject message;
    public void CreateGame()
    {
        CheckARSupport();
        CheckCameraPermission();
        if (arSupport && cameraPermission)
        {
            SceneManager.LoadSceneAsync("BaseScene");
        }
        else message.SetActive(true);
    }
    public void JoinGame(InputField field)
    {
        CheckARSupport();
        CheckCameraPermission();
        if (arSupport && cameraPermission)
        {
            SceneManager.LoadSceneAsync("BaseScene");
        }
        else message.SetActive(true);
    }
    void CheckARSupport()
    {
        if (ARSession.state == ARSessionState.Unsupported || !IsARCoreSupported())
        {
            arSupport = false;
        }
        else
        {
            arSupport = true;
        }
    }
    private bool IsARCoreSupported()
    {
        var loader = (ARCoreLoader)UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager.activeLoader;
        return loader != null;
    }
    void CheckCameraPermission()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            cameraPermission = false;
            Permission.RequestUserPermission(Permission.Camera);
        }
        else
        {
            cameraPermission = true;
        }
        if (!cameraPermission)
        {
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                cameraPermission = false;
            }
            else
            {
                cameraPermission = true;
            }
        }
    }
}
