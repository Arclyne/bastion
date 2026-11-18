using System;
using Microsoft.EntityFrameworkCore;

namespace Bastion.Server.Adapters.Persistence;

public sealed class NewContextRunner<TFailure>
    where TFailure : Exception
{
    private readonly IDbContextFactory<BastionDbContext> _contextFactory;
    private readonly DatabaseFailureTranslator<TFailure> _failureTranslator;

    public NewContextRunner(
        IDbContextFactory<BastionDbContext> contextFactory,
        Func<Exception, TFailure> failureFactory)
    {
        ArgumentNullException.ThrowIfNull(contextFactory);
        ArgumentNullException.ThrowIfNull(failureFactory);

        _contextFactory = contextFactory;
        _failureTranslator = new DatabaseFailureTranslator<TFailure>(failureFactory);
    }

    public TResult Run<TResult>(Func<BastionDbContext, TResult> operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        Func<TResult> runInNewContext = () => RunInNewContext(operation);
        return _failureTranslator.Run(runInNewContext);
    }

    private TResult RunInNewContext<TResult>(Func<BastionDbContext, TResult> operation)
    {
        using BastionDbContext context = _contextFactory.CreateDbContext();
        return operation(context);
    }
}
