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

    Transform breadSlot;
    float breadHeight = 0.2f;
    float currentTopHeight = 0f;

    Vector3 destination;

    enum State
    {
        Idle,
        Move,
        StackIdle,
        StackMove,
    }
    State state;

    // Start is called before the first frame update
    void Start()
    {
        state = State.Idle;

        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        breadSlot = transform.Find("BreadStackPoint");

        //오븐 근처에 왔는지 전달받도록
        EventManager.OnPlayerNearOven += nearOvenStatus;
        //빵을 주는지 알도록
        EventManager.OnPlayerReceiveBreads += ReceiveBreads;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAnimation();
    }

    void UpdateAnimation()
    {
        switch (state)
        {
            case State.Idle:
                anim.SetInteger("State", 0);
                break;
            case State.Move:
                anim.SetInteger("State", 1);
                break;
            case State.StackIdle:
                anim.SetInteger("State", 2);
                break;
            case State.StackMove:
                anim.SetInteger("State", 3);
                break;
        }
    }

    public void SetMoving(bool isMoving)
    {
        state = isMoving ? State.Move : State.Idle;
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
        Debug.Log(remainingSpace);
        if (remainingSpace <= 0)
        {
            Debug.Log("자리 없어");
            return;
        }

        //int breadsToReceive = 0;
        //if (receivedBreads.Count <= remainingSpace)
        //{
        //    breadsToReceive = receivedBreads.Count;
        //}
        //else
        //{
        //    breadsToReceive = remainingSpace;
        //}

        foreach (GameObject bread in receivedBreads)
        {
            bread.transform.parent = breadSlot;

            bread.transform.localPosition = new Vector3(0, breadHeight * breadSlot.childCount, 0);
            bread.transform.localEulerAngles = Vector3.zero;

            breads.Add(bread);
        }
  
        hasRequestedBread = false;
    }

}
