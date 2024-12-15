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
    int currentBreadCount;
    Transform assignedBreadPos;

    Sprite breadSprite;
    AudioClip getBreadClip;

    SellBox sellBox;

    public GetBreadState(Customer customer)
    {
        this.customer = customer;
        this.agent = customer.agent;
        this.manager = customer.manager;
        currentBreadCount = 0;
        breadSprite = Resources.Load<Sprite>("Croissant");
        getBreadClip = Resources.Load<AudioClip>("Get_Object");
        sellBox = GameObject.FindObjectOfType<SellBox>();
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
        customer.orderCount.gameObject.SetActive(false);
        manager.LeavingBreadPos(customer);
        sellBox.customerRemove(customer);
    }

    void decideRequestBread()
    {
        requestBreadCount = Random.Range(1, 4);
        customer.requestBreadCount = requestBreadCount;
    }

    public void MoveToBreadStand()
    {
        Transform assignedPos = manager.AssignBreadPositionToCustomer(customer);
        assignedBreadPos = assignedPos;
        if (assignedPos != null)
        {
            agent.SetDestination(assignedPos.position);
            agent.isStopped = false;
            customer.StartCoroutine(WaitForCustomerArrive());
        }
    }

    IEnumerator WaitForCustomerArrive()
    {
        yield return new WaitUntil(() => agent.velocity.sqrMagnitude >= 0.2f * 0.2f
        && agent.remainingDistance <= 0.5f);

        agent.isStopped = true;

        Quaternion startRot = customer.transform.rotation;
        Quaternion targetRot = assignedBreadPos.rotation;

        float duration = 1.0f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            customer.transform.rotation = Quaternion.Slerp(startRot, targetRot,
                (elapsed / duration));
            yield return null;
        }

        customer.transform.rotation = targetRot;
        SetUI();
        customer.StartCoroutine(requestBreads());
    }

    void SetUI()
    {
        customer.canvas.gameObject.SetActive(true);
        customer.orderObj.sprite = breadSprite;
        customer.orderCount.text = requestBreadCount.ToString();
    }

    IEnumerator requestBreads()
    {
        while(currentBreadCount != requestBreadCount)
        {
            GameObject bread = sellBox.CustomerRequestBread(customer);
            if(bread != null)
            {
                yield return customer.StartCoroutine(moveBreads(bread));
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
        }
        customer.StartCoroutine(delay());
    }

    IEnumerator moveBreads(GameObject bread)
    {
        float breadHeight = 0.3f;

        Transform breadStackPoint = customer.transform.Find("BreadStackPoint");
        Vector3 startPos = bread.transform.position;
        Vector3 endPos = breadStackPoint.position
            + new Vector3(0, breadHeight * currentBreadCount, 0);
        Vector3 targetPos = Vector3.Lerp(startPos, endPos, 0.5f);
        targetPos.y += 1f;

        float duration = 0.3f;
        float elapsed = 0f;

        while(elapsed < duration)
        {
            if(elapsed < duration/2)
            {
                bread.transform.position = Vector3.Lerp(startPos, targetPos,
                    elapsed / (duration / 2));
            }
            else
            {
                bread.transform.position = Vector3.Lerp(targetPos, endPos,
                    (elapsed - duration / 2) / (duration / 2));
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        bread.transform.parent = breadStackPoint;
        bread.transform.localPosition = new Vector3(0, breadHeight*currentBreadCount, 0);
        bread.transform.localRotation = Quaternion.Euler(0, 90f, 0);

        currentBreadCount++;
        AudioSource audio = customer.GetComponent<AudioSource>();
        audio.PlayOneShot(getBreadClip);
        customer.SetBread(bread);
    }

    IEnumerator delay()
    {
        yield return new WaitForSeconds(1f);

        customer.ChangeState(customer.checkOutState);
    }

}

public class CheckOutState : ICustomerState
{
    Customer customer;
    NavMeshAgent agent;
    CustomerManager manager;

    public bool isAtCounter = false;
    public bool checkOutEnded;

    Sprite counterSprite;

    float checkInterval = 1.0f;
    float nextCheckTime = 0f;

    public CheckOutState(Customer customer)
    {
        this.customer = customer;
        this.agent = customer.agent;
        this.manager = customer.manager;

        counterSprite = Resources.Load<Sprite>("Pay");
        isAtCounter = false;
    }

    public void Enter()
    {
        Vector3 initialPos = new Vector3(-2f, 0.5f, -1.5f);
        agent.SetDestination(initialPos);
        agent.isStopped = false;
        SetUI();
        customer.StartCoroutine(WaitUntilArrive());
    }
    public void Execute()
    {

    }
    public void Exit()
    {
        manager.leavingCustomerAtCounter(customer);
    }

    IEnumerator WaitUntilArrive()
    {
        yield return new WaitUntil(() => agent.velocity.sqrMagnitude >= 0.2f * 0.2f
            && agent.remainingDistance <= 0.5f);

        manager.addCustomerToCounter(customer);
    }

    public void UpdateDestination(Vector3 newPos)
    {
        if(!isAtCounter)
        {
            isAtCounter = true;
            agent.SetDestination(newPos);
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(newPos);
        }
        customer.StartCoroutine(waitToArriveAtCounter());
    }

    IEnumerator waitToArriveAtCounter()
    {
        yield return new WaitUntil(() => agent.velocity.sqrMagnitude >= 0.2f * 0.2f
            && agent.remainingDistance <= 0.5f && !agent.pathPending);

        if (customer.willRequestSeat)
            customer.ChangeState(customer.requestSeatState);
        else
            customer.StartCoroutine(SmoothRotateTowards());
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

    IEnumerator SmoothRotateTowards()
    {
        agent.isStopped = true;
        Quaternion startRotation = customer.transform.rotation;
        Quaternion endRotation = Quaternion.Euler(0, 90, 0);

        float elapsed = 0.0f;
        //float duration = 1.0f;
        float duration = 0.3f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            customer.transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsed / duration);
            yield return null;
        }
        customer.transform.rotation = endRotation;
    }

    void disableUI()
    {
        customer.canvas.gameObject.SetActive(false);
    }

    public void GetBag(GameObject bag)
    {
        customer.StartCoroutine(moveBagToCustomer(bag));
    }

    IEnumerator moveBagToCustomer(GameObject bag)
    {
        Vector3 bagStartPos = bag.transform.position;

        Transform breadStackPoint
            = customer.gameObject.transform.Find("BreadStackPoint");

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
        disableUI();
        yield return new WaitForSeconds(0.5f);
        customer.ChangeState(customer.leaveStoreState);
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
        EventManager.CustomerPay(customer, customer.requestBreadCount);

        Debug.Log("Leave Store State");
        //setUI();
        //manager.customerEndedCheckout(customer);


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
        Debug.Log("Seat Enter");

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

        if (check)
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
            if (!isCheckingSeat)
            {
                agent.SetDestination(pos);
                isCheckingSeat = true;
                customer.StartCoroutine(waitAndRetry());
            }
        }
    }

    IEnumerator waitAndRetry()
    {
        while (isCheckingSeat)
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

        for (int i = customer.breads.Count - 1; i >= 0; i--)
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

        while (elapsed < duration)
        {
            bread.transform.position = Vector3.Lerp(startPos,
                targetPos, (elapsed / duration));
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
        for (int i = 0; i < customer.breads.Count; i++)
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


