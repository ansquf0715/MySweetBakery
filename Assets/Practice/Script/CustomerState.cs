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
    //Transform breadStackPoint;

    public GetBreadState(Customer customer)
    {
        this.customer = customer;
        this.agent = customer.agent;
        this.manager = customer.manager;
    }
    public void Enter() 
    {
        EventManager.OnCustomerReceiveBreads += ReceiveBreads;
        EventManager.OnSellboxHaveNotEnoughBread += RetryRequestBread;

        MoveToBreadStand();
    }
    public void Execute() 
    {
    }
    public void Exit() 
    {
        EventManager.OnSellboxHaveNotEnoughBread -= RetryRequestBread;
    }

    public void MoveToBreadStand()
    {
        Transform assignedPos = manager.AssignBreadPositionToCustomer();
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
        //int breadCount = Random.Range(1, 4);
        requestBreadCount = Random.Range(1, 4);
        Debug.Log("°í¸¥ »§ÀÇ °¹¼ö" + requestBreadCount);

        EventManager.CustomerRequestToSellBox(requestBreadCount);
    }

    void ReceiveBreads(List<GameObject> receivedBreads)
    {
        customer.StartCoroutine(MoveBreadsToStack(receivedBreads));
    }

    IEnumerator MoveBreadsToStack(List<GameObject> receivedBreads)
    {
        Transform breadStackPoint = customer.transform.Find("BreadStackPoint");

        float breadHeight = 0.3f;
        //float moveDuration = 0.5f;

        for(int i=0; i<receivedBreads.Count; i++)
        {
            GameObject bread = receivedBreads[i];
            Vector3 startPos = bread.transform.position;
            Vector3 endPos = breadStackPoint.position
                + new Vector3(0, breadHeight * i, 0);

            float duration = 0.5f;
            float elapsed = 0f;

            while(elapsed < duration)
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

    void RetryRequestBread()
    {
        customer.StartCoroutine(RequestBreadAgain());
    }

    IEnumerator RequestBreadAgain()
    {
        yield return new WaitForSeconds(1f);
        EventManager.CustomerRequestToSellBox(requestBreadCount);
    }


}

public class CheckOutState : ICustomerState
{
    Customer customer;

    public CheckOutState(Customer customer)
    {
        Debug.Log("check out state »ý¼ºÀÚ");
        this.customer = customer;
    }

    public void Enter() { }
    public void Execute() { }
    public void Exit() { }
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
