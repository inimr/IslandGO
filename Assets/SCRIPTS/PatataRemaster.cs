using UnityEngine;


public class PatataRemaster : MonoBehaviour
{

    [SerializeField]private float distanciaMinima = 0.25f;
    public LayerMask mask;

    private float maxRayDistance = 1f;
    private void LateUpdate()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, maxRayDistance, mask))
        
        {
          //  Debug.Log("El nombre del objeto golpeado por el rayo es " + hit.transform.gameObject.name);
            Debug.DrawRay(transform.position, new Vector3(0, -50,0), Color.red);
          

            float posY = hit.point.y + distanciaMinima;
            Vector3 nuevaPos = new(transform.position.x, posY, transform.position.z);

            transform.position = nuevaPos;

            //Debug.Log("La altura del objeto es de " + transform.position.y);
        }
    }
}