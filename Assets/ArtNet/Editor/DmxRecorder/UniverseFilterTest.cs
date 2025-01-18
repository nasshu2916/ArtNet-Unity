using System.Collections.Generic;
using NUnit.Framework;

namespace ArtNet.Editor.DmxRecorder
{
    public class UniverseFilterTest
    {
        [TestCase("", false)]
        [TestCase("1,2,3,4,5", false)]
        [TestCase("1  , 2 , -3,  5", false)]
        [TestCase("1-4, 7", false)]
        [TestCase("1-4, 3, 3", false)]
        [TestCase("1 1, 4", false)]
        [TestCase("1a", true)]
        [TestCase("0x01", true)]
        public void InvalidFilterTextFormatTest(string filterText, bool expected)
        {
            var universeFilter = new UniverseFilter
            {
                FilterText = filterText
            };
            Assert.AreEqual(expected, universeFilter.InvalidFilterTextFormat());
        }

        [TestCase("0-2 5", new string[] { })]
        [TestCase("", new[] { "Universe filter is empty" })]
        [TestCase("1a, 2, 3", new[] { "Invalid universe filter text format" })]
        [TestCase("1 -2, 3", new[] { "Universe filter contains invalid universe numbers. Valid range is 0-32767" })]
        [TestCase("1 100000000", new[] { "Universe filter contains invalid universe numbers. Valid range is 0-32767" })]
        public void GetErrorsTest(string filterText, string[] expected)
        {
            var universeFilter = new UniverseFilter
            {
                FilterText = filterText,
                Enabled = true
            };
            var errors = new List<string>();
            universeFilter.GetErrors(errors);
            CollectionAssert.AreEqual(expected, errors);
        }

        [TestCase("1 2, 3, , 4,5", new[] { 1, 2, 3, 4, 5 })]
        [TestCase("1  , 2 , -3,  5", new[] { -3, 1, 2, 5 })]
        [TestCase("0-4, 2, 7", new[] { 0, 1, 2, 3, 4, 7 })]
        [TestCase("1-4, 3, 3", new[] { 1, 2, 3, 4 })]
        public void ParseFilterTextTest(string filterText, int[] expected)
        {
            var universeFilter = new UniverseFilter
            {
                FilterText = filterText
            };
            Assert.IsTrue(universeFilter.ParseFilterText(out var universeList));
            var sortedList = new List<int>(universeList);
            sortedList.Sort();
            CollectionAssert.AreEqual(expected, sortedList);
        }
    }
}
