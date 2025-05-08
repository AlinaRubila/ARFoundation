using UnityEngine;
using UnityEngine.XR.ARCore;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Android;
using UnityEngine.SceneManagement;

public class HandleScenes : MonoBehaviour
{
    bool arSupport = false;
    bool cameraPermission = false;
    public void Load3D()
    {
        Debug.Log("Loaded 3D scene");
        SceneManager.LoadSceneAsync("ReplacingScene", LoadSceneMode.Additive);
    }
    public void LoadAR()
    {
        CheckARSupport();
        Debug.Log("Tried to check");
        CheckCameraPermission();
        Debug.Log("Tried to get a permission");
        if (arSupport && cameraPermission)
        {
            Debug.Log("AR scene is on");
            SceneManager.LoadSceneAsync("SampleScene", LoadSceneMode.Additive);
        }
        else SceneManager.LoadSceneAsync("ReplacingScene", LoadSceneMode.Additive);
    }
    void CheckARSupport()
    {
        if (ARSession.state == ARSessionState.Unsupported || !IsARCoreSupported())
        {
            Debug.Log("AR is not supported");
            arSupport = false;
        }
        else 
        {
            Debug.Log("AR is supported");
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
            Debug.Log("Getting camera permission");
            Permission.RequestUserPermission(Permission.Camera);
        }
        else
        {
            Debug.Log("Camera is on");
            cameraPermission = true;
        }
        if (!cameraPermission)
        {
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Debug.Log("Can't get a camera permission");
                cameraPermission = false;
            }
            else 
            {
                Debug.Log("Camera is on");
                cameraPermission = true;
            }
        }
    }
}
