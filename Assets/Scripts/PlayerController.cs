using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using static BuildingPieceObject;

public enum PlayerMode { Default, Build }

public class PlayerController : Singleton<PlayerController>
{
    [SerializeField] private int startingMoney;

    private PlayerMode playerMode;
    private List<WorldObject> mousedOverWorldObjects;
    private List<Character> mousedOverCharacters;
    private Room mousedOverRoom;
    private Cell mousedOverCell;

    private Character characterSelected;
    private WorldObject objectSelected;
    private Room roomSelected;
    private int money;
    private bool isPaused;

    private UnityAction onMoneyChanged;

    public PlayerMode PlayerMode { get { return playerMode; } }
    public WorldObject MousedOverWorldObject { get { return mousedOverWorldObjects.FirstOrDefault(); } }
    public Cell MousedOverCell { get { return mousedOverCell; } }
    public Character SelectedCharacter { get { return characterSelected; } }
    public WorldObject SelectedObject { get { return objectSelected; } }
    public Room SelectedRoom { get { return roomSelected; } }
    public int Money { get { return money; } }
    public UnityAction OnMoneyChanged { get { return onMoneyChanged; } set { onMoneyChanged = value; } }
    public bool IsPaused { get { return isPaused; } }

    protected override void Awake()
    {
        base.Awake();
        mousedOverWorldObjects = new();
        mousedOverCharacters = new();

        money = startingMoney;
    }

    void Update()
    {
        mousedOverCell = Mall.CurrentFloor.GetCell(Mall.Instance.GetMousePositionCellCoords());

        if (playerMode != PlayerMode.Build)
        {
            updateRoomSelection();

            if (InputHandler.Instance.WasPressedThisFrame("Select") && !EventSystem.current.IsPointerOverGameObject())
            {
                characterSelected?.SetSelected(false);
                objectSelected?.SetSelected(false);
                roomSelected?.SetSelected(false);
                ContextMenuManager.Instance?.ToggleContextMenu(ContextMenuManager.ContextMenuType.NullType);

                characterSelected = mousedOverCharacters.FirstOrDefault();
                characterSelected?.SetSelected(true);

                objectSelected = mousedOverWorldObjects.FirstOrDefault();
                objectSelected?.SetSelected(true);

                roomSelected = mousedOverRoom;
                roomSelected?.SetSelected(true);

                if (HUD.Instance && HUD.Instance.RoomBuildPanel.isOpenCheck == false && HUD.Instance.ObjectsPanel.isOpenCheck == false)
                {
                    if (characterSelected)
                    {
                        var Char = SelectedCharacter.GetComponent<ShopAssistant>();
                        var CustomerVar = SelectedCharacter.GetComponent<Customer>();
                        if (CustomerVar) ContextMenuManager.Instance?.ToggleContextMenu(ContextMenuManager.ContextMenuType.Customer);
                        else if (!Char) ContextMenuManager.Instance?.ToggleContextMenu(ContextMenuManager.ContextMenuType.Employee);
                    }
                    else
                    {

                        if (roomSelected) ContextMenuManager.Instance?.ToggleContextMenu(ContextMenuManager.ContextMenuType.Room);


                        else if (objectSelected)
                        {
                            var BuildType = objectSelected.GetComponent<BuildingPieceObject>();
                            if (BuildType)
                            {

                                if(BuildType.type != PieceType.Wall) ContextMenuManager.Instance?.ToggleContextMenu(ContextMenuManager.ContextMenuType.Object);

                            }
                            else
                            {
                                ContextMenuManager.Instance?.ToggleContextMenu(ContextMenuManager.ContextMenuType.Object);

                            }



                        }


                        else if (objectSelected) ContextMenuManager.Instance?.ToggleContextMenu(ContextMenuManager.ContextMenuType.Object);

                    }
                    //   else if (objectSelected) ContextMenuManager.Instance?.ToggleContextMenu(ContextMenuManager.ContextMenuType.Object);                }


                }
                else
                {

                    //   if (objectSelected) ContextMenuManager.Instance?.ToggleContextMenu(ContextMenuManager.ContextMenuType.Object);
                }
            }

            /*            if (Input.GetKeyDown(KeyCode.X))
                        {
                            if (objectSelected)
                            {
                                Destroy(objectSelected.gameObject);
                                objectSelected.PlayRemovalSound();
                            }
                            if (roomSelected)
                            {
                                roomSelected.DestroyRoom();
                                AkSoundEngine.PostEvent("destroyWalls", gameObject);
                            }
                        }*/

            /*            if(Input.GetKeyDown(KeyCode.U))
                        {
                            if(roomSelected)
                            {
                                HUD.Instance.RoomBuildPanel.Open(roomSelected);
                            }
                        }*/
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SetPaused(!IsPaused);
        }
    }

    public void SetPlayerMode(PlayerMode mode)
    {
        playerMode = mode;
    }

    public void AddMousedOverObject(WorldObject worldObject)
    {
        mousedOverWorldObjects.Add(worldObject);
    }

    public void RemoveMousedOverObject(WorldObject worldObject)
    {
        mousedOverWorldObjects.Remove(worldObject);
    }

    public void AddMousedOverCharacter(Character character)
    {
        mousedOverCharacters.Add(character);
    }

    public void RemoveMousedOverCharacter(Character character)
    {
        mousedOverCharacters.Remove(character);
    }

    public void EnterBuildMode()
    {
        SetPlayerMode(PlayerMode.Build);
        BuildingSystem.Instance.ChangeBuildingStage(BuildingSystem.BuildingStage.ChooseStartCell);
    }

    public void EnterSingleObjectBuildMode()
    {
        SetPlayerMode(PlayerMode.Build);
        BuildingSystem.Instance.ChangeBuildingStage(BuildingSystem.BuildingStage.PlaceSingleObject);
    }

    public void ExitBuildMode()
    {
        SetPlayerMode(PlayerMode.Default);
        BuildingSystem.Instance.ChangeBuildingStage(BuildingSystem.BuildingStage.None);
    }

    public void SetPaused(bool paused)
    {
        isPaused = paused;

        if (paused)
        {
            Time.timeScale = 0;
            HUD.Instance.PauseMenu.Show();
        }
        else
        {
            TimeManager.Instance.UpdateTimeScaleFromSpeed();
            HUD.Instance.PauseMenu.Hide();
        }
    }

    public bool CanAfford(int amount)
    {
        return money >= amount;
    }

    public void AddMoney(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarningFormat("Trying to add a negative amount of money ({0}), use RemoveMoney instead", amount);
            return;
        }
        money += amount;

        onMoneyChanged?.Invoke();
    }

    public bool RemoveMoney(int amount)
    {
        int newMoney = money - amount;

        if (newMoney >= 0)
        {
            money = newMoney;
            onMoneyChanged?.Invoke();
            return true;
        }

        return false;
    }

    private void updateRoomSelection()
    {
        Room mousedOverRoomPrev = mousedOverRoom;
        mousedOverRoom = null;

        if (mousedOverWorldObjects.Count == 0 && mousedOverCharacters.Count == 0 && mousedOverCell != null && mousedOverCell.room)
        {
            mousedOverRoom = mousedOverCell.room;
        }

        // We no longer highlight a room if the mouse is over a wall piece - only the floor!
        //WorldObject firstMousedOverObject = mousedOverWorldObjects.FirstOrDefault();
        //if (firstMousedOverObject is BuildingPieceObject)
        //{
        //    mousedOverRoom = (firstMousedOverObject as BuildingPieceObject).RoomFront;
        //}

        if (mousedOverRoom != mousedOverRoomPrev)
        {
            // Set the room as highlighted
            mousedOverRoomPrev?.SetHighlighted(false);
            mousedOverRoom?.SetHighlighted(true);
        }
    }
}
