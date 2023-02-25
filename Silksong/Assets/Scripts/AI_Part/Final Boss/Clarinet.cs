using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clarinet : MonoBehaviour
{
    // Start is called before the first frame update
    //private float screenWidth;
    //private float screenHeight;
    private Animator animator;
    public string _chart;
    public GameObject spike;
    private Camera mainCamera;
    private int numCols; 
    void Start()
    {
        //transform.SetParent(null);
        numCols = 8;
        animator = GetComponent<Animator>();
    }

    public void Generate()
    {
        /*
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        float cameraHeight = mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * (1 + mainCamera.aspect);
        float cameraX = mainCamera.transform.position.x;
        float cameraY = mainCamera.transform.position.y;

        float cellWidth = cameraWidth / numCols;
        float cellHeight = cameraHeight;

        for (int col = 0; col < numCols; col++)
        {
            // 计算单元格的中心坐标 1.28 magic number why?
            float x = cameraX - cameraWidth / 2 * 1.28f + cellWidth * (col + 0.5f) * 1.28f;
            float y = cameraY;
            Vector3 cellPosition = new Vector3(x, y, 0f);

            // 实例化 Prefab
            GameObject cellObject = Instantiate(spike, transform);
            // 修改实例化后的物体名称
            cellObject.name = "Key " + col;
            cellObject.transform.position = cellPosition;
            cellObject.transform.parent = transform;
        }
        */
    }

    // Update is called once per frame
    void Update()
    {
    }
    public void Attack()
    {
        //this.gameObject.SetActive(true);
        if (this.transform.childCount <= 0)
        {
            //Generate(numCols);
        }
        for (int col = 0; col < numCols; ++col)
        {
            GameObject cellObject = transform.GetChild(col).gameObject;
        }
        animator.Play("up");
    }

    public void End()
    {
        animator.Play("down");
    }
}
