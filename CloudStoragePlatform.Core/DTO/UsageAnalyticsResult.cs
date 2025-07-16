using System;
using System.Collections.Generic;

namespace CloudStoragePlatform.Core.DTO
{
    public class UsageAnalyticsResult
    {
        /// <summary>
        /// Top 10 file extensions by total size (MB)
        /// </summary>
        public List<ExtensionSizeSummary> TopExtensionsBySize { get; set; } = new();
        /// <summary>
        /// Top 10 files by size (MB)
        /// </summary>
        public List<FileSizeSummary> TopFilesBySize { get; set; } = new();
        /// <summary>
        /// Total number of folders
        /// </summary>
        public int TotalFolders { get; set; }
        /// <summary>
        /// Total number of files
        /// </summary>
        public int TotalFiles { get; set; }
        /// <summary>
        /// Total number of folders and files marked as favorite
        /// </summary>
        public int FavoriteItems { get; set; }
        /// <summary>
        /// Total number of folders and files that are shared (valid share link)
        /// </summary>
        public int ItemsShared { get; set; }
    }

    public class ExtensionSizeSummary
    {
        public string Extension { get; set; } = string.Empty;
        public float TotalSize { get; set; }
    }

    public class FileSizeSummary
    {
        public string FileName { get; set; } = string.Empty;
        public float Size { get; set; }
    }
} 