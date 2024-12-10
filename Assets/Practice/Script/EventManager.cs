using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static event Action OnRequestBake; //빵 구우라는 이벤트
    public static event Action<GameObject> OnBreadBaked; //빵이 구워졌을 때 들어오는 이벤트

    public static event Action<bool> OnPlayerNearOven; //player가 oven 근처라는 이벤트
    public static event Action<int> OnPlayerBreadRequest; //player가 빵을 달라고 요청하는 이벤트
    public static event Action<List<GameObject>> OnPlayerReceiveBreads; //player가 빵을 받는 이벤트

    public static void RequestBread()
    {
        OnRequestBake?.Invoke();
    }

    public static void BreadBaked(GameObject bread)
    {
        OnBreadBaked?.Invoke(bread);
    }

    public static void SetPlayerNearOven(bool isNear)
    {
        OnPlayerNearOven?.Invoke(isNear);
    }

    public static void PlayerBreadRequest(int amount)
    {
        OnPlayerBreadRequest?.Invoke(amount);
    }

    public static void DeliverBreadsToPlayer(List<GameObject> bread)
    {
        OnPlayerReceiveBreads?.Invoke(bread);
    }
}
