using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System.Diagnostics.Contracts;
using UnityEngine.UI;

public class Customer : MonoBehaviour
{
    public NavMeshAgent agent {  get; private set; }
    public Animator animator { get; private set; }

    public ICustomerState currentState;
    public CustomerManager manager { get; private set; }

    bool sitting = false;
    public bool eating = false;
    public bool willRequestSeat = false;

    public IdleState idleState;
    public GetBreadState getBreadState;
    public CheckOutState checkOutState;
    public LeaveStoreState leaveStoreState;
    public RequestSeatState requestSeatState;

    bool stateChanged = false;

    int currentBreadCount = 0;
    public List<GameObject> breads = new List<GameObject>();
    public int requestBreadCount = 0;

    GameObject bag;

    Camera cam;
    public Canvas canvas;
    public Image orderObj;
    public TMP_Text orderCount;


    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        manager = FindObjectOfType<CustomerManager>();

        idleState = new IdleState(this);
        getBreadState = new GetBreadState(this);
        checkOutState = new CheckOutState(this);
        leaveStoreState = new LeaveStoreState(this);
        requestSeatState = new RequestSeatState(this);

        ChangeState(idleState);

        bag = null;

        cam = Camera.main;
        canvas = GetComponentInChildren<Canvas>();

        orderObj = canvas.transform.Find("OrderObj").GetComponent<Image>();  
        orderCount = canvas.transform.Find("OrderCount").gameObject.GetComponent<TMP_Text>();
        canvas.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(stateChanged)
        {
            currentState.Execute();
        }
        UpdateAnimation();

        if(canvas.gameObject.activeSelf)
        {
            canvas.transform.LookAt(
                canvas.transform.position + cam.transform.rotation * Vector3.forward,
                cam.transform.rotation * Vector3.up);

            canvas.transform.Rotate(0, 180f, 0);
        }
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
        if (eating)
        {
            animator.SetInteger("State", 4);
        }
        else
        {
            bool isMoving = agent.velocity.magnitude > 0.1f;
            if (breads.Count > 0 || bag != null)
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
    }

    public void UpdateDestination(Vector3 newPos)
    {
        if (currentState == checkOutState)
        {
            checkOutState.UpdateDestination(newPos);
        }
    }

    public void seatUpdateDestination(Vector3 newPos)
    {
        if(currentState == requestSeatState)
        {
            requestSeatState.UpdateDestination(newPos);
        }
    }

    public bool isCashingState()
    {
        if(currentState == checkOutState)
            return true;
        return false;
    }

    public void GetBag(GameObject bag)
    {
        if (currentState == checkOutState)
        {
            checkOutState.GetBag(bag);
            this.bag = bag;
        }
    }

    public void SetBread(GameObject bread)
    {
        breads.Add(bread);
        currentBreadCount++;
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

    public bool GetSitting()
    {
        return sitting;
    }

    public void SetSitting(bool isSitting)
    {
        sitting = isSitting;
    }

    public void destroyBreads()
    {
        for(int i=breads.Count-1; i>=0; i--)
        {
            GameObject bread = breads[i];
            breads.RemoveAt(i);
            //Destroy(bread);
            EventManager.OnReturnBreads(bread);
        }
        breads.Clear();
    }

    public void DestroySelf()
    {
        Destroy(this.gameObject);
    }
}
