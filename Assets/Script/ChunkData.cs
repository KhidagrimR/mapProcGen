using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "ChunkData", menuName = "ScriptableObjects/ChunkData")]
public class ChunkData : ScriptableObject
{
    public string chunkName;
    [Serializable]
    public struct Exit
    {
        public enum ExitPos { center, right, left }; // center, right, left
        public ExitPos exitPosition;
        public int z_position; // 0, +x or -x
    }

    public List<Exit> northExits;
    public List<Exit> southExits;
    public List<Exit> eastExits;
    public List<Exit> westExits;

    public List<Exit> allExits
    {
        get
        {
            List<Exit> all = new List<Exit>(northExits.Count +
                                    southExits.Count +
                                    westExits.Count +
                                    eastExits.Count);
            all.AddRange(northExits);
            all.AddRange(southExits);
            all.AddRange(eastExits);
            all.AddRange(westExits);

            return all;
        }
    }

    public List<CompoGenerator.Directions> allDirections
    {
        get
        {
            List<CompoGenerator.Directions> dirs = new List<CompoGenerator.Directions>();
            if (northExits.Any()) dirs.Add(CompoGenerator.Directions.North);
            if (southExits.Any()) dirs.Add(CompoGenerator.Directions.South);
            if (westExits.Any()) dirs.Add(CompoGenerator.Directions.West);
            if (eastExits.Any()) dirs.Add(CompoGenerator.Directions.East);

            return dirs;
        }
    }

    public Exit GetExitsFromDirection(CompoGenerator.Directions directions, Exit.ExitPos[] matchableExits = null)
    {
        switch (directions)
        {
            case CompoGenerator.Directions.East:
                return eastExits.FindAll(x => HasMatchableExit(x, matchableExits))[Random.Range(0, eastExits.Count)];
            case CompoGenerator.Directions.West:
                return westExits.FindAll(x => HasMatchableExit(x, matchableExits))[Random.Range(0, westExits.Count)];
            case CompoGenerator.Directions.North:
                return northExits.FindAll(x => HasMatchableExit(x, matchableExits))[Random.Range(0, northExits.Count)];
            case CompoGenerator.Directions.South:
                return southExits.FindAll(x => HasMatchableExit(x, matchableExits))[Random.Range(0, southExits.Count)];
        }
        return new Exit();
    }

    public Vector3 GetNorthExitPosition(int index) // NORTH
    {
        Exit ex = northExits[index];
        Vector3 exPos = new Vector3();

        switch (ex.exitPosition)
        {
            case Exit.ExitPos.center:
                exPos = new Vector3(0, -24, ex.z_position);
                break;

            case Exit.ExitPos.left:
                exPos = new Vector3(-8, -24, ex.z_position);
                break;

            case Exit.ExitPos.right:
                exPos = new Vector3(8, -24, ex.z_position);
                break;
        }

        return exPos;
    }

    public Vector3 GetSouthExitPosition(int index) // SOUTH
    {
        Exit ex = southExits[index];
        Vector3 exPos = new Vector3();

        switch (ex.exitPosition)
        {
            case Exit.ExitPos.center:
                exPos = new Vector3(0, 24, ex.z_position);
                break;

            case Exit.ExitPos.left:
                exPos = new Vector3(8, 24, ex.z_position);
                break;

            case Exit.ExitPos.right:
                exPos = new Vector3(-8, 24, ex.z_position);
                break;
        }

        return exPos;
    }

    public Vector3 GetEastExitPosition(int index) // East
    {
        Exit ex = eastExits[index];
        Vector3 exPos = new Vector3();

        switch (ex.exitPosition)
        {
            case Exit.ExitPos.center:
                exPos = new Vector3(24, 0, ex.z_position);
                break;

            case Exit.ExitPos.left:
                exPos = new Vector3(24, -8, ex.z_position);
                break;

            case Exit.ExitPos.right:
                exPos = new Vector3(24, 8, ex.z_position);
                break;
        }

        return exPos;
    }

    public Vector3 GetWestExitPosition(int index) // WEST
    {
        Exit ex = westExits[index];
        Vector3 exPos = new Vector3();

        switch (ex.exitPosition)
        {
            case Exit.ExitPos.center:
                exPos = new Vector3(24, 0, ex.z_position);
                break;

            case Exit.ExitPos.left:
                exPos = new Vector3(24, 8, ex.z_position);
                break;

            case Exit.ExitPos.right:
                exPos = new Vector3(24, -8, ex.z_position);
                break;
        }

        return exPos;
    }

    bool HasMatchableExit(Exit exit, Exit.ExitPos[] matchableExits)
    {
        if (matchableExits == null)
            return true;

        for (int i = 0, n = matchableExits.Length; i < n; i++)
        {
            if (exit.exitPosition == matchableExits[i])
                return true;
        }

        return false;
    }
}
