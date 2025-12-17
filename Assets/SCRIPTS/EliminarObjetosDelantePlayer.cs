using UnityEngine;

public class EliminarObjetosDelantePlayer : MonoBehaviour
{


    public float radius;
    public float maxDistance;
    public GameObject currentHitObject;

    private Vector3 origin;
    private Vector3 direction;
    
    private Jugador ownerPlayer;

    [SerializeField] LayerMask ignoreCamaraLayer;

 

    // QUEDA RARO Y DAN ERRORES, LO MEJOR, HACERLO MEDIANTE DITHER Y QUE EL JUGADOR SOBRESALGA
    // Update is called once per frame
    void Update()
    {

        if (ownerPlayer == null) return;

        origin = transform.position;

        direction = transform.forward;

        if(Physics.SphereCast(origin, radius, direction, out RaycastHit hit, maxDistance, ignoreCamaraLayer))
        {
            if(hit.transform.gameObject != null)
            {
                currentHitObject = hit.transform.gameObject;
                if (ownerPlayer.enMovimiento) return;

                currentHitObject.SetActive(false);
            }
        }
        else
        {
            if(currentHitObject != null)
            {
                if (!ownerPlayer.enMovimiento) return;
                currentHitObject.SetActive(true);
                currentHitObject = null;
            }
        }
    }
   
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Debug.DrawLine(origin, origin + direction * maxDistance);
        Gizmos.DrawWireSphere(origin + direction * maxDistance, radius);
    }


    public void SetOwnerPlayer(Jugador player)
    {
        ownerPlayer = player;
    }
}
