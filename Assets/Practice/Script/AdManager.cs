using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdManager : MonoBehaviour
{
    public GameObject menuPage;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleMenu()
    {
        if(menuPage.activeSelf == false)
        {
            menuPage.SetActive(true);

            Time.timeScale = 0f;
        }
    }

    public void exitMenu()
    {
        Time.timeScale = 1f;

        menuPage.SetActive(false);
    }
}
