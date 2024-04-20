using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelBase : MonoBehaviour
{
    // Ƥ��·��
    public string skinPath;

    // Ƥ��
    public GameObject skin;

    // �㼶
    public PanelMgr.PanelLayer layer;

    // ������
    public object[] args;

    #region ��������
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

    #region ����
    public virtual void Close()
    {
        string name = this.GetType().ToString();
        PanelMgr.instance.ClosePanel(name);
    }

    #endregion
}
