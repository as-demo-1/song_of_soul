using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

/// <summary>
/// 简易的按钮，开放了点击、长按，按下、抬起的事件
/// </summary>
public class EasyButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{

    public Text text;

    [Header("点击事件")]
    public UnityEvent OnBtnClick;
    [Header("长按事件")]
    public UnityEvent OnBtnHold;

    [Header("按下事件")]
    public UnityEvent OnBtnDown;
    [Header("抬起事件")]
    public UnityEvent OnBtnUp;

    [Header("鼠标进入事件")]
    public UnityEvent OnEnter;

    [Header("鼠标离开事件")]
    public UnityEvent OnExit;

    [Header("绑定按键")]
    public KeyCode bindKey;
    
    [Header("是否开启动画")]
    public bool withAnim;

    [Header("按住判定时间")]
    [SerializeField] private float holdTime;

    private float timer;

    private bool isHolding;

    private Vector3 textScale;

    private Vector3 btnScale;

    public bool interactable;
    // Start is called before the first frame update
    void Start()
    {
        textScale = text.transform.localScale;
        btnScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        if (interactable)
        {
            if (Input.GetKeyDown(bindKey))
            {
                ButtonDown();
            }

            if (Input.GetKeyUp(bindKey))
            {
                ButtonUp();
            }
            if (isHolding)
            {
                timer += Time.deltaTime;
                if (timer > holdTime)
                {
                    OnBtnHold.Invoke();
                }
            }
        }
        

        
    }

    public void ButtonDown()
    {
        Debug.Log("EasyBtn Down");
        isHolding = true;
        OnBtnDown.Invoke();
        if (withAnim)
        {
            transform.DOScale(0.8f*btnScale, 0.2f).SetEase(Ease.InOutElastic);
        }
    }
    public void ButtonUp()
    {
        if (timer < holdTime)
        {
            timer = 0;
            isHolding = false;
            OnBtnClick.Invoke();
        }

        OnBtnUp.Invoke();
        
        if (withAnim)
        {
            transform.DOScale(btnScale, 0.2f).SetEase(Ease.InOutElastic);
        }
    }

    #region UnityEvent



    

    public void OnPointerDown(PointerEventData eventData)
    {
        ButtonDown();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ButtonUp();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnEnter.Invoke();
        if (withAnim)
        {
            text.transform.DOScale(1.2f*textScale, 0.5f).SetEase(Ease.InOutElastic);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnExit.Invoke();
        if (withAnim)
        {
            text.transform.DOScale(textScale, 0.5f).SetEase(Ease.InOutElastic);
        }
    }
    
    #endregion
}
