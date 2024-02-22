using timer_app.Gateway.Interfaces;
using timer_app.UseCases.Interfaces;

namespace timer_app.UseCases
{
    public class DeleteProjectUseCase : IDeleteProjectUseCase
    {
        private readonly IProjectGateway _gateway;

        public DeleteProjectUseCase(IProjectGateway gateway)
        {
            _gateway = gateway;
        }

        public async Task<bool> ExecuteAsync(int projectId, string userId)
        {
            return await _gateway.DeleteProject(projectId, userId);
        }
    }
}
