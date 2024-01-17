using timer_app.Boundary.Response;
using timer_app.Factories;
using timer_app.Gateway.Interfaces;
using timer_app.UseCases.Interfaces;

namespace timer_app.UseCases
{
    public class GetAllProjectsUseCase : IGetAllProjectsUseCase
    {
        private readonly IProjectGateway _gateway;

        public GetAllProjectsUseCase(IProjectGateway gateway)
        {
            _gateway = gateway;
        }

        public async Task<IEnumerable<ProjectResponse>> ExecuteAsync(int userId)
        {
            var projects = await _gateway.GetAllProjects(userId);

            return projects.ToResponse();
        }
    }
}
