using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using timer_app.Infrastructure;

namespace timer_app_tests
{
    public static class MockDbContext
    {
        private static TimerAppDbContext _context;

        public static TimerAppDbContext Instance
        {
            get
            {
                if (_context == null)
                {
                    var builder = new DbContextOptionsBuilder<TimerAppDbContext>();

                    builder.UseInMemoryDatabase(Guid.NewGuid().ToString(), db => db.EnableNullChecks(false));

                    _context = new TimerAppDbContext(builder.Options);
                    _context.Database.EnsureCreated();
                }
                return _context;
            }
        }

        public static void Teardown()
        {
            _context = null;
        }
    }
}
