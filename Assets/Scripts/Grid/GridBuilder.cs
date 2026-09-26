using System.Collections.Generic;
using UnityEngine;

public class GridBuilder : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] Transform rowBlock;
    [SerializeField] Transform columnBlock;
    [SerializeField] int rows = 5;
    [SerializeField] int columns = 5;
    [SerializeField] int spacing;

    [Header("Blocks")]
    [SerializeField] List<Transform> gridBlocks = new List<Transform>();

    int rowSpacing = 0;

    private void Start()
    {
        CreateGrid();
    }

    void CreateGrid()
    {
        int pos = 0;
        for (int i = 0; i < rows; i++)
        {
            var clonedRow = Instantiate(rowBlock, new Vector3(0, 0, rowSpacing), Quaternion.identity, transform);
            clonedRow.name = $"Row {i+1}";

            int columnSpacing = 0;

            for (int j = 0; j < columns; j++)
            {
                var clonedColumn = Instantiate(columnBlock, clonedRow);
                clonedColumn.localPosition = new Vector3(columnSpacing, 0, 0);
                clonedColumn.name = $"Column {i + 1}";

                pos++;
                clonedColumn.GetComponent<GridBlock>().Position = pos;

                clonedColumn.GetComponent<GridBlock>().X = j;
                clonedColumn.GetComponent<GridBlock>().Y = i;

                columnSpacing += spacing;

                gridBlocks.Add(clonedColumn);
            }

            rowSpacing += spacing;
        }

        int size = spacing * (columns - 1);
        int center = size / 2;

        transform.localPosition = new Vector3(-center, 0, 0);
    }

    public List<Transform> GetBlocks()
    {
        return gridBlocks;
    }

    public int GetLength() => columns;
}
