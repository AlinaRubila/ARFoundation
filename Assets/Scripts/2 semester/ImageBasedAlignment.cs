using UnityEngine;
using Fusion;
using UnityEngine.XR.ARFoundation;

public class ImageBasedAlignment : MonoBehaviour
{
    public ARTrackedImageManager imageManager;
    public Transform arOrigin;    // XR Origin
    private NetworkRunner runner;
    private bool aligned = false;

    private void Start()
    {
        runner = FindObjectOfType<NetworkRunner>();
        imageManager.trackedImagesChanged += OnChanged;
    }

    private void OnDestroy()
    {
        imageManager.trackedImagesChanged -= OnChanged;
    }

    private void OnChanged(ARTrackedImagesChangedEventArgs args)
    {
        if (aligned) return;

        // клиент выравнивается
        if (!runner.IsSharedModeMasterClient && ImageTracking.anchorDefined)
        {
            foreach (var img in args.added)
                Align(img);

            foreach (var img in args.updated)
                Align(img);
        }
    }

    private void Align(ARTrackedImage img)
    {
        if (AlignedWith(img)) return;

        // 1. Где картинка у клиента?
        Vector3 clientImagePos = img.transform.position;
        Quaternion clientImageRot = img.transform.rotation;

        // 2. Где она должна быть? (по хосту)
        //Vector3 hostImagePos = ImageTracking.sharedImageTransform.position;
        //Quaternion hostImageRot = ImageTracking.sharedImageTransform.rotation;
        Vector3 hostImagePos = ImageTracking.sharedImagePos;
        Quaternion hostImageRot = ImageTracking.sharedImageRot;

        // 3. Вычисляем смещение
        Vector3 deltaPos = hostImagePos - clientImagePos;

        // 4. Поворачиваем AR Origin так, чтобы клиентская картинка смотрела как у хоста
        Quaternion deltaRot = hostImageRot * Quaternion.Inverse(clientImageRot);

        // 5. Применяем
        arOrigin.position += deltaPos;
        arOrigin.rotation = deltaRot * arOrigin.rotation;

        aligned = true;

        Debug.Log("AR world aligned to host");

        FixNetworkObjectsAfterAlignment();
    }

    private bool AlignedWith(ARTrackedImage img)
    {
        return aligned;
    }

    private void FixNetworkObjectsAfterAlignment()
    {
        if (runner == null) return;

        foreach (var obj in runner.GetAllNetworkObjects())
        {
            var tr = obj.transform;
            tr.position = tr.position;   // пересбор позиции
            tr.rotation = tr.rotation;
        }

        Debug.Log("Network objects repositioned after alignment");
    }

}

