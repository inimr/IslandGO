using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PatataRemaster : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 180f;
    public Transform camaraTransform;
    private CharacterController controller;

    public CamaraController camara;
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Movimiento hacia adelante y atrás (W y S)
        float vertical = Input.GetAxis("Vertical");
        Vector3 move = transform.forward * vertical;

        // Aplicar movimiento
        controller.SimpleMove(move * moveSpeed);

        // Rotación izquierda/derecha (A y D)
        float horizontal = Input.GetAxis("Horizontal");
        transform.Rotate(0, horizontal * rotationSpeed * Time.deltaTime, 0);


        if (Input.GetKeyDown(KeyCode.Space))
        {
            camara.CrearTarget(camaraTransform);
        }
    }
}