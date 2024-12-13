using Newtonsoft.Json.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CustomerManager : MonoBehaviour
{
    public GameObject customerPrefab;
    public Transform customerSpawnPos;
    public Transform customerBreadPos;
    public Transform customerCounterPos;

    List<Transform> breadStandPositions = new List<Transform>();
    List<KeyValuePair<Customer, bool>> breadStandUsed = new List<KeyValuePair<Customer, bool>>();

    //계산대 대기줄은 이걸 기준으로 x값만 조절해주면 됨
    float counterSpacing = 1.5f;
    Queue<Customer> checkOutQueue = new Queue<Customer>();

    List<Customer> allCustomers = new List<Customer>();
    public int cashedCustomer = 0;

    readonly object positionLock = new object();

    // Start is called before the first frame update
    void Start()
    {
        EventManager.FirstQuestIsReady();


        for (int i = 0; i < customerBreadPos.childCount; i++)
        {
            breadStandPositions.Add(customerBreadPos.GetChild(i));
            breadStandUsed.Add(new KeyValuePair<Customer, bool>(null, false));
        }

        StartCoroutine(SpawnCustomer(3));
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    IEnumerator SpawnCustomer(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject newCustomer = Instantiate(customerPrefab, customerSpawnPos.position, Quaternion.identity);
            Customer customerComp = newCustomer.GetComponent<Customer>();
            allCustomers.Add(customerComp);

            yield return new WaitForSeconds(2f);
        }
    }

    public Transform AssignBreadPositionToCustomer(Customer customer)
    {
        lock(positionLock)
        {
            for(int i=0; i<breadStandPositions.Count; i++)
            {
                if (!breadStandUsed[i].Value)
                {
                    breadStandUsed[i] = new KeyValuePair<Customer, bool>(customer, true);
                    //Debug.Log("Assigned position: " + breadStandPositions[i].position);
                    return breadStandPositions[i];
                }
            }
            return null;
        }
    }

    //고객이 bread pos를 떠나면, 기다리고 있는 customer에게 연락하기
    public void LeavingBreadPos(Customer customer)
    {
        for(int i=0; i<breadStandUsed.Count; i++)
        {
            if (breadStandUsed[i].Key == customer)
            {
                breadStandUsed[i] = new KeyValuePair<Customer, bool>(null, false);
                
                SpawnCustomer(1);
                break;
            }
        }
    }

    public Vector3 AssignCounterPositionToCustomer(Customer customer)
    {
        checkOutQueue.Enqueue(customer);
        EventManager.CustomerAtCounter(customer);
        int customerIndex = GetCustomerQueueIndex(customer);

        if(customerIndex == -1)
        {
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

    public void destroyCustomer(Customer customer)
    {

    }

    public void addHandledCustomer()
    {
        cashedCustomer++;
        if(cashedCustomer == 5)
        {
            EventManager.FirstQuestIsReady();
        }
    }
}
