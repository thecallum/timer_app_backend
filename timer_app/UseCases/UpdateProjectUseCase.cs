using timer_app.Boundary.Request;
using timer_app.Boundary.Response;
using timer_app.Factories;
using timer_app.Gateway.Interfaces;
using timer_app.UseCases.Interfaces;

namespace timer_app.UseCases
{
    public class UpdateProjectUseCase : IUpdateProjectUseCase
    {
        private readonly IProjectGateway _gateway;

        public UpdateProjectUseCase(IProjectGateway gateway)
        {
            _gateway = gateway;
        }

        public async Task<ProjectResponse> ExecuteAsync(int projectId, UpdateProjectRequest request, string userId)
        {
            var project = await _gateway.UpdateProject(projectId, request, userId);
            if (project == null) return null;

            return project.ToResponse();
        }
    }
}
