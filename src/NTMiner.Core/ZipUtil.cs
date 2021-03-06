﻿using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;

namespace NTMiner {
    public static class ZipUtil {
        public static void DecompressZipFile(string zipFileFullName, string destDir) {
            if (string.IsNullOrEmpty(zipFileFullName)) {
                throw new ArgumentNullException(nameof(zipFileFullName));
            }
            FileInfo fileInfo = new FileInfo(zipFileFullName);
            if (!fileInfo.Exists) {
                throw new FileNotFoundException("file not found", zipFileFullName);
            }
            using (FileStream fs = fileInfo.OpenRead()) {
                Decompress(fs, destDir);
            }
        }

        public static void Decompress(Stream stream, string destDir) {
            if (stream == null) {
                throw new ArgumentNullException(nameof(stream));
            }
            if (string.IsNullOrEmpty(destDir)) {
                throw new ArgumentNullException(nameof(destDir));
            }
            using (ZipInputStream zipInputStream = new ZipInputStream(stream)) {
                ZipEntry theEntry;
                string directoryName = destDir;

                while ((theEntry = zipInputStream.GetNextEntry()) != null) {
                    string path = Path.Combine(destDir, theEntry.Name);
                    if (theEntry.IsDirectory) {
                        DirectoryInfo dir = new DirectoryInfo(path);
                        if (!dir.Exists) {
                            dir.Create();
                        }
                    }
                    else if (theEntry.IsFile) {
                        string destfile = path;
                        FileStream streamWriter = File.Create(destfile);
                        const int bufferSize = 1024 * 30;// 30kb
                        byte[] data = new byte[bufferSize];
                        StreamUtils.Copy(zipInputStream, streamWriter, data);
                        streamWriter.Close();
                    }
                }
                zipInputStream.Close();
            }
        }
    }
}
