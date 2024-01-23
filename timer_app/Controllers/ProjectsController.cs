using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using timer_app.Boundary.Request;
using timer_app.Infrastructure.Exceptions;
using timer_app.UseCases.Interfaces;

namespace timer_app.Controllers
{
    [Route("api/projects")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly IGetAllProjectsUseCase _getAllProjectsUseCase;
        private readonly IUpdateProjectUseCase _updateProjectUseCase;
        private readonly IDeleteProjectUseCase _deleteProjectUseCase;
        private readonly ICreateProjectUseCase _createProjectUseCase;

        private readonly IValidator<CreateProjectRequest> _createProjectRequestValidator;
        private readonly IValidator<UpdateProjectRequest> _updateProjectRequestValidator;

        private const int PlaceholderUserId = 1;

        public ProjectsController(
            IGetAllProjectsUseCase getAllProjectsUseCase,
            IUpdateProjectUseCase updateProjectUseCase,
            IDeleteProjectUseCase deleteProjectUseCase,
            ICreateProjectUseCase createProjectUseCase,
            IValidator<CreateProjectRequest> createProjectRequestValidator,
            IValidator<UpdateProjectRequest> updateProjectRequestValidator)
        {
            _getAllProjectsUseCase = getAllProjectsUseCase;
            _updateProjectUseCase = updateProjectUseCase;
            _deleteProjectUseCase = deleteProjectUseCase;
            _createProjectUseCase = createProjectUseCase;
            _createProjectRequestValidator = createProjectRequestValidator;
            _updateProjectRequestValidator = updateProjectRequestValidator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
        {
            var validationResult = await _createProjectRequestValidator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var response = await _createProjectUseCase.ExecuteAsync(request, PlaceholderUserId);

            return Ok(response);
        }

        [HttpDelete]
        [Route("{projectId}")]
        public async Task<IActionResult> DeleteProject([FromRoute] ProjectQuery query)
        {
            try
            {
                var response = await _deleteProjectUseCase.ExecuteAsync(query.ProjectId, PlaceholderUserId);
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

            try
            {
                var response = await _updateProjectUseCase.ExecuteAsync(query.ProjectId, request, PlaceholderUserId);
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
            var projects = await _getAllProjectsUseCase.ExecuteAsync(PlaceholderUserId);

            return Ok(projects);
        }
    }
}
