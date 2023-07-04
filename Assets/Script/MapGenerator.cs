using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using System.Linq;

public class MapGenerator : MonoBehaviour
{
    public Transform chunksParent;
    public int amountOfChunksToSpawn;
    public int chunksToSpawnRemaining;

    public int chunkSize; // 16 for 16*16 OR 32 for 32*32 (/!\ depend on prefab size)

    public GameObject[] chunksToSouthArray;
    public GameObject[] chunksToNorthArray;
    public GameObject[] chunksToEastArray;
    public GameObject[] chunksToWestArray;

    public GameObject[] chunksToStart;

    [Serializable]
    public class GridTile
    {
        public List<ChunkHandler.Direction> directionsToProvide;
        public Vector3Int position;
        public bool hasSpawned = false;

        public GridTile(Vector3Int p_position)
        {
            directionsToProvide = new List<ChunkHandler.Direction>();
            position = p_position;
        }

        public Vector2Int GetPosition()
        {
            return new Vector2Int(position.x, position.z);
        }

        public void AddDirection(ChunkHandler.Direction p_directionsToProvide)
        {
            directionsToProvide.Add(p_directionsToProvide);
        }

        public void AddInvertedPosition(ChunkHandler.Direction p_directionsToProvide)
        {
            AddDirection(MapGenerator.GetInvertedDirection(p_directionsToProvide));
        }
    }

    //public List<ChunkToSpawn> chunkToSpawnList;

    public Queue chunksToManage; // list of Vector2 (coordinate of the grid)  where we must spawn a chunk
    public Queue chunksToAddOnGrid;

    void Start()
    {
        //GameObject firstChunk = chunksToStart[Random.Range(0, chunksToStart.Length)];
        //GameObject go = Instantiate(firstChunk, chunksParent.position, Quaternion.identity);
        //go.transform.SetParent(chunksParent.transform);


        // -- CONSTRAINTS MAP -- //
        // On va d'abord créer une grille abstraite
        // Cette grille va accueillir des tuiles qui vont posséder des contraintes
        // Une fois toutes les contraintes posées sur chaque tuiles, on génére la vrai map après 

        chunksToSpawnRemaining = amountOfChunksToSpawn;
        chunksToManage = new Queue();
        chunksToAddOnGrid = new Queue();
        SetupGrid();
        GenerateMap();
        GenerateGridContraintsMap();
        GenerateMap();
        GenerateGridContraintsMap();
        GenerateMap();
        GenerateGridContraintsMap();
        GenerateMap();
        GenerateGridContraintsMap();
        GenerateMap();
        GenerateGridContraintsMap();
        GenerateMap();

        // -- CHUNKS TO MANAGE -- //
        //chunksToManage = new Queue();
        //chunksToManage.Enqueue(go.GetComponent<ChunkHandler>());
        //StartCoroutine(GenerateMap());
    }

    // IDEE
    // GENERER une grille, et mettre au centre le spawn
    // INSTANTIER LE SPAWN
    // Pour chaque point de sortie du spawn, générer un objet contrainte à cet endroit
    // Puis générer un prefab une fois toutes les contraintes posées
    // Continuer jusqu'à la fin



    public GridTile[,] gridTileMap;
    void SetupGrid()
    {
        Vector2Int mapSize = new Vector2Int(15, 15); // valeur théorique arbitraire. Doit être suffisament grande pour accueillir la map
        gridTileMap = new GridTile[mapSize.x, mapSize.y]; // on crée un ableau bi-dimentionnel de 10 * 10

        // La première tuile (celle de départ) apparait au centre de la grille (+ ou -)
        Vector2Int pos = new Vector2Int((int)mapSize.x / 2, (int)mapSize.y / 2);
        gridTileMap[pos.x, pos.y] = new GridTile(new Vector3Int(pos.x, 0, pos.y));

        chunksToManage.Enqueue(new Vector2Int(pos.x, pos.y));
    }

    IEnumerator GenerateWorld()
    {
        yield return null;
    }

    void GenerateGridContraintsMap()
    {
        while (chunksToAddOnGrid.Count > 0)
        {
            ChunkHandler chunk = ((GameObject)chunksToAddOnGrid.Dequeue()).GetComponent<ChunkHandler>();
            foreach (ChunkHandler.Direction dir in chunk.GetAllDirections())
            {
                Vector2Int tilePos = GetGridTilePosition(chunk.gridPosition, dir);
                if (gridTileMap[tilePos.x, tilePos.y] == null)
                {
                    gridTileMap[tilePos.x, tilePos.y] = new GridTile(new Vector3Int(tilePos.x, 0, tilePos.y));
                    gridTileMap[tilePos.x, tilePos.y].AddInvertedPosition(dir);
                }
                else
                {
                    gridTileMap[tilePos.x, tilePos.y].AddInvertedPosition(dir);
                }
                chunksToManage.Enqueue(tilePos);
            }
        }
    }

    void GenerateMap()
    {
        while (chunksToManage.Count > 0)
        {
            Vector2Int chunkPosToSpawn = (Vector2Int)chunksToManage.Dequeue(); // on prends la position sur la grille du chunk à spawn
            GridTile gridTile = gridTileMap[chunkPosToSpawn.x, chunkPosToSpawn.y]; // On recupére la grid tile sur laquelle se trouve les contraintes

            if (gridTile.hasSpawned == true) // on a déjà fait cette tile avant
                continue;

            List<ChunkHandler> matchableChunks = GetMatchableChunks(gridTile);

            Debug.Log("Amount of matchable chunks : " + matchableChunks.Count + " for current grid tile position : "+gridTile.position);

            ChunkHandler chunkToSpawn = matchableChunks[Random.Range(0, matchableChunks.Count)];

            // on retire un nombre de chunks a spawn dépendant d'entrée/sortie que notre chunk va apporter
            chunksToSpawnRemaining -= (chunkToSpawn.GetAllDirections().Length - 1);


            SpawnChunk(chunkToSpawn.gameObject, chunksParent.position, chunkPosToSpawn);
            gridTile.hasSpawned = true;
        }

        #region toRemove
        /*for(int x = 0, n = gridTileMap.Length; x < n; x++)
        {
            for(int y = 0, m = gridTileMap[x].Length; y < m; y++)
            {
                // on regarde chaque tile de la constraint map, et on en cherche une qui n'as pas été spawn et qui n est pas null
                if(gridTileMap[x][y] != null && gridTileMap[x][y].hasSpawned == false)
                {

                }
            }
        }*/
        #endregion
    }

    Vector2Int GetGridTilePosition(Vector2Int pos, ChunkHandler.Direction dir)
    {
        Vector2Int targetPos = new Vector2Int();
        switch (dir)
        {
            case ChunkHandler.Direction.South: // SOUTH 
                targetPos = pos + new Vector2Int(1, 0);
                break;

            case ChunkHandler.Direction.North: // NORTH 
                targetPos = pos + new Vector2Int(-1, 0);
                break;

            case ChunkHandler.Direction.West:// WEST
                targetPos = pos + new Vector2Int(0, -1);
                break;

            case ChunkHandler.Direction.East: // EAST 
                targetPos = pos + new Vector2Int(0, 1);
                break;
        }
        return targetPos;
    }

    List<ChunkHandler> GetMatchableChunks(GridTile chosenTile)
    {
        List<ChunkHandler.Direction> directionConstraints = chosenTile.directionsToProvide;
        // invert directions to have correct match
        List<ChunkHandler.Direction> invertedDirectionConstraint = new List<ChunkHandler.Direction>();
        foreach (ChunkHandler.Direction dir in directionConstraints)
        {
            invertedDirectionConstraint.Add(GetInvertedDirection(dir));
        }
        List<ChunkHandler> matchableChunks = new List<ChunkHandler>();

        List<GameObject> allChunks = new List<GameObject>();
        // on peuple all chunks
        #region peuple all chunks
        string allChunksInfo = "";
        if (directionConstraints.Contains(ChunkHandler.Direction.West) || !directionConstraints.Any()) // East => West
        {
            allChunks.AddRange(chunksToWestArray.ToList());
            allChunksInfo += "Add West || ";
        }
        if (directionConstraints.Contains(ChunkHandler.Direction.South) || !directionConstraints.Any()) // North => South
        {
            allChunks.AddRange(chunksToSouthArray.ToList());
            allChunksInfo += "Add South || ";
        }
        if (directionConstraints.Contains(ChunkHandler.Direction.North) || !directionConstraints.Any()) // South => North
        {
            allChunks.AddRange(chunksToNorthArray.ToList());
            allChunksInfo += "Add North || ";
        }
        if (directionConstraints.Contains(ChunkHandler.Direction.East) || !directionConstraints.Any()) // West => East
        {
            allChunks.AddRange(chunksToEastArray.ToList());
            allChunksInfo += "Add East || ";
        }
            
        #endregion

        //allChunks = chunksToStart.ToList();
        Debug.Log("=================");
        Debug.Log("All chunks = " + allChunks.Count + " with within : "+allChunksInfo);
        foreach (GameObject chunk in allChunks)
        {
            ChunkHandler ch = chunk.GetComponent<ChunkHandler>();
            int amountOfValidatedDirections = 0;

            if (ch.GetAllDirections().Length - 1 >= chunksToSpawnRemaining)
                continue;

            Debug.Log("Test for chunk = " + chunk.name);

            foreach (ChunkHandler.Direction dir in ch.GetAllDirections())
            {
                Debug.Log("Dir = "+dir+" and invertedDirectionConstraint = "+(invertedDirectionConstraint.Any()?invertedDirectionConstraint[0] : "null"));
                if (invertedDirectionConstraint.Contains(dir) && invertedDirectionConstraint.Any())
                {
                    Vector2Int tileAround = GetGridTilePosition(chosenTile.GetPosition(), dir);
                    // on regarde si y a pas déjà une tile à l'emplacement de chaque embouchure potentielle
                    Debug.Log("Selected position = " + tileAround.x + ", " + tileAround.y);
                    if (gridTileMap[tileAround.x, tileAround.y] != null) // si oui, alors les 2 entrées/sorties doivent coincider
                    {
                        if (gridTileMap[tileAround.x, tileAround.y].directionsToProvide.Contains(GetInvertedDirection(dir))) // pour que les entrées sorties matchent, on doit flip
                        {
                            amountOfValidatedDirections++;
                        }
                    }
                    else // si non : osef
                        amountOfValidatedDirections++;
                }
            }

            if (amountOfValidatedDirections >= invertedDirectionConstraint.Count)
            {
                matchableChunks.Add(ch);
            }
        }
        return matchableChunks;
    }

    public static ChunkHandler.Direction GetInvertedDirection(ChunkHandler.Direction dir)
    {
        ChunkHandler.Direction invertedDir;
        switch (dir)
        {
            case ChunkHandler.Direction.South: // SOUTH 
                invertedDir = ChunkHandler.Direction.North;
                break;

            case ChunkHandler.Direction.North: // NORTH 
                invertedDir = ChunkHandler.Direction.South;
                break;

            case ChunkHandler.Direction.West:// WEST
                invertedDir = ChunkHandler.Direction.East;
                break;

            default: // EAST 
                invertedDir = ChunkHandler.Direction.West;
                break;
        }

        return invertedDir;
    }

    /*IEnumerator GenerateMapTest() // deprecated
    {
        while(chunksToManage.Count > 0)
        {
            // get current chunk
            ChunkHandler currentChunk = (ChunkHandler) chunksToManage.Dequeue();
            // get all his directions
            ChunkHandler.Direction[] directions = currentChunk.GetAllDirections();

            // for each of thoses directions, spawn a chunk facing the right direction
            for(int i = 0, n = directions.Length; i < n; i++)
            {
                GameObject nextChunk = GetARandomChunkForTargetDirection(directions[i]);
                SpawnChunk(nextChunk, currentChunk.transform.position, directions[i]);
            }

            yield return new WaitForSeconds(0.8f);
        }
        UnityEngine.Debug.Log("Dungeon map generation ended !");
    }*/

    /*  public GameObject GetARandomChunkForTargetDirection(ChunkHandler.Direction direction)
      {
          switch (direction)
          {
              case ChunkHandler.Direction.South: // SOUTH => NORTH
                  return chunksToSouthArray[Random.Range(0, chunksToSouthArray.Length)];
                  break;

              case ChunkHandler.Direction.North: // NORTH => SOUTH
                  return chunksToNorthArray[Random.Range(0, chunksToNorthArray.Length)];
                  break;

              case ChunkHandler.Direction.West:// WEST => EAST
                  return chunksToWestArray[Random.Range(0, chunksToWestArray.Length)];
                  break;

              case ChunkHandler.Direction.East: // EAST => WEST
                  return chunksToEastArray[Random.Range(0, chunksToEastArray.Length)];
                  break;
          }

          return null;
      }*/

    public void SpawnChunk(GameObject chunkToSpawn, Vector3 parentChunkPosition, Vector2Int position)
    {
        Vector3 positionToSpawn = new Vector3(position.x, 0, position.y) * chunkSize;
        Debug.Log("Spawn of " + chunkToSpawn.name);

        GameObject go = Instantiate(chunkToSpawn, positionToSpawn, Quaternion.identity);
        go.transform.SetParent(chunksParent.transform);

        go.GetComponent<ChunkHandler>().SetupChunk(position);

        chunksToAddOnGrid.Enqueue(go);
    }
}
