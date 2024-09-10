using UnityEngine;

/// <summary>
/// This is a funny class, it is used ensure that our singleton systems are created when the game starts up.
/// To make this magic work, we need to have a Prefab named "Bootstrapper" in the Resources folder.
/// </summary>
public static class Bootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Execute()
    {
        Object.DontDestroyOnLoad(Object.Instantiate(Resources.Load("Bootstrapper")));
    }
}