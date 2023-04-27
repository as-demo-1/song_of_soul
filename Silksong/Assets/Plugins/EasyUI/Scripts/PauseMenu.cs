using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

/// <summary>
/// 暂停菜单
/// </summary>
public class PauseMenu : MonoBehaviour
{
    private bool isActive;

    [SerializeField]
    private GameObject panel;

    [SerializeField]
    private bool withAnim;

    private Vector3 originScale;
    // Start is called before the first frame update
    void Start()
    {
        originScale = panel.transform.localScale;
        ClosePauseMenu();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isActive)
                ClosePauseMenu();
            else 
                ActivePauseMenu();
        }
    }
    
    
    public void ActivePauseMenu()
    {
        Debug.Log("暂停");
        
        isActive = true;
        panel.gameObject.SetActive(true);
        if (withAnim)
        {
            panel.transform.localScale = Vector3.zero;
            panel.transform.DOScale(originScale, 0.5f).SetEase(Ease.OutElastic)
                .OnComplete(() => Time.timeScale = 0.0f);
            return;
        }
        Time.timeScale = 0.0f;

        
        
    }
    
    public void ClosePauseMenu()
    {
        Debug.Log("取消暂停");
        Time.timeScale = 1.0f;
        isActive = false;
        Sequence seq = DOTween.Sequence();
        if (withAnim)
        {
            panel.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InOutElastic).OnComplete(()=>panel.gameObject.SetActive(false));
            return;
        }
        panel.gameObject.SetActive(false);
        
    }

    #region click

    public void OnClickBack()
    {
        ClosePauseMenu();
    }

    public void OnClickRestart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnClickQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    #endregion
}
