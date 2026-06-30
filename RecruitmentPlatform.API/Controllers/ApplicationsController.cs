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
    public class ApplicationsController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public ApplicationsController(MongoDbContext context)
        {
            _context = context;
        }

        // POST /api/applications — Apply for a job (Candidate)
        [HttpPost]
        [Authorize(Roles = "Candidate")]
        public async Task<IActionResult> Apply(ApplyDto dto)
        {
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                      ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Invalid token." });

            // Check if job exists
            var job = await _context.JobPostings
                .Find(j => j.Id == dto.JobPostingId && j.IsActive)
                .FirstOrDefaultAsync();

            if (job == null)
                return NotFound(new { message = "Job not found or inactive." });

            // Check if already applied
            var existing = await _context.Applications
                .Find(a => a.JobPostingId == dto.JobPostingId && a.CandidateId == userId)
                .FirstOrDefaultAsync();

            if (existing != null)
                return BadRequest(new { message = "You have already applied for this job." });

            // Generate a simple AI match score (random for now, would use real AI in production)
            var random = new Random();
            var matchScore = Math.Round((decimal)(random.NextDouble() * 40 + 60), 2); // Score between 60-100

            var application = new Application
            {
                JobPostingId = dto.JobPostingId,
                CandidateId = userId,
                ResumeUrl = dto.ResumeUrl,
                AiMatchScore = matchScore
            };

            await _context.Applications.InsertOneAsync(application);

            return Ok(new { message = "Application submitted successfully.", matchScore });
        }

        // GET /api/applications/my — Get candidate's own applications
        [HttpGet("my")]
        [Authorize(Roles = "Candidate")]
        public async Task<IActionResult> GetMyApplications()
        {
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                      ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            var applications = await _context.Applications
                .Find(a => a.CandidateId == userId)
                .SortByDescending(a => a.AppliedAt)
                .ToListAsync();

            var response = new List<ApplicationResponseDto>();
            foreach (var app in applications)
            {
                var job = await _context.JobPostings
                    .Find(j => j.Id == app.JobPostingId)
                    .FirstOrDefaultAsync();

                response.Add(new ApplicationResponseDto
                {
                    Id = app.Id,
                    JobPostingId = app.JobPostingId,
                    JobTitle = job?.Title ?? "Deleted Job",
                    CandidateId = app.CandidateId,
                    ResumeUrl = app.ResumeUrl,
                    Status = app.Status,
                    AiMatchScore = app.AiMatchScore,
                    AppliedAt = app.AppliedAt
                });
            }

            return Ok(response);
        }

        // GET /api/applications/job/{jobId} — Get applications for a job (Recruiter)
        [HttpGet("job/{jobId}")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> GetByJob(string jobId)
        {
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                      ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Verify this job belongs to the recruiter
            var job = await _context.JobPostings
                .Find(j => j.Id == jobId && j.RecruiterId == userId)
                .FirstOrDefaultAsync();

            if (job == null)
                return NotFound(new { message = "Job not found or not yours." });

            var applications = await _context.Applications
                .Find(a => a.JobPostingId == jobId)
                .SortByDescending(a => a.AiMatchScore)
                .ToListAsync();

            var response = new List<ApplicationResponseDto>();
            foreach (var app in applications)
            {
                var candidate = await _context.Users
                    .Find(u => u.Id == app.CandidateId)
                    .FirstOrDefaultAsync();

                response.Add(new ApplicationResponseDto
                {
                    Id = app.Id,
                    JobPostingId = app.JobPostingId,
                    JobTitle = job.Title,
                    CandidateId = app.CandidateId,
                    CandidateName = (candidate?.FirstName + " " + candidate?.LastName).Trim(),
                    CandidateEmail = candidate?.Email ?? "",
                    ResumeUrl = app.ResumeUrl,
                    Status = app.Status,
                    AiMatchScore = app.AiMatchScore,
                    AppliedAt = app.AppliedAt
                });
            }

            return Ok(response);
        }

        // PUT /api/applications/{id}/status — Update application status (Recruiter)
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> UpdateStatus(string id, UpdateStatusDto dto)
        {
            var validStatuses = new[] { "Applied", "Shortlisted", "Interviewed", "Rejected", "Hired" };
            if (!validStatuses.Contains(dto.Status))
                return BadRequest(new { message = "Invalid status." });

            var application = await _context.Applications
                .Find(a => a.Id == id)
                .FirstOrDefaultAsync();

            if (application == null)
                return NotFound(new { message = "Application not found." });

            // Verify the recruiter owns the job
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                      ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            var job = await _context.JobPostings
                .Find(j => j.Id == application.JobPostingId && j.RecruiterId == userId)
                .FirstOrDefaultAsync();

            if (job == null)
                return Forbid();

            var update = Builders<Application>.Update.Set(a => a.Status, dto.Status);
            await _context.Applications.UpdateOneAsync(a => a.Id == id, update);

            return Ok(new { message = $"Status updated to {dto.Status}." });
        }

        // GET /api/applications/stats — Dashboard stats
        [HttpGet("stats")]
        [Authorize]
        public async Task<IActionResult> GetStats()
        {
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                      ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = User.FindFirstValue("role");

            if (role == "Recruiter")
            {
                var myJobs = await _context.JobPostings
                    .Find(j => j.RecruiterId == userId)
                    .ToListAsync();
                var jobIds = myJobs.Select(j => j.Id).ToList();

                var applications = await _context.Applications
                    .Find(a => jobIds.Contains(a.JobPostingId))
                    .ToListAsync();

                return Ok(new DashboardStatsDto
                {
                    TotalJobs = myJobs.Count(j => j.IsActive),
                    TotalApplications = applications.Count,
                    Shortlisted = applications.Count(a => a.Status == "Shortlisted"),
                    Interviewed = applications.Count(a => a.Status == "Interviewed"),
                    Hired = applications.Count(a => a.Status == "Hired"),
                    Rejected = applications.Count(a => a.Status == "Rejected"),
                    AvgMatchScore = applications.Count > 0 ? Math.Round(applications.Average(a => a.AiMatchScore), 1) : 0
                });
            }
            else
            {
                var applications = await _context.Applications
                    .Find(a => a.CandidateId == userId)
                    .ToListAsync();

                return Ok(new DashboardStatsDto
                {
                    TotalJobs = 0,
                    TotalApplications = applications.Count,
                    Shortlisted = applications.Count(a => a.Status == "Shortlisted"),
                    Interviewed = applications.Count(a => a.Status == "Interviewed"),
                    Hired = applications.Count(a => a.Status == "Hired"),
                    Rejected = applications.Count(a => a.Status == "Rejected"),
                    AvgMatchScore = applications.Count > 0 ? Math.Round(applications.Average(a => a.AiMatchScore), 1) : 0
                });
            }
        }
    }
}
