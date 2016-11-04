using UnityEngine;
using System.Collections;

public class CacheManager : MonoBehaviour
{
    public static CacheManager Instance { get; private set; }

    //public static void Store(string label, GameObject obj)
    //{
    //    Instance.Store_Impl(label, obj);
    //}

    public static void Store(string label, GameObject obj, float timeDelay = 0)
    {
        Instance.Store_Impl(label, obj, timeDelay);
    }

    public static GameObject ActivateRandom(string label)
    {
        return Instance.ActivateRandom_Impl(label);
    }

    private GameObject cacheObject;

    void Awake()
    {
        Instance = this;
        cacheObject = new GameObject("Cache");
    }

    private GameObject CreateNewBucket(string label)
    {
        var obj = new GameObject(label);
        obj.transform.parent = cacheObject.transform;
        return obj;
    }

    //private void Store_Impl(string label, GameObject obj)
    //{
    //    var container = cacheObject.transform.Find(label);
    //    if (container == null)
    //    {
    //        container = CreateNewBucket(label).transform;
    //    }
    //    obj.transform.parent = container;
    //    obj.SetActive(false);
    //}

    private void Store_Impl(string label, GameObject obj, float timeDelay)
    {
        StartCoroutine(IEnumStore_Impl(label, obj, timeDelay));
    }

    private IEnumerator IEnumStore_Impl(string label, GameObject obj, float timeDelay)
    {
        yield return new WaitForSeconds(timeDelay);

        var container = cacheObject.transform.Find(label);
        if (container == null)
        {
            container = CreateNewBucket(label).transform;
        }
        obj.transform.parent = container;
        obj.SetActive(false);
    }

    private GameObject ActivateRandom_Impl(string label)
    {
        var container = cacheObject.transform.Find(label);
        if (container == null) return null;

        var count = container.childCount;
        if (count <= 0) return null;

        var obj = container.GetChild(Random.Range(0, count)).gameObject;
        obj.transform.parent = null;
        obj.SetActive(true);

        return obj;
    }
}
