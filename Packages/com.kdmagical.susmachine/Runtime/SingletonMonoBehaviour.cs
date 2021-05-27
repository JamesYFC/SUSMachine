using UnityEngine;

public class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
{
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                T[] currentInstances = Object.FindObjectsOfType(typeof(T)) as T[];
                if (currentInstances.Length != 0)
                {
                    if (currentInstances.Length == 1)
                    {
                        instance = currentInstances[0];
                    }
                    else
                    {
                        Debug.LogError("Class " + typeof(T).Name + " exists multiple times in violation of singleton pattern. Returning first copy and deleting the rest...");
                        instance = currentInstances[0];

                        for (int i = 1; i < currentInstances.Length; i++)
                        {
                            Destroy(currentInstances[i]);
                        }
                    }
                }
                else
                {
                    CreateNewSingletonInstance();
                }

            }
            return instance;
        }
        set
        {
            instance = value as T;
        }
    }

    private static void CreateNewSingletonInstance()
    {
        var go = new GameObject(typeof(T).Name, typeof(T));
        DontDestroyOnLoad(go);
        instance = go.GetComponent<T>();
    }

    private static T instance;
}

