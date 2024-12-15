using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SellBox : MonoBehaviour
{
    public List<GameObject> breads = new List<GameObject>();
    public List<bool> breadReady = new List<bool>();

    Transform breadSlot;
    Vector3 breadSpacing = new Vector3(0.5f, 0f, 0f);
    float breadHeightOffset = 0.5f;

    int maxBreadCount = 12;
    int currentBreadCount = 0;

    bool nearPlayer = false;
    bool isProcessingCustomer = false;
    List<Customer> enteredCustomer = new List<Customer>();

    AudioSource audioSource;
    public AudioClip putBreadSound;

    bool customerIsRequesting = false;

    // Start is called before the first frame update
    void Start()
    {
        breadSlot = transform.Find("BreadSortSlot");
        EventManager.OnPlayerGiveBreadToSellBox += ReceiveBreadFromPlayer;

        GameObject gameManager = GameObject.Find("GameManager");
        audioSource = gameManager.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (nearPlayer && currentBreadCount < maxBreadCount)
            StartCoroutine(RequestBreadCoroutine());
    }

    public GameObject CustomerRequestBread(Customer customer)
    {
        if (enteredCustomer.Contains(customer)) 
        {
            for (int i = breads.Count - 1; i >= 0; i--)
            {
                if (breadReady[i])
                {
                    currentBreadCount--;
                    GameObject bread = breads[i];
                    breads.RemoveAt(i);
                    breadReady.RemoveAt(i);

                    return bread;
                }
            }
        }
        return null; 
    }

    IEnumerator RequestBreadCoroutine()
    {
        while (nearPlayer && currentBreadCount < maxBreadCount)
        {
            EventManager.SellBoxRequestBread();
            yield return new WaitForSeconds(1f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            EventManager.OnArrowAction(2);
            nearPlayer = true;
        }
        if (other.gameObject.CompareTag("Customer"))
        {
            Customer cust = other.gameObject.GetComponent<Customer>();
            enteredCustomer.Add(cust);
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
        breadReady.Add(false);

        audioSource.clip = putBreadSound;
        audioSource.loop = true;
        audioSource.Play();

        StartCoroutine(MoveBreadToSlot(receivedBread, breads.Count-1));
    }

    IEnumerator MoveBreadToSlot(GameObject bread, int index)
    {
        Vector3 startPos = bread.transform.position;

        int maxPerRow = 4;
        float breadWidth = 0.5f;
        float breadHeight = 0.5f;

        int row = (currentBreadCount - 1) % maxPerRow;
        int floor = (currentBreadCount - 1) / maxPerRow;

        Vector3 targetPos = breadSlot.position
        + new Vector3(row * breadWidth,
                      floor * breadHeight,
                      0);
        Vector3 midPos = Vector3.Lerp(startPos, targetPos, 0.5f) + new Vector3(0, 1, 0);
        float elapsedTime = 0f;
        float moveDuration = 0.5f;

        while (elapsedTime < moveDuration / 2)
        {
            bread.transform.position = Vector3.Lerp(startPos, midPos, (elapsedTime / (moveDuration / 2)));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        while (elapsedTime < moveDuration)
        {
            bread.transform.position = Vector3.Lerp(midPos, targetPos, ((elapsedTime - (moveDuration / 2)) / (moveDuration / 2)));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        bread.transform.position = targetPos;
        bread.transform.rotation = Quaternion.Euler(0, 0, 0);
        bread.transform.SetParent(breadSlot);

        breadReady[index] = true;

        audioSource.Stop();
    }


    //IEnumerator MoveBreadToSlot(GameObject bread, int index)
    //{
    //    Vector3 startPos = bread.transform.position;

    //    int maxPerRow = 4;
    //    float breadWidth = 0.5f;
    //    float breadHeight = 0.5f;

    //    int row = (currentBreadCount - 1) % maxPerRow;
    //    int floor = (currentBreadCount - 1) / maxPerRow;

    //    Vector3 targetPos = breadSlot.position
    //    + new Vector3(row * breadWidth, 
    //                  floor * breadHeight, 
    //                  0);
    //    float elapsedTime = 0f;
    //    float moveDuration = 0.5f;

    //    while (elapsedTime < moveDuration)
    //    {
    //        bread.transform.position = Vector3.Lerp(startPos, targetPos, (elapsedTime / moveDuration));
    //        elapsedTime += Time.deltaTime;
    //        yield return null;
    //    }

    //    bread.transform.position = targetPos;
    //    bread.transform.rotation = Quaternion.Euler(0, 0, 0);
    //    bread.transform.SetParent(breadSlot);

    //    breadReady[index] = true;

    //    audioSource.Stop();
    //}

    public void customerRemove(Customer customer)
    {
        for(int i=0; i<enteredCustomer.Count; i++)
        {
            if(enteredCustomer[i] == customer)
            {
                enteredCustomer.RemoveAt(i);
            }
        }
    }

}
