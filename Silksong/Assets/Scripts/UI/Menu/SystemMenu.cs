using FairyGUI;
using System.Collections.Generic;
using UnityEngine;

public class SystemMenu : MonoBehaviour
{
    public List<ListBtnInfo> btnInfos;
    GComponent _mainView;
    GList _list;

    void Awake()
    {
        btnInfos = new List<ListBtnInfo>(){
            new ListBtnInfo("地图", OnShowMap),
            new ListBtnInfo("装备", ChangeState),
            new ListBtnInfo("护符", ChangeState),
            new ListBtnInfo("图鉴", ChangeState),
            new ListBtnInfo("成就", ChangeState),
            new ListBtnInfo("选项", ChangeState),
        };
    }

    void Start()
    {
        _mainView = this.GetComponent<UIPanel>().ui;
        _list = _mainView.GetChild("list_btn").asList;
        _list.itemRenderer = RenderListItem;
        _list.onClickItem.Add(onClickItem);
        _list.numItems = btnInfos.Count;
        OnShowMap(0);
    }

    void RenderListItem(int index, GObject obj)
    {
        obj.name = index.ToString();
        GComponent comp = obj.asCom;
        GTextField text = comp.GetChild("title").asTextField;
        text.text = btnInfos[index].text;
        GTextField text2 = comp.GetChild("title2").asTextField;
        text2.text = btnInfos[index].text;
    }

    void onClickItem(EventContext context)
    {
        GObject item = (GObject)context.data;
        int index = int.Parse(item.name);
        ListBtnInfo info = btnInfos[index];
        info.callback.Invoke(index);
    }

    void ChangeState(int index)
    {
        Controller _curController = _mainView.GetController("state");
        _curController.selectedIndex = index;
    }

    private GameObject mapPack;
    private MapController mapController;
    public GameObject Prefabs_MapPack;
    void OnShowMap(int index)
    {
        if (mapPack == null)
        {
            mapPack = Instantiate(Prefabs_MapPack);
            mapPack.SetActive(true);
            GComponent gmap = _mainView.GetChild("state_map").asCom;
            GGraph holder_level = gmap.GetChild("level").asGraph;
            GGraph holder_origin = gmap.GetChild("origin").asGraph;

            MapController comp_mapPack = mapPack.GetComponent<MapController>();
            Canvas canvas_level = comp_mapPack.levelMap.GetComponent<Canvas>();
            canvas_level.worldCamera = CameraController.Instance.mStageCamera;
            RectTransform rectTrans_level = canvas_level.GetComponent<RectTransform>();
            rectTrans_level.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, holder_level.width);
            rectTrans_level.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, holder_level.height);

            Canvas canvas_region = comp_mapPack.regionMap.GetComponent<Canvas>();
            canvas_region.worldCamera = CameraController.Instance.mStageCamera;
            RectTransform rectTrans_region = canvas_region.GetComponent<RectTransform>();
            rectTrans_region.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, holder_origin.width);
            rectTrans_region.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, holder_origin.height);

            GoWrapper gw_level = new GoWrapper(canvas_level.gameObject);
            holder_level.SetNativeObject(gw_level);

            GoWrapper gw_region = new GoWrapper(canvas_region.gameObject);
            holder_origin.SetNativeObject(gw_region);

            canvas_level.gameObject.SetActive(false);
            canvas_region.gameObject.SetActive(false);

            mapController = gameObject.AddComponent<MapController>();
            mapController.levelMap = canvas_level.gameObject;
            mapController.regionMap = canvas_region.gameObject;
            mapController.Init();

            mapPack.SetActive(false);
        }
        mapController.ShowLevel();
        mapController = GetComponent<MapController>();
        ChangeState(index);
    }
}
