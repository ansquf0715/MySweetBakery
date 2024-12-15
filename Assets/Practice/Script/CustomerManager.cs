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
    //public Transform customerSeatWaitingPos;
    Vector3 customerSeatWaitingPos = new Vector3(-2f, 0.5f, 5f);

    List<Transform> breadStandPositions = new List<Transform>();
    List<KeyValuePair<Customer, bool>> breadStandUsed = new List<KeyValuePair<Customer, bool>>();

    float counterSpacing = 1.5f;
    //Dictionary<Customer, float> customerDistances = new Dictionary<Customer, float>();
    //List<Customer> customerOrder = new List<Customer>();
    Queue<Customer> customerWaitingOrder=  new Queue<Customer>();

    //seat을 원하는 customer
    List<Seat> seats = new List<Seat>();
    Queue<Customer> seatWaitingQueue = new Queue<Customer>();

    int createdCustomers = 0;
    List<Customer> allCustomers = new List<Customer>();
    public int cashedCustomer = 0;

    bool isUpdatingSeats = false;

    readonly object positionLock = new object();

    // Start is called before the first frame update
    void Start()
    {
        //EventManager.FirstQuestIsReady();

        EventManager.OnNewSeatAvailable += handleNewSeat;

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

            createdCustomers++;

            //if (createdCustomers % 3 == 0)
            //{
            //    customerComp.willRequestSeat = true;
            //}

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

    //public void customerEndedCheckout(Customer customer)
    //{
    //    Queue<Customer> newQ = new Queue<Customer>();
    //    foreach(var c in checkOutQueue)
    //    {
    //        if(c!=customer)
    //            newQ.Enqueue(c);
    //    }
    //    checkOutQueue = newQ;
    //    customerArrivedAtCounter(customer);
    //}

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

    void handleNewSeat(GameObject chair)
    {
        seats.Add(new Seat(chair));
    }

    public (Vector3, bool) assignCustomerSeatPos(Customer customer)
    {
        foreach (var seat in seats)
        {
            if (!seat.isUsed && !seat.isDirty)
            {
                // Assign the seat to the customer
                seat.isUsed = true;
                seat.assignedCustomer = customer;

                // Remove the customer from the waiting queue if they are present
                if (seatWaitingQueue.Contains(customer))
                {
                    seatWaitingQueue = new Queue<Customer>(seatWaitingQueue.Where(c => c != customer));
                }

                // Return the seat position
                return (seat.seatPos, true);
            }
        }

        // If no seats are available, enqueue the customer and return a waiting position
        if (!seatWaitingQueue.Contains(customer))
            seatWaitingQueue.Enqueue(customer);

        int waitingIndex = 0;
        foreach (var c in seatWaitingQueue)
        {
            if (c == customer)
                break;
            waitingIndex++;
        }

        Vector3 waitingPos = customerSeatWaitingPos;
        waitingPos.x -= waitingIndex * 1f; // Adjust the x-position based on the queue index

        return (waitingPos, false);
    }

    public void NotifySeatAvailable()
    {
        if (seatWaitingQueue.Count > 0)
        {
            Customer nextCustomer = seatWaitingQueue.Peek();

            foreach (var seat in seats)
            {
                if (!seat.isUsed && !seat.isDirty)
                {
                    // Assign seat to the next customer
                    seat.isUsed = true;
                    seat.assignedCustomer = nextCustomer;

                    seatWaitingQueue.Dequeue(); // Remove from queue

                    // Recheck and update customer position
                    (Vector3 pos, bool success) = assignCustomerSeatPos(nextCustomer);
                    if (success)
                    {
                        nextCustomer.moveToFirstWaitingSeatPos(pos);
                    }
                    break;
                }
            }
        }

        UpdateSeatWaitingQueuePos();
    }

    void UpdateSeatWaitingQueuePos()
    {
        int index = 0;

        foreach (var customer in seatWaitingQueue)
        {
            // Calculate new waiting position based on queue index
            Vector3 waitingPos = customerSeatWaitingPos;
            waitingPos.x -= index * 1f; // Adjust x based on index

            StartCoroutine(UpdateCustomerWaitingPosition(customer, waitingPos));
            index++;
        }
    }

    IEnumerator UpdateCustomerWaitingPosition(Customer customer, Vector3 waitingPos)
    {
        yield return new WaitForSeconds(0.5f); // Small delay for smooth transition
        //customer.moveToFirstWaitingSeatPos(waitingPos);
        customer.moveToWaitingPos(waitingPos);
    }


    public GameObject GetChair(Customer customer)
    {
        return seats[0].chair;
    }

    public void LeaveSeat(Customer customer, GameObject trash)
    {
        foreach(var seat in seats)
        {
            if(seat.assignedCustomer == customer)
            {
                if(!seat.isDirty)
                {
                    seat.setTrash(trash);
                    seat.isDirty = true;
                    EventManager.SeatDirty(seat);
                }
                seat.assignedCustomer = null;
                seat.isUsed = false;

                break;

            }
        }

        NotifySeatAvailable();
    }
}
