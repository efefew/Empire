using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Tilemaps;

public class MapCreator : MonoBehaviour
{
    #region Enums

    private enum TypeDirection
    {
        up, down, left, right
    }

    #endregion Enums

    #region Fields

    public Tilemap mapStone;
    public TileBase tileStone;

    #endregion Fields

    #region Methods

    private void OnEnable() => CreateTileArray(CreateMaze(width: 99, height: 99, new Vector2Int(0, 0)).AddBorders(true), mapStone, tileStone);

    private void CreateTileArray(bool[,] points, Tilemap map, TileBase tile, int addX = 0, int addY = 0)
    {
        for (int x = 0; x < points.GetLength(0); x++)
        {
            for (int y = 0; y < points.GetLength(1); y++)
            {
                if (points[x, y])
                    map.SetTile(new Vector3Int(x + addX, y + addY, 0), tile);
            }
        }
    }

    public bool[,] CreateMaze(int width, int height, Vector2Int start)
    {
        bool[,] maze = new bool[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
                maze[x, y] = true;
        }

        bool[,] visited = new bool[width, height];
        int X = start.x, Y = start.y;
        List<Vector2Int> path = new() { start };
        while (maze.Length > 0)
        {
            visited[X, Y] = true;
            maze[X, Y] = false;

            List<TypeDirection> directions = new();
            if (Y + 2 < height && maze[X, Y + 1] && !visited[X, Y + 2])
                directions.Add(TypeDirection.up);
            if (Y - 2 >= 0 && maze[X, Y - 1] && !visited[X, Y - 2])
                directions.Add(TypeDirection.down);
            if (X + 2 < width && maze[X + 1, Y] && !visited[X + 2, Y])
                directions.Add(TypeDirection.right);
            if (X - 2 >= 0 && maze[X - 1, Y] && !visited[X - 2, Y])
                directions.Add(TypeDirection.left);

            if (directions.Count == 0)
            {
                _ = path.Remove(path.Last());
                if (path.Count == 0)
                    break;
                X = path.Last().x;
                Y = path.Last().y;
                continue;
            }

            TypeDirection direction = directions[Random.Range(0, directions.Count)];
            switch (direction)
            {
                case TypeDirection.up:
                    maze[X, Y + 1] = false;
                    Y += 2;
                    break;

                case TypeDirection.down:
                    maze[X, Y - 1] = false;
                    Y -= 2;
                    break;

                case TypeDirection.right:
                    maze[X + 1, Y] = false;
                    X += 2;
                    break;

                case TypeDirection.left:
                    maze[X - 1, Y] = false;
                    X -= 2;
                    break;
            }

            path.Add(new Vector2Int(X, Y));
        }

        return maze;
    }

    #endregion Methods
}