using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellGenerator : Singleton<CellGenerator>
{
    [SerializeField] private ListGameObjectVariable _cellsList;
    [SerializeField] private GameConfigs _gameConfigs;
    [SerializeField] private GameObject _cellPrefab;

    protected override void Awake()
    {
        base.Awake();
        _cellsList.Value.Clear();
        Generate();
    }

    private void Generate()
    {
        #region Assert

        Debug.Assert(_gameConfigs.Columns > 0 && _gameConfigs.Rows > 0, $"{gameObject.name}: No cell was made! Both row and column need to be greater than 0.");
        Debug.Assert(_gameConfigs.CellDimension.x > 0 && _gameConfigs.CellDimension.y > 0, $"{gameObject.name}: Width and height of cell need to be greater than 0.");
        Debug.Assert(_gameConfigs.CellBackground, $"{gameObject.name}: Cell background sprite is none!");
        Debug.Assert(_gameConfigs.CellBackgroundColors.Length > 0, $"{gameObject.name}: Cell background colors are none!");
        Debug.Assert(_gameConfigs.CellSprites.Length > 0, $"{gameObject.name}: Cell sprites are none!");
#if DEBUG
        if (_gameConfigs.CellBackgroundColors.Length > 0)
            foreach (var color in _gameConfigs.CellBackgroundColors)
                Debug.Assert(color.a > 0, $"{gameObject.name}: Some cell background colors are set to 0!");
#endif

        #endregion Assert

        int colorIndex = 0;
        for (int row = 0; row < _gameConfigs.Rows * 2; row++)
        {
            for (int col = 0; col < _gameConfigs.Columns; col++)
            {
                // Instantiate cell
                var cell = Instantiate(_cellPrefab);
                var cellCmp = cell.GetComponent<Cell>();
                var cellPos = new Vector3(transform.position.x + _gameConfigs.CellDimension.x * col, transform.position.y + _gameConfigs.CellDimension.y * row, transform.position.z);
                cellCmp.Init();
                cell.transform.position = cellPos;
                _cellsList.Value.Add(cell);
                cell.name = "Cell" + (_cellsList.Value.Count - 1);

                // Instantiate cell background
                var cellBackground = new GameObject();
                cellBackground.name = "BG" + (_cellsList.Value.Count - 1);
                cellBackground.transform.position = cellPos;
                var bgRenderer = cellBackground.AddComponent<SpriteRenderer>();
                bgRenderer.sortingOrder = 0;
                bgRenderer.sprite = _gameConfigs.CellBackground;
                bgRenderer.color = _gameConfigs.CellBackgroundColors[colorIndex++ % _gameConfigs.CellBackgroundColors.Length];
            }
            colorIndex++;
        }
    }
}