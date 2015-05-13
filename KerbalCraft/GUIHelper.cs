using System;
using System.Collections.Generic;
using UnityEngine;

namespace KerbalCraft
{
    public static class GUIHelper
    {
        /// <summary>
        /// Dynamically renders a clickable grid of strings. If transpose is false the first row is rendered as bold labels otherwise the first column is rendered as bold labels, these are not clickable.
        /// </summary>
        /// <param name="dimension">Specifies either the number of columns if transpose is false or the number of rows if transpose is true.</param>
        /// <param name="transpose">Specifies how to interpret the dimension parameter.</param>
        /// <param name="cells">Specifies the strings to render in the grid.</param>
        /// <returns>The clicked cell as an index into cells.</returns>
        public static int Grid(int dimension, bool transpose, IList<string> cells)
        {
            var columns = transpose ? Math.Ceiling((float) cells.Count/dimension) : dimension;
            var rows = transpose ? dimension : Math.Ceiling((float) cells.Count/dimension);
            var clickedCell = -1;
            GUILayout.BeginHorizontal();
            for (var col = 0; col < columns; col++)
            {
                GUILayout.BeginVertical();
                for (var row = 0; row < rows; row++)
                {
                    var cell = transpose ? row + col * dimension : col + row * dimension;
                    if (cell >= cells.Count) break;
                    if (transpose && col == 0 || !transpose && row == 0) GUILayout.Label(cells[cell], ModGlobals.HeadStyle);
                    else if (GUILayout.Button(cells[cell], ModGlobals.RowStyle)) clickedCell = cell;
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
            return clickedCell;
        }
    }
}