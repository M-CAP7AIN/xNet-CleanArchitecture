using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Note> Notes { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
