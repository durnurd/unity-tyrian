using System.Collections;
using UnityEngine;

public class CoroutineRunner : MonoBehaviour
{
    private static CoroutineRunner _instance;
    public static CoroutineRunner Instance
    {
        get
        {
            if ((System.Object)_instance == null)
                _instance = new GameObject("CoroutineRunner").AddComponent<CoroutineRunner>();
            return _instance;
        }
    }

    public static Coroutine Run(IEnumerator coroutine)
    {
        return Instance.StartCoroutine(coroutine);
    }
}
