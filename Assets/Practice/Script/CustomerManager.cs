using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    public Transform customerBreadPos;
    List<Transform> breadStandPositions = new List<Transform>();
    List<bool> breadStandUsed = new List<bool>();

    //���� ������� �̰� �������� x���� �������ָ� ��
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

    //���� bread pos�� ������, ��ٸ��� �ִ� customer���� �����ϱ�

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
