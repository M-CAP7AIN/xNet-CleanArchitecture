using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebApi.FunctionalTests
{
    public static class TestExtensions
    {
        public static Task<Unit> GetUnitTask(this Unit unit)
        {
            return Task.FromResult(unit);
        }
    }
}
