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
        private const string SplitPattern = @"[\s,]+";
        private const string FilterRangePattern = @"^\d+[-~]\d+$";
        private const string RangePattern = @"[-~]";
        private Regex _invalidFilterTextRegex = new(@"[^\d\s,-]");

        [SerializeField] private bool _enabled;
        [SerializeField] private string _filterText = "";

        private HashSet<int> _cachedFilterUniverses;

        public bool Enabled { get => _enabled; set => _enabled = value; }

        public string FilterText
        {
            get => _filterText;
            set
            {
                _cachedFilterUniverses = null;
                _filterText = value;
            }
        }

        public bool InvalidFilterTextFormat()
        {
            return _invalidFilterTextRegex.IsMatch(FilterText);
        }

        public bool Invalid()
        {
            var errors = new List<string>();
            GetErrors(errors);
            return errors.Count > 0;
        }

        public void GetErrors(List<string> errors)
        {
            if (Enabled == false) return;

            if (InvalidFilterTextFormat())
            {
                errors.Add("Invalid universe filter text format");
            }
            else if (ParseFilterText(out var universeList) == false)
            {
                errors.Add("Invalid universe filter text");
            }
            else if (universeList.Count == 0)
            {
                errors.Add("Universe filter is empty");
            }
            else if (universeList.Any(u => u is < 0 or > 0x7FFF))
            {
                errors.Add("Universe filter contains invalid universe numbers. Valid range is 0-32767");
            }
        }

        public HashSet<int> FilterUniverse()
        {
            if (_cachedFilterUniverses != null)
            {
                return _cachedFilterUniverses;
            }

            var result = ParseFilterText(out var universeList);
            return result == false ? new HashSet<int>() : universeList;
        }

        public IEnumerable<T> Filter<T>(IEnumerable<T> frames, Func<T, int> universeSelector)
        {
            if (Enabled == false || Invalid()) return frames;

            var filterUniverse = FilterUniverse();
            return frames.Where(f => filterUniverse.Contains(universeSelector(f)));
        }

        public bool ParseFilterText(out HashSet<int> universes)
        {
            if (_cachedFilterUniverses is not null)
            {
                universes = _cachedFilterUniverses;
                return true;
            }

            universes = new HashSet<int>();
            var filterParts = Regex.Split(FilterText, SplitPattern).Where(s => !string.IsNullOrWhiteSpace(s));

            foreach (var part in filterParts)
            {
                // 数値のみの場合
                if (int.TryParse(part, out var singleNumber))
                {
                    universes.Add(singleNumber);
                    continue;
                }

                // 範囲以外の文字列が含まれている場合は false を返す
                if (!Regex.IsMatch(part, FilterRangePattern)) return false;

                var rangeParts = Regex.Split(part, RangePattern).Select(int.Parse).ToArray();
                if (rangeParts.Length != 2) return false;

                var start = rangeParts[0];
                var end = rangeParts[1];
                if (start > end)
                {
                    (start, end) = (end, start);
                }
                universes.UnionWith(Enumerable.Range(start, end - start + 1));
            }

            _cachedFilterUniverses = universes;
            return true;
        }
    }
}
