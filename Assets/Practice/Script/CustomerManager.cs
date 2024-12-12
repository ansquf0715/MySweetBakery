using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    public Transform customerBreadPos;
    List<Transform> breadStandPositions = new List<Transform>();
    List<bool> breadStandUsed = new List<bool>();

    //계산대 대기줄은 이걸 기준으로 x값만 조절해주면 됨
    public Transform customerCounterPos;
    float counterSpacing = 4f;
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

    //public Quaternion GetBreadRotation()
    //{
    //}

    //고객이 bread pos를 떠나면, 기다리고 있는 customer에게 연락하기

    public void AddCounterCustomer(Customer customer)
    {
        checkOutQueue.Enqueue(customer);
    }

    public Vector3 AssignCounterPositionToCustomer(Customer customer)
    {
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

        Debug.Log("newPos" + newPos);
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
}
