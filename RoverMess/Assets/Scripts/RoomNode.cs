using UnityEngine;

public class RoomNode : MonoBehaviour
{

    [Header("Объекты стен-заглушек")]
    public GameObject wallNorth; // Смотрит вдоль оси +Z
    public GameObject wallSouth; // Смотрит вдоль оси -Z
    public GameObject wallEast;  // Смотрит вдоль оси +X
    public GameObject wallWest;  // Смотрит вдоль оси -X

    // Метод для открытия/закрытия конкретного прохода
    // isOpen = true означает, что проход ЕСТЬ (стена выключается)
    public void SetDoor(Vector2Int direction, bool isOpen)
    {
        bool isWallActive = !isOpen; // Если дверь открыта, стена должна исчезнуть

        if (direction == Vector2Int.up) if (wallNorth) wallNorth.SetActive(isWallActive);
        if (direction == Vector2Int.down) if (wallSouth) wallSouth.SetActive(isWallActive);
        if (direction == Vector2Int.right) if (wallEast) wallEast.SetActive(isWallActive);
        if (direction == Vector2Int.left) if (wallWest) wallWest.SetActive(isWallActive);
    }
}