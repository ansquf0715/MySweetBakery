using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum CustomerState
{
    Idle,
    GetBread,
    CheckOut,
    LeaveStore,
    RequestSeat,
};

public class Customer : MonoBehaviour
{
    CustomerState currentState;
    CustomerManager manager;

    NavMeshAgent agent;
    Animator animator;

    //public Transform breadStand;

    public List<Transform> customerPositions = new List<Transform>();

    float moveSpeed = 2f;

    bool stateChanged = false;

    // Start is called before the first frame update
    void Start()
    {
        manager = FindObjectOfType<CustomerManager>();

        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        animator = GetComponent<Animator>();

        currentState = CustomerState.Idle;
        StartCoroutine(ChangeToGetBread());
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Current State" + currentState);
        if(stateChanged)
        {
            checkState();
            stateChanged = false;
        }

        UpdateAnimation();
    }

    IEnumerator ChangeToGetBread()
    {
        yield return new WaitForSeconds(1f);
        changeState(CustomerState.GetBread);
    }

    void changeState(CustomerState newState)
    {
        if(currentState != newState)
        {
            currentState = newState;
            stateChanged = true;
        }
    }

    void checkState()
    {
        switch(currentState)
        {
            case CustomerState.Idle:
                break;
            case CustomerState.GetBread:
                MoveToBreadStand();
                break;
            case CustomerState.CheckOut:
                break;
            case CustomerState.LeaveStore:
                break;
            case CustomerState.RequestSeat:
                break;
        }
    }

    void MoveToBreadStand()
    {
        currentState = CustomerState.GetBread;
        Transform assignedPos = manager.AssignBreadPositionToCustomer();
        if(assignedPos != null)
        {
            agent.SetDestination(assignedPos.position);
        }
        else
        {
            Debug.Log("갈자리없음");
        }
    }

    void UpdateAnimation()
    {
        if(agent.velocity.magnitude > 0.1f)
        {
            animator.SetBool("isMoving", true);
        }
        else
        {
            animator.SetBool("isMoving", false);
        }
    }
}
