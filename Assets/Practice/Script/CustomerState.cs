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
        //customer.ChangeState(customer.requestSeatState);
    }
}

public class GetBreadState : ICustomerState
{
    Customer customer;
    NavMeshAgent agent;
    CustomerManager manager;

    int requestBreadCount;
    Transform assignedBreadPos;

    Sprite breadSprite;
    AudioClip getBreadClip;

    public GetBreadState(Customer customer)
    {
        this.customer = customer;
        this.agent = customer.agent;
        this.manager = customer.manager;

        breadSprite = Resources.Load<Sprite>("Croissant");
        //getBreadClip = Resources.Load<AudioClip>("Get_Object");
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
        //customer.gameObject.SetActive(false);
        customer.orderCount.gameObject.SetActive(false);
        manager.LeavingBreadPos(customer);
    }

    void decideRequestBread()
    {
        requestBreadCount = Random.Range(1, 4);
        //Debug.Log("빵 갯수" + requestBreadCount);
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

        SetUI();
        RequestBreadCount();
    }

    void SetUI()
    {
        customer.canvas.gameObject.SetActive(true);
        //customer.orderSprite = breadSprite;
        customer.orderObj.sprite = breadSprite;
        customer.orderCount.text = requestBreadCount.ToString();
    }

    //void receiveBreads(Customer customer, List<GameObject> receivedBreads)
    //{
    //    customer.StartCoroutine(MoveBreadsToStack(receivedBreads));
    //}

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
        AudioSource audioSource = customer.GetComponent<AudioSource>();
        AudioClip getObjectsClip = Resources.Load<AudioClip>("Get_Object");

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

            audioSource.PlayOneShot(getObjectsClip);

            bread.transform.parent = breadStackPoint;
            bread.transform.localPosition = new Vector3(0, breadHeight * i, 0);
            bread.transform.localRotation = Quaternion.Euler(0, 90, 0);
        }

        customer.SetBreads(receivedBreads);

        yield return new WaitForSeconds(1f);
        customer.ChangeState(customer.checkOutState);

        //customer.StartCoroutine(delay());
    }

    IEnumerator delay()
    {
        yield return new WaitForSeconds(1f);

        if (customer.willRequestSeat)
            customer.ChangeState(customer.requestSeatState);
        else
            customer.ChangeState(customer.checkOutState);
    }

}

public class CheckOutState : ICustomerState
{
    Customer customer;
    NavMeshAgent agent;
    CustomerManager manager;

    public bool checkOutEnded;

    Sprite counterSprite;

    public CheckOutState(Customer customer)
    {
        this.customer = customer;
        this.agent = customer.agent;
        this.manager = customer.manager;

        counterSprite = Resources.Load<Sprite>("Pay");
    }

    public void Enter() 
    {
        checkOutEnded = false;
        agent.isStopped = false;
        Debug.Log("CheckoutPos");
        SetUI();
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
        EventManager.CustomerPay(customer, customer.requestBreadCount);
    }

    void SetUI()
    {
        customer.orderObj.sprite = counterSprite;

        Vector3 currentPos = customer.orderObj.rectTransform.localPosition;
        customer.orderObj.rectTransform.localPosition = new Vector3(
            currentPos.x - 0.2f, currentPos.y, currentPos.z);

        customer.orderObj.rectTransform.localScale = new Vector3(
            1.4f, 1.4f, 1.4f);
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
        disableUI();

        //여기가 문제
        //if (customer.willRequestSeat)
        //    customer.ChangeState(customer.requestSeatState);

        customer.StartCoroutine(RotateToTarget());
    }

    void disableUI()
    {
        customer.canvas.gameObject.SetActive(false);
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
        Debug.Log("Leave Store State");
        //setUI();
        manager.customerEndedCheckout(customer);


        agent.isStopped = false;
        //Vector3 leavePos = new Vector3(-14f, 0.5f, 4f);
        Vector3 leavePos = new Vector3(-12f, 0.5f, 3.4f);
        agent.SetDestination(leavePos);

        //NavMeshHit hit;
        //if (NavMesh.SamplePosition(leavePos, out hit, 1.0f, NavMesh.AllAreas))
        //    agent.SetDestination(hit.position);
        //else
        //    Debug.Log("목적지가 포함 x");
    }

    public void Execute() { }
    public void Exit() 
    {
    }

    //void setUI()
    //{
    //    customer.canvas.gameObject.SetActive(false);
    //}

    IEnumerator WaitForArrive()
    {
        yield return new WaitUntil(() => agent.remainingDistance <= agent.stoppingDistance
        && !agent.pathPending);

        Debug.Log("이동 끝");
    }
}

public class RequestSeatState : ICustomerState
{
    Customer customer;
    NavMeshAgent agent;
    CustomerManager manager;

    Sprite seatSprite;
    bool isCheckingSeat = false;

    GameObject trashPrefab;
    GameObject trash;

    ParticleSystem smile;

    public RequestSeatState(Customer customer)
    {
        this.customer = customer;
        this.agent = customer.agent;
        this.manager = customer.manager;

        seatSprite = Resources.Load<Sprite>("TableChair");
        trashPrefab = Resources.Load<GameObject>("Trash");
        smile = Resources.Load<ParticleSystem>("VFX_EmojiSmile");
    }

    public void Enter() 
    {
        SetUI();
        agent.isStopped = false;
        checkForSeat();
    }
    public void Execute() 
    {

    }
    public void Exit() 
    {
    }

    void SetUI()
    {
        customer.orderObj.sprite = seatSprite;

        Vector3 currentPos = customer.orderObj.rectTransform.localPosition;
        customer.orderObj.rectTransform.localPosition = new Vector3(
            currentPos.x - 0.2f, currentPos.y, currentPos.z);

        customer.orderObj.rectTransform.localScale = new Vector3(
            1.4f, 1.4f, 1.4f);
    }

    void checkForSeat()
    {
        (Vector3 pos, bool check) = manager.assignCustomerSeatPos(customer);

        if(check)
        {
            Vector3 newPos = pos;
            newPos.x += 0.5f;
            agent.SetDestination(newPos);
            isCheckingSeat = false;

            customer.canvas.gameObject.SetActive(false);

            customer.StartCoroutine(WaitForArriveSeat());
        }
        else
        {
            if(!isCheckingSeat)
            {
                agent.SetDestination(pos);
                isCheckingSeat = true;
                customer.StartCoroutine(waitAndRetry());
            }
        }
    }

    IEnumerator waitAndRetry()
    {
        while(isCheckingSeat)
        {
            yield return new WaitForSeconds(1f);
            
            checkForSeat();
        }
    }

    public void moveToFirstPos(Vector3 targetPos)
    {
        Debug.Log("really move pos");
        agent.SetDestination(targetPos);
        isCheckingSeat = true;

        customer.StartCoroutine(WaitForArriveSeat());
    }

    public void moveToWaitingPos(Vector3 target)
    {
        agent.SetDestination(target);
    }


    IEnumerator WaitForArriveSeat()
    {
        yield return new WaitUntil(() => agent.remainingDistance <= agent.stoppingDistance
        && !agent.pathPending);

        agent.isStopped = true;
        agent.updatePosition = false;
        manager.NotifySeatAvailable();
        Vector3 newPos = customer.transform.position;
        //newPos.y += 0.5f;
        newPos.y = 1f;
        customer.transform.position = newPos;
        customer.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        customer.SetSitting(true);

        yield return customer.StartCoroutine(placeBreadAtTable());
    }

    IEnumerator placeBreadAtTable()
    {
        Vector3 startPos = new Vector3(-5.4f, 1.5f, 7.7f);
        float yOffset = 0.3f;

        for(int i=customer.breads.Count-1; i>=0; i--)
        {
            GameObject bread = customer.breads[i];

            Vector3 targetPos = startPos;
            targetPos.y += yOffset * (customer.breads.Count - 1 - i);

            yield return customer.StartCoroutine(MoveBread(bread, targetPos));
            //yield return new WaitForSeconds(0.5f);
        }

        customer.StartCoroutine(readyToLeave());
    }

    IEnumerator MoveBread(GameObject bread, Vector3 targetPos)
    {
        float duration = 0.2f;
        Vector3 startPos = bread.transform.position;
        float elapsed = 0f;

        while(elapsed < duration)
        {
            bread.transform.position = Vector3.Lerp(startPos,
                targetPos, (elapsed/duration));
            elapsed += Time.deltaTime;
            yield return null;
        }
        bread.transform.position = targetPos;
    }

    IEnumerator readyToLeave()
    {
        yield return new WaitForSeconds(3f);

        Vector3 newPos = customer.transform.position;

        newPos.x += 0.5f;
        newPos.y = 0.5f;
        newPos.z -= 1f;
        customer.transform.position = newPos;

        agent.updatePosition = true;
        customer.SetSitting(false);

        Vector3 particlePos = customer.transform.position;
        particlePos.y = particlePos.y + 2f;
        ParticleSystem particle = GameObject.Instantiate(smile,
            particlePos, Quaternion.identity);
        particle.Play();

        GameObject.Destroy(particle.gameObject, 1f);

        makeDirty();

        manager.LeaveSeat(customer, trash);
        customer.ChangeState(customer.leaveStoreState);
    }

    void makeDirty()
    {
        for(int i=0; i<customer.breads.Count; i++)
        {
            customer.destroyBreads();
        }
        GameObject chair = customer.manager.GetChair(customer);
        Vector3 currentRot = chair.transform.rotation.eulerAngles;
        currentRot.y = (currentRot.y) + 45 % 360f;
        chair.transform.rotation = Quaternion.Euler(currentRot);

        Vector3 trashPos = new Vector3(-5.55f, 1.5f, 7.7f);
        trash = GameObject.Instantiate(trashPrefab,
            trashPos, Quaternion.identity);
    }
    
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
