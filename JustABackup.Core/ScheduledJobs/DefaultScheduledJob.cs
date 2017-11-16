﻿using JustABackup.Base;
using JustABackup.Database;
using JustABackup.Database.Entities;
using JustABackup.Database.Enum;
using Microsoft.EntityFrameworkCore;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JustABackup.Core.ScheduledJobs
{
    public class DefaultScheduledJob : IJob
    {
        private DefaultContext databaseContext;

        public DefaultScheduledJob(DefaultContext databaseContext)
        {
            this.databaseContext = databaseContext;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            int jobId = int.Parse(context.JobDetail.Key.Name);

            BackupJob job = databaseContext.Jobs
                .Include(j => j.BackupProvider)
                .Include(j => j.BackupProvider.Provider)
                .Include(j => j.BackupProvider.Values)
                .ThenInclude(x => x.Property)
                .Include(j => j.StorageProvider)
                .Include(j => j.StorageProvider.Provider)
                .Include(j => j.StorageProvider.Values)
                .ThenInclude(x => x.Property)
                .Include(j => j.TransformProviders)
                .ThenInclude(x => x.Provider)
                .Include(j => j.TransformProviders)
                .ThenInclude(x => x.Values)
                .ThenInclude(x => x.Property)
                .FirstOrDefault(j => j.ID == jobId);

            BackupJobHistory history = new BackupJobHistory();
            history.Started = DateTime.Now;

            job.History.Add(history);
            await databaseContext.SaveChangesAsync();

            IBackupProvider backupProvider = ConvertToProvider<IBackupProvider>(job.BackupProvider);
            IStorageProvider storageProvider = ConvertToProvider<IStorageProvider>(job.StorageProvider);

            IEnumerable<ITransformProvider> transformProviders = job.TransformProviders.Select(tp => ConvertToProvider<ITransformProvider>(tp));

            var items = await backupProvider.GetItems();
            foreach (var item in items)
            {
                Dictionary<ITransformProvider, Dictionary<BackupItem, IEnumerable<BackupItem>>> transformers = new Dictionary<ITransformProvider, Dictionary<BackupItem, IEnumerable<BackupItem>>>(transformProviders.Count());
                foreach(var transformProvider in transformProviders)
                {
                    if (transformers.Count > 0)
                    {
                        var mappedItems = await transformProvider.TransformList(transformers.Last().Value.Select(x => x.Key));
                        transformers.Add(transformProvider, mappedItems);
                    }
                    else
                    {
                        var mappedItems = await transformProvider.TransformList(items);
                        transformers.Add(transformProvider, mappedItems);
                    }
                }

                using (var stream = await backupProvider.OpenRead(item))
                {
                    await storageProvider.StoreItem(item, stream);
                }
            }

            history.Completed = DateTime.Now;
            history.Message = $"{items.Count()} files were copied.";
            history.Status = ExitCode.Success;

            await databaseContext.SaveChangesAsync();
        }

        private T ConvertToProvider<T>(ProviderInstance providerInstance) where T : class
        {
            Type providerType = Type.GetType(providerInstance.Provider.Namespace);
            T convertedProvider = Activator.CreateInstance(providerType) as T;

            foreach (var property in providerInstance.Values)
            {
                PropertyInfo propertyInfo = providerType.GetProperty(property.Property.TypeName);
                object originalValueType = Convert.ChangeType(property.Value, propertyInfo.PropertyType);

                propertyInfo.SetValue(convertedProvider, originalValueType);
            }

            return convertedProvider;
        }
    }
}
