using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton
{
    // 私有的静态字段，用于存储单例实例
    public static Singleton instance;

    // 私有的构造函数，防止外部创建实例
    private Singleton()
    {

    }

    // 公共的静态属性，用于获取单例实例
    public static Singleton Instance
    {
        get
        {
            if(instance == null)
            {
                instance = new Singleton();
            }
            return instance;
        }
    }
}
