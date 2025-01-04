using JetBrains.Annotations;
using Newtonsoft.Json.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Seat
{
    public GameObject chair;
    public Vector3 seatPos { get; set; }
    public bool isUsed { get; set; }
    public Customer assignedCustomer { get; set; }
    public bool isDirty;
    public GameObject trash;

    public bool checkDirty()
    {
        return isDirty;
    }

    public void setTrash(GameObject trash)
    {
        this.trash = trash;
    }

    public Seat(GameObject chair, bool isUsed = false, Customer assignedCustomer = null)
    {
        this.chair = chair;
        this.seatPos = chair.transform.position;
        this.isUsed = isUsed;
        this.assignedCustomer = assignedCustomer;
    }
}

public class CustomerManager : MonoBehaviour
{
    public GameObject customerPrefab;
    public Transform customerSpawnPos;
    public Transform customerBreadPos;
    public Transform customerCounterPos;
    Vector3 customerSeatWaitingPos = new Vector3(-2f, 0.5f, 5f);

    List<Transform> breadStandPositions = new List<Transform>();
    List<KeyValuePair<Customer, bool>> breadStandUsed = new List<KeyValuePair<Customer, bool>>();

    //float counterSpacing = 1.5f;
    Queue<Customer> customerWaitingOrder=  new Queue<Customer>();

    List<Seat> seats = new List<Seat>();
    Queue<Customer> seatWaitingQueue = new Queue<Customer>();

    int createdCustomers = 0;
    public int cashedCustomer = 0;

    readonly object positionLock = new object();

    // Start is called before the first frame update
    void Start()
    {
        //EventManager.FirstQuestIsReady();

        EventManager.OnNewSeatAvailable += handleNewSeat;
        EventManager.OnSeatCleaned += handleCleanedSeat;

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
            if (count == 1)
            {
                yield return new WaitForSeconds(2f);
            }
            GameObject newCustomer = Instantiate(customerPrefab, customerSpawnPos.position, Quaternion.identity);
            Customer customerComp = newCustomer.GetComponent<Customer>();

            createdCustomers++;

            if (createdCustomers % 5 == 0)
            {
                customerComp.willRequestSeat = true;
            }

            //customerComp.willRequestSeat = true;
            
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
                    return breadStandPositions[i];
                }
            }
            return null;
        }
    }

    public void LeavingBreadPos(Customer customer)
    {
        for(int i=0; i<breadStandUsed.Count; i++)
        {
            if (breadStandUsed[i].Key == customer)
            {
                breadStandUsed[i] = new KeyValuePair<Customer, bool>(null, false);
                StartCoroutine(SpawnCustomer(1));
                break;
            }
        }
    }

    public void addCustomerToCounter(Customer customer)
    {
        if(!customerWaitingOrder.Contains(customer))
        {
            customerWaitingOrder.Enqueue(customer);
            UpdateCustomerPos();
        }
    }

    void UpdateCustomerPos()
    {
        int i = 0;
        foreach (Customer cust in customerWaitingOrder)
        {
            Vector3 newPos = customerCounterPos.position - new Vector3(1.2f * i, 0, 0);
            cust.UpdateDestination(newPos);
            i++;
        }
    }

    public void leavingCustomerAtCounter(Customer customer)
    {
        if(customerWaitingOrder.Count > 0 && customerWaitingOrder.Peek() ==  customer)
        {
            customerWaitingOrder.Dequeue();
            moveRestWaitingCounterCustomers();
        }
    }

    public void checkSeatCustomerIsAtCounter(Customer customer)
    {
        Queue<Customer> tempQueue = new Queue<Customer>();
        bool found = false;

        while (customerWaitingOrder.Count > 0)
        {
            Customer currentCustomer = customerWaitingOrder.Dequeue();
            if (currentCustomer.Equals(customer))
            {
                found = true;
                continue;
            }
            tempQueue.Enqueue(currentCustomer);
        }
        while (tempQueue.Count > 0)
        {
            customerWaitingOrder.Enqueue(tempQueue.Dequeue());
        }
        if (found)
        {
            moveRestWaitingCounterCustomers();
        }
    }

    void moveRestWaitingCounterCustomers()
    {
        int i = 0;
        foreach (Customer customer in customerWaitingOrder)
        {
            Vector3 newPos = customerCounterPos.position - new Vector3(1.2f * i, 0, 0);
            customer.UpdateDestination(newPos);
            i++;
        }
    }

    void UpdateCustomerSeatWaitingPos()
    {
        int i = 0;
        foreach (Customer cust in seatWaitingQueue)
        {
            Vector3 newPos = customerSeatWaitingPos - new Vector3(1.2f * i, 0, 0);
            cust.seatUpdateDestination(newPos);
            i++;
        }
    }

    public void addCustomerToSeat(Customer customer)
    {
        if (!seatWaitingQueue.Contains(customer))
        {
            seatWaitingQueue.Enqueue(customer);
            UpdateCustomerSeatWaitingPos();
        }
    }

    public void AssignSeatToCustomer(Customer customer = null)
    {
        if(seats.Count > 0 && seatWaitingQueue.Count > 0)
        {
            for (int i = 0; i < seats.Count; i++)
            {
                Seat seat = seats[i];
                if (!seat.isUsed && !seat.isDirty && cashedCustomer>=2)
                {
                    seat.isUsed = true;

                    Customer assignedCustomer = seatWaitingQueue.Peek();
                    assignedCustomer.SetSitting(true);
                    assignedCustomer.seatUpdateDestination(seat.seatPos);

                    seat.assignedCustomer = assignedCustomer;
                    seatWaitingQueue.Dequeue();
                    UpdateCustomerSeatWaitingPos();
                    break;
                }
            }
        }
    }

    public void destroyCustomer(Customer customer)
    {

    }

    public void checkHandledCustomer(Customer customer)
    {
        cashedCustomer++;
        if(cashedCustomer == 1)
        {
            EventManager.FirstQuestIsReady();
        }
    }

    void handleNewSeat(GameObject chair)
    {
        seats.Add(new Seat(chair));
        AssignSeatToCustomer();
    }

    public GameObject leaveSeat(Customer customer, GameObject trash)
    {
        foreach(Seat seat in seats)
        {
            if(seat.assignedCustomer == customer)
            {
                seat.isUsed = false;
                seat.isDirty = true;
                seat.assignedCustomer = null;
                seat.trash = trash;

                EventManager.SeatDirty(seat);
                return seat.chair;
            }
        }
        return null;
    }

    void handleCleanedSeat(Seat seat)
    {
        for(int i=0; i<seats.Count; i++)
        {
            if(seat == seats[i])
            {
                seat.isDirty = false;
                seat.trash = null;

                AssignSeatToCustomer();
            }
        }
    }
}
