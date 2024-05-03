using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Random = UnityEngine.Random;
using Unity.VisualScripting;

public class CompoGeneratorParameters
{
    public bool uniqueChunks = true; // means that a chunk is unique
}


public class jsonEntry
{
    public string chunkName;
    public Vector3 chunkPosition;

    public jsonEntry(string p_chunkName, Vector3 p_chunkPosition)
    {
        chunkName = p_chunkName;
        chunkPosition = p_chunkPosition;
    }
}

public class CompoGenerator : MonoBehaviour
{
    public int mapSize = 3;
    public List<ChunkData> chunkDatas;
    public List<ChunkData> chunkEnds; // in 8*8
    public List<ChunkData> chunkBridges; // in 8*8

    public CompoGeneratorParameters compoParam;
    public enum Directions { North, South, West, East, Null }

    //public ChunkData[] map;
    public List<ChunkConnexionData> chunkConnexionDatas;

    [Serializable]
    public struct ChunkConnexionData
    {
        public ChunkData chunkData;
        public Vector3 position;
        public List<Directions> connexionDirection;
        public List<Directions> remainingDirections;
        public ChunkConnexionData(ChunkData p_chunkData, Vector3 p_position, Directions p_connexionDirection)
        {
            chunkData = p_chunkData;
            position = p_position;

            connexionDirection = new List<Directions>();
            connexionDirection.Add(p_connexionDirection);

            remainingDirections = new List<Directions>();

            if (chunkData.northExits.Any())
                remainingDirections.Add(Directions.North);
            else if (chunkData.southExits.Any())
                remainingDirections.Add(Directions.South);
            else if (chunkData.eastExits.Any())
                remainingDirections.Add(Directions.East);
            else if (chunkData.westExits.Any())
                remainingDirections.Add(Directions.West);
        }
    }

    int zTracker = 0;


    public void CreateMap()
    {
        GenerateAMap();
        TextWriter.Write(ChunkToString(chunkConnexionDatas));
    }

    void GenerateAMap()
    {
        // we want to make a map of size Mapsize
        chunkConnexionDatas = new List<ChunkConnexionData>();

        // we start with a random chunk
        chunkConnexionDatas.Add(new ChunkConnexionData(chunkDatas[Random.Range(0, chunkDatas.Count)], new Vector3(0, 0, 0), Directions.Null)); // "Directions.North" is arbitrary here and don't do anything
        Debug.Log("Chosen chunk = " + chunkConnexionDatas[0].chunkData.chunkName);


        for (int i = 0, n = mapSize - 1; i < n; i++)
        {
            chunkDatas = Shuffle<ChunkData>(chunkDatas);

            // ## ADD CHUNK ## //
            // On rajoute un chunk sur l'une de ses exits available
            ChunkData matchableChunk = chunkDatas.Find(x => IsChunkMatching(chunkConnexionDatas[i].chunkData, x, chunkConnexionDatas[i].connexionDirection));
            // is chunkMatching add all available connexions to the var "directionMatched"

            if (matchableChunk == null)
                break;

            Directions chosenDirection = InvertDirection(directionMatched[Random.Range(0, directionMatched.Count)]);

            // On ajoute le chunk choisis
            ChunkConnexionData ccd = new ChunkConnexionData(matchableChunk, GetPositionFromDirection(chosenDirection, chunkConnexionDatas[i].position), chosenDirection);
            chunkConnexionDatas.Add(ccd);
            ccd.remainingDirections.Remove(chosenDirection);
            Debug.Log("<color=red>======</color>");
            Debug.Log("Targetted Direction = " + chosenDirection);
            Debug.Log("current chunk pos = " + ccd.position.z);
            Debug.Log("Position of current chunk exit = " + ccd.chunkData.GetExitsFromDirection(chosenDirection).z_position);
            Debug.Log("previous chunk pos = " + chunkConnexionDatas[i].position.z);
            Debug.Log("Position of previous chunk exit = " + chunkConnexionDatas[i].chunkData.GetExitsFromDirection(InvertDirection(chosenDirection)).z_position);
            Debug.Log("<color=red>======</color>");

            ccd.position.z = chunkConnexionDatas[i].position.z +
                                                        chunkConnexionDatas[i].chunkData.GetExitsFromDirection(InvertDirection(chosenDirection)).z_position -
                                                        ccd.chunkData.GetExitsFromDirection(chosenDirection).z_position;
        }

        chunkConnexionDatas = CleanRemainingExits(chunkConnexionDatas); // add an ends to remaining exits
    }

    List<Directions> directionMatched;
    bool IsChunkMatching(ChunkData chunkA, ChunkData chunkB, List<Directions> directionConstraints = null)
    {
        directionMatched = new List<Directions>(); // liste toutes les directions ou le chunkB peut se match au chunkA
        // 2 chunks "matchent" si ils ont au moins une entrée/sortie opposée commune
        // Exemple : North-Center match avec South-Center; West-Right match avec East-Left
        // Pour une V1, on écrit tout à la mano

        if (directionConstraints == null)
            directionConstraints = new List<Directions>();

        // ### NORTH ### //
        if (chunkA.northExits.Any() && !directionConstraints.Contains(Directions.North))
        {
            for (int i = 0, n = chunkA.northExits.Count; i < n; i++)
            {
                int amountOfMatchableExits = 0;
                switch (chunkA.northExits[i].exitPosition)
                {
                    case ChunkData.Exit.ExitPos.center:
                        amountOfMatchableExits += chunkB.southExits.FindAll(x => x.exitPosition == ChunkData.Exit.ExitPos.center).Count; // will be 1 or 0
                        break;
                    case ChunkData.Exit.ExitPos.left:
                        amountOfMatchableExits += chunkB.southExits.FindAll(x => x.exitPosition == ChunkData.Exit.ExitPos.right).Count; // will be 1 or 0
                        break;
                    case ChunkData.Exit.ExitPos.right:
                        amountOfMatchableExits += chunkB.southExits.FindAll(x => x.exitPosition == ChunkData.Exit.ExitPos.left).Count; // will be 1 or 0
                        break;
                }

                if (amountOfMatchableExits > 0)
                    directionMatched.Add(Directions.North);
            }
        }

        // ### SOUTH  ### //
        if (chunkA.southExits.Any() && !directionConstraints.Contains(Directions.South))
        {
            for (int i = 0, n = chunkA.southExits.Count; i < n; i++)
            {
                int amountOfMatchableExits = 0;
                switch (chunkA.southExits[i].exitPosition)
                {
                    case ChunkData.Exit.ExitPos.center:
                        amountOfMatchableExits += chunkB.northExits.FindAll(x => x.exitPosition == ChunkData.Exit.ExitPos.center).Count; // will be 1 or 0
                        break;
                    case ChunkData.Exit.ExitPos.left:
                        amountOfMatchableExits += chunkB.northExits.FindAll(x => x.exitPosition == ChunkData.Exit.ExitPos.right).Count; // will be 1 or 0
                        break;
                    case ChunkData.Exit.ExitPos.right:
                        amountOfMatchableExits += chunkB.northExits.FindAll(x => x.exitPosition == ChunkData.Exit.ExitPos.left).Count; // will be 1 or 0
                        break;
                }

                if (amountOfMatchableExits > 0)
                    directionMatched.Add(Directions.South);
            }
        }

        // ### EAST  ### //
        if (chunkA.eastExits.Any() && !directionConstraints.Contains(Directions.East))
        {
            for (int i = 0, n = chunkA.eastExits.Count; i < n; i++)
            {
                int amountOfMatchableExits = 0;
                switch (chunkA.eastExits[i].exitPosition)
                {
                    case ChunkData.Exit.ExitPos.center:
                        amountOfMatchableExits += chunkB.westExits.FindAll(x => x.exitPosition == ChunkData.Exit.ExitPos.center).Count; // will be 1 or 0
                        break;
                    case ChunkData.Exit.ExitPos.left:
                        amountOfMatchableExits += chunkB.westExits.FindAll(x => x.exitPosition == ChunkData.Exit.ExitPos.right).Count; // will be 1 or 0
                        break;
                    case ChunkData.Exit.ExitPos.right:
                        amountOfMatchableExits += chunkB.westExits.FindAll(x => x.exitPosition == ChunkData.Exit.ExitPos.left).Count; // will be 1 or 0
                        break;
                }

                if (amountOfMatchableExits > 0)
                    directionMatched.Add(Directions.East);
            }
        }

        // ### WEST  ### //
        if (chunkA.westExits.Any() && !directionConstraints.Contains(Directions.West))
        {
            for (int i = 0, n = chunkA.westExits.Count; i < n; i++)
            {
                int amountOfMatchableExits = 0;
                switch (chunkA.westExits[i].exitPosition)
                {
                    case ChunkData.Exit.ExitPos.center:
                        amountOfMatchableExits += chunkB.eastExits.FindAll(x => x.exitPosition == ChunkData.Exit.ExitPos.center).Count; // will be 1 or 0
                        break;
                    case ChunkData.Exit.ExitPos.left:
                        amountOfMatchableExits += chunkB.eastExits.FindAll(x => x.exitPosition == ChunkData.Exit.ExitPos.right).Count; // will be 1 or 0
                        break;
                    case ChunkData.Exit.ExitPos.right:
                        amountOfMatchableExits += chunkB.eastExits.FindAll(x => x.exitPosition == ChunkData.Exit.ExitPos.left).Count; // will be 1 or 0
                        break;
                }

                if (amountOfMatchableExits > 0)
                    directionMatched.Add(Directions.West);
            }
        }
        return directionMatched.Any();
    }
    public Directions InvertDirection(Directions directionToInvert)
    {
        switch (directionToInvert)
        {
            case Directions.North:
                return Directions.South;

            case Directions.South:
                return Directions.North;

            case Directions.East:
                return Directions.West;

            case Directions.West:
                return Directions.East;
        }
        return Directions.North;
    }
    public Vector3 GetPositionFromDirection(Directions direction, Vector3 initialPosition)
    {
        Vector3 offset = Vector3.zero;
        switch (direction)
        {
            case Directions.North:
                offset = new Vector3(0, 23, 0); // North play on Y axis, going north use a Negative value
                break;
            case Directions.South:
                offset = new Vector3(0, -23, 0);
                break;
            case Directions.East:
                offset = new Vector3(-24, 0, 0);
                break;
            case Directions.West:
                offset = new Vector3(24, 0, 0);  // West play with X axis, going west use a positive value
                break;
        }

        return initialPosition + offset;
    }
    public static List<T> Shuffle<T>(List<T> _list)
    {
        for (int i = 0; i < _list.Count; i++)
        {
            T temp = _list[i];
            int randomIndex = Random.Range(i, _list.Count);
            _list[i] = _list[randomIndex];
            _list[randomIndex] = temp;
        }
        return _list;
    }


    public string ChunkToString(List<ChunkConnexionData> p_chunkDatasList)
    {
        string str = "{ \"chunks\" : [ ";
        int index = 0;
        foreach (ChunkConnexionData cData in p_chunkDatasList)
        {
            str += "{";
            str += " \"room\" : \"" + cData.chunkData.chunkName + "\" , ";
            str += " \"pos\" : { ";
            str += " \"x\" : " + cData.position.x + " , ";
            str += " \"y\" : " + cData.position.y + " , ";
            str += " \"z\" : " + cData.position.z + "  ";
            str += "}";
            str += "}";

            if (index != mapSize - 1)
                str += ",";

            index++;
        }

        str += "]}";
        return str;
    }

    public List<ChunkConnexionData> CleanRemainingExits(List<ChunkConnexionData> p_chunkConnexionDatas)
    {
        for (int i = 0, n = p_chunkConnexionDatas.Count; i < n; i++) // pour chaque chunk
        {
            for (int j = 0, m = p_chunkConnexionDatas[i].remainingDirections.Count; j < m; j++) // pour chacune des directions qu'il lui reste à remplir
            {
                switch (p_chunkConnexionDatas[i].remainingDirections[j])
                {
                    case Directions.North:
                        int index = 0;
                        foreach (ChunkData.Exit ex in p_chunkConnexionDatas[i].chunkData.northExits)
                        {
                            Vector3 exitPos = p_chunkConnexionDatas[i].chunkData.GetNorthExitPosition(index);
                            ChunkConnexionData endChunk = new ChunkConnexionData(chunkEnds.Find(x => x.northExits.Any()), exitPos, Directions.North);
                            index++;
                            p_chunkConnexionDatas.Add(endChunk);
                        }
                        break;
                    case Directions.South:
                        index = 0;
                        foreach (ChunkData.Exit ex in p_chunkConnexionDatas[i].chunkData.southExits)
                        {
                            Vector3 exitPos = p_chunkConnexionDatas[i].chunkData.GetSouthExitPosition(index);
                            ChunkConnexionData endChunk = new ChunkConnexionData(chunkEnds.Find(x => x.southExits.Any()), exitPos, Directions.South);
                            index++;
                            p_chunkConnexionDatas.Add(endChunk);
                        }
                        break;
                    case Directions.East:
                        index = 0;
                        foreach (ChunkData.Exit ex in p_chunkConnexionDatas[i].chunkData.eastExits)
                        {
                            Vector3 exitPos = p_chunkConnexionDatas[i].chunkData.GetEastExitPosition(index);
                            ChunkConnexionData endChunk = new ChunkConnexionData(chunkEnds.Find(x => x.eastExits.Any()), exitPos, Directions.East);
                            index++;
                            p_chunkConnexionDatas.Add(endChunk);
                        }
                        break;
                    case Directions.West:
                        index = 0;
                        foreach (ChunkData.Exit ex in p_chunkConnexionDatas[i].chunkData.westExits)
                        {
                            Vector3 exitPos = p_chunkConnexionDatas[i].chunkData.GetWestExitPosition(index);
                            ChunkConnexionData endChunk = new ChunkConnexionData(chunkEnds.Find(x => x.westExits.Any()), exitPos, Directions.West);
                            index++;
                            p_chunkConnexionDatas.Add(endChunk);
                        }
                        break;
                }

            }
        }
        return p_chunkConnexionDatas;
    }
}
