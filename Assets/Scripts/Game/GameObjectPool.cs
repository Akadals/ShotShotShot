using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool
{
    private readonly GameObject prefab;
    private readonly Transform parent;
    private readonly Queue<GameObject> objects;
    private readonly HashSet<GameObject> pooled;
    private readonly HashSet<GameObject> allObjects;

    public GameObjectPool(GameObject prefab, Transform parent, int initialSize)
    {
        this.prefab = prefab;
        this.parent = parent;
        objects = new Queue<GameObject>(initialSize);
        pooled = new HashSet<GameObject>(initialSize);
        allObjects = new HashSet<GameObject>(initialSize);
        for (int i = 0; i < initialSize; i++) objects.Enqueue(Create());
    }

    private GameObject Create()
    {
        GameObject instance = Object.Instantiate(prefab, parent);
        instance.SetActive(false);
        pooled.Add(instance);
        allObjects.Add(instance);
        return instance;
    }

    public GameObject Get(Vector3 position, Quaternion rotation)
    {
        while (objects.Count > 0 && objects.Peek() == null) objects.Dequeue();
        GameObject instance = objects.Count > 0 ? objects.Dequeue() : Create();
        pooled.Remove(instance);
        instance.transform.SetPositionAndRotation(position, rotation);
        return instance;
    }

    public void Release(GameObject instance)
    {
        if (instance == null || !allObjects.Contains(instance) || !pooled.Add(instance)) return;
        instance.SetActive(false);
        instance.transform.SetParent(parent);
        objects.Enqueue(instance);
    }

    public void ReleaseAll()
    {
        foreach (GameObject instance in allObjects)
        {
            if (instance != null && !pooled.Contains(instance)) Release(instance);
        }
    }
}
