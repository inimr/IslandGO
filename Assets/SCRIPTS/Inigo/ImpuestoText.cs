using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpuestoText : MonoBehaviour
{
    Camera mainCam;
    private void Awake()
    {
        mainCam = FindAnyObjectByType<Camera>();
    }

    private void Start()
    {
        Destroy(gameObject, 2);
    }

    private void LateUpdate()
    {
        transform.position += Vector3.up * Time.deltaTime;
        gameObject.transform.LookAt(mainCam.transform, Vector3.up);

    }
}
