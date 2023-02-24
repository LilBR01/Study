using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

 
public abstract class BasePanel : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    public CanvasGroup canvasGroupValue
    {
        get
        {
            return canvasGroup;
        }
        set
        {
            canvasGroup = value;
        }
    }

    public void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    //UI进入时执行的操作，只会执行一次
    public virtual void OnEnter()
    {
        canvasGroup.alpha = 1;
    }

    //UI暂停时执行的操作
    public virtual void OnPause()
    {
        canvasGroup.blocksRaycasts = false;
    }

    //UI继续时执行的操作
    public virtual void OnResume()
    {
        canvasGroup.blocksRaycasts = true;
    }

    //UI退出时执行的操作
    public virtual void OnExit()
    {
        canvasGroup.alpha = 0;
    }

    public virtual void OnClose()
    {
        UIManager.Instance.PopPanel();
    }

    //获取鼠标停留位置下的UI元素
    protected GameObject GetOverUI(GameObject canvas)
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;
        GraphicRaycaster gr = canvas.GetComponent<GraphicRaycaster>();
        List<RaycastResult> results = new List<RaycastResult>();
        gr.Raycast(pointerEventData, results);
        if(results.Count != 0)
        {
            return results[0].gameObject;
        }
        return null;
    }

    //查找gameobject的所有子对象中名为elementname的ui元素
    protected GameObject FindUIElement(string elementName)
    {
        GameObject returnGameObject = null;
        foreach(var child in gameObject.GetComponentsInChildren<Transform>(true))
        {
            if(child.name.Equals(elementName))
            {
                returnGameObject = child.gameObject;
            }
        }
        return returnGameObject;
    }
}
