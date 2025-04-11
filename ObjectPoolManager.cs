using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    [Header("Object Pooling")]
    [SerializeField]
    private int objectPoolSize = 10;

    public static ObjectPoolManager Instance { get; private set; }

    private Dictionary<string, GameObject> PoolingParentLookup =
        new Dictionary<string, GameObject>();

    private Dictionary<string, List<PoolingTuple>> objectRepository =
        new Dictionary<string, List<PoolingTuple>>();

    private void Awake()
    {
        Instance = this;
    }

    private PoolingTuple getAvailableObject(
        string prefabName,
        GameObject prefab,
        Vector3 transform,
        Quaternion rotation
    )
    {
        if (!objectRepository.ContainsKey(prefabName))
        {
            Debug.LogError("Repository for prefab: " + prefabName + " not found!");
            return null;
        }

        foreach (PoolingTuple tuple in objectRepository[prefabName])
        {
            if (!tuple.isActive)
            {
                tuple.isActive = true;
                return tuple;
            }
        }

        return createNewObject(prefabName, prefab, transform, rotation);
    }

    private PoolingTuple createNewObject(
        string prefabName,
        GameObject prefab,
        Vector3 transform,
        Quaternion rotation
    )
    {
        // Create new instance of prefab
        GameObject obj = Instantiate(prefab, transform, rotation);

        // Get object's metadata
        PoolingMetadata objMetadata = obj.GetComponent<PoolingMetadata>();
        PoolingTuple tuple = new PoolingTuple(obj, objMetadata, false);
        objMetadata.tuple = tuple;
        objectRepository[prefabName].Add(tuple);

        // set GameObject to be child of ObjectPoolManager
        obj.transform.SetParent(PoolingParentLookup[prefabName].transform);

        // Set object to inactive
        obj.SetActive(false);

        return tuple;
    }

    public GameObject New(GameObject prefab, Vector3 transform, Quaternion rotation)
    {
        if (Instance == null)
        {
            Debug.LogError("ObjectPoolManager not found in scene");
            return null;
        }

        PoolingMetadata metadata = prefab.GetComponent<PoolingMetadata>();
        if (metadata == null)
        {
            Debug.LogError(
                "Prefab: "
                    + prefab.name
                    + " was not configured to have a `PoolingMetadata` component!"
            );
            return null;
        }

        string prefabName = metadata.objectUniqueIdentifier;

        // Create the new repository if it doesnt already exist for the prefab
        if (!objectRepository.ContainsKey(prefabName))
        {
            GameObject poolingParent = new GameObject("[ObjectPool] " + prefabName);
            poolingParent.transform.SetParent(this.transform);

            PoolingParentLookup.Add(prefabName, poolingParent);

            Debug.Log("[ObjectPoolManager] Created new repository ID: " + prefabName);
            objectRepository.Add(prefabName, new List<PoolingTuple>());

            for (int i = 0; i < objectPoolSize; i++)
            {
                createNewObject(prefabName, prefab, transform, rotation);
            }
        }

        PoolingTuple returnedTuple = getAvailableObject(prefabName, prefab, transform, rotation);
        returnedTuple.prefab.transform.position = transform;
        returnedTuple.prefab.transform.rotation = rotation;

        return returnedTuple.prefab;
    }
}
