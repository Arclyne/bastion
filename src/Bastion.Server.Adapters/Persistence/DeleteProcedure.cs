using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Bastion.Server.Adapters.Persistence;

public static class DeleteProcedure
{
    public static DeletedRowCount Run(DatabaseFacade database, FormattableString command)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(command);

        IQueryable<DeletedRowCount> results = database.SqlQuery<DeletedRowCount>(command);
        DeletedRowCount deletedRowCount = results
            .AsEnumerable()
            .Single();
        return deletedRowCount;
    }
}
