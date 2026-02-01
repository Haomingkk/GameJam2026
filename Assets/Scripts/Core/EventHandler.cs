using System;
using UnityEngine.UI;
public static class EventHandler
{
    public static event Action<Image> InstantiateMonsterFace;
    public static void CallInstantiateMonsterFace(Image monsterFace)
    {
        InstantiateMonsterFace?.Invoke(monsterFace);
    }
}

