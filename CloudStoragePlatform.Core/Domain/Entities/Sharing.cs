﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.Domain.Entities
{
    public class Sharing
    {
        [Key]
        public Guid SharingId { get; set; } 
        public string? ShareLinkUrl { get; set; }
        public DateTime? ShareLinkExpiry { get; set; } 
        public int? CurrentShareLinkTimesSeen { get; set; } 
        public DateTime? ShareLinkCreateDate { get; set; } 
    }
}