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
        //Debug.Log("Enter");
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
        manager.LeavingBreadPos(customer);
    }

    void decideRequestBread()
    {
        requestBreadCount = Random.Range(1, 4);
        Debug.Log("»§ °¹¼ö" + requestBreadCount);
        customer.requestBreadCount = requestBreadCount;
    }

    public void MoveToBreadStand()
    {
        Transform assignedPos = manager.AssignBreadPositionToCustomer(customer);
        assignedBreadPos = assignedPos;
        if(assignedPos != null)
        {
            //Debug.Log("assigned POs" + assignedPos.position);
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
        //yield return new WaitUntil(() => agent.remainingDistance <= agent.stoppingDistance
        //&& !agent.pathPending);
        yield return new WaitUntil(() =>
        !agent.pathPending && agent.remainingDistance <= 0.2f);

        agent.isStopped = true;
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

    public bool checkOutEnded;

    public CheckOutState(Customer customer)
    {
        this.customer = customer;
        this.agent = customer.agent;
        this.manager = customer.manager;
    }

    public void Enter() 
    {
        checkOutEnded = false;
        agent.isStopped = false;
        MoveToCounter();
    }
    public void Execute() 
    {
        if(checkOutEnded)
        {
            customer.ChangeState(customer.leaveStoreState);
        }
    }
    public void Exit() 
    {

    }

    void MoveToCounter()
    {
        Vector3 counterPos = manager.AssignCounterPositionToCustomer(customer);
        agent.SetDestination(counterPos);
        customer.StartCoroutine(WaitForArrival());
    }

    IEnumerator WaitForArrival()
    {
        yield return new WaitUntil(() => agent.remainingDistance <= agent.stoppingDistance
        && !agent.pathPending);

        agent.isStopped = true;
        customer.StartCoroutine(RotateToTarget());
    }

    IEnumerator RotateToTarget()
    {
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

    public void GetBag(GameObject bag)
    {
        Debug.Log("customer get bag?");
        customer.StartCoroutine(moveBagToCustomer(bag));
    }

    IEnumerator moveBagToCustomer(GameObject bag)
    {
        Vector3 bagStartPos = bag.transform.position;

        Transform breadStackPoint 
            = customer.gameObject.transform.Find("BreadStackPoint");

        if (breadStackPoint == null)
        {
            Debug.LogError("BreadStackPoint not found in customer.");
            yield break;
        }

        bag.transform.SetParent(breadStackPoint);
        bag.transform.localPosition = Vector3.zero;
        bag.transform.rotation = Quaternion.Euler(0, 90f, 0);

        float elapsedTime = 0f;
        float durationTime = 1f;

        while (elapsedTime < durationTime)
        {
            bag.transform.position = Vector3.Lerp(bagStartPos, breadStackPoint.position, (elapsedTime / durationTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        bag.transform.position = breadStackPoint.position;

        yield return new WaitForSeconds(0.5f);
        manager.addHandledCustomer();
        checkOutEnded = true;
    }

}

public class LeaveStoreState : ICustomerState
{
    Customer customer;
    NavMeshAgent agent;
    CustomerManager manager;

    public LeaveStoreState(Customer customer)
    {
        this.customer = customer;
        this.agent = customer.agent;
        this.manager = customer.manager;
    }

    public void Enter() 
    {
        manager.customerEndedCheckout(customer);

        Debug.Log("Leave Store STate");
        EventManager.CustomerPay(customer, customer.requestBreadCount);

        agent.isStopped = false;
        Vector3 leavePos = new Vector3(-14f, 0.5f, 4f);
        agent.SetDestination(leavePos);
    }
    public void Execute() { }
    public void Exit() 
    {
    }

    IEnumerator WaitForArrive()
    {
        yield return new WaitUntil(() => agent.remainingDistance <= agent.stoppingDistance
        && !agent.pathPending);

        Debug.Log("ÀÌµ¿ ³¡");
    }
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
