using UnityEngine;
using UnityEngine.Rendering;

public class CameraManager : MonoBehaviour
{

    [SerializeField] Camera[] arrayCamerasPlayers;

    [SerializeField] Camera camaraTrampero;

    [SerializeField] Camera camaraZoom;

   public static CameraManager Instance;
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }


    public Camera EscogerCamara(int pos)
    {
        return arrayCamerasPlayers[pos];
    }

    public void ResetCamaraValues(int pos)
    {
        arrayCamerasPlayers[pos].transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(0, 0, 0));
    }


}
