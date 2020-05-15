using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Omnipoof {
  namespace SuperTiled2Unity {
    public class SuperMapUtils {
      /// <summary>
      /// Creates a boolean representation of the given tilemap's boundaries.  This
      /// boundary map is used as a parameter for other <see cref="SuperMapUtils"/>
      /// methods.
      /// </summary>
      /// <param name="tilemap">
      /// A Unity tilemap where all tiles are meant to represent boundaries and empty
      /// tiles represent open spaces.
      /// </param>
      /// <returns>
      /// A two-dimensional bool array where boundary tiles are represented as "false"
      /// and empty tiles are represented as "true".
      /// </returns>
      public static bool[,] CreateBoundaryMap(Tilemap tilemap) {
        Vector3Int size = tilemap.size;
        bool[,] boundaryMap = new bool[size.x, size.y];
        for (int x = 0; x < size.x; x++) {
          for (int y = 0; y < size.y; y++) {
            // We have to negate the y value because SuperTiled2Unity uses negative y tile
            // coordinates but the y index in the boundary map needs to be positive
            TileBase tile = tilemap.GetTile(new Vector3Int(x, -y, 0));
            boundaryMap[x, y] = tile == null;
          }
        }

        return boundaryMap;
      }

      /// <summary>
      /// Finds a path of tile coordinates between the given starting (exclusive) and
      /// destination (inclusive) tile coordinates.
      /// </summary>
      /// <param name="boundaryMap">
      /// A two-dimentional bool array created from <see cref="CreateBoundaryMap"/>.
      /// </param>
      /// <param name="startingTileCoordinate">
      /// The tile coordinate of where to start the pathfinding.
      /// </param>
      /// <param name="destinationTileCoordinate">
      /// The tile coordinate of where to end the pathfinding.
      /// </param>
      /// <returns>
      /// A list of tile coordinates representing the path to the destination tile coordinate.
      /// </returns>
      public static List<Vector3Int> GetTileCoordinatePath(bool[,] boundaryMap, Vector3Int startingTileCoordinate, Vector3Int destinationTileCoordinate) {
        // We have to negate the y value because SuperTiled2Unity uses negative y tile
        // coordinates but the y index in the boundary map needs to be positive
        if (boundaryMap[destinationTileCoordinate.x, -destinationTileCoordinate.y] == false) {
          return new List<Vector3Int>();
        }

        PathFind.Grid boundaryGrid = CreatePathFindGrid(boundaryMap);
        PathFind.Point startingPoint = new PathFind.Point(startingTileCoordinate.x, startingTileCoordinate.y);
        PathFind.Point destinationPoint = new PathFind.Point(destinationTileCoordinate.x, destinationTileCoordinate.y);
        List<PathFind.Point> pathPoints = PathFind.Pathfinding.FindPath(boundaryGrid, startingPoint, destinationPoint);
        return ConvertToTileCoordinates(pathPoints);
      }

      /// <summary>
      /// Finds the adjacent tile coordinate to the given tile coordinate that is the furthest
      /// from the given reference tile coordinate.
      /// </summary>
      /// <param name="boundaryMap">
      /// A two-dimentional bool array created from <see cref="CreateBoundaryMap"/>.
      /// </param>
      /// <param name="tileCoordinate">
      /// The tile coordinate by which adjacent tile coordinates will be checked.
      /// </param>
      /// <param name="referenceTileCoordinate">
      /// The tile coordinate from which the distance will be calculated to tile coordinates
      /// adjacent to <see cref="tileCoordinate"/>.
      /// </param>
      /// <returns>
      /// The adjacent tile coordinate that is the furthest from the reference tile coordinate
      /// </returns>
      public static Vector3Int? GetFurthestAdjacentTileCoordinate(bool[,] boundaryMap, Vector3Int tileCoordinate, Vector3Int referenceTileCoordinate) {
        Vector3Int? furthestAdjacentTileCoordinate = null;
        List<Vector3Int> candidateTileCoordinates = new List<Vector3Int>();
        candidateTileCoordinates.Add(tileCoordinate + new Vector3Int(0, 1, 0)); // Up
        candidateTileCoordinates.Add(tileCoordinate + new Vector3Int(1, 0, 0)); // Right
        candidateTileCoordinates.Add(tileCoordinate + new Vector3Int(0, -1, 0)); // Down
        candidateTileCoordinates.Add(tileCoordinate + new Vector3Int(-1, 0, 0)); // Left

        float furthestDistance = -1;
        float candidateDistance;

        foreach (Vector3Int candidateTileCoordinate in candidateTileCoordinates) {
          candidateDistance = (candidateTileCoordinate - referenceTileCoordinate).magnitude;

          // We have to negate the y value because SuperTiled2Unity uses negative y tile
          // coordinates but the y index in the boundary map needs to be positive
          if (candidateDistance > furthestDistance && boundaryMap[candidateTileCoordinate.x, -candidateTileCoordinate.y]) {
            furthestAdjacentTileCoordinate = candidateTileCoordinate;
            furthestDistance = candidateDistance;
          }
        }

        return furthestAdjacentTileCoordinate;
      }

      /// <summary>
      /// Creates a grid that will be used for pathfinding.
      /// </summary>
      /// <param name="boundaryMap">
      /// A two-dimentional bool array created from <see cref="CreateBoundaryMap"/>.
      /// </param>
      /// <returns>
      /// A grid that will be used for pathfinding.
      /// </returns>
      private static PathFind.Grid CreatePathFindGrid(bool[,] boundaryMap) {
        return new PathFind.Grid(boundaryMap.GetLength(0), boundaryMap.GetLength(1), boundaryMap);
      }

      /// <summary>
      /// Converts a list of pathfinding coordinates to tile coordinates.
      /// </summary>
      /// <param name="pathPoints">
      /// A list of points created while pathfinding.
      /// </param>
      /// <returns>
      /// A list of tile coordinates.
      /// </returns>
      private static List<Vector3Int> ConvertToTileCoordinates(List<PathFind.Point> pathPoints) {
        List<Vector3Int> tileCoordinates = new List<Vector3Int>();
        if (pathPoints.Count != 0) {
          for (int i = 0; i < pathPoints.Count; i++) {
            PathFind.Point pathPoint = pathPoints[i];
            tileCoordinates.Add(new Vector3Int(pathPoint.x, -pathPoint.y, 0));
          }
        }

        return tileCoordinates;
      }
    }
  }
}
