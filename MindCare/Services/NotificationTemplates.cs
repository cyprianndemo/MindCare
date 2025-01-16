namespace MindCare.Services
{
    public static class NotificationTemplates
    {
        public static (string title, string message) GetAppointmentBookedTemplate(DateTime appointmentTime)
        {
            return (
                "Appointment Booked",
                $"Your appointment has been scheduled for {appointmentTime:MMM dd, yyyy HH:mm}"
            );
        }

        public static (string title, string message) GetAppointmentApprovedTemplate(DateTime appointmentTime)
        {
            return (
                "Appointment Approved",
                $"Your appointment for {appointmentTime:MMM dd, yyyy HH:mm} has been approved"
            );
        }

        public static (string title, string message) GetAppointmentCancelledTemplate(DateTime appointmentTime)
        {
            return (
                "Appointment Cancelled",
                $"Your appointment scheduled for {appointmentTime:MMM dd, yyyy HH:mm} has been cancelled"
            );
        }

        public static (string title, string message) GetAppointmentRescheduledTemplate(DateTime oldTime, DateTime newTime)
        {
            return (
                "Appointment Rescheduled",
                $"Your appointment has been rescheduled from {oldTime:MMM dd, yyyy HH:mm} to {newTime:MMM dd, yyyy HH:mm}"
            );
        }

        public static (string title, string message) GetAppointmentReminderTemplate(DateTime appointmentTime)
        {
            return (
                "Upcoming Appointment Reminder",
                $"Reminder: You have an appointment scheduled for {appointmentTime:MMM dd, yyyy HH:mm}"
            );
        }
    }
}
