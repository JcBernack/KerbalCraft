using System.Collections.Generic;
using UnityEngine;

namespace CraftShare
{
    public class GUITable
    {
        private string[] _headers;
        private readonly List<string[]> _rows;

        public GUITable()
        {
            _rows = new List<string[]>();
        }

        public void AddColumns(params string[] headers)
        {
            _headers = headers;
        }

        public void AddRow(params string[] cells)
        {
            _rows.Add(cells);
        }

        public void ClearRows()
        {
            _rows.Clear();
        }

        public bool Render(out int clickedRow)
        {
            clickedRow = -1;
            GUILayout.BeginHorizontal();
            for (var col = 0; col < _headers.Length; col++)
            {
                GUILayout.BeginVertical();
                GUILayout.Label(_headers[col], ModGlobals.HeadStyle);
                for (var row = 0; row < _rows.Count; row++)
                {
                    if (col > _rows[row].Length) continue;
                    if (GUILayout.Button(_rows[row][col], ModGlobals.RowStyle))
                    {
                        clickedRow = row;
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
            return clickedRow > -1;
        }
    }
}