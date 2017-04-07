﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WixToolset.Dtf.Compression;

namespace _3PA.Lib.Compression.Pl {

    internal class PlInfo : IArchiveInfo {

        ProcessIo _prolibExe;
        private string _archivePath;

        public PlInfo(string archivePath, string prolibPath) {
            _archivePath = archivePath;
            _prolibExe = new ProcessIo(prolibPath);
        }

        public void PackFileSet(IDictionary<string, string> files, CompressionLevel compLevel, EventHandler<ArchiveProgressEventArgs> progressHandler) {

            // check that the folder to the archive exists
            var archiveFolder = Path.GetDirectoryName(_archivePath);
            if (!string.IsNullOrEmpty(archiveFolder) && !Directory.Exists(archiveFolder)) {
                Directory.CreateDirectory(archiveFolder);
            }
            if (string.IsNullOrEmpty(archiveFolder)) {
                throw new Exception("Couldn't find the folder for the targeted archive");
            }

            // create a unique temp folder for this .pl
            var uniqueTempFolder = Path.Combine(archiveFolder, Path.GetFileName(_archivePath) + "~" + Path.GetRandomFileName());
            var dirInfo = Directory.CreateDirectory(uniqueTempFolder);
            dirInfo.Attributes |= FileAttributes.Hidden;

            var subFolders = new Dictionary<string, List<FilesToMove>>();

            foreach (var file in files) {
                var subFolderPath = Path.GetDirectoryName(Path.Combine(uniqueTempFolder, file.Key));
                if (!string.IsNullOrEmpty(subFolderPath)) {
                    if (!subFolders.ContainsKey(subFolderPath)) {
                        subFolders.Add(subFolderPath, new List<FilesToMove>());
                        if (!Directory.Exists(subFolderPath)) {
                            Directory.CreateDirectory(subFolderPath);
                        }
                    }
                    subFolders[subFolderPath].Add(new FilesToMove(file.Value, Path.Combine(uniqueTempFolder, file.Key), file.Key));
                }
            }

            _prolibExe.StartInfo.WorkingDirectory = uniqueTempFolder;

            foreach (var subFolder in subFolders) {

                _prolibExe.Arguments = _archivePath.ProQuoter() + " -create -nowarn -add " + Path.Combine(subFolder.Key.Replace(uniqueTempFolder, "").TrimStart('\\'), "*").ProQuoter();

                // move files to the temp subfolder
                Parallel.ForEach(subFolder.Value, file => {
                    try {
                        if (file.Move) {
                            File.Move(file.Origin, file.Temp);
                        } else {
                            File.Copy(file.Origin, file.Temp);
                        }
                    } catch (Exception) {
                        // ignore
                    }
                });

                // now we just need to add the content of temp folders into the .pl
                var prolibOk = _prolibExe.TryDoWait(true);

                // move files from the temp subfolder
                Parallel.ForEach(subFolder.Value, file => {
                    Exception ex = null;
                    try {
                        if (file.Move) {
                            File.Move(file.Temp, file.Origin);
                        } else if (!File.Exists(file.Temp)) {
                            throw new Exception("Couldn't find temp file for " + file.Origin);
                        }
                    } catch (Exception e) {
                        ex = e;
                    }
                    if (progressHandler != null) {
                        progressHandler(this, new ArchiveProgressEventArgs(ArchiveProgressType.FinishFile, file.RelativePath, ex ?? (prolibOk ? null : new Exception(_prolibExe.ErrorOutput.ToString()))));
                    }
                });
            }

            // compress .pl
            _prolibExe.Arguments = _archivePath.ProQuoter() + " -compress -nowarn";
            _prolibExe.TryDoWait(true);

            // delete temp folder
            Directory.Delete(uniqueTempFolder, true);
        }

        private class FilesToMove {
            public string Origin { get; private set; }
            public string Temp { get; private set; }
            public string RelativePath { get; private set; }
            public bool Move { get; private set; }
            public FilesToMove(string origin, string temp, string relativePath) {
                Origin = origin;
                Temp = temp;
                RelativePath = relativePath;
                Move = origin.Length > 2 && temp.Length > 2 && origin.Substring(0, 2).EqualsCi(temp.Substring(0, 2));
            }
        }
        
    }
}
