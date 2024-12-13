using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CustomerManager : MonoBehaviour
{
    public Transform customerBreadPos;
    List<Transform> breadStandPositions = new List<Transform>();
    List<bool> breadStandUsed = new List<bool>();

    //계산대 대기줄은 이걸 기준으로 x값만 조절해주면 됨
    public Transform customerCounterPos;
    float counterSpacing = 1.5f;
    Queue<Customer> checkOutQueue = new Queue<Customer>();

    List<Customer> allCustomers = new List<Customer>();
    
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < customerBreadPos.childCount; i++)
        {
            breadStandPositions.Add(customerBreadPos.GetChild(i));
            breadStandUsed.Add(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Transform AssignBreadPositionToCustomer()
    {
        for(int i=0; i<breadStandPositions.Count; i++)
        {
            if (!breadStandUsed[i])
            {
                breadStandUsed[i] = true;
                return breadStandPositions[i];
            }
        }
        return null;
    }

    //고객이 bread pos를 떠나면, 기다리고 있는 customer에게 연락하기

    //public void AddCounterCustomer(Customer customer)
    //{
    //    checkOutQueue.Enqueue(customer);
    //    EventManager.CustomerAtCounter(customer);
    //}

    public Vector3 AssignCounterPositionToCustomer(Customer customer)
    {
        checkOutQueue.Enqueue(customer);
        EventManager.CustomerAtCounter(customer);
        int customerIndex = GetCustomerQueueIndex(customer);

        if(customerIndex == -1)
        {
            Debug.Log("Customer is not int checkout queue");
            return Vector3.zero;
        }

        Vector3 newPos = customerCounterPos.position;
        newPos.x = customerCounterPos.position.x - (customerIndex * 1.2f);
        newPos.y = customerCounterPos.position.y;
        newPos.z = customerCounterPos.position.z;
        return newPos;
    }

    int GetCustomerQueueIndex(Customer customer)
    {
        int index = 0;
        foreach(var c in checkOutQueue)
        {
            if(c == customer)
                return index;
            index++;
        }
        return -1;
    }

    public void customerArrivedAtCounter(Customer customer)
    {
        StartCoroutine(DelayMoveCustomers(customer));
    }

    IEnumerator DelayMoveCustomers(Customer customer)
    {
        yield return new WaitForSeconds(2f);

        moveRestCustomers(customer);
    }

    void moveRestCustomers(Customer customer)
    {
        int index = 0;
        foreach (var c in checkOutQueue)
        {
            if (c == customer)
                continue;

            Vector3 newPos = customerCounterPos.position - new Vector3(
                index * counterSpacing, 0, 0f);

            StartCoroutine(CheckNextCustomerArrive(c, newPos));

            index++;

            StartCoroutine(delay());
        }

    }

    IEnumerator delay()
    {
        yield return new WaitForSeconds(1f);
    }

    IEnumerator CheckNextCustomerArrive(Customer customer, Vector3 targetPos)
    {
        while (Vector3.Distance(customer.transform.position, targetPos) > 0.1f)
        {
            customer.transform.position = Vector3.MoveTowards(customer.transform.position, targetPos, Time.deltaTime);
            yield return null;
        }
        yield return new WaitForSeconds(1f);
    }

    public void customerEndedCheckout(Customer customer)
    {
        Queue<Customer> newQ = new Queue<Customer>();
        foreach(var c in checkOutQueue)
        {
            if(c!=customer)
                newQ.Enqueue(c);
        }
        checkOutQueue = newQ;
        customerArrivedAtCounter(customer);
    }
}
