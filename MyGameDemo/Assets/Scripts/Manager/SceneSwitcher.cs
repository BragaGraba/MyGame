using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    // ˽�о�̬���������浥��ʵ��
    private static SceneSwitcher _instance = null;

    // ������̬��������ȡ����ʵ��
    public static SceneSwitcher Instance
    {
        get
        {
            if (_instance == null)
            {
                // ����ͨ��GameObject.FindObjectOfType�ҵ������еĵ�������  
                // ��ͨ���������ڳ�������ʱ�ֶ���������ֵ��_instance  
                // ����Ϊ�˼�ʾ��������Instance����Ҫʱ�Żᱻ���ã����ҳ������Ѿ�����һ��SceneSwitcherʵ��
                _instance = FindObjectOfType<SceneSwitcher>();

                // ����ű���û���ҵ����򴴽�һ���µ�GameObject������SceneSwitcher�ű�
                if (_instance == null)
                {
                    GameObject singleton = new GameObject();
                    singleton.name = "SceneSwitcher Singleton";
                    _instance = singleton.AddComponent<SceneSwitcher>();
                    DontDestroyOnLoad(singleton); // ȷ���ڼ����³���ʱ������������
                }
            }
            return _instance;
        }
    }   

    // �������ƻ򹹽�����
    public string sceneName = "GameScene";

    // ˽�й��캯����ȷ���ⲿ�޷�ֱ�Ӵ���ʵ��
    private SceneSwitcher() { }

    public void SwitchToScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}
