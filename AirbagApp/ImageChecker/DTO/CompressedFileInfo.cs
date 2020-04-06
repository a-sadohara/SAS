using System;
using System.Diagnostics;

namespace ImageChecker.DTO
{
    class CompressedFileInfo
    {
        /// <summary>
        /// zipファイル解凍
        /// </summary>
        /// <param name="sourceArchive">zipファイルパス</param>
        /// <param name="destination">解凍先</param>
        public void ExtractFile(string sourceArchive, string destination)
        {
            try
            {
                using (Process pr7zip = new Process())
                {
                    pr7zip.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    pr7zip.StartInfo.FileName = @"7z-extra\x64\7za.exe";
                    pr7zip.StartInfo.Arguments = string.Format("x \"{0}\" -y -o\"{1}\"", sourceArchive, destination);
                    pr7zip.Start();
                    pr7zip.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}