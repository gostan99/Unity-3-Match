using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellGenerator : MonoBehaviour
{
    [SerializeField] private ListGameObjectVariable _cellsList;
    [SerializeField] private GameConfigs _gameConfigs;

    private void Awake()
    {
        _cellsList.Value.Clear();
        Generate();
    }

    private void Generate()
    {
        #region Assert

        Debug.Assert(_gameConfigs.Columns > 0 && _gameConfigs.Rows > 0, "Grid: No cell was made! Both row and column need to be greater than 0.");
        Debug.Assert(_gameConfigs.CellDimension.x > 0 && _gameConfigs.CellDimension.y > 0, "Grid: Width and height of cell need to be greater than 0.");
        Debug.Assert(_gameConfigs.CellBackground, "Grid: Cell background sprite is none!");
        Debug.Assert(_gameConfigs.CellBackgroundColors.Length > 0, "Grid: Cell background colors are none!");
        Debug.Assert(_gameConfigs.CellSprites.Length > 0, "Grid: Cell sprites are none!");
#if DEBUG
        if (_gameConfigs.CellBackgroundColors.Length > 0)
            foreach (var color in _gameConfigs.CellBackgroundColors)
                Debug.Assert(color.a > 0, "Grid: Some cell background colors are set to 0!");
#endif

        #endregion Assert

        int colorIndex = 0;
        for (int row = 0; row < _gameConfigs.Rows; row++)
        {
            for (int col = 0; col < _gameConfigs.Columns; col++)
            {
                // Instantiate cell
                var cell = new GameObject();
                var collider = cell.AddComponent<BoxCollider2D>(); // for Raycast2D
                var cellRenderer = cell.AddComponent<SpriteRenderer>();
                var cellPos = new Vector3(transform.position.x + _gameConfigs.CellDimension.x * col, transform.position.y + _gameConfigs.CellDimension.y * row, transform.position.z);
                cell.transform.position = cellPos;
                cell.layer = LayerMask.NameToLayer(_gameConfigs.CellLayer); // for raycasting
                collider.size = _gameConfigs.CellDimension;
                cellRenderer.sortingOrder = 1;
                cellRenderer.sprite = GetRandomCellSprite();
                _cellsList.Value.Add(cell);
                cell.name = "Cell" + (_cellsList.Value.Count - 1);

                // Instantiate cell background
                var cellBackground = new GameObject();
                cellBackground.transform.position = cellPos;
                var bgRenderer = cellBackground.AddComponent<SpriteRenderer>();
                bgRenderer.sortingOrder = 0;
                bgRenderer.sprite = _gameConfigs.CellBackground;
                bgRenderer.color = _gameConfigs.CellBackgroundColors[colorIndex++ % _gameConfigs.CellBackgroundColors.Length];
            }
            colorIndex++;
        }
    }

    private Sprite GetRandomCellSprite()
    {
        int index = Random.Range(0, _gameConfigs.CellSprites.Length);
        return _gameConfigs.CellSprites[index];
    }
}