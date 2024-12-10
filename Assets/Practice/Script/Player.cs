using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    Rigidbody rb;
    Animator anim;
    Vector3 m_Movement;
    public float moveSpeed = 5f;
    public float turnSpeed = 5f;

    // Start is called before the first frame update
    void Start()
    {
        //JoyStick joystick = FindObjectOfType<JoyStick>();
        //if (joystick != null)
        //    joystick.onDirectionChanged += SetDirection;

        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        move();
    }

    void move()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        m_Movement = new Vector3(horizontal, 0, vertical).normalized;

        transform.position += m_Movement * moveSpeed * Time.deltaTime;

        if(m_Movement != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(m_Movement, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, turnSpeed * Time.deltaTime);
        }

        anim.SetBool("isMoving", m_Movement !=  Vector3.zero);
    }

}
