using Microsoft.EntityFrameworkCore;
using timer_app.Boundary.Request;
using timer_app.Boundary.Response;
using timer_app.Factories;
using timer_app.Gateway.Interfaces;
using timer_app.Infrastructure;
using timer_app.Infrastructure.Exceptions;

namespace timer_app.Gateway
{
    public class ProjectGateway : IProjectGateway
    {
        private readonly TimerAppDbContext _context;

        public ProjectGateway(TimerAppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProjectWithCount>> GetAllProjects(string userId)
        {
            var projects = await _context.Projects
                .Where(x => x.UserId == userId)
                .Select(x => new ProjectWithCount
                {
                    Project = x,
                    TotalEventDurationInMinutes = _context.CalendarEvents
                        .Where(x => x.UserId == x.UserId)
                        .Where(y => y.ProjectId == x.Id)
                        .Select(x => (int)(x.EndTime - x.StartTime).TotalMinutes)
                        .Sum()
                })
                .ToListAsync();

            return projects;
        }

        public async Task<Project> CreateProject(CreateProjectRequest request, string userId)
        {
            var project = request.ToDb(userId);

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return project;
        }

        public async Task<Project> UpdateProject(int projectId, UpdateProjectRequest request, string userId)
        {
            var existingProject = await _context.Projects.FindAsync(projectId);
            if (existingProject == null) return null;

            if (existingProject.UserId != userId)
            {
                throw new UserUnauthorizedToAccessProjectException(userId);
            }

            if (!existingProject.IsActive)
            {
                throw new ProjectIsArchivedException(projectId);
            }

            // Map through project fields
            existingProject.Description = request.Description;

            existingProject.ProjectColor = new ProjectColor
            {
                Light = request.ProjectColor.Light,
                Lightest = request.ProjectColor.Lightest,
                Darkest = request.ProjectColor.Darkest,
                Dark = request.ProjectColor.Dark,
            };

            _context.Entry(existingProject).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return existingProject;
        }

        public async Task<bool> DeleteProject(int projectId, string userId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return false;

            if (project.UserId != userId)
            {
                throw new UserUnauthorizedToAccessProjectException(userId);
            }

            if (!project.IsActive)
            {
                throw new ProjectIsArchivedException(projectId);
            }

            // Soft delete
            project.IsActive = false;
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
