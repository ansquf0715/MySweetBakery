using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static event Action OnRequestBake; //�� ������ �̺�Ʈ
    public static event Action<GameObject> OnBreadBaked; //���� �������� �� ������ �̺�Ʈ

    public static event Action<bool> OnPlayerNearOven; //player�� oven ��ó��� �̺�Ʈ
    public static event Action<int> OnPlayerBreadRequest; //player�� ���� �޶�� ��û�ϴ� �̺�Ʈ
    public static event Action<List<GameObject>> OnPlayerReceiveBreads; //player�� ���� �޴� �̺�Ʈ

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
