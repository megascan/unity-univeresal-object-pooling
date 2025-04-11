using System.Collections;
using UnityEngine;

public class PoolingTuple
{
    public GameObject prefab;
    public PoolingMetadata metadata;
    public bool isActive;

    public PoolingTuple(GameObject prefab, PoolingMetadata metadata, bool isActive)
    {
        this.prefab = prefab;
        this.metadata = metadata;
        this.isActive = isActive;
    }
}

public class PoolingMetadata : MonoBehaviour
{
    [Header("Object Pooling")]
    [SerializeField]
    public string objectUniqueIdentifier;
    public PoolingTuple tuple;

    public void Release(float delay = 0f)
    {
        if (!tuple.isActive)
            return;

        if (delay <= 0f)
        {
            ReleaseImmediate();
        }
        else
        {
            StartCoroutine(DelayedRelease(delay));
        }
    }

    private void ReleaseImmediate()
    {
        tuple.isActive = false;
        tuple.prefab.SetActive(false);
    }

    private IEnumerator DelayedRelease(float delay)
    {
        yield return new WaitForSeconds(delay);
        ReleaseImmediate();
    }
}
