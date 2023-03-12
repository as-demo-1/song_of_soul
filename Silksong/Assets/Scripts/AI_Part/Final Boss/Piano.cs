using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piano : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private string[] chartList;
    [SerializeField] private GameObject key;
    [SerializeField] private GameObject leftup;
    [SerializeField] private GameObject rightdown;
    [SerializeField] private int currentChart;
    private Camera mainCamera;
    private int numCols; 
    void Start()
    {
    }

    public void Generate()
    {
        numCols = chartList[currentChart].Length;
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        float height = leftup.transform.localPosition.y - rightdown.transform.localPosition.y;
        float width = rightdown.transform.localPosition.x - leftup.transform.localPosition.x;
        float cameraX = transform.position.x;
        float cameraY = transform.position.y;

        float cellWidth = width / numCols;
        float cellHeight = height;

        for (int col = 0; col < numCols; col++)
        {
            // 计算单元格的中心坐标 1.28 magic number why?
            float x = cameraX - width / 2 * 1.28f + cellWidth * (col + 0.5f) * 1.28f;
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
            if (chartList[currentChart][col] == '0')
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
        if (this.transform.childCount <= 2)
        {
            Generate();
        }
        for (int col = 0; col < numCols; ++col)
        {
            GameObject cellObject = transform.GetChild(col+2).gameObject;
            SpriteRenderer spriteRenderer = cellObject.GetComponent<SpriteRenderer>();
            BoxCollider2D boxCollider = cellObject.GetComponent<BoxCollider2D>();
            spriteRenderer.color += new Color(0, 0, 0, 0.5f);
            if (chartList[currentChart][col] == '1')
            {
                boxCollider.enabled = true;
                boxCollider.size = spriteRenderer.size;
            }
        }
    }

    public void End()
    {
        for (int i = 0; i < transform.childCount - 2; ++i)
        {
            Destroy(transform.GetChild(i+2).gameObject);
        }
        currentChart = (currentChart + 1) % chartList.Length;
    }
}
