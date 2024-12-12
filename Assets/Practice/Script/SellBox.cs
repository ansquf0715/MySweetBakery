using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SellBox : MonoBehaviour
{
    public List<GameObject> breads = new List<GameObject>();

    Transform breadSlot;
    Vector3 breadSpacing = new Vector3(0.5f, 0f, 0f);
    float breadHeightOffset = 0.5f;

    int maxBreadCount = 12;
    int currentBreadCount = 0;

    bool nearPlayer = false;
    bool nearCustomer = false;

    // Start is called before the first frame update
    void Start()
    {
        breadSlot = transform.Find("BreadSortSlot");
        EventManager.OnPlayerGiveBreadToSellBox += ReceiveBreadFromPlayer;
        EventManager.OnCustomerRequestToSellBox += checkCustomerRequest;


    }

    // Update is called once per frame
    void Update()
    {
        if (nearPlayer && currentBreadCount < maxBreadCount)
            StartCoroutine(RequestBreadCoroutine());
    }

    IEnumerator RequestBreadCoroutine()
    {
        while(nearPlayer && currentBreadCount < maxBreadCount)
        {
            EventManager.SellBoxRequestBread();
            yield return new WaitForSeconds(1f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            nearPlayer = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            nearPlayer = false;
        }
    }

    void ReceiveBreadFromPlayer(GameObject receivedBread)
    {
        if (currentBreadCount >= maxBreadCount)
            return;

        currentBreadCount++;
        breads.Add(receivedBread);
        //SortBreads();

        StartCoroutine(MoveBreadToSlot(receivedBread));
    }

    IEnumerator MoveBreadToSlot(GameObject bread)
    {
        Vector3 startPos = bread.transform.position;

        //빵의 목표 위치
        int maxPerRow = 4;
        float breadWidth = 0.5f;
        float breadHeight = 0.5f;

        int row = (currentBreadCount - 1) % maxPerRow;
        int floor = (currentBreadCount - 1) / maxPerRow;

        Vector3 targetPos = breadSlot.position
        + new Vector3(row * breadWidth,  // 가로 위치
                      floor * breadHeight, // 세로 위치 (아래로 이동)
                      0); // Z 위치는 변하지 않음

        float elapsedTime = 0f;
        float moveDuration = 0.5f;

        while(elapsedTime < moveDuration)
        {
            bread.transform.position = Vector3.Lerp(startPos, targetPos, (elapsedTime / moveDuration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        bread.transform.position = targetPos;
        bread.transform.rotation = Quaternion.Euler(0, 0, 0);
        bread.transform.SetParent(breadSlot);
    }

    void checkCustomerRequest(int count)
    {
        if(count <= breads.Count)
        {
            List<GameObject> breadsToGive = new List<GameObject>();
            for(int i=0; i<count; i++)
            {
                GameObject bread = breads[breads.Count-1];
                breadsToGive.Add(bread);
                breads.RemoveAt(breads.Count-1);
            }
            EventManager.CustomerReceiveBreads(breadsToGive);
        }
        else
        {
            Debug.Log("Not enough breads available");
            EventManager.SellboxHaveNotEnoughBread();
        }
    }

}
