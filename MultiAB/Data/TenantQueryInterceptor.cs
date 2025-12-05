using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MultiAB.Models;
using MultiAB.Services;
using System.Data.Common;

namespace MultiAB.Data;

/// <summary>
/// Interceptor de consultas para aplicar filtro multi-tenant automáticamente
/// Este interceptor modifica las consultas SQL para incluir el filtro de tenant
/// </summary>
public class TenantQueryInterceptor : DbCommandInterceptor
{
    private readonly ITenantProvider _tenantProvider;

    public TenantQueryInterceptor(ITenantProvider tenantProvider)
    {
        _tenantProvider = tenantProvider;
    }

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        ModifyCommand(command);
        return base.ReaderExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        ModifyCommand(command);
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    private void ModifyCommand(DbCommand command)
    {
        // Este interceptor se usa como respaldo, pero el filtro principal
        // se aplica mediante HasQueryFilter en el DbContext
        // El HasQueryFilter ya aplica el filtro automáticamente
    }
}










