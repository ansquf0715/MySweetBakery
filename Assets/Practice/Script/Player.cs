using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    Rigidbody rb;
    Animator anim;
    Vector3 m_Movement;

    public float moveSpeed = 5f;
    public float turnSpeed = 5f;

    bool isNearOven = false;
    bool hasRequestedBread = false;

    int maxBreadCount = 8;
    int currentBreadCount = 0;
    public List<GameObject> breads = new List<GameObject>();

    Transform breadStackPoint;
    float breadHeight = 0.3f;
    float currentTopHeight = 0f;

    Vector3 destination;

    enum State
    {
        Idle,
        Move,
    }
    State state;

    // Start is called before the first frame update
    void Start()
    {
        //JoyStick joystick = FindObjectOfType<JoyStick>();
        //if (joystick != null)
        //    joystick.onDirectionChanged += SetDirection;
        state = State.Idle;

        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        breadStackPoint = transform.Find("BreadStackPoint");

        //오븐 근처에 왔는지 전달받도록
        EventManager.OnPlayerNearOven += nearOvenStatus;
        //빵을 주는지 알도록
        EventManager.OnPlayerReceiveBreads += ReceiveBreads;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            RaycastHit hit;
            int layerMask = 1 << LayerMask.NameToLayer("Floor");
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            {
                if(hit.collider.CompareTag("Floor"))
                {
                    Vector3 hitPoint = hit.point;
                    Debug.Log("hit pos" + hitPoint);

                    Vector3 targetPos = new Vector3(hit.point.x, 0.5f, hit.point.z);
                    SetDestination(targetPos);
                }
            }
            state = State.Move;
            Move();
        }
        else
        {
            state = State.Idle;
        }
        switch (state)
        {
            case State.Idle:
                anim.SetInteger("State", 0);
                break;
            case State.Move:
                anim.SetInteger("State", 1);
                break;
        }
    }

    void SetDestination(Vector3 dest)
    {
        destination = dest;
    }

    void Move()
    {
        Vector3 dir = destination - transform.position;
        if(dir.magnitude > 0.1f)
        {
            Quaternion toRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Lerp(transform.rotation, toRot, turnSpeed * Time.deltaTime);

            transform.position += dir.normalized * Time.deltaTime * moveSpeed;
        }
        else
        {
            destination = Vector3.zero;
        }
    }

    void nearOvenStatus(bool isNear)
    {
        isNearOven = isNear;

        if(isNearOven && !hasRequestedBread)
        {
            StartCoroutine(RequestBread());
        }
        else if(!isNearOven)
        {
            hasRequestedBread = false;
        }
    }

    IEnumerator RequestBread()
    {
        int requestBreadCount = maxBreadCount - currentBreadCount;
        if(requestBreadCount > 0)
        {
            EventManager.PlayerBreadRequest(requestBreadCount);
            hasRequestedBread = true;
        }

        yield return null;
    }

    void ReceiveBreads(List<GameObject> receivedBreads)
    {
        int remainingSpace = maxBreadCount - breads.Count;
        if (remainingSpace <= 0)
            return;

        int breadsToReceive = 0;
        if(receivedBreads.Count <= remainingSpace)
        {
            breadsToReceive = receivedBreads.Count;
        }
        else
        {
            breadsToReceive = remainingSpace;
        }

        if (breads.Count == 0)
            currentTopHeight = 0f;
        if(breads.Count > 0)
        {
            currentTopHeight = breads[breads.Count - 1].transform.localPosition.y + breadHeight;
        }

        foreach(GameObject bread in receivedBreads)
        {
            bread.transform.SetParent(breadStackPoint);

            Vector3 stackPos = new Vector3(0, currentTopHeight, 0);
            bread.transform.localPosition = stackPos;

            currentTopHeight += breadHeight;
            breads.Add(bread);
        }

        hasRequestedBread = false;
    }

}
