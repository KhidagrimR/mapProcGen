using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkHandler : MonoBehaviour
{
    public enum Direction {North,South,East,West};
    public Vector2Int gridPosition = new Vector2Int();
    [SerializeField] private Direction[] allowedDirection;

    public void SetupChunk(Vector2Int p_gridPosition)
    {
        gridPosition = p_gridPosition;
    }

    public Direction[] GetAllDirections()
    {
        return allowedDirection;
    }
}
