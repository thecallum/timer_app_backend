using System.ComponentModel.DataAnnotations.Schema;
using timer_app.Infrastructure;

namespace timer_app.Boundary.Response
{
    public class ProjectResponse
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public ProjectColorResponse ProjectColor { get; set; }
    }
}
