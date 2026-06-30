namespace RecruitmentPlatform.API.DTOs
{
    public class ApplyDto
    {
        public string JobPostingId { get; set; } = string.Empty;
        public string ResumeUrl { get; set; } = string.Empty;
    }

    public class ApplicationResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string JobPostingId { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public string CandidateId { get; set; } = string.Empty;
        public string CandidateName { get; set; } = string.Empty;
        public string CandidateEmail { get; set; } = string.Empty;
        public string ResumeUrl { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal AiMatchScore { get; set; }
        public DateTime AppliedAt { get; set; }
    }

    public class UpdateStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }

    public class DashboardStatsDto
    {
        public int TotalJobs { get; set; }
        public int TotalApplications { get; set; }
        public int Shortlisted { get; set; }
        public int Interviewed { get; set; }
        public int Hired { get; set; }
        public int Rejected { get; set; }
        public decimal AvgMatchScore { get; set; }
    }
}
