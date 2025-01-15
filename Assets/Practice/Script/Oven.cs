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

    [SerializeField]
    int maxBreadCount = 10;
    int currentBreadCount = 0;
    float bakeInterval = 1.0f;

    float moveSpeed = 2f;

    Vector3 breadSpawnPos = new Vector3(4f, 1.8f, -5f);
    public List<GameObject> breads = new List<GameObject>();

    int initialPoolSize = 10;
    Queue<GameObject> breadPool = new Queue<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        EventManager.OnPlayerBreadRequest += GivePlayerBreads;
        //EventManager.OnReturnBreads += ReturnBreadToPool;

        basket = transform.Find("Basket");
        basketCol = basket.GetComponent<Collider>();

        //initializeBreadPool();
        StartCoroutine(bakeBreads());
    }

    // Update is called once per frame
    void Update()
    {

    }

    void initializeBreadPool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject bread = Instantiate(breadPrefab);
            bread.SetActive(false);
            breadPool.Enqueue(bread);
        }
    }

    GameObject GetBreadFromPool()
    {
        if (breadPool.Count > 0)
        {
            GameObject bread = breadPool.Dequeue();
            bread.SetActive(true);
            return bread;
        }
        else
        {
            GameObject bread = Instantiate(breadPrefab);
            return bread;
        }
    }

    public void ReturnBreadToPool(GameObject bread)
    {
        bread.SetActive(false);
        breadPool.Enqueue(bread);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            EventManager.OnArrowAction(1);
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
        ParticleSystem bakeParticle = Instantiate(bakeParticlePrefab, new Vector3(5f, 2f, -5f), Quaternion.identity);
        //bakeParticle.time = 5f;
        //bakeParticle.Play();

        GameObject bread = Instantiate(breadPrefab,
            breadSpawnPos, Quaternion.identity);
        //StartCoroutine(MoveBreadToBasket(bread, bakeParticle));

        //Debug.Log("Bake");
        //GameObject bread = GetBreadFromPool();
        //if (bread == null)
        //    Debug.Log("bread is null");
        bread.transform.position = breadSpawnPos;
        //Destroy(bakeParticle);

        StartCoroutine(MoveBreadToBasket(bread, bakeParticle));
    }

    IEnumerator MoveBreadToBasket(GameObject bread, ParticleSystem bake)
    {
        Destroy(bake);
        float targetX = basket.position.x + 0.7f;
        Vector3 currentPos = bread.transform.position;

        Rigidbody breadRB = bread.GetComponent<Rigidbody>();
        //breadRB.isKinematic = true;

        while(Mathf.Abs(bread.transform.position.x - targetX) > 0.1f)
        {
            float newX = Mathf.MoveTowards(bread.transform.position.x,
                targetX, moveSpeed * Time.deltaTime);
            bread.transform.position = new Vector3(newX, currentPos.y, currentPos.z);
            yield return null;
        }
        //breadRB.isKinematic = false;
        yield return new WaitForSeconds(0.5f);
        if(!breads.Contains(bread))
        {
            breads.Add(bread);
            currentBreadCount++;
            Destroy(bake);
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
