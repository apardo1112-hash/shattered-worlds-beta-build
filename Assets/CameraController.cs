using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Start is called before the first frame update
    public float sensitivity=0.5f;
    public Transform playerHead;
    private Vector3 PrevMouse;
    private float xrotation=0f;
    void Start()
    {
        PrevMouse=Input.mousePosition;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousedelta=Input.mousePosition-PrevMouse;
        transform.Rotate(Vector3.up, mousedelta.x*sensitivity, Space.World);
        xrotation=Mathf.Clamp(xrotation-mousedelta.y, -90f, 90f);
        transform.eulerAngles=new Vector3(xrotation, transform.eulerAngles.y, transform.eulerAngles.z);
        transform.position=playerHead.position;
        

        PrevMouse=Input.mousePosition;
    }
}
