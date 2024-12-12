using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public interface ICustomerState
{
    void Enter();
    void Execute();
    void Exit();
}

public class IdleState : ICustomerState
{
    Customer customer;

    public IdleState(Customer customer)
    {
        this.customer = customer;
    }
    public void Enter() 
    {
        customer.StartCoroutine(WaitForGetBread());
    }

    public void Execute() { }
    public void Exit() { }

    IEnumerator WaitForGetBread()
    {
        yield return new WaitForSeconds(1f);
        customer.ChangeState(customer.getBreadState);
    }
}

public class GetBreadState : ICustomerState
{
    Customer customer;
    NavMeshAgent agent;
    CustomerManager manager;

    int requestBreadCount;
    Transform assignedBreadPos;

    public GetBreadState(Customer customer)
    {
        this.customer = customer;
        this.agent = customer.agent;
        this.manager = customer.manager;
    }
    public void Enter() 
    {
        decideRequestBread();
        MoveToBreadStand();
    }
    public void Execute() 
    {
    }
    public void Exit() 
    {

    }

    void decideRequestBread()
    {
        requestBreadCount = Random.Range(1, 4);
        Debug.Log("»§ °¹¼ö" + requestBreadCount);
    }

    public void MoveToBreadStand()
    {
        Transform assignedPos = manager.AssignBreadPositionToCustomer();
        assignedBreadPos = assignedPos;
        if(assignedPos != null)
        {
            agent.SetDestination(assignedPos.position);
            agent.isStopped = false;

            customer.StartCoroutine(WaitForCustomerArrive());
        }
        else
        {
            Debug.Log("No space near bread");
        }
    }

    IEnumerator WaitForCustomerArrive()
    {
        yield return new WaitUntil(() => agent.remainingDistance <= agent.stoppingDistance
        && !agent.pathPending);
        ArriveAtBreadStand();
    }

    void ArriveAtBreadStand()
    {
        customer.StartCoroutine(rotateToTarget());
    }

    IEnumerator rotateToTarget()
    {
        float rotationSpeed = 2f;

        Quaternion targetRot = assignedBreadPos.rotation;
        float elapsed = 0f;
        float duration = 1f;

        while(elapsed < duration)
        {
            customer.transform.rotation = Quaternion.Slerp(customer.transform.rotation,
                targetRot, (elapsed/duration));
            elapsed += Time.deltaTime;
            yield return null;
        }
        customer.transform.rotation = targetRot;

        RequestBreadCount();
    }

    void receiveBreads(Customer customer, List<GameObject> receivedBreads)
    {
        customer.StartCoroutine(MoveBreadsToStack(receivedBreads));
    }

    public int RequestBreadCount()
    {
        return requestBreadCount;
    }

    public void ReceiveBreads(List<GameObject> receivedBreads)
    {
        customer.StartCoroutine(MoveBreadsToStack(receivedBreads));
    }

    IEnumerator MoveBreadsToStack(List<GameObject> receivedBreads)
    {
        Transform breadStackPoint = customer.transform.Find("BreadStackPoint");

        float breadHeight = 0.3f;
        //float moveDuration = 0.5f;

        for (int i = 0; i < receivedBreads.Count; i++)
        {
            GameObject bread = receivedBreads[i];
            Vector3 startPos = bread.transform.position;
            Vector3 endPos = breadStackPoint.position
                + new Vector3(0, breadHeight * i, 0);

            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                bread.transform.position = Vector3.Lerp(startPos, endPos, (elapsed / duration));
                yield return null;
            }

            bread.transform.parent = breadStackPoint;
            bread.transform.localPosition = new Vector3(0, breadHeight * i, 0);
            bread.transform.localRotation = Quaternion.Euler(0, 90, 0);
        }

        customer.SetBreads(receivedBreads);
        customer.ChangeState(customer.checkOutState);
    }

}

public class CheckOutState : ICustomerState
{
    Customer customer;
    NavMeshAgent agent;
    CustomerManager manager;

    public CheckOutState(Customer customer)
    {
        this.customer = customer;
        this.agent = customer.agent;
        this.manager = customer.manager;
    }

    public void Enter() 
    {
        manager.AddCounterCustomer(customer);
        MoveToCounter();
    }
    public void Execute() { }
    public void Exit() { }

    void MoveToCounter()
    {
        Vector3 counterPos = manager.AssignCounterPositionToCustomer(customer);
        Debug.Log("counter pos" + counterPos);
        agent.SetDestination(counterPos);
        customer.StartCoroutine(WaitForArrival());
    }

    IEnumerator WaitForArrival()
    {
        yield return new WaitUntil(() => agent.remainingDistance <= agent.stoppingDistance
        && !agent.pathPending);

        customer.StartCoroutine(RotateToTarget());
    }

    IEnumerator RotateToTarget()
    {
        float rotatioinSpeed = 2f;
        Quaternion targetRot = Quaternion.Euler(0, 90, 0);
        float elapsed = 0f;
        float duration = 1f;

        while(elapsed < duration)
        {
            customer.transform.rotation = Quaternion.Slerp(customer.transform.rotation,
                targetRot, (elapsed / duration));
            elapsed += Time.deltaTime;
            yield return null;
        }
        customer.transform.rotation = targetRot;
    }
}

public class LeaveStoreState : ICustomerState
{
    public void Enter() { }
    public void Execute() { }
    public void Exit() { }
}

public class RequestSeatState : ICustomerState
{
    public void Enter() { }
    public void Execute() { }
    public void Exit() { }
}



//public class CustomerState : MonoBehaviour
//{
//    // Start is called before the first frame update
//    void Start()
//    {
        
//    }

//    // Update is called once per frame
//    void Update()
//    {
        
//    }
//}
