﻿using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using timer_app.Boundary.Request;
using timer_app.Infrastructure.Exceptions;
using timer_app.Middleware.Interfaces;
using timer_app.UseCases.Interfaces;

namespace timer_app.Controllers
{
    [Route("api/projects")]
    [ApiController]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly IGetAllProjectsUseCase _getAllProjectsUseCase;
        private readonly IUpdateProjectUseCase _updateProjectUseCase;
        private readonly IDeleteProjectUseCase _deleteProjectUseCase;
        private readonly ICreateProjectUseCase _createProjectUseCase;

        private readonly IValidator<CreateProjectRequest> _createProjectRequestValidator;
        private readonly IValidator<UpdateProjectRequest> _updateProjectRequestValidator;

        private readonly IUserService _currentUserService;

        public ProjectsController(
            IGetAllProjectsUseCase getAllProjectsUseCase,
            IUpdateProjectUseCase updateProjectUseCase,
            IDeleteProjectUseCase deleteProjectUseCase,
            ICreateProjectUseCase createProjectUseCase,
            IValidator<CreateProjectRequest> createProjectRequestValidator,
            IValidator<UpdateProjectRequest> updateProjectRequestValidator,
            IUserService currentUserService)
        {
            _getAllProjectsUseCase = getAllProjectsUseCase;
            _updateProjectUseCase = updateProjectUseCase;
            _deleteProjectUseCase = deleteProjectUseCase;
            _createProjectUseCase = createProjectUseCase;
            _createProjectRequestValidator = createProjectRequestValidator;
            _updateProjectRequestValidator = updateProjectRequestValidator;
            _currentUserService = currentUserService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
        {
            var validationResult = await _createProjectRequestValidator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var userId = _currentUserService.GetId();

            var response = await _createProjectUseCase.ExecuteAsync(request, userId);

            return Ok(response);
        }

        [HttpDelete]
        [Route("{projectId}")]
        public async Task<IActionResult> DeleteProject([FromRoute] ProjectQuery query)
        {
            var userId = _currentUserService.GetId();

            try
            {
                var response = await _deleteProjectUseCase.ExecuteAsync(query.ProjectId, userId);
                if (!response) return NotFound(query.ProjectId);

                return NoContent();
            }
            catch (UserUnauthorizedToAccessProjectException e)
            {
                return Unauthorized(e.Message);
            }
            catch (ProjectIsArchivedException e)
            {
                return UnprocessableEntity(e.Message);
            }
        }

        [HttpPut]
        [Route("{projectId}")]
        public async Task<IActionResult> UpdateProject([FromRoute] ProjectQuery query, [FromBody] UpdateProjectRequest request)
        {
            var validationResult = await _updateProjectRequestValidator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var userId = _currentUserService.GetId();

            try
            {
                var response = await _updateProjectUseCase.ExecuteAsync(query.ProjectId, request, userId);
                if (response == null) return NotFound(query.ProjectId);

                return Ok(response);
            }
            catch (UserUnauthorizedToAccessProjectException e)
            {
                return Unauthorized(e.Message);
            }
            catch (ProjectIsArchivedException e)
            {
                return UnprocessableEntity(e.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProjects()
        {
            var userId = _currentUserService.GetId();

            var projects = await _getAllProjectsUseCase.ExecuteAsync(userId);

            return Ok(projects);
        }
    }
}
