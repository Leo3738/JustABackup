﻿using JustABackup.Database.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace JustABackup.Models
{
    public class CreateJobProviderModel : BaseViewModel
    {
        public string Action { get; set; }

        public string ProviderName { get; set; }
        
        public List<ProviderPropertyModel> Properties { get; set; }

        public ProviderInstance CreateProviderInstance(Provider provider)
        {
            ProviderInstance backupProvider = new ProviderInstance();
            backupProvider.Provider = provider;
            foreach (var property in Properties)
            {
                ProviderInstanceProperty instanceProperty = new ProviderInstanceProperty();
                instanceProperty.Value = property.Value;
                instanceProperty.Property = provider.Properties.FirstOrDefault(p => p.Name == property.Name);
                backupProvider.Values.Add(instanceProperty);
            }
            return backupProvider;
        }
    }
}
