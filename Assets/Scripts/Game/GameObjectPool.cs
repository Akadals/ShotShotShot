using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool
{
    private readonly GameObject m_prefab;
    private readonly Transform m_parent;
    private readonly Queue<GameObject> m_objects;
    private readonly HashSet<GameObject> m_pooled;
    private readonly HashSet<GameObject> m_allObjects;

    public GameObjectPool(GameObject _prefab, Transform _parent, int _initialSize)
    {
        m_prefab = _prefab;
        m_parent = _parent;
        m_objects = new Queue<GameObject>(_initialSize);
        m_pooled = new HashSet<GameObject>(_initialSize);
        m_allObjects = new HashSet<GameObject>(_initialSize);
        for (int i = 0; i < _initialSize; i++) m_objects.Enqueue(Create());
    }

    private GameObject Create()
    {
        GameObject instance = Object.Instantiate(m_prefab, m_parent);
        instance.SetActive(false);
        m_pooled.Add(instance);
        m_allObjects.Add(instance);
        return instance;
    }

    public GameObject Get(Vector3 _position, Quaternion _rotation)
    {
        while (m_objects.Count > 0 && m_objects.Peek() == null) m_objects.Dequeue();
        GameObject instance = m_objects.Count > 0 ? m_objects.Dequeue() : Create();
        m_pooled.Remove(instance);
        instance.transform.SetPositionAndRotation(_position, _rotation);
        return instance;
    }

    public void Release(GameObject _instance)
    {
        if (_instance == null || !m_allObjects.Contains(_instance) || !m_pooled.Add(_instance)) return;
        _instance.SetActive(false);
        _instance.transform.SetParent(m_parent);
        m_objects.Enqueue(_instance);
    }

    public void ReleaseAll()
    {
        foreach (GameObject instance in m_allObjects)
            if (instance != null && !m_pooled.Contains(instance)) Release(instance);
    }
}
