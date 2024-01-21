using System.ComponentModel.DataAnnotations.Schema;

namespace timer_app.Infrastructure
{
    public class ProjectColor
    {
        public string Lightest { get; set; }
        public string Light { get; set; }
        public string Dark { get; set; }
        public string Darkest { get; set; }
    }
}
