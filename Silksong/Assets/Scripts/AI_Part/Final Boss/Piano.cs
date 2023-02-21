using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piano : MonoBehaviour
{
    // Start is called before the first frame update
    //private float screenWidth;
    //private float screenHeight;
    public string _chart;
    public GameObject key;
    private Camera mainCamera;
    private int numCols; 
    void Start()
    {
        //transform.SetParent(null);
    }

    public void Generate(string chart)
    {
        _chart = chart;
        numCols = chart.Length;
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
            GameObject cellObject = Instantiate(key, transform);
            // 修改实例化后的物体名称
            cellObject.name = "Key " + col;
            cellObject.transform.position = cellPosition;
            cellObject.transform.parent = transform;

            // 添加一个SpriteRenderer组件
            SpriteRenderer spriteRenderer = cellObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f));
            if (chart[col] == '0')
            {
                spriteRenderer.color = Color.white;
                spriteRenderer.color += new Color(0, 0, 0, -1);
            }
            else
            {
                spriteRenderer.color = Color.black;
                spriteRenderer.color += new Color(0, 0, 0, -0.5f);
                spriteRenderer.sortingOrder = 1;
            }
            spriteRenderer.drawMode = SpriteDrawMode.Sliced;

            // 设置单元格的大小
            spriteRenderer.size = new Vector3(cellWidth, cellHeight, 1f);

            
        }
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
            Generate(_chart);
        }
        for (int col = 0; col < numCols; ++col)
        {
            GameObject cellObject = transform.GetChild(col).gameObject;
            SpriteRenderer spriteRenderer = cellObject.GetComponent<SpriteRenderer>();
            BoxCollider2D boxCollider = cellObject.GetComponent<BoxCollider2D>();
            spriteRenderer.color += new Color(0, 0, 0, 0.5f);
            if (_chart[col] == '1')
            {
                boxCollider.enabled = true;
                boxCollider.size = spriteRenderer.size;
            }
        }
    }

    public void End()
    {
        for (int i = 0; i < numCols; ++i)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }
}
