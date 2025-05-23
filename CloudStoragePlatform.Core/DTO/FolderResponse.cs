﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.DTO
{
    public class FolderResponse
    {
        public Guid FolderId { get; set; }
        public string FolderName { get; set; }
        public string? FolderPath { get; set; }
        public bool? IsFavorite { get; set; }
        public bool? IsTrash { get; set; }
        public float Size { get; set; }
    }
}
