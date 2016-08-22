using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pool : MonoBehaviour
{
    [SerializeField]
    private PoolableObject m_PooledPrefab;

    [SerializeField]
    private int m_Amount;

    private List<PoolableObject> m_PooledObjects;

    private void Start()
    {
        if (m_PooledObjects == null)
            m_PooledObjects = new List<PoolableObject>();

        Clear();
        AddPooledObjects(m_Amount);
    }

    private void AddPooledObjects(int count)
    {
        for (int i = 0; i < count; ++i)
        {
            AddPooledObject();
        }
    }

    private PoolableObject AddPooledObject()
    {
        PoolableObject instance = Instantiate(m_PooledPrefab) as PoolableObject;

        if (instance == null)
        {
            Destroy(instance);
            throw new MissingComponentException("Component PoolableObject was not found on the prefab " + m_PooledPrefab.ToString());
        }
        else
        {
            instance.gameObject.transform.parent = this.transform;
            instance.Initialize();
            instance.Deactivate();
            m_PooledObjects.Add(instance);
            return instance;
        }
    }

    public PoolableObject ActivateAvailableObject()
    {
        PoolableObject pooledObject = GetAvailableObject();
        pooledObject.Activate();
        return pooledObject;
    }

    private PoolableObject GetAvailableObject()
    {
        for (int i = 0; i < m_PooledObjects.Count; ++i)
        {
            if (m_PooledObjects[i].IsAvailable())
            {
                return m_PooledObjects[i];
            }
        }

        //If there is no object available, we DOUBLE the pool!
        int firstNewIndex = m_PooledObjects.Count;
        AddPooledObjects(m_PooledObjects.Count);

        return m_PooledObjects[firstNewIndex];
    }

    private void Clear()
    {
        if (m_PooledObjects.Count != 0)
        {
            for (int i = 0; i < m_PooledObjects.Count; ++i)
            {
                Destroy(m_PooledObjects[i].gameObject);
            }
        }
    }

    public void ResetAll()
    {
        for (int i = 0; i < m_PooledObjects.Count; ++i)
        {
            m_PooledObjects[i].Deactivate();
        }
    }

    //FIX
    //Workaround because generics cannot be MonoBehaviours
    public bool IsPoolType<T>()
    {
        return (m_PooledPrefab is T);
    }
}
