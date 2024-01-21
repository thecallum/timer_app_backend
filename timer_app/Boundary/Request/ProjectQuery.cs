using Microsoft.AspNetCore.Mvc;

namespace timer_app.Boundary.Request
{
    public class ProjectQuery
    {
        [FromRoute(Name = "projectId")]
        public int ProjectId { get; set; }
    }
}
