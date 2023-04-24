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
        //float height = 2 * Camera.main.orthographicSize;
        //float width = height * Camera.main.aspect;
        //float cameraX = transform.position.x;
        //float cameraY = transform.position.y;
        float centerX = leftup.transform.localPosition.x + width / 2.0f;
        float centerY = rightdown.transform.localPosition.y + height / 2.0f;

        float cellWidth = width / numCols;
        float cellHeight = height;

        GameObject bg = Instantiate(key, transform);
        bg.name = "bg";
        bg.transform.position = transform.position;
        bg.transform.parent = transform;
        SpriteRenderer sr = bg.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f));
        sr.color = Color.white;
        sr.color += new Color(0, 0, 0, -0.5f);
        sr.sortingOrder = 1;
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = new Vector3(width * 2, height, 1f);

        for (int col = 0; col < numCols; col++)
        {
            //float x = cameraX - width / 2 * 1.28f + cellWidth * (col + 0.5f) * 1.28f;
            float x = centerX - width / 2.0f  + cellWidth * (col + 0.5f) ;
            float y = centerY;
            Vector3 cellPosition = new Vector3(x, y, 0f);

            // 实例化 Prefab
            GameObject cellObject = Instantiate(key, transform);
            // 修改实例化后的物体名称
            cellObject.name = "Key " + col;
            cellObject.transform.localPosition = cellPosition;
            cellObject.transform.parent = transform;

            // 添加一个SpriteRenderer组件
            SpriteRenderer spriteRenderer = cellObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f));
            if (chartList[currentChart][col] == '0')
            {
                spriteRenderer.color = Color.white;
                spriteRenderer.color += new Color(0, 0, 0, -0.5f);
                spriteRenderer.sortingOrder = 1;
            }
            else
            {
                spriteRenderer.color = Color.black;
                spriteRenderer.color += new Color(0, 0, 0, -0.5f);
                spriteRenderer.sortingOrder = 2;
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
        if (this.transform.childCount <= 3)
        {
            Generate();
        }
        for (int col = 0; col < numCols; ++col)
        {
            GameObject cellObject = transform.GetChild(col+3).gameObject;
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
