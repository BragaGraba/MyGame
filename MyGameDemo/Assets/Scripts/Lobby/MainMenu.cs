using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button menuBtn1;
    public Button menuBtn2;
    public Button menuBtn3;
    public Button menuBtn4;

    // Start is called before the first frame update
    void Start()
    {
        // bind ClickListener
        menuBtn1.onClick.AddListener(OnClickGameMode1Btn);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnClickGameMode1Btn()
    {
        SceneSwitcher.Instance.SwitchToScene();
    }
}
