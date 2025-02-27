﻿using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using OpenSleigh.Core.Persistence;

[assembly: InternalsVisibleTo("OpenSleigh.Persistence.SQL.Tests")]
namespace OpenSleigh.Persistence.SQL
{    
    internal sealed class SqlTransaction : ITransaction, IDisposable
    {
        private IDbContextTransaction _transaction;

        public SqlTransaction(IDbContextTransaction transaction)
        {
            _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
        }

        public Task CommitAsync(CancellationToken cancellationToken = default) =>
            _transaction.CommitAsync(cancellationToken);

        public Task RollbackAsync(CancellationToken cancellationToken = default) =>
            _transaction.RollbackAsync(cancellationToken);

        public void Dispose()
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }
}