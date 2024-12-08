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

        public static (string title, string message) GetAppointmentReminderTemplate(DateTime appointmentTime)
        {
            return (
                "Upcoming Appointment Reminder",
                $"You have an appointment scheduled for {appointmentTime:MMM dd, yyyy HH:mm}"
            );
        }

        public static (string title, string message) GetMessageReceivedTemplate(string senderName)
        {
            return (
                "New Message",
                $"You have received a new message from {senderName}"
            );
        }
    }
}
