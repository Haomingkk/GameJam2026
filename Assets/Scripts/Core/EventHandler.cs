using System;
using UnityEngine;
public static class EventHandler
{
    public static event Action<Sprite> InstantiateMonsterFace;
    public static void CallInstantiateMonsterFace(Sprite monsterFace)
    {
        InstantiateMonsterFace?.Invoke(monsterFace);
    }
}

