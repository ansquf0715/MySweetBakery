using JetBrains.Annotations;
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
        //sellBox.customerRemove();
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
            if (bread != null)
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

        //customer.ChangeState(customer.requestSeatState);
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

        if(customer.willRequestSeat)
        {
            manager.leavingCustomerAtCounter(customer);
            customer.ChangeState(customer.requestSeatState);
        }
    }
    public void Execute()
    {

    }
    public void Exit()
    {
        if(!customer.willRequestSeat)
        {
            manager.leavingCustomerAtCounter(customer);
        }
        manager.checkSeatCustomerIsAtCounter(customer);
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
        Vector3 startPos = bag.transform.position;
        Transform breadStackPoint = customer.gameObject.transform.Find("BreadStackPoint");

        Vector3 midPos = new Vector3(
            (startPos.x + breadStackPoint.position.x) / 2,
            breadStackPoint.position.y + 2,
            (startPos.z + breadStackPoint.position.z) / 2);

        float elapsedTime = 0f;
        float duration = 0.5f;
        float halfDuration = duration / 2f;

        while(elapsedTime < halfDuration)
        {
            float t = elapsedTime / halfDuration;
            bag.transform.position = Vector3.Lerp(startPos, midPos, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0f;

        while(elapsedTime < halfDuration)
        {
            float t = elapsedTime / halfDuration;
            bag.transform.position = Vector3.Lerp(midPos, breadStackPoint.position, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        bag.transform.position = breadStackPoint.position;

        bag.transform.SetParent(breadStackPoint);
        bag.transform.localPosition = Vector3.zero;
        bag.transform.rotation = Quaternion.Euler(0, 90f, 0);

        disableUI();
        yield return new WaitForSeconds(0.5f);
        EventManager.CustomerPay(customer, customer.requestBreadCount);
        customer.ChangeState(customer.leaveStoreState);
    }

    //IEnumerator moveBagToCustomer(GameObject bag)
    //{
    //    Vector3 bagStartPos = bag.transform.position;

    //    Transform breadStackPoint
    //        = customer.gameObject.transform.Find("BreadStackPoint");

    //    bag.transform.SetParent(breadStackPoint);
    //    bag.transform.localPosition = Vector3.zero;
    //    bag.transform.rotation = Quaternion.Euler(0, 90f, 0);

    //    float elapsedTime = 0f;
    //    float durationTime = 1f;

    //    while (elapsedTime < durationTime)
    //    {
    //        bag.transform.position = Vector3.Lerp(bagStartPos, breadStackPoint.position, (elapsedTime / durationTime));
    //        elapsedTime += Time.deltaTime;
    //        yield return null;
    //    }

    //    bag.transform.position = breadStackPoint.position;
    //    disableUI();
    //    yield return new WaitForSeconds(0.5f);
    //    EventManager.CustomerPay(customer, customer.requestBreadCount);
    //    customer.ChangeState(customer.leaveStoreState);
    //}

}

public class LeaveStoreState : ICustomerState
{
    Customer customer;
    NavMeshAgent agent;
    CustomerManager manager;

    ParticleSystem smile;
    ParticleSystem smileInstance;

    public LeaveStoreState(Customer customer)
    {
        this.customer = customer;
        this.agent = customer.agent;
        this.manager = customer.manager;

        smile = Resources.Load<ParticleSystem>("VFX_EmojiSmile");
    }

    public void Enter()
    {
        manager.checkSeatCustomerIsAtCounter(customer);

        manager.checkHandledCustomer(customer);
        agent.isStopped = false;
        Vector3 leavePos = new Vector3(-12f, 0.5f, 3.4f);
        agent.SetDestination(leavePos);

        smileInstance = GameObject.Instantiate(smile);
        UpdateParticlePosition();
        smileInstance.Play();

        customer.StartCoroutine(DestroyParticleAfterDelay());
        customer.StartCoroutine(WaitForArrive());
    }

    public void Execute() 
    {
        if (smileInstance != null)
            UpdateParticlePosition();
    }
    public void Exit()
    {
    }

    void UpdateParticlePosition()
    {
        Vector3 particlePosition = new Vector3(customer.transform.position.x, customer.transform.position.y + 2, customer.transform.position.z);
        smileInstance.transform.position = particlePosition;
    }
    IEnumerator DestroyParticleAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        if (smileInstance != null)
            GameObject.Destroy(smileInstance.gameObject);
    }

    IEnumerator WaitForArrive()
    {
        yield return new WaitUntil(() => agent.velocity.sqrMagnitude >= 0.2f * 0.2f
        && agent.remainingDistance <= 0.5f);

        yield return new WaitForSeconds(1f);
        customer.DestroySelf();
    }
}

public class RequestSeatState : ICustomerState
{
    Customer customer;
    NavMeshAgent agent;
    CustomerManager manager;

    Sprite seatSprite;

    GameObject trashPrefab;
    GameObject trash;

    public RequestSeatState(Customer customer)
    {
        this.customer = customer;
        this.agent = customer.agent;
        this.manager = customer.manager;

        seatSprite = Resources.Load<Sprite>("TableChair");
        trashPrefab = Resources.Load<GameObject>("Trash");
    }

    public void Enter()
    {
        Vector3 initialPos = new Vector3(-2.5f, 0.5f, 4.5f);
        agent.SetDestination(initialPos);
        agent.isStopped = false;
        customer.StartCoroutine(WaitUntilArriveInitialPos());
        manager.checkSeatCustomerIsAtCounter(customer);
        SetUI();
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

        //Vector3 currentPos = customer.orderObj.rectTransform.localPosition;
        //customer.orderObj.rectTransform.localPosition = new Vector3(
        //    currentPos.x - 0.2f, currentPos.y, currentPos.z);

        //customer.orderObj.rectTransform.localScale = new Vector3(
        //    1.4f, 1.4f, 1.4f);
    }

    IEnumerator WaitUntilArriveInitialPos()
    {
        yield return new WaitUntil(() => agent.velocity.sqrMagnitude >= 0.2f * 0.2f
        && agent.remainingDistance <= 0.5f);

        manager.addCustomerToSeat(customer);
    }

    public void UpdateDestination(Vector3 newPos)
    {
        if (agent.isStopped)
            agent.isStopped = false;
        if (customer.GetSitting()) //assigned seat
        {
            manager.checkSeatCustomerIsAtCounter(customer);
            disableUI();
            agent.SetDestination(newPos);
            customer.StartCoroutine(WaitForArrive());
        }
        else //waiting for seat
        {
            manager.checkSeatCustomerIsAtCounter(customer);
            agent.SetDestination(newPos);
            customer.StartCoroutine(WaitForArrive());
        }
    }

    void disableUI()
    {
        customer.canvas.gameObject.SetActive(false);
    }

    IEnumerator WaitForArrive()
    {
        if (!customer.GetSitting())
        {
            yield return new WaitUntil(() => agent.velocity.sqrMagnitude >= 0.2f * 0.2f
                && agent.remainingDistance <= 0.5f);

            agent.isStopped = true;

            Quaternion startRot = customer.transform.rotation;
            Quaternion endRot = Quaternion.Euler(0, 90f, 0);

            float elapsed = 0f;
            float duration = 0.3f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                customer.transform.rotation = Quaternion.Slerp(startRot, endRot,
                    (elapsed / duration));
                yield return null;
            }
            customer.transform.rotation = endRot;
            manager.AssignSeatToCustomer(customer);
        }
        else
        {
            disableUI();

            yield return new WaitUntil(() => agent.velocity.sqrMagnitude >= 0.2f * 0.2f
                && agent.remainingDistance <= 0.5f);

            agent.isStopped = true;

            Quaternion startRot = customer.transform.rotation;
            Quaternion endRot = Quaternion.Euler(0, 90f, 0);

            float elapsed = 0f;
            float duration = 0.3f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                customer.transform.rotation = Quaternion.Slerp(startRot, endRot,
                    (elapsed / duration));
                yield return null;
            }
            customer.transform.rotation = endRot;
            agent.updatePosition = false;
            customer.transform.position += new Vector3(0, 0.5f, 0);
            customer.eating = true;
            customer.StartCoroutine(MoveBreadsToTable());
        }
    }

    IEnumerator MoveBreadsToTable()
    {
        Vector3 destPos = new Vector3(-5.4f, 1.6f, 7.7f);
        float yOffset = 0.3f;

        for(int i=customer.breads.Count-1; i>=0; i--)
        {
            GameObject bread = customer.breads[i];
            Vector3 startPos = bread.transform.position;
            Vector3 endPos = new Vector3(destPos.x,
                destPos.y + (customer.breads.Count-1-i) * yOffset , destPos.z);

            Vector3 midPos = (startPos + endPos) / 2 + new Vector3(0, 1.5f, 0);

            float elapsed = 0;
            float duration = 0.25f;

            while(elapsed < duration)
            {
                elapsed += Time.deltaTime;
                bread.transform.position = Vector3.Lerp(startPos, midPos,
                    elapsed / duration);
                yield return null;
            }

            elapsed = 0;
            while(elapsed < duration)
            {
                elapsed += Time.deltaTime;
                bread.transform.position = Vector3.Lerp(midPos, endPos,
                    elapsed / duration);
                yield return null;
            }

            bread.transform.position = endPos;
            yield return new WaitForSeconds(0.5f);
        }

        yield return new WaitForSeconds(1f);
        //customer.ChangeState(customer.leaveStoreState);
        customer.StartCoroutine(readyToLeave());
    }

    IEnumerator readyToLeave()
    {
        Vector3 newPos = new Vector3(customer.transform.position.x,
            0.5f, customer.transform.position.z);
        customer.transform.position = newPos;

        customer.eating = false;

        for (int i = 0; i < customer.breads.Count; i++)
        {
            customer.destroyBreads();
        }
        Vector3 trashPos = new Vector3(-5.55f, 1.5f, 7.7f);
        trash = GameObject.Instantiate(trashPrefab,
            trashPos, Quaternion.identity);

        GameObject chair = manager.leaveSeat(customer, trash);
        if (chair != null)
        {
            Quaternion newRot = Quaternion.Euler(chair.transform.rotation.eulerAngles.x,
                chair.transform.rotation.eulerAngles.y + 45,
                chair.transform.rotation.eulerAngles.z);
            chair.transform.rotation = newRot;
        }

        yield return new WaitForSeconds(0.5f);
        agent.updatePosition = true;
        agent.isStopped = false;
        EventManager.SeatPay(customer.requestBreadCount);
        customer.ChangeState(customer.leaveStoreState);
    }

}


