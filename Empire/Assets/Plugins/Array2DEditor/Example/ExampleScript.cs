using UnityEngine;

namespace Array2DEditor
{
    public class ExampleScript : MonoBehaviour
    {
        [SerializeField] private Array2DBool shape;

        [SerializeField] private GameObject prefabToInstantiate;


        private void Start()
        {
            if (shape == null || prefabToInstantiate == null)
            {
                Debug.LogError("Fill in all the fields in order to start this example.");
                return;
            }

            bool[,] cells = shape.GetCells();

            GameObject piece = new("Piece");

            for (int y = 0; y < shape.GridSize.y; y++)
            for (int x = 0; x < shape.GridSize.x; x++)
                if (cells[y, x])
                {
                    GameObject prefabGO = Instantiate(prefabToInstantiate, new Vector3(y, 0, x), Quaternion.identity,
                        piece.transform);
                    prefabGO.name = $"({x}, {y})";
                }
        }
    }
}