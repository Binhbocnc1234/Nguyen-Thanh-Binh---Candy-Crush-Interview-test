using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    [Header("Singleton")]
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();

                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject(typeof(T).Name);
                    _instance = singletonObject.AddComponent<T>();
                }
            }

            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null){
            _instance = this as T;
        }
        else{
            Debug.LogError("Detect multiple singleton: " + typeof(T).Name);
            Destroy(gameObject); // Prevent duplicates
        }
    }
}
