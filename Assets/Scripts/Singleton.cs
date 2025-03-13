using UnityEngine;

public class Singleton <T>: GameBehaviour where T : GameBehaviour
{
    private static T _instance;
    public static T instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = FindFirstObjectByType<T>();
                if(_instance == null ) 
                {
                    GameObject singleton = new GameObject(typeof(T).Name);
                    singleton.AddComponent<T>();
                }
            }
            return _instance;
        }
    }
    protected virtual void Awake()
    {
        if (_instance == null)
            _instance = this as T;
        else
            Destroy(gameObject);
    }
}
