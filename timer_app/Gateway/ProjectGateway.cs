using Microsoft.EntityFrameworkCore;
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

        public async Task<Project> CreateProject(Project project, int userId)
        {
            project.UserId = userId;

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return project;
        }

        public async Task<bool> UpdateProject(Project project, int userId)
        {
            var existingProject = await _context.Projects.FindAsync(project.Id);
            if (existingProject == null) return false;

            if (existingProject.UserId != userId)
            {
                throw new UserUnauthorizedException(userId);
            }

            // Map through project fields
            existingProject.Description = project.Description;
            existingProject.DisplayColour = project.DisplayColour;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteProject(int projectId, int userId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return false;

            if (project.UserId != userId)
            {
                throw new UserUnauthorizedException(userId);
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
