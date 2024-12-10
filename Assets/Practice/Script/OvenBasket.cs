using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OvenBasket : MonoBehaviour
{
    public List<GameObject> breads = new List<GameObject>();

    public int currentBreadCount = 0;
    int maxBreadCount = 10;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(RequestBreads());
        EventManager.OnBreadBaked += AddBreadToList;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator RequestBreads()
    {
        Debug.Log("current bread count" + currentBreadCount);
        Debug.Log("max bread count" +  maxBreadCount);
        while (currentBreadCount <= maxBreadCount)
        {
            Debug.Log("Requesting bread");
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

    //private void OnTriggerEnter(Collider other)
    //{
    //    if(other.gameObject.CompareTag("Bread"))
    //    {
    //        if(!breads.Contains(other.gameObject))
    //        {
    //            breads.Add(other.gameObject);
    //        }
    //    }
    //}

}
