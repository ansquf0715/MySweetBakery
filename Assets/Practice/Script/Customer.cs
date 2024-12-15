using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System.Diagnostics.Contracts;
using UnityEngine.UI;

//public enum CustomerState
//{
//    LeaveStore,
//    RequestSeat,
//};

public class Customer : MonoBehaviour
{
    public NavMeshAgent agent {  get; private set; }
    public Animator animator { get; private set; }

    public ICustomerState currentState;
    public CustomerManager manager { get; private set; }

    bool sitting = false;
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

    Sprite seatSprite;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        manager = FindObjectOfType<CustomerManager>();
        //eventManager = FindObjectOfType<EventManager>();
        //sitting = false;

        idleState = new IdleState(this);
        getBreadState = new GetBreadState(this);
        checkOutState = new CheckOutState(this);
        leaveStoreState = new LeaveStoreState(this);
        requestSeatState = new RequestSeatState(this);

        ChangeState(idleState);

        bag = null;

        //seatSprite = Resources.Load<Sprite>("TableChair");

        cam = Camera.main;
        canvas = GetComponentInChildren<Canvas>();

        orderObj = canvas.transform.Find("OrderObj").GetComponent<Image>();  
        orderObj.sprite = seatSprite;

        orderCount = canvas.transform.Find("OrderCount").gameObject.GetComponent<TMP_Text>();

        canvas.gameObject.SetActive(false);

        Debug.Log("will request seat" + willRequestSeat);
    }

    // Update is called once per frame
    void Update()
    {
        if(stateChanged)
        {
            currentState.Execute();
            //stateChanged = false;
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
        if (sitting)
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

    //public void HasReached(bool hasReached)
    //{
    //    //return hasReached;
    //    if (currentState == checkOutState)
    //        checkOutState.isAtCounter = hasReached;
    //}

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

    //public int GetBreadsCount()
    //{
    //    return breads.Count;
    //}

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

    //public void SetCustomerCheckOutEnd()
    //{
    //    checkOutState.checkOutEnded = true;
    //    //Debug.Log("CheckOutState. checkoutended" +  checkOutState.checkOutEnded);
    //}

    public void moveToFirstWaitingSeatPos(Vector3 targetPos)
    {
        Debug.Log("Customer Move to target pos");
        requestSeatState.moveToFirstPos(targetPos);
    }

    public void moveToWaitingPos(Vector3 targetPos)
    {
        requestSeatState.moveToWaitingPos(targetPos);
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
            Destroy(bread);
        }
        breads.Clear();
    }
}
