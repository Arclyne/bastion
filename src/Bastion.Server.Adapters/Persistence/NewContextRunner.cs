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
        Func<Exception, TFailure> createFailure)
    {
        ArgumentNullException.ThrowIfNull(contextFactory);

        _contextFactory = contextFactory;
        _failureTranslator = new DatabaseFailureTranslator<TFailure>(createFailure);
    }

    public TResult Run<TResult>(Func<BastionDbContext, TResult> operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        return _failureTranslator.Run(() => RunInNewContext(operation));
    }

    private TResult RunInNewContext<TResult>(Func<BastionDbContext, TResult> operation)
    {
        using BastionDbContext context = _contextFactory.CreateDbContext();
        return operation(context);
    }
}
