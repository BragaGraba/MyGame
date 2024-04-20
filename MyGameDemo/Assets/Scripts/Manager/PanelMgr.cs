using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PanelMgr : MonoBehaviour
{
    public static PanelMgr instance;

    private GameObject canvas;

    public Dictionary<string, PanelBase> dict;

    private Dictionary<PanelLayer, Transform> layerDict;

    public void Awake()
    {
        instance = this;
        InitLayer();
        dict = new Dictionary<string, PanelBase>();
    }

    private void InitLayer()
    {
        // canvas
        canvas = GameObject.Find("Canvas");
        if (canvas == null)
            Debug.Log("PanelMgr.InitLayer fail, canvas is null");

        // 各个层级
        layerDict = new Dictionary<PanelLayer, Transform>();

        foreach (PanelLayer pl in Enum.GetValues(typeof(PanelLayer)))
        {
            string name = pl.ToString();
            Transform transform = canvas.transform.Find(name);
            layerDict.Add(pl, transform);
        }
    }

    // 打开面板
    public void OpenPanel<T>(string skinPath, params object[] args) where T : PanelBase
    {
        // 已经打开
        string name = typeof(T).ToString();
        if (dict.ContainsKey(name))
            return;

        // 面板脚本
        PanelBase panel = canvas.AddComponent<T>();
        panel.Init(args);
        dict.Add(name, panel);

        // 加载皮肤
        skinPath = (skinPath != "" ? skinPath : panel.skinPath);
        GameObject skin = Resources.Load<GameObject>(skinPath); // 根据skin Path加载皮肤资源。Resources.Load后面的参数代表资源相对于Resources目录下的路径
        if (skin == null)
            Debug.LogError("panelMgr.OpenPanel fail, skil is null, skinPath = " + skinPath);
        panel.skin = (GameObject)Instantiate(skin);

        // 设置坐标
        Transform skinTrans = panel.skin.transform;
        PanelLayer layer = panel.layer;
        Transform parent = layerDict[layer];
        skinTrans.SetParent(parent, false);
        // 生命周期开始
        panel.OnShowing();
        // anim
        panel.OnShowed();
    }

    // 关闭
    public void ClosePanel(string name)
    {
        PanelBase panel = (PanelBase)dict[name];
        if (panel == null)
            return;

        panel.OnClosing();
        dict.Remove(name);
        panel.OnClosed();
        GameObject.Destroy(panel.skin);
        Component.Destroy(panel);
    }

    public enum PanelLayer
    {
        Panel,
        Tips,
    }
}
