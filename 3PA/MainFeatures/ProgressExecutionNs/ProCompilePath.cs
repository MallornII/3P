﻿#region header
// ========================================================================
// Copyright (c) 2015 - Julien Caillon (julien.caillon@gmail.com)
// This file (CompilationPath.cs) is part of 3P.
// 
// 3P is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// 3P is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with 3P. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using _3PA.Lib;

namespace _3PA.MainFeatures.ProgressExecutionNs {
    public class ProCompilePath {

        #region fields

        private static List<CompilationPathItem> _compilationPathList = new List<CompilationPathItem>();
        private const string FileName = "_CompilationPath.conf";

        #endregion

        #region Read and Export

        /// <summary>
        /// Read the list of compilation Path Items,
        /// if the file is present in the Config dir, use it
        /// </summary>
        public static void Import() {
            _compilationPathList.Clear();
            ConfLoader.ForEachLine(Path.Combine(Npp.GetConfigDir(), FileName), new byte[0], Encoding.Default, s => {
                var items = s.Split('\t');
                if (items.Count() == 4) {
                    _compilationPathList.Add(new CompilationPathItem {
                        ApplicationFilter = items[0],
                        EnvLetterFilter = items[1],
                        InputPathPattern = items[2].Trim().Replace('/', '\\'),
                        OutputPathAppend = items[3].Trim().Replace('/', '\\')
                    });
                }
            });
        }

        /// <summary>
        /// Export this class info to a config file 
        /// </summary>
        public static void Export() {
            //if (_keywords.Count == 0) return;
            //var strBuilder = new StringBuilder();
            //foreach (var keyword in _keywords)
            //{
            //    strBuilder.AppendLine(keyword.DisplayText + "\t" + keyword.SubString + "\t" + ((keyword.Flag.HasFlag(ParseFlag.Reserved)) ? "1" : "0") + "\t" + keyword.Ranking);
            //}
            //File.WriteAllText(_filePath, strBuilder.ToString());
        }

        #endregion

        #region public methods

        /// <summary>
        /// This method returns the correct compilation directory for the given source path,
        /// returns null if invalid sourcePath
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <returns></returns>
        public static string GetCompilationDirectory(string sourcePath) {
            if (string.IsNullOrEmpty(sourcePath))
                return null;

            var baseComp = ProEnvironment.Current.BaseCompilationPath;

            // filter and sort the list
            var filteredList = _compilationPathList.Where(Predicate).ToList();
            filteredList.Sort(Comparison);

            // try to find the first item that match the input pattern
            if (filteredList.Count > 0) {
                var canFind = filteredList.FirstOrDefault(item => sourcePath.Contains(item.InputPathPattern));
                if (canFind != null) {
                    baseComp = Path.Combine(baseComp, canFind.OutputPathAppend);
                }
            }
            return baseComp;
        }

        private static int Comparison(CompilationPathItem compilationPathItem, CompilationPathItem pathItem) {
            int compare = string.IsNullOrWhiteSpace(compilationPathItem.ApplicationFilter).CompareTo(string.IsNullOrWhiteSpace(pathItem.ApplicationFilter));
            if (compare != 0) return compare;

            compare = string.IsNullOrWhiteSpace(compilationPathItem.EnvLetterFilter).CompareTo(string.IsNullOrWhiteSpace(pathItem.EnvLetterFilter));
            return compare;
        }

        private static bool Predicate(CompilationPathItem compilationPathItem) {
            // returns true if (appli is "" or (appli is currentAppli and (envletter is currentEnvletter or envletter = "")))
            return string.IsNullOrWhiteSpace(compilationPathItem.ApplicationFilter) || (compilationPathItem.ApplicationFilter.EqualsCi(Config.Instance.EnvName) && (compilationPathItem.EnvLetterFilter.EqualsCi(Config.Instance.EnvSuffix) || string.IsNullOrWhiteSpace(compilationPathItem.EnvLetterFilter)));
        }

        #endregion
    }

    /// <summary>
    /// The compilation path item
    /// </summary>
    public class CompilationPathItem {
        /// <summary>
        /// This compilation path applies to a given application (can be empty)
        /// </summary>
        public string ApplicationFilter { get; set; }
        /// <summary>
        /// This compilation path applies to a given Env letter (can be empty)
        /// </summary>
        public string EnvLetterFilter { get; set; }
        /// <summary>
        /// Pattern to match in the source path
        /// </summary>
        public string InputPathPattern { get; set; }
        /// <summary>
        /// String to append to the compilation directory if the match is true
        /// </summary>
        public string OutputPathAppend { get; set; }
    }
}