﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace JustABackup.Core.Entities.Database
{
    internal class ProviderInstance
    {
        [Key]
        [Required]
        public int ID { get; set; }

        [Required]
        public Provider Provider { get; set; }

        public List<ProviderInstanceProperty> Values { get; set; }

        public ProviderInstance()
        {
            Values = new List<ProviderInstanceProperty>();
        }
    }
}
