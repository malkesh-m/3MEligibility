using System.Collections.Concurrent;
using MEligibilityPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Newtonsoft.Json;
using MEligibilityPlatform.Application.Services.Interface;

namespace MEligibilityPlatform.Middleware
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuditInterceptor"/> class.
    /// </summary>
    /// <param name="userContext">Provides access to the current user/request context for capturing user and request details.</param>
    public class AuditInterceptor(IUserContextService userContext) : SaveChangesInterceptor
    {
        private readonly ConcurrentBag<AuditInfo> _pendingAuditInfos = [];
        private readonly IUserContextService _userContext = userContext;

        /// <summary>
        /// Called before EF Core saves changes. Captures pending audits for all tracked entities.
        /// </summary>
        /// <param name="eventData">Contextual information about the current <see cref="DbContext"/> event.</param>
        /// <param name="result">The result of the save operation.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A <see cref="ValueTask{T}"/> wrapping <see cref="InterceptionResult{Int32}"/> that may influence saving behavior.</returns>
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            if (context != null)
            {
                _pendingAuditInfos.Clear();
                CaptureAudits(context);
            }
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        /// <summary>
        /// Called after EF Core saves changes. Processes and persists captured audit logs.
        /// </summary>
        /// <param name="eventData">Contextual information about the completed save operation.</param>
        /// <param name="result">The number of state entries written to the database.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A <see cref="ValueTask{Int32}"/> containing the number of affected records.</returns>
        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            if (context != null && !_pendingAuditInfos.IsEmpty)
            {
                await ProcessAuditsAfterSave(context, cancellationToken);
            }
            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }
        private static readonly HashSet<string> ExcludedAuditFields =
            [
            "Password",
            "PasswordHash",   // if you have hashed passwords
            "UserPassword"    // if you store salt
            ];
        /// <summary>
        /// Processes pending audits and saves them to the database after entities are persisted.
        /// </summary>
        /// <param name="context">The EF Core database context.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        private async Task ProcessAuditsAfterSave(DbContext context, CancellationToken cancellationToken)
        {


            var auditsToSave = new List<Audit>();

            foreach (var auditInfo in _pendingAuditInfos)
            {
                var audit = auditInfo.Audit;

                if (audit.ActionName == "Add")
                {
                    var entityEntry = context.ChangeTracker.Entries()
                        .FirstOrDefault(e => e.Entity == auditInfo.Entity);

                    if (entityEntry != null && entityEntry.State == EntityState.Unchanged)
                    {
                        var entityType = context.Model.FindEntityType(entityEntry.Entity.GetType());
                        if (entityType != null)
                        {
                            var primaryKey = entityType.FindPrimaryKey();
                            if (primaryKey != null)
                            {
                                var primaryKeyProperty = primaryKey.Properties[0];
                                var recordIdValue = entityEntry.Property(primaryKeyProperty.Name).CurrentValue;

                                if (recordIdValue != null && int.TryParse(recordIdValue.ToString(), out int parsedId) && parsedId > 0)
                                {
                                    audit.RecordId = parsedId;

                                    if (!string.IsNullOrEmpty(audit.NewValue) && audit.NewValue != "{}")
                                    {
                                        try
                                        {
                                            var newValueObj = JsonConvert.DeserializeObject(audit.NewValue);
                                            if (newValueObj != null)
                                            {
                                                var propertyInfo = newValueObj.GetType().GetProperty(primaryKeyProperty.Name);
                                                if (propertyInfo != null && propertyInfo.CanWrite)
                                                {
                                                    propertyInfo.SetValue(newValueObj, parsedId);
                                                    audit.NewValue = JsonConvert.SerializeObject(newValueObj, _auditJsonSettings);
                                                }
                                                else
                                                {
                                                    var newValueDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(audit.NewValue);
                                                    if (newValueDict != null)
                                                    {
                                                        newValueDict[primaryKeyProperty.Name] = parsedId;
                                                        audit.NewValue = JsonConvert.SerializeObject(newValueDict, _auditJsonSettings);
                                                    }
                                                }
                                            }
                                        }
                                        catch
                                        {
                                            try
                                            {
                                                var currentValues = context.Entry(auditInfo.Entity).CurrentValues;
                                                var valuesDict = new Dictionary<string, object>();
                                                foreach (var property in currentValues.Properties)
                                                {
                                                    valuesDict[property.Name] = currentValues[property]!;
                                                }
                                                audit.NewValue = JsonConvert.SerializeObject(valuesDict, _auditJsonSettings);
                                            }
                                            catch
                                            {
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                auditsToSave.Add(audit);
            }

            if (auditsToSave.Count != 0)
            {
                context.Set<Audit>().AddRange(auditsToSave);
                await context.SaveChangesAsync(cancellationToken);
            }

            _pendingAuditInfos.Clear();
        }

        /// <summary>
        /// Captures audit information for all tracked entity changes (Added, Modified, Deleted).
        /// </summary>
        /// <param name="context">The EF Core database context containing tracked entities.</param>
        private void CaptureAudits(DbContext context)
        {
            var now = DateTime.UtcNow;

            foreach (var entry in context.ChangeTracker.Entries()
                .Where(e => e.State != EntityState.Unchanged && e.State != EntityState.Detached))
            {
                if (entry.Entity is Audit) continue;

                var entityType = context.Model.FindEntityType(entry.Entity.GetType());
                if (entityType == null) continue;

                var tableName = entityType.GetTableName() ?? "";
                var primaryKey = entityType.FindPrimaryKey();
                if (primaryKey == null) continue;

                var primaryKeyProperty = primaryKey.Properties[0];
                var primaryKeyPropertyName = primaryKeyProperty.Name;

                int recordId = 0;
                var recordIdValue = entry.Property(primaryKeyPropertyName).CurrentValue;
                if (recordIdValue != null && int.TryParse(recordIdValue.ToString(), out int parsedId))
                {
                    recordId = parsedId;
                }

                string oldValueJson = "{}";
                string newValueJson = "{}";
                bool hasRealChanges = false;

                if (entry.State == EntityState.Modified)
                {
                    var originalValues = entry.OriginalValues;
                    var currentValues = entry.CurrentValues;

                    foreach (var property in entry.Properties)
                    {
                        var originalValue = originalValues[property.Metadata.Name];
                        var currentValue = currentValues[property.Metadata.Name];

                        bool isDifferent = false;

                        if (property.Metadata.ClrType == typeof(decimal) || property.Metadata.ClrType == typeof(decimal?))
                        {
                            var originalDecimal = originalValue != null ? Convert.ToDecimal(originalValue) : (decimal?)null;
                            var currentDecimal = currentValue != null ? Convert.ToDecimal(currentValue) : (decimal?)null;
                            isDifferent = originalDecimal != currentDecimal;
                        }
                        else if (property.Metadata.ClrType == typeof(float) || property.Metadata.ClrType == typeof(float?))
                        {
                            var originalFloat = originalValue != null ? Convert.ToSingle(originalValue) : (float?)null;
                            var currentFloat = currentValue != null ? Convert.ToSingle(currentValue) : (float?)null;
                            isDifferent = Math.Abs((originalFloat ?? 0) - (currentFloat ?? 0)) > 0.0001;
                        }
                        else
                        {
                            var originalStr = originalValue?.ToString();
                            var currentStr = currentValue?.ToString();
                            isDifferent = originalStr != currentStr;
                        }

                        if (isDifferent)
                        {
                            hasRealChanges = true;
                            break;
                        }
                    }

                    if (!hasRealChanges)
                        continue;

                    oldValueJson = SerializeWithoutExcludedFields(originalValues);
                    newValueJson = SerializeWithoutExcludedFields(currentValues);
                }
                if (entry.State == EntityState.Added)
                {
                    newValueJson = SerializeWithoutExcludedFields(entry.CurrentValues);
                }
                else if (entry.State == EntityState.Deleted)
                {
                    oldValueJson = SerializeWithoutExcludedFields(entry.OriginalValues);
                }

                var ip = _userContext.GetIpAddress();
                var userName = _userContext.GetUserName()??"";
                int tenantId = _userContext.GetTenantId();
                string actionName = entry.State switch
                {
                    EntityState.Modified => "Update",
                    EntityState.Added => "Add",
                    EntityState.Deleted => "Delete",
                    _ => entry.State.ToString()
                };

                var audit = new Audit
                {
                    ActionDate = now,
                    TableName = tableName,
                    ActionName = actionName,
                    OldValue = oldValueJson,
                    NewValue = newValueJson,
                    RecordId = recordId,
                    FieldName = "",
                    UpdatedByDateTime = now,
                    IPAddress = ip,
                    UserName = userName,
                    TenantId=tenantId
                };

                _pendingAuditInfos.Add(new AuditInfo
                {
                    Audit = audit,
                    Entity = entry.Entity,
                    PrimaryKeyName = primaryKeyPropertyName
                });
            }
        }
        private static readonly JsonSerializerSettings _auditJsonSettings =
        new()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffffffZ"
        };

        private static string SerializeWithoutExcludedFields(PropertyValues values)
        {
            var dict = new Dictionary<string, object?>();

            foreach (var property in values.Properties)
            {
                if (ExcludedAuditFields.Contains(property.Name))
                    continue;

                dict[property.Name] = values[property];
            }

            return JsonConvert.SerializeObject(dict, _auditJsonSettings);
        }

        /// <summary>
        /// Holds temporary audit information for an entity until after it is saved.
        /// </summary>
        private class AuditInfo
        {
            /// <summary>
            /// Gets or sets the audit entity being tracked.
            /// </summary>
            public required Audit Audit { get; set; }

            /// <summary>
            /// Gets or sets the entity instance being audited.
            /// </summary>
            public required object Entity { get; set; }

            /// <summary>
            /// Gets or sets the name of the primary key property for the audited entity.
            /// </summary>
            public required string PrimaryKeyName { get; set; }
        }
    }
}
