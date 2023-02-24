using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : SingletonMono<UIManager>
{
    //存储所有的ui面板
    private Dictionary<UIPanelType, string> panelPathDic;
    private Dictionary<UIPanelType, BasePanel> panelDic;
    //用于存储激活的ui面板
    private Stack<BasePanel> panelStack = new Stack<BasePanel>();
    
    public Transform canvasTransform;
    private Transform CansvasTransform
    {
        get
        {
            if(canvasTransform == null)
                canvasTransform = GameObject.Find("Canvas").transform;
            return canvasTransform;
        }
    }
    
    private UIManager(){ParseUIPanelTypeJson();}

    public void PushPanel(UIPanelType panelType)
    {
        if(panelStack == null)
        {
            panelStack = new Stack<BasePanel>();       
        }

        if(panelStack.Count > 0)
        {
            BasePanel topPanel = panelStack.Peek();
            topPanel.OnPause();
        }

        BasePanel panel = GetPanel(panelType);
        panel.OnEnter();
        panelStack.Push(panel);
    }

    public void PopPanel()
    {
        if(panelStack == null)
        {
            panelStack = new Stack<BasePanel>();
        }
        if(panelStack.Count <= 0)   return;

        BasePanel basePanel = panelStack.Pop();
        basePanel.OnExit();

        if(panelStack.Count <= 0)   return;
        BasePanel topPanel = panelStack.Peek();
        topPanel.OnResume();
    }

    private BasePanel GetPanel(UIPanelType panelType)
    {
        if(panelDic == null)
        {
            panelDic = new Dictionary<UIPanelType, BasePanel>();
        }

        panelDic.TryGetValue(panelType, out var panel);

        if(panel == null)
        {
            panelPathDic.TryGetValue(panelType, out var path);

            GameObject initPanel = Instantiate(Resources.Load(path)) as GameObject;
            initPanel.transform.SetParent(canvasTransform, false);

            panelDic.Add(panelType, initPanel.GetComponent<BasePanel>());
            return initPanel.GetComponent<BasePanel>();
        }
        else
        {
            return panel;
        }
    }

    [SerializeField]
    class UIPanelTypeJson
    {
        public List<UIPanelinfo> infoList;
    }

    //需在Resource文件夹下创建json文件
    private void ParseUIPanelTypeJson()
    {
        panelPathDic = new Dictionary<UIPanelType, string>();
        TextAsset textAsset = Resources.Load<TextAsset>("Json/UIPanels");
        UIPanelTypeJson jsObject = JsonUtility.FromJson<UIPanelTypeJson>(textAsset.text);
        foreach(UIPanelinfo info in jsObject.infoList)
        {
            panelPathDic.Add(info.panelType, info.path);
        } 
    }
}
