﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace JustABackup.Database.Entities
{
    public class BackupJob
    {
        [Key]
        [Required]
        public int ID { get; set; }

        [Required]
        public string Name { get; set; }
        
        [Required]
        public ProviderInstance StorageProvider { get; set; }

        [Required]
        public ProviderInstance BackupProvider { get; set; }

        public List<ProviderInstance> TransformProviders { get; set; }

        [Required]
        public bool HasChangedModel { get; set; }

        public List<BackupJobHistory> History { get; set; }

        public BackupJob()
        {
            HasChangedModel = false;
            TransformProviders = new List<ProviderInstance>();
            History = new List<BackupJobHistory>();
        }
    }
}
