using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelBase : MonoBehaviour
{
    // 皮肤路径
    public string skinPath;

    // 皮肤
    public GameObject skin;

    // 层级
    public PanelMgr.PanelLayer layer;

    // 面板参数
    public object[] args;

    #region 生命周期
    public virtual void Init(params object[] args)
    {
        this.args = args;
    }

    public virtual void OnShowing() { }

    public virtual void OnShowed() { }

    public virtual void OnUpdate() { }

    public virtual void OnClosing() { }

    public virtual void OnClosed() { }

    #endregion

    #region 操作
    public virtual void Close()
    {
        string name = this.GetType().ToString();
        PanelMgr.instance.ClosePanel(name);
    }

    #endregion
}
