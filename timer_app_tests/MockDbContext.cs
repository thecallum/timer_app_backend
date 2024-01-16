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
        private static TimerAppContext _context;

        public static TimerAppContext Instance
        {
            get
            {
                if (_context == null)
                {
                    var builder = new DbContextOptionsBuilder<TimerAppContext>();

                    builder.UseInMemoryDatabase(Guid.NewGuid().ToString(), db => db.EnableNullChecks(false));

                    _context = new TimerAppContext(builder.Options);
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
