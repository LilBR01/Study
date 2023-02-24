using System;
using UnityEngine;

[Serializable]
public class UIPanelinfo : ISerializationCallbackReceiver
{
    [NonSerialized]
    public UIPanelType panelType;

    public string panelTypeString;
    public string path;

    //序列化
    public void OnBeforeSerialize()
    {

    }

    //反序列化，从文本到对象
    public void OnAfterDeserialize()
    {
        //使用Parse方法将字符串转换成枚举对象
        var type = (UIPanelType)System.Enum.Parse(typeof(UIPanelType), panelTypeString);
        panelType = type;
    }
}
