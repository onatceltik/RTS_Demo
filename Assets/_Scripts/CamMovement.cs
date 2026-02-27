using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CamMovement : MonoBehaviour
{
    float speed = 35f;
    Rigidbody2D ghostPlayerRBody;

    // Start is called before the first frame update
    void Awake()
    {
        transform.position = new Vector3(0, 0, 0);
        ghostPlayerRBody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
     
        Vector2 destination = ghostPlayerRBody.position + new Vector2(horizontal, vertical) * speed * Time.deltaTime;
        ghostPlayerRBody.MovePosition(destination);
    }
    // void FixedUpdate()
    // {
    //     // ghostPlayerRBody.MovePosition(ghostPlayerRBody.position + new Vector3(horizontal, vertical, 0) * speed * Time.deltaTime);
    //     // transform.position = transform.position + new Vector3(horizontal, vertical, 0) * speed * Time.deltaTime;
    // }
}
