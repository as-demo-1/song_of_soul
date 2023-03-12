using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clarinet : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private string[] chartList;
    [SerializeField] private int currentChart;
    public GameObject spike;
    private Animator animator;
    private Camera mainCamera;
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void Generate()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
    public void Attack()
    {
        for (int col = 0; col < transform.childCount; ++col)
        {
            GameObject cellObject = transform.GetChild(col).gameObject;
            if (chartList[currentChart][col] == '1')
            {
                cellObject.SetActive(true);
            }else
            {
                cellObject.SetActive(false);
            }
        }
        animator.Play("up");
    }

    IEnumerator PlayDownAfter()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        while (stateInfo.IsName("up")) {
            yield return null;
        }
        animator.Play("down");
    }
    public void End()
    {
        currentChart = (currentChart + 1) % chartList.Length;
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("up"))
        {
            StartCoroutine(PlayDownAfter());
        } else if (stateInfo.IsName("idle"))
        {
            animator.Play("down");
        }
    }
}
