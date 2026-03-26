using UnityEngine;
using System.Collections.Generic;
using Factory.Factory;

namespace Factory.Core
{
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }

        [SerializeField] private int width = 50;
        [SerializeField] private int height = 50;
        [SerializeField] private float cellSize = 1f;
        public float CellSize => cellSize;

        private Dictionary<Vector2Int, FactoryBuilding> gridBuildings = new Dictionary<Vector2Int, FactoryBuilding>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public Vector3 GetNearestPointOnGrid(Vector3 position)
        {
            float x = Mathf.Round(position.x / cellSize) * cellSize;
            float z = Mathf.Round(position.z / cellSize) * cellSize;
            return new Vector3(x, 0, z);
        }

        public Vector2Int WorldToGridPos(Vector3 position)
        {
            int x = Mathf.FloorToInt(position.x / cellSize);
            int y = Mathf.FloorToInt(position.z / cellSize);
            return new Vector2Int(x, y);
        }

        public bool IsTileOccupied(Vector2Int position) => gridBuildings.ContainsKey(position);

        public bool IsAreaOccupied(Vector2Int position, Vector2Int size)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    if (gridBuildings.ContainsKey(position + new Vector2Int(x, y))) return true;
                }
            }
            return false;
        }

        public void RegisterBuilding(Vector2Int position, Vector2Int size, FactoryBuilding building)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    Vector2Int p = position + new Vector2Int(x, y);
                    if (!gridBuildings.ContainsKey(p))
                    {
                        gridBuildings.Add(p, building);
                    }
                }
            }
        }

        public void RemoveBuilding(Vector2Int position, Vector2Int size)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    Vector2Int p = position + new Vector2Int(x, y);
                    if (gridBuildings.ContainsKey(p))
                    {
                        gridBuildings.Remove(p);
                    }
                }
            }
        }

        public FactoryBuilding GetBuilding(Vector2Int position)
        {
            return gridBuildings.ContainsKey(position) ? gridBuildings[position] : null;
        }

        public IEnumerable<FactoryBuilding> GetAllBuildings()
        {
            HashSet<FactoryBuilding> unique = new HashSet<FactoryBuilding>();
            foreach (var b in gridBuildings.Values) unique.Add(b);
            return unique;
        }
    }
}
