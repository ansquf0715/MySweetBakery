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

    // Start is called before the first frame update
    void Start()
    {
        breadSlot = transform.Find("BreadSortSlot");

        EventManager.OnPlayerGiveBreadToSellBox += ReceiveBreadsFromPlayer;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            int requestCount = maxBreadCount - currentBreadCount;
            EventManager.SellBoxRequestBread(requestCount);
        }
    }

    void ReceiveBreadsFromPlayer(List<GameObject> receivedBreads)
    {
        int space = maxBreadCount - currentBreadCount;

        for(int i=0; i<receivedBreads.Count; i++)
        {
            if(currentBreadCount >= maxBreadCount)
            {
                Debug.Log("빵 자리 없어요옹");
                break;
            }

            currentBreadCount++;

            breads.Add(receivedBreads[i]);
        }

        Debug.Log("현재 빵 갯수" + currentBreadCount);
        SortBreads();
    }

    void SortBreads()
    {
        int maxPerRow = 4; // 한 층에 최대 4개의 빵
        float breadWidth = 0.5f; // 빵의 가로 크기
        float breadHeight = 0.5f; // 빵의 세로 크기

        Vector3 startPos = breadSlot.position; // breadSlot 위치는 정렬 시작의 왼쪽 상단 위치

        int rowCount = 0; 
        int floor = 0; 

        for (int i = 0; i < breads.Count; i++)
        {
            if (rowCount >= maxPerRow)
            {
                rowCount = 0; 
                floor++; 
            }

            Vector3 breadPos = new Vector3(startPos.x + rowCount * breadWidth + (breadWidth / 2), // 빵의 가로 크기만큼 오른쪽으로 이동, 가운데 정렬
                                           startPos.y + floor * breadHeight, // 위에서 아래로, 각 층이 다 차면 다음 층으로 이동
                                           startPos.z + (breadWidth *1.5f)); // Z 위치는 변하지 않음

            breads[i].transform.position = breadPos;
            breads[i].transform.rotation = Quaternion.Euler(0, 0, 0); // 회전값 0으로 설정

            breads[i].transform.SetParent(breadSlot);

            rowCount++;
        }
    }
}
