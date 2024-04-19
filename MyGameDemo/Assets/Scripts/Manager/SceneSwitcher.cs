using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    // 私有静态变量来保存单例实例
    private static SceneSwitcher _instance = null;

    // 公共静态属性来获取单例实例
    public static SceneSwitcher Instance
    {
        get
        {
            if (_instance == null)
            {
                // 可以通过GameObject.FindObjectOfType找到场景中的单例对象  
                // 但通常建议是在场景加载时手动创建并赋值给_instance  
                // 这里为了简化示例，假设Instance在需要时才会被调用，并且场景中已经有了一个SceneSwitcher实例
                _instance = FindObjectOfType<SceneSwitcher>();

                // 如果脚本中没有找到，则创建一个新的GameObject并附加SceneSwitcher脚本
                if (_instance == null)
                {
                    GameObject singleton = new GameObject();
                    singleton.name = "SceneSwitcher Singleton";
                    _instance = singleton.AddComponent<SceneSwitcher>();
                    DontDestroyOnLoad(singleton); // 确保在加载新场景时单例不被销毁
                }
            }
            return _instance;
        }
    }   

    // 场景名称或构建索引
    public string sceneName = "GameScene";

    // 私有构造函数，确保外部无法直接创建实例
    private SceneSwitcher() { }

    public void SwitchToScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}
