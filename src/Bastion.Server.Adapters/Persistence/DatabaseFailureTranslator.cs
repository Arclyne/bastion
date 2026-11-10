using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace Bastion.Server.Adapters.Persistence;

public sealed class DatabaseFailureTranslator<TFailure>
    where TFailure : Exception
{
    private readonly Func<Exception, TFailure> _createFailure;

    public DatabaseFailureTranslator(Func<Exception, TFailure> createFailure)
    {
        ArgumentNullException.ThrowIfNull(createFailure);

        _createFailure = createFailure;
    }

    public TResult Run<TResult>(Func<TResult> operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        try
        {
            return operation();
        }
        catch (DbUpdateException ex)
        {
            throw _createFailure(ex);
        }
        catch (DbException ex)
        {
            throw _createFailure(ex);
        }
    }
}
