using Microsoft.EntityFrameworkCore;
using timer_app.Boundary.Request;
using timer_app.Factories;
using timer_app.Gateway.Interfaces;
using timer_app.Infrastructure;
using timer_app.Infrastructure.Exceptions;

namespace timer_app.Gateway
{
    public class ProjectGateway : IProjectGateway
    {
        private readonly TimerAppContext _context;

        public ProjectGateway(TimerAppContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Project>> GetAllProjects(int userId)
        {
            // may need to include a sum subquery for events under this project
            var projects = await _context.Projects
                .Where(x => x.UserId == userId)
                .ToListAsync();

            return projects;
        }

        public async Task<Project> CreateProject(CreateProjectRequest request, int userId)
        {
            var project = request.ToDb(userId);

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return project;
        }

        public async Task<Project> UpdateProject(int projectId, UpdateProjectRequest request, int userId)
        {
            var existingProject = await _context.Projects.FindAsync(projectId);
            if (existingProject == null) return null;

            if (existingProject.UserId != userId)
            {
                throw new UserUnauthorizedToAccessProjectException(userId);
            }

            // Map through project fields
            existingProject.Description = request.Description;
            existingProject.DisplayColour = request.DisplayColour;

            await _context.SaveChangesAsync();

            return existingProject;
        }

        public async Task<bool> DeleteProject(int projectId, int userId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return false;

            if (project.UserId != userId)
            {
                throw new UserUnauthorizedToAccessProjectException(userId);
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
