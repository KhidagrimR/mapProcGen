using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Random = UnityEngine.Random;

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
    public ChunkConnexionData[] chunkConnexionDatas;

    [Serializable]
    public struct ChunkConnexionData
    {
        public ChunkData chunkData;
        public Vector3 position;
        public List<Directions> connexionDirection;
        public ChunkConnexionData(ChunkData p_chunkData, Vector3 p_position, Directions p_connexionDirection)
        {
            chunkData = p_chunkData;
            position = p_position;

            connexionDirection = new List<Directions>();
            connexionDirection.Add(p_connexionDirection);
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
        chunkConnexionDatas = new ChunkConnexionData[mapSize];

        // we start with a random chunk
        chunkConnexionDatas[0] = new ChunkConnexionData(chunkDatas[Random.Range(0, chunkDatas.Count)], new Vector3(0, 0, 0), Directions.Null); // "Directions.North" is arbitrary here and don't do anything
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
            chunkConnexionDatas[i + 1] = new ChunkConnexionData(matchableChunk, GetPositionFromDirection(chosenDirection, chunkConnexionDatas[i].position), chosenDirection);
            Debug.Log("<color=red>======</color>");
            Debug.Log("Targetted Direction = "+chosenDirection);
            Debug.Log("current chunk pos = " + chunkConnexionDatas[i + 1].position.z);
            Debug.Log("Position of current chunk exit = "+chunkConnexionDatas[i + 1].chunkData.GetExitsFromDirection(chosenDirection).z_position);
            Debug.Log("previous chunk pos = " +  chunkConnexionDatas[i].position.z );
            Debug.Log("Position of previous chunk exit = "+chunkConnexionDatas[i].chunkData.GetExitsFromDirection(InvertDirection(chosenDirection)).z_position);
            Debug.Log("<color=red>======</color>");

            chunkConnexionDatas[i + 1].position.z = chunkConnexionDatas[i].position.z +
                                                        chunkConnexionDatas[i].chunkData.GetExitsFromDirection(InvertDirection(chosenDirection)).z_position -
                                                        chunkConnexionDatas[i + 1].chunkData.GetExitsFromDirection(chosenDirection).z_position;
        }

        #region removed
        /*
        // ## ADD FIRST CHUNK ## //
        // On rajoute un chunk sur l'une de ses exits available
        ChunkData matchableChunk = chunkDatas.Find(x => IsChunkMatching(chunkConnexionDatas[0].chunkData, x, chunkConnexionDatas[0].connexionDirection));
        // is chunkMatching add all available connexions to the var "directionMatched"
        Directions chosenDirection = directionMatched[Random.Range(0, directionMatched.Count)];

        // On ajoute le chunk choisis
        chunkConnexionDatas[1] = new ChunkConnexionData(matchableChunk, GetPositionFromDirection(chosenDirection, chunkConnexionDatas[0].position), chosenDirection);


        // ----------------  REPEAT
        chunkDatas = Shuffle<ChunkData>(chunkDatas);
        // On rajoute un chunk sur l'une de ses exits available
        matchableChunk = chunkDatas.Find(x => IsChunkMatching(chunkConnexionDatas[1].chunkData, x, chunkConnexionDatas[1].connexionDirection));
        // is chunkMatching add all available connexions to the var "directionMatched"
        chosenDirection = directionMatched[Random.Range(0, directionMatched.Count)];
        // On ajoute le chunk choisis
        chunkConnexionDatas[2] = new ChunkConnexionData(matchableChunk, GetPositionFromDirection(chosenDirection, chunkConnexionDatas[1].position), chosenDirection);
        */
        #endregion

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


    public string ChunkToString(ChunkConnexionData[] p_chunkDatasList)
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


}

/*
void GenerateMap()
{
      // we want to make a map of size Mapsize
        map = new ChunkData[mapSize];

        // we start with a random chunk
        map[0] = chunkDatas[Random.Range(0, chunkDatas.Count)];


        int exitIndex = 0;
        // for each of that chunk's exit, we add a new chunk nearby or stop the generation if we exceed the amount of chunk
        for(int i = 0, n = map[0].allExits.Count; i < n; i++)
        {
            // on trouve un chunck qui peut "matcher"
            ChunkData[] matchableChunk = chunkDatas.FindAll(x => IsChunkMatching(map[i], x).Any()).ToArray(); 
            map[i+1] = matchableChunk[Random.Range(0, matchableChunk.Length)]; // et on l'ajoute

            // puis on ajoute la connexion
            List<Directions> listDir = IsChunkMatching(map[i], map[i+1]);
            Directions dir = listDir[Random.Range(0, listDir.Count)];
            chunksConnexionsDict.Add(dir.ToString(), InvertDirection(dir).ToString());

            if(mapSize <= i+1) // si on a le max chunk amount
                break; // on termine
        }
        exitIndex++;

        // si on a pas mis assez de chunks, on regarde si il reste encore des sorties utilisables
        // si oui
        if(map[exitIndex].allExits.Count - 1 > 0)
        {
            
        }
        // si non
        else
        {

        }

        // puis on transform notre tableau en JSON



        //
        for (int i = 0, n = mapSize - 1; i < n; i++)
        {
            // ce chunk a plusieurs entrées/sorties, on va donc prendre un nouveau chunk dont les entrées sorties coincident
            ChunkData[] matchableChunk = chunkDatas.FindAll(x => IsChunkMatching(map[i], x).Any()).ToArray();
            map[i+1] = matchableChunk[Random.Range(0, matchableChunk.Length)];


            // We might need to exit the loop when there is no more exits
        }//
}*/