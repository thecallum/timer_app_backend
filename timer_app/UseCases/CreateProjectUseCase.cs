﻿using timer_app.Boundary.Request;
using timer_app.Boundary.Response;
using timer_app.Factories;
using timer_app.Gateway.Interfaces;
using timer_app.UseCases.Interfaces;

namespace timer_app.UseCases
{
    public class CreateProjectUseCase : ICreateProjectUseCase
    {
        private readonly IProjectGateway _gateway;

        public CreateProjectUseCase(IProjectGateway gateway)
        {
            _gateway = gateway;
        }

        public async Task<ProjectResponse> ExecuteAsync(CreateProjectRequest request, string userId)
        {
            var createdProject = await _gateway.CreateProject(request, userId);

            return createdProject.ToResponse();
        }
    }
}
