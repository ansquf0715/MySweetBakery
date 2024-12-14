using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Oven : MonoBehaviour
{
    public GameObject breadPrefab;
    public Transform basket;
    Collider basketCol;

    public ParticleSystem bakeParticlePrefab;
    ParticleSystem bakeParticle;

    [SerializeField]
    int maxBreadCount = 10;
    int currentBreadCount = 0;
    float bakeInterval = 1.0f;
    float moveDuration = 1.0f;

    float moveSpeed = 2f;

    Vector3 breadSpawnPos = new Vector3(4f, 1.8f, -5f);
    List<GameObject> breads = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        EventManager.OnPlayerBreadRequest += GivePlayerBreads;

        basket = transform.Find("Basket");
        basketCol = basket.GetComponent<Collider>();

        StartCoroutine(bakeBreads());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            EventManager.SetPlayerNearOven(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            EventManager.SetPlayerNearOven(false);
        }
    }

    void Bake()
    {
        bakeParticle = Instantiate(bakeParticlePrefab, new Vector3(5f, 2f, -5f), Quaternion.identity);
        bakeParticle.time = 5f;
        bakeParticle.Play();

        GameObject bread = Instantiate(breadPrefab,
            breadSpawnPos, Quaternion.identity);
        //currentBreadCount++;
        StartCoroutine(MoveBreadToBasket(bread));
    }

    IEnumerator MoveBreadToBasket(GameObject bread)
    {
        float targetX = basket.position.x;
        Vector3 currentPos = bread.transform.position;

        Rigidbody breadRB = bread.GetComponent<Rigidbody>();
        breadRB.isKinematic = true;

        while(Mathf.Abs(bread.transform.position.x - targetX) > 0.1f)
        {
            float newX = Mathf.MoveTowards(bread.transform.position.x,
                targetX, moveSpeed * Time.deltaTime);
            bread.transform.position = new Vector3(newX, currentPos.y, currentPos.z);
            yield return null;
        }

        breadRB.isKinematic = false;
        yield return new WaitForSeconds(0.5f);
        if(!breads.Contains(bread))
        {
            breads.Add(bread);
            currentBreadCount++;
        }
    }

    IEnumerator bakeBreads()
    {
        while(true)
        {
            if(currentBreadCount < maxBreadCount)
            {
                Bake();
                yield return new WaitForSeconds(bakeInterval);
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
        }
    }

    void GivePlayerBreads()
    {
        if(breads.Count > 0)
        {
            GameObject bread = breads[0];
            breads.RemoveAt(0);
            currentBreadCount--;

            EventManager.DeliverBreadToPlayer(bread);
        }

    }

}
