using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OvenBasket : MonoBehaviour
{
    public List<GameObject> breads = new List<GameObject>();

    int currentBreadCount = 0;
    int maxBreadCount = 10;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(RequestBreads());
        EventManager.OnBreadBaked += AddBreadToList;
        EventManager.OnPlayerBreadRequest += GivePlayerBreads;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator RequestBreads()
    {
        while (currentBreadCount <= maxBreadCount)
        {
            EventManager.RequestBread();
            currentBreadCount++;
            yield return new WaitForSeconds(1f);
        }
    }

    void AddBreadToList(GameObject bread)
    {
        if(!breads.Contains(bread))
        {
            breads.Add(bread);
        }
    }

    void GivePlayerBreads(int amount)
    {
        List<GameObject> breadsToGive = new List<GameObject>();
        int breadsToGiveCount = amount;
        if(breadsToGiveCount > breads.Count)
        {
            breadsToGiveCount = breads.Count;
        }
        if(breadsToGiveCount <= 0)
        {
            Debug.Log("No breads availabe to give");
            return;
        }

        for(int i=0; i< breadsToGiveCount; i++)
        {
            GameObject bread = breads[0];
            breads.RemoveAt(0);
            currentBreadCount--;

            Rigidbody breadRb = bread.GetComponent<Rigidbody>();
            breadRb.isKinematic = true;
            breadRb.useGravity = false;

            Collider breadCol = bread.GetComponent<Collider>();
            breadCol.enabled = false;

            breadsToGive.Add(bread);
            //Destroy(breads[0]);
        }

        EventManager.DeliverBreadsToPlayer(breadsToGive);
    }

}
