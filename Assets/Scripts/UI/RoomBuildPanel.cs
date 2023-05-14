using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum RoomBuildTab
{
    Objects, Walls, Floors
}

public class RoomBuildPanel : ListPanel
{
    [SerializeField] private TextMeshProUGUI roomNameText;
    [SerializeField] private GameObject[] frontTabs;
    [SerializeField] private TextureButton textureButtonPrefab;
    [SerializeField] private RectTransform scrollViewContentRect;

    private int currentTab;
    private Room room;
    public Room _room => room;
    private TextureButton selectedTextureButton;
    private List<GameObject> objectButtons;
    private List<GameObject> wallButtons;
    private List<GameObject> floorButtons;

    [Header("Prefabs")]
    [SerializeField] private BuildObjectCell ObjectCellPrefab;

    [SerializeField] private GameObject DeviderPrefab;
    [SerializeField] private GameObject TextHeaderPrefab;
    [SerializeField] private BuildObjectCell SelectedObject;

    protected override void Awake()
    {
        objectButtons = new();
        wallButtons = new();
        floorButtons = new();

        base.Awake();
    }

    public void Open(Room room)
    {
        this.room = room;

        Open();
        SwitchTab(0);

        roomNameText.text = room.RoomParams.Name;
    }

    public void SwitchTab(int tab)
    {
        currentTab = tab;

        for (int i = 0; i < frontTabs.Length; i++)
        {
            frontTabs[i].SetActive(i == tab);
        }

        populateList();

        //foreach (GameObject button in objectButtons) button.SetActive(tab == (int) RoomBuildTab.Objects);
        //foreach (GameObject button in wallButtons) button.SetActive(tab == (int) RoomBuildTab.Walls);
        //foreach (GameObject button in floorButtons) button.SetActive(tab == (int) RoomBuildTab.Floors);
    }

    protected override void onClose()
    {
        base.onClose();

        PlayerController.Instance.ExitBuildMode();
    }

    protected override void populateList()
    {
        base.populateList();

        if (room == null)
        {
            return;
        }

        foreach (GameObject button in objectButtons) Destroy(button);
        foreach (GameObject button in wallButtons) Destroy(button);
        foreach (GameObject button in floorButtons) Destroy(button);

        objectButtons.Clear();
        wallButtons.Clear();
        floorButtons.Clear();

        RoomBuildTab tab = (RoomBuildTab)currentTab;

        switch (tab)
        {
            // Create a button for each object that can be placed in the room
            case RoomBuildTab.Objects:

                setVerticalLayout();

                SpawnHeaderText("Key Items");

                bool KeyItemDevider = false;

                for (int i = 0; i < room.RoomParams.ObjectsCanBuild.Length; i++)
                {
                    WorldObject worldObject = room.RoomParams.ObjectsCanBuild[i];

                    if (!RequiredItemCheck(worldObject) && !KeyItemDevider)
                    {
                        GameObject devider = Instantiate(DeviderPrefab, scrollViewContentRect.transform);
                        devider.transform.localScale = new Vector3(1, 1, 1);
                        objectButtons.Add(devider);
                        SpawnHeaderText("Decoration Items");
                        KeyItemDevider = true;
                    }

                    BuildObjectCell button = Instantiate(ObjectCellPrefab);
                    button.transform.SetParent(scrollViewContentRect.transform);
                    button.ButtonStart(worldObject, room);
                    button.SetTick(button.ItemObjectKey_Check(button._CurrentRoom));
                    button.SetIcon(worldObject.Icon);
                    

                    //  button.SetValues(worldObject.DisplayText, worldObject.Cost);
                    // button.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -60 - 90 * i, 0);
                    int index = i;
                    button.GetComponent<Button>().onClick.AddListener(delegate
                    {
                        PlayerController.Instance.EnterSingleObjectBuildMode();
                        BuildingSystem.Instance.SetCurrentRoom(room);
                        BuildingSystem.Instance.ChangeSingleObject(worldObject);

                        // SetSelected();

                        button.GetComponent<BuildObjectCell>().SetSelectedOutline(true);
                        button.GetComponent<BuildObjectCell>().SetSelected = true;
                    });

                    objectButtons.Add(button.gameObject);
                }

                for (int i = 0; i < BuildingSystem.Instance.CommonAreaObjects.Length; i++)
                {
                    WorldObject worldObject = BuildingSystem.Instance.CommonAreaObjects[i];

                    BuildObjectCell button = Instantiate(ObjectCellPrefab);
                    button.transform.SetParent(scrollViewContentRect.transform);
                    button.ButtonStart(worldObject, room);
                    button.SetTick(button.ItemObjectKey_Check(button._CurrentRoom));
                    button.SetIcon(worldObject.Icon);

                    int index = i;
                    button.GetComponent<Button>().onClick.AddListener(delegate
                    {
                        PlayerController.Instance.EnterSingleObjectBuildMode();
                        BuildingSystem.Instance.SetCurrentRoom(room);
                        BuildingSystem.Instance.ChangeSingleObject(worldObject);

                        // SetSelected();

                        button.GetComponent<BuildObjectCell>().SetSelectedOutline(true);
                        button.GetComponent<BuildObjectCell>().SetSelected = true;
                    });

                    objectButtons.Add(button.gameObject);
                }

                static bool RequiredItemCheck(WorldObject Item)
                {
                    if (Item.GetComponent<ItemObjectKey>() != null)
                    {
                        return true;
                    }
                    else if (Item.GetComponent<ItemObjectTill>() != null)
                    {
                        return true;
                    }
                    return false;
                }

                //  float height = 40 + objectButtons.Count * 90;
                // scrollViewContentRect.sizeDelta = new Vector2(scrollViewContentRect.sizeDelta.x, height);

                break;

            // Create a button for each wall texture
            case RoomBuildTab.Walls:

                setGridLayout();

                int currentWallMaterialSetIndex = Array.IndexOf(BuildingSystem.Instance.WallMaterialSets, room.WallMaterialSet);

                for (int i = 0; i < BuildingSystem.Instance.WallMaterialSets.Length; i++)
                {
                    WallMaterialSet wallMaterials = BuildingSystem.Instance.WallMaterialSets[i];
                    TextureButton button = Instantiate(textureButtonPrefab, scrollViewContentRect);
                    button.SetTexture(wallMaterials.WallMaterial.GetTexture("_MainTex"));
                    button.SetSelected(i == currentWallMaterialSetIndex);
                    button.Rect.anchoredPosition = new Vector3(75 + 115 * (i % 3), -70 - 115 * (i / 3), 0);
                    int index = i;
                    button.SetOnClick(delegate
                    {
                        // TODO: Spend money?
                        selectTextureButton(button);
                        room.SetWallMaterialSet(wallMaterials);
                    });

                    wallButtons.Add(button.gameObject);

                    if (i == currentWallMaterialSetIndex)
                    {
                        selectedTextureButton = button;
                    }
                }

                /*              height = 40 + Mathf.Ceil(wallButtons.Count / 3.0f) * 115;
                              scrollViewContentRect.sizeDelta = new Vector2(scrollViewContentRect.sizeDelta.x, height);*/

                break;

            // Create a button for each floor texture
            case RoomBuildTab.Floors:

                setGridLayout();

                int currentFloorTextureSetIndex = Array.IndexOf(BuildingSystem.Instance.FloorTextureSets, room.FloorTextureSet);

                for (int i = 0; i < BuildingSystem.Instance.FloorTextureSets.Length; i++)
                {
                    TextureSet floorTextures = BuildingSystem.Instance.FloorTextureSets[i];
                    TextureButton button = Instantiate(textureButtonPrefab, scrollViewContentRect);
                    button.SetTexture(floorTextures.mainTexture);
                    button.SetSelected(i == currentFloorTextureSetIndex);
                    button.Rect.anchoredPosition = new Vector3(75 + 115 * (i % 3), -70 - 115 * (i / 3), 0);
                    int index = i;
                    button.SetOnClick(delegate
                    {
                        // TODO: Spend money?
                        selectTextureButton(button);
                        room.SetFloorTextureSet(floorTextures);
                    });

                    floorButtons.Add(button.gameObject);

                    if (i == currentFloorTextureSetIndex)
                    {
                        selectedTextureButton = button;
                    }
                }
                /*
                                height = 40 + Mathf.Ceil(floorButtons.Count / 3.0f) * 115;
                                scrollViewContentRect.sizeDelta = new Vector2(scrollViewContentRect.sizeDelta.x, height);*/

                break;
        }
    }

    private void selectTextureButton(TextureButton textureButton)
    {
        if (selectedTextureButton)
        {
            selectedTextureButton.SetSelected(false);
        }

        selectedTextureButton = textureButton;
        selectedTextureButton.SetSelected(true);
    }

    public void resetSelected()
    {
        if (objectButtons != null && objectButtons.Count != 0)
        {
            foreach (GameObject item in objectButtons)
            {
                if (item.GetComponent<BuildObjectCell>())
                {
                    item.GetComponent<BuildObjectCell>().SetSelected = false;
                    item.GetComponent<BuildObjectCell>().SetSelectedOutline(false);
                }
            }
        }
    }

    private void SpawnHeaderText(string Text)
    {
        GameObject KeyText = Instantiate(TextHeaderPrefab, scrollViewContentRect.transform);
        //Fetches Text element
        KeyText.transform.GetChild(1).GetComponent<TMP_Text>().text = Text;
        objectButtons.Add(KeyText);
    }

    private void SetSelected()
    {
        for (int i = 0; i < objectButtons.Count; i++)
        {
            objectButtons[i].GetComponent<BuildObjectCell>().SetSelected = false;
            objectButtons[i].GetComponent<BuildObjectCell>().SetSelectedOutline(false);
        }
    }

    //This is ugly, please ignore :)
    private void setGridLayout()
    {
        if (scrollViewContentRect.GetComponent<VerticalLayoutGroup>()) DestroyImmediate(scrollViewContentRect.GetComponent<VerticalLayoutGroup>());
        if (scrollViewContentRect.GetComponent<GridLayoutGroup>()) DestroyImmediate(scrollViewContentRect.GetComponent<GridLayoutGroup>());
        GridLayoutGroup layout = scrollViewContentRect.AddComponent<GridLayoutGroup>();
        layout.padding = new RectOffset(16, 0, 20, 20);
        layout.cellSize = new Vector2(100, 100);
        layout.spacing = new Vector2(16, 16);
    }

    private void setVerticalLayout()
    {
        if (scrollViewContentRect.GetComponent<GridLayoutGroup>()) DestroyImmediate(scrollViewContentRect.GetComponent<GridLayoutGroup>());
        if (scrollViewContentRect.GetComponent<VerticalLayoutGroup>()) DestroyImmediate(scrollViewContentRect.GetComponent<VerticalLayoutGroup>());
        VerticalLayoutGroup layout = scrollViewContentRect.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(16, 32, 20, 20);
        layout.spacing = 16;
        layout.childControlHeight = false;
        layout.childScaleHeight = true;
        layout.childScaleWidth = true;
    }
}