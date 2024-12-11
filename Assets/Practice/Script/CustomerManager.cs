using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    public Transform customerBreadPos;
    public List<Transform> breadStandPositions = new List<Transform>();

    List<bool> breadStandUsed = new List<bool>();

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
}
