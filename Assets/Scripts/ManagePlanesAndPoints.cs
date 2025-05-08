using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ManagePlanesAndPoints : MonoBehaviour
{
    public ARPlaneManager planeManager;
    public ARPointCloudManager pointCloudManager;
    public ARRaycastManager raycastManager;
    bool turning = true;

    public void ManagersTurning()
    {
        turning = !turning;
        if (turning)
        {
            planeManager.enabled = true;
            pointCloudManager.enabled = true;
            raycastManager.enabled = true;
        }
        else
        {
            planeManager.enabled = false;
            pointCloudManager.enabled = false;
            raycastManager.enabled = false;
        }
    }
    void Update()
    {
        
    }
}
