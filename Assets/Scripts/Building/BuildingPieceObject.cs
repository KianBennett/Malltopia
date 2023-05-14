using UnityEngine;

public class BuildingPieceObject : WorldObject
{
    public enum PieceType { Wall, Window, Doorway }

    [Header("Wall Renderers")]
    [SerializeField] private Renderer wallSingle;
    [SerializeField] private Renderer wallDoubleFront;
    [SerializeField] private Renderer wallDoubleBack;
    [SerializeField] private Renderer wallTopFront;
    [SerializeField] private Renderer wallTopBack;
    [SerializeField] private bool useTrimMaterial;
    [SerializeField] private int trimMaterialIndex;

    [Header("Wall Parameters")]
    public PieceType type;
    public int connectedPiecesLeft;
    public int connectedPiecesRight;
    public BuildingPieceObject[] additionalPiecesLeft;
    public BuildingPieceObject[] additionalPiecesRight;

    [HideInInspector] public CellEdge cellEdge;

    public Room RoomFront { get { return cellEdge?.neighbourCells[DirectionFacing]?.room; } }
    public Room RoomBack { get { return cellEdge?.neighbourCells[Mall.GetOppositeDirection(DirectionFacing)]?.room; } }

    public void UpdateWallMaterials()
    {
        if(RoomFront == null && RoomBack == null)
        {
            Debug.LogWarning("Wall has no rooms on either side of it! This shouldn't be happening");
        }

        // If there is only one room (i.e. the wall is not bordering two rooms
        bool isDoubleWall = RoomFront != null && RoomBack != null;

        if (wallSingle) wallSingle.gameObject.SetActive(!isDoubleWall);
        if (wallDoubleFront) wallDoubleFront.gameObject.SetActive(isDoubleWall);
        if (wallDoubleBack) wallDoubleBack.gameObject.SetActive(isDoubleWall);

        if (isDoubleWall)
        {
            if (wallDoubleFront) wallDoubleFront.SetMaterial(RoomFront.WallMaterialSet.WallMaterial, 0);
            if (wallDoubleBack) wallDoubleBack.SetMaterial(RoomBack.WallMaterialSet.WallMaterial, 0);
            
            if (useTrimMaterial)
            {
                if (wallDoubleFront) wallDoubleFront.SetMaterial(RoomFront.WallMaterialSet.TrimMaterial, trimMaterialIndex);
                if (wallDoubleBack) wallDoubleBack.SetMaterial(RoomBack.WallMaterialSet.TrimMaterial, trimMaterialIndex);
            }

            if (wallTopFront) wallTopFront.material = RoomFront.WallMaterialSet.TopMaterial;
            if (wallTopBack) wallTopBack.material = RoomBack.WallMaterialSet.TopMaterial;
        }
        else
        {
            Room singleRoom = RoomFront != null ? RoomFront : RoomBack;

            if (wallSingle)
            {
                wallSingle.SetMaterial(singleRoom.WallMaterialSet.WallMaterial, 0);

                if (useTrimMaterial)
                {
                    wallSingle.SetMaterial(singleRoom.WallMaterialSet.TrimMaterial, trimMaterialIndex);
                }
            }

            if (wallTopFront) wallTopFront.material = singleRoom.WallMaterialSet.TopMaterial;
            if (wallTopBack) wallTopBack.material = singleRoom.WallMaterialSet.TopMaterial;
        }
    }
}