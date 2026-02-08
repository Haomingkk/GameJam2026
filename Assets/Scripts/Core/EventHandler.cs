using System;
using UnityEngine;
public static class EventHandler
{
    public static event Action<Sprite> InstantiateMonsterFace;
    public static void CallInstantiateMonsterFace(Sprite monsterFace)
    {
        InstantiateMonsterFace?.Invoke(monsterFace);
    }

    public static event Action<Transform> NotifyActiveMaskB;
    public static void CallNotifyActiveMaskB(Transform player)
    {
        NotifyActiveMaskB?.Invoke(player);
    }

    public static event Action<Transform> NotifyDeactiveMaskB;
    public static void CallNotifyDeactiveMaskB(Transform player)
    {
        NotifyDeactiveMaskB?.Invoke(player);
    }

    public static event Action MonsterChaseEnter;
    public static void CallMonsterChaseEnter()
    {
        MonsterChaseEnter?.Invoke();
    }

    public static event Action MonsterChaseExit;
    public static void CallMonsterChaseExit()
    {
        MonsterChaseExit?.Invoke();
    }
}

