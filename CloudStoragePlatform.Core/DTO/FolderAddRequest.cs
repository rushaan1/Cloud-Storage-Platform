﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.DTO
{
    public class FolderAddRequest
    {
        [Required]
        public string FolderName { get; set; }
        [Required]
        public string FolderPath { get; set; }
    }
}