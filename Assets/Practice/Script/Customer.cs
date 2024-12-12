using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//public enum CustomerState
//{
//    CheckOut,
//    LeaveStore,
//    RequestSeat,
//};

public class Customer : MonoBehaviour
{
    public NavMeshAgent agent {  get; private set; }
    public Animator animator { get; private set; }

    public ICustomerState currentState;
    public CustomerManager manager { get; private set; }
    //public EventManager eventManager { get; private set; }

    public IdleState idleState;
    public GetBreadState getBreadState;
    public CheckOutState checkOutState;
    public LeaveStoreState leaveStoreState;
    public RequestSeatState requestSeatState;

    bool stateChanged = false;

    int currentBreadCount = 0;
    public List<GameObject> breads = new List<GameObject>();

    GameObject bag;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        manager = FindObjectOfType<CustomerManager>();
        //eventManager = FindObjectOfType<EventManager>();

        idleState = new IdleState(this);
        getBreadState = new GetBreadState(this);
        checkOutState = new CheckOutState(this);
        leaveStoreState = new LeaveStoreState(this);
        requestSeatState = new RequestSeatState();

        ChangeState(idleState);

        bag = null;
    }

    // Update is called once per frame
    void Update()
    {
        if(stateChanged)
        {
            currentState.Execute();
            stateChanged = false;
        }
        UpdateAnimation();
    }

    public void ChangeState(ICustomerState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
        stateChanged = true;
    }

    void UpdateAnimation()
    {
        bool isMoving = agent.velocity.magnitude > 0.1f;
        if(breads.Count > 0 || bag != null)
        {
            if (isMoving)
                animator.SetInteger("State", 3);
            else
                animator.SetInteger("State", 2);
        }
        else
        {
            if (isMoving)
                animator.SetInteger("State", 1);
            else
                animator.SetInteger("State", 0);
        }
    }

    public void GetBag(GameObject bag)
    {
        if(currentState == checkOutState)
        {
            checkOutState.GetBag(bag);
            this.bag = bag;
        }
    }

    public void SetBreads(List<GameObject> receivedBreads)
    {
        breads.AddRange(receivedBreads);
        currentBreadCount += receivedBreads.Count;
    }

    public List<GameObject> GetBreads()
    {
        return breads;
    }

    public void RemoveBread(GameObject bread)
    {
        if(breads.Contains(bread))
        {
            breads.Remove(bread);
            currentBreadCount--;
        }
    }

    public Vector3 GetBreadStackPoint()
    {
        return transform.Find("BreadStackPoint").position;
    }
}
