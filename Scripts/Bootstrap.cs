using UnityEngine;

public static class Bootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        if (GameObject.Find("RunToFinish") != null) return;
        var go = new GameObject("RunToFinish");
        go.AddComponent<Game>();
    }
}
