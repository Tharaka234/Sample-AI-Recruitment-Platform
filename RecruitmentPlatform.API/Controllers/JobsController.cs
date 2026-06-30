using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using RecruitmentPlatform.API.Data;
using RecruitmentPlatform.API.DTOs;
using RecruitmentPlatform.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RecruitmentPlatform.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public JobsController(MongoDbContext context)
        {
            _context = context;
        }

        // GET /api/jobs — List all active jobs (public)
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search)
        {
            var filter = Builders<JobPosting>.Filter.Eq(j => j.IsActive, true);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchFilter = Builders<JobPosting>.Filter.Or(
                    Builders<JobPosting>.Filter.Regex(j => j.Title, new MongoDB.Bson.BsonRegularExpression(search, "i")),
                    Builders<JobPosting>.Filter.Regex(j => j.RequiredSkills, new MongoDB.Bson.BsonRegularExpression(search, "i"))
                );
                filter = Builders<JobPosting>.Filter.And(filter, searchFilter);
            }

            var jobs = await _context.JobPostings
                .Find(filter)
                .SortByDescending(j => j.CreatedAt)
                .ToListAsync();

            var response = new List<JobResponseDto>();
            foreach (var job in jobs)
            {
                var recruiter = await _context.Users
                    .Find(u => u.Id == job.RecruiterId)
                    .FirstOrDefaultAsync();

                response.Add(new JobResponseDto
                {
                    Id = job.Id,
                    Title = job.Title,
                    Description = job.Description,
                    RequiredSkills = job.RequiredSkills,
                    RecruiterName = recruiter?.FirstName + " " + recruiter?.LastName,
                    RecruiterId = job.RecruiterId,
                    CreatedAt = job.CreatedAt,
                    IsActive = job.IsActive
                });
            }

            return Ok(response);
        }

        // GET /api/jobs/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var job = await _context.JobPostings
                .Find(j => j.Id == id)
                .FirstOrDefaultAsync();

            if (job == null)
                return NotFound(new { message = "Job not found." });

            var recruiter = await _context.Users
                .Find(u => u.Id == job.RecruiterId)
                .FirstOrDefaultAsync();

            return Ok(new JobResponseDto
            {
                Id = job.Id,
                Title = job.Title,
                Description = job.Description,
                RequiredSkills = job.RequiredSkills,
                RecruiterName = recruiter?.FirstName + " " + recruiter?.LastName,
                RecruiterId = job.RecruiterId,
                CreatedAt = job.CreatedAt,
                IsActive = job.IsActive
            });
        }

        // POST /api/jobs — Create job (Recruiter only)
        [HttpPost]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> Create(CreateJobDto dto)
        {
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                      ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Invalid token." });

            var job = new JobPosting
            {
                Title = dto.Title,
                Description = dto.Description,
                RequiredSkills = dto.RequiredSkills,
                RecruiterId = userId
            };

            await _context.JobPostings.InsertOneAsync(job);

            return Ok(new { message = "Job posted successfully.", id = job.Id });
        }

        // GET /api/jobs/my — Get recruiter's own jobs
        [HttpGet("my")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> GetMyJobs()
        {
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                      ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            var jobs = await _context.JobPostings
                .Find(j => j.RecruiterId == userId)
                .SortByDescending(j => j.CreatedAt)
                .ToListAsync();

            var recruiter = await _context.Users
                .Find(u => u.Id == userId)
                .FirstOrDefaultAsync();

            var response = jobs.Select(job => new JobResponseDto
            {
                Id = job.Id,
                Title = job.Title,
                Description = job.Description,
                RequiredSkills = job.RequiredSkills,
                RecruiterName = recruiter?.FirstName + " " + recruiter?.LastName,
                RecruiterId = job.RecruiterId,
                CreatedAt = job.CreatedAt,
                IsActive = job.IsActive
            }).ToList();

            return Ok(response);
        }

        // DELETE /api/jobs/{id} — Delete job (owner only)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> Delete(string id)
        {
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                      ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            var job = await _context.JobPostings
                .Find(j => j.Id == id && j.RecruiterId == userId)
                .FirstOrDefaultAsync();

            if (job == null)
                return NotFound(new { message = "Job not found or not yours." });

            await _context.JobPostings.DeleteOneAsync(j => j.Id == id);
            await _context.Applications.DeleteManyAsync(a => a.JobPostingId == id);

            return Ok(new { message = "Job deleted." });
        }
    }
}
