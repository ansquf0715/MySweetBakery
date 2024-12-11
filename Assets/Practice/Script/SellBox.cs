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

    // Start is called before the first frame update
    void Start()
    {
        breadSlot = transform.Find("BreadSortSlot");
        EventManager.OnPlayerGiveBreadToSellBox += ReceiveBreadFromPlayer;
    }

    // Update is called once per frame
    void Update()
    {
        if (nearPlayer && currentBreadCount < maxBreadCount)
            EventManager.SellBoxRequestBread();
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
        SortBreads();
    }

    void SortBreads()
    {
        int maxPerRow = 4; // �� ���� �ִ� 4���� ��
        float breadWidth = 0.5f; // ���� ���� ũ��
        float breadHeight = 0.5f; // ���� ���� ũ��

        Vector3 startPos = breadSlot.position; // breadSlot ��ġ�� ���� ������ ���� ��� ��ġ

        int rowCount = 0; 
        int floor = 0; 

        for (int i = 0; i < breads.Count; i++)
        {
            if (rowCount >= maxPerRow)
            {
                rowCount = 0; 
                floor++; 
            }

            Vector3 breadPos = new Vector3(startPos.x + rowCount * breadWidth + (breadWidth / 2), // ���� ���� ũ�⸸ŭ ���������� �̵�, ��� ����
                                           startPos.y + floor * breadHeight, // ������ �Ʒ���, �� ���� �� ���� ���� ������ �̵�
                                           startPos.z + (breadWidth *1.5f)); // Z ��ġ�� ������ ����

            breads[i].transform.position = breadPos;
            breads[i].transform.rotation = Quaternion.Euler(0, 0, 0); // ȸ���� 0���� ����

            breads[i].transform.SetParent(breadSlot);

            rowCount++;
        }
    }
}
