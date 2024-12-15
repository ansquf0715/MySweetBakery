using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Counter : MonoBehaviour
{
    public GameObject bagPrefab;
    public AudioClip cashSound;
    AudioSource audioSource;

    //Queue<Customer> waitingCustomers = new Queue<Customer>();
    Customer cashingCustomer;
    bool playerIsCashing = false;
    bool alreadyCashedCustomer = false;
    bool customerReadyToCash = false;

    GameObject bag;

    Collider boxCol;
    Collider circleCol;

    // Start is called before the first frame update
    void Start()
    {
        //EventManager.OnCustomerAtCounter += addCustomer;

        boxCol = GetComponent<BoxCollider>();
        circleCol = GetComponent<SphereCollider>();

        audioSource = GetComponent<AudioSource>();

        //EventManager.OnNextCustomerArrivedAtCounterPosition += handleCustomerArrival;
    }

    // Update is called once per frame
    void Update()
    {
        //if(playerIsCashing && cashingCustomer != null && !alreadyCashedCustomer)
        //{
        //    if(cashingCustomer.isCashingState())
        //    {
        //        checkingOut();
        //        alreadyCashedCustomer=true;
        //    }
        //}
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            EventManager.OnArrowAction(3);
            //playerIsCashing = true;
            //StartCoroutine(delayCashing());
            StartCoroutine(DelayCashing("player"));
        }
        else if(other.gameObject.CompareTag("Customer"))
        {
            cashingCustomer = other.gameObject.GetComponent<Customer>();
            StartCoroutine(DelayCashing("customer"));
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            if(cashingCustomer != null)
            {
                if (cashingCustomer.isCashingState() && !alreadyCashedCustomer
                && playerIsCashing && customerReadyToCash)
                {
                    checkingOut();
                    alreadyCashedCustomer = true;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            playerIsCashing = false;
        }
        if (other.gameObject.CompareTag("Customer"))
        {
            customerReadyToCash= false;
        }
    }

    IEnumerator DelayCashing(string type)
    {
        yield return new WaitForSeconds(0.5f);
        if(type == "player")
            playerIsCashing=true;
        else if(type == "customer")
        {
            if(cashingCustomer != null)
                customerReadyToCash = true;
        }
    }

    void checkingOut()
    {
        audioSource.PlayOneShot(cashSound);

        alreadyCashedCustomer = true;

        List<GameObject> breads = cashingCustomer.GetBreads();

        bag = Instantiate(bagPrefab,
            new Vector3(0, 1.4f, 1.6f), Quaternion.identity);
        Animator bagAnim = bag.GetComponent<Animator>();
        bagAnim.SetBool("isOpen", true);

        StartCoroutine(MoveBreadsToBag(breads));
    }

    IEnumerator MoveBreadsToBag(List<GameObject> breads)
    {
        while (breads.Count > 0)
        {
            GameObject bread = breads[breads.Count - 1];
            bread.transform.rotation = Quaternion.Euler(0, 90f, 0);

            Vector3 startPos = bread.transform.position;
            Vector3 targetPos = bag.transform.position;
            Vector3 midPos = new Vector3(targetPos.x, targetPos.y + 2, targetPos.z);

            float elapsedTime = 0f;
            float duration = 0.3f;
            float halfDuration = duration / 2;

            while (elapsedTime < halfDuration)
            {
                bread.transform.position = Vector3.Lerp(startPos, midPos, (elapsedTime / halfDuration));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            float secondHalfElapsedTime = 0;
            while (secondHalfElapsedTime < halfDuration)
            {
                bread.transform.position = Vector3.Lerp(midPos, targetPos, (secondHalfElapsedTime / halfDuration));
                secondHalfElapsedTime += Time.deltaTime;
                yield return null;
            }

            bread.transform.position = targetPos;

            cashingCustomer.RemoveBread(bread);

            Destroy(bread);
        }

        Animator bagAnim = bag.GetComponent<Animator>();
        bagAnim.SetBool("isOpen", false);

        cashingCustomer.GetBag(bag);

        StartCoroutine(delayCheckingOut());
    }

    IEnumerator delayCheckingOut()
    {
        yield return new WaitForSeconds(1f);
        //cashingCustomer.SetCustomerCheckOutEnd();
        bag = null;
        cashingCustomer = null;
        alreadyCashedCustomer = false;
    }

}
