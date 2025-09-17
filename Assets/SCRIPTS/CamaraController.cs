using UnityEngine;

public class CamaraController : MonoBehaviour
{
    public Transform target;

    private void LateUpdate()
    {
        if (target == null) return;
        transform.SetPositionAndRotation(target.position, target.parent.rotation);
    }
    //ESTO TENDREMOS QUE HACERLO EN EL OnNetworkSpawn de cada player si es el Owner
    public void CrearTarget(Transform tra)
    {
        target = tra;
        transform.SetPositionAndRotation(target.position, Quaternion.Euler(0f, 0f, 0f));
    }
}
