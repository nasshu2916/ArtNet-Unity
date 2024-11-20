using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ArtNet.Editor.DmxRecorder
{
    [Serializable]
    public class UniverseFilter
    {
        [SerializeField] private bool _enabled = true;
        [SerializeField] private string _filterText = "";

        private const string SplitPattern = @"[\s,]+";
        private const string FilterRangePattern = @"^\d+[-~]\d+$";

        public bool Enabled { get => _enabled; set => _enabled = value; }
        public string FilterText { get => _filterText; set => _filterText = value; }

        public bool IsInvalidFilterText()
        {
            var result = ParseFilterText(out var universeList);
            if (result == false) return true;
            return universeList.Count == 0;
        }

        private bool ParseFilterText(out List<int> universeList)
        {
            universeList = new List<int>();
            var result = new HashSet<int>();
            var filterParts = Regex.Split(FilterText, SplitPattern).Where(s => !string.IsNullOrWhiteSpace(s));

            foreach (var part in filterParts)
            {
                if (int.TryParse(part, out var singleNumber))
                {
                    result.Add(singleNumber);
                    continue;
                }

                if (Regex.IsMatch(part, FilterRangePattern))
                {
                    var rangeParts = Regex.Split(part, @"[-~]").Select(int.Parse).ToArray();
                    if (rangeParts.Length != 2) return false;

                    var start = rangeParts[0];
                    var end = rangeParts[1];
                    if (start > end)
                    {
                        (start, end) = (end, start);
                    }
                    result.UnionWith(Enumerable.Range(start, end - start + 1));
                    continue;
                }

                return false;
            }

            universeList = result.OrderBy(x => x).ToList();
            return true;
        }
    }
}
