using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MindCare.Models;

namespace MindCare.ViewModels
{
    public class EditUserViewModel
    {
        public string Id { get; set; }

        [Required]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; }

        public List<RoleSelection> Roles { get; set; }
    }

    public class RoleSelection
    {
        public string RoleName { get; set; }
        public bool Selected { get; set; }
    }

    // New view models for enhanced functionality
    public class CreateUserViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        public List<RoleSelection> Roles { get; set; }
    }

    public class UserManagementViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public DateTime DateRegistered { get; set; }
        public string Status { get; set; }
        public string Roles { get; set; }
    }

    // System Performance Report ViewModels
    public class SystemPerformanceReportViewModel
    {
        public int TotalUsers { get; set; }
        public int StudentCount { get; set; }
        public int TherapistCount { get; set; }
        public int PsychiatristCount { get; set; }
        public int MonthlyActiveUsers { get; set; }

        public int TotalSessions { get; set; }
        public int CompletedSessions { get; set; }
        public int UpcomingSessions { get; set; }
        public int CancelledSessions { get; set; }

        public List<MonthlySession> MonthlySessionData { get; set; }
        public SystemPerformanceMetrics SystemPerformance { get; set; }
        public List<FeedbackSummary> UserFeedback { get; set; }
    }

    public class SystemPerformanceMetrics
    {
        public double Uptime { get; set; }
        public double AverageResponseTime { get; set; }
        public int ErrorCount { get; set; }
        public int CriticalErrors { get; set; }
    }

    public class FeedbackSummary
    {
        public string Issue { get; set; }
        public int Count { get; set; }
    }

    // Financial Transactions Report ViewModels
    public class FinancialTransactionsReportViewModel
    {
        public decimal TotalPaymentsReceived { get; set; }
        public int TotalTransactions { get; set; }
        public int SuccessfulTransactions { get; set; }
        public int FailedTransactions { get; set; }
        public string ReceiptNumber { get; set; }

        public List<PaymentMethodSummary> PaymentMethodSummary { get; set; }
        public List<MonthlyPayment> MonthlyPaymentData { get; set; }
        public List<PaymentStatusSummary> PaymentStatusSummary { get; set; }
        public List<Payment> RecentTransactions { get; set; }
    }

    public class PaymentMethodSummary
    {
        public string Method { get; set; }
        public int Count { get; set; }
        public decimal Total { get; set; }
    }

    public class PaymentStatusSummary
    {
        public string Status { get; set; }
        public int Count { get; set; }
        public decimal Total { get; set; }
    }

    public class MonthlyPayment
    {
        public string Month { get; set; }
        public decimal Amount { get; set; }
    }

    public class MonthlySession
    {
        public string Month { get; set; }
        public int SessionCount { get; set; }
    }

    // Existing view model
    public class PerformanceReportViewModel
    {
        public int StudentCount { get; set; }
        public int TherapistCount { get; set; }
        public int PsychiatristCount { get; set; }
        public int TotalSessions { get; set; }
        public int CompletedSessions { get; set; }
        public int UpcomingSessions { get; set; }
    }

    public class SystemUsageReportViewModel
    {
        public int TotalSessions { get; set; }
        public int CurrentMonthSessions { get; set; }
        public List<MonthlySession> MonthlySessionData { get; set; }
    }
    public class DashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalSessions { get; set; }
        public int PendingSessions { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<UserActivityViewModel> RecentActivities { get; set; }
        public List<MonthlyDataPoint> MonthlySessions { get; set; }
        public List<MonthlyDataPoint> MonthlyUsers { get; set; }
        public SystemPerformanceViewModel SystemPerformance { get; set; }
    }

    public class UserActivityViewModel
    {
        public string UserName { get; set; }
        public string Action { get; set; }
        public string Details { get; set; }
        public DateTime Timestamp { get; set; }

        public string TimeAgo
        {
            get
            {
                var timeSpan = DateTime.Now - Timestamp;

                if (timeSpan.TotalMinutes < 1)
                    return "just now";
                if (timeSpan.TotalMinutes < 60)
                    return $"{(int)timeSpan.TotalMinutes} minute{((int)timeSpan.TotalMinutes == 1 ? "" : "s")} ago";
                if (timeSpan.TotalHours < 24)
                    return $"{(int)timeSpan.TotalHours} hour{((int)timeSpan.TotalHours == 1 ? "" : "s")} ago";

                return $"{(int)timeSpan.TotalDays} day{((int)timeSpan.TotalDays == 1 ? "" : "s")} ago";
            }
        }
    }

    public class MonthlyDataPoint
    {
        public string Month { get; set; }
        public int Count { get; set; }
    }

    public class SystemPerformanceViewModel
    {
        public double Uptime { get; set; }
        public double ResponseTime { get; set; }
        public int CpuUsage { get; set; }
        public int MemoryUsage { get; set; }
    }
}