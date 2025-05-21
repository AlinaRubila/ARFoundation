using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ManagePlanesAndPoints : MonoBehaviour
{
    public ARPlaneManager planeManager;
    public ARPointCloudManager pointCloudManager;

    public void ManagersTurning(bool state)
    {
        if (state)
        {
            planeManager.enabled = true;
            pointCloudManager.enabled = true;
        }
        else
        {
            planeManager.enabled = false;
            foreach (var plane in planeManager.trackables)
            {
                plane.gameObject.SetActive(false);
            }
            pointCloudManager.enabled = false;
            foreach (var particle in pointCloudManager.trackables)
            {
                particle.gameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        
    }
}
