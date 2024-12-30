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
        private Regex _invalidFilterTextRegex = new(@"[^\d\s,-]");

        [SerializeField] private bool _enabled;
        [SerializeField] private string _filterText = "";

        private bool _cacheEnabled;
        private List<int> _cachedFilterUniverseList = new();

        public bool Enabled { get => _enabled; set => _enabled = value; }

        public string FilterText
        {
            get => _filterText;
            set
            {
                _cacheEnabled = false;
                _filterText = value;
            }
        }

        public bool InvalidFilterTextFormat()
        {
            return _invalidFilterTextRegex.IsMatch(FilterText);
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

        public bool IsMatch(int universe)
        {
            if (Enabled == false) return true;

            return GetUniverseList().Contains(universe);
        }

        public List<int> GetUniverseList()
        {
            ParseFilterText(out var universeList);
            return universeList;
        }

        public bool ParseFilterText(out List<int> universeList)
        {
            universeList = new List<int>();
            if (_cacheEnabled)
            {
                universeList = new List<int>(_cachedFilterUniverseList);
                return true;
            }

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
            _cachedFilterUniverseList = new List<int>(universeList);
            _cacheEnabled = true;
            return true;
        }
    }
}
