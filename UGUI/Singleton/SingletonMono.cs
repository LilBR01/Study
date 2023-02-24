using UnityEngine;

// 定义一个泛型类，继承自MonoBehaviour，约束泛型参数T必须是SingletonMono<T>的子类
public class SingletonMono<T> : MonoBehaviour where T : SingletonMono<T>
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = FindObjectOfType<T>();
            }

            if(_instance == null)
            {
                GameObject obj = new GameObject(typeof(T).Name);
                _instance = obj.AddComponent<T>();
            }
            return _instance;
        }
    }

    //// 定义一个虚拟的公有方法，用于初始化单例实例
    public virtual void Init()
    {
        // 可以在子类中重写该方法，实现自定义的初始化逻辑
    }

    // 定义一个私有的Awake方法，用于在脚本实例被加载时执行
    private void Awake()
    {
        // 如果单例实例为空，就将当前组件赋值给它
        if(_instance == null)
        {
            _instance = this as T;
        }
        // 调用初始化方法
        Init();
    }
}
