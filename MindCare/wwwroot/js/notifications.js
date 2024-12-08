// wwwroot/js/notification.js
$(document).ready(function () {
    // Initialize SignalR
    var connection = new signalR.HubConnectionBuilder()
        .withUrl("/notificationHub")
        .build();

    // Start the connection
    connection.start().catch(function (err) {
        console.error(err.toString());
    });

    // Handle receiving notifications
    connection.on("ReceiveNotification", function (notification) {
        // Add notification to list
        addNotificationToList(notification);

        // Show toast notification
        showToast(notification.message);

        // Update notification count
        updateNotificationCount();
    });

    function addNotificationToList(notification) {
        var html = `
            <div class="notification-item" data-id="${notification.id}">
                <div class="notification-message">${notification.message}</div>
                <div class="notification-time">${new Date(notification.createdAt).toLocaleString()}</div>
                <button class="mark-read" onclick="markAsRead(${notification.id})">Mark as Read</button>
            </div>
        `;
        $("#notificationList").prepend(html);
    }

    function showToast(message) {
        // Create and show toast notification
        var toast = $(`
            <div class="toast" role="alert">
                <div class="toast-body">${message}</div>
            </div>
        `);

        $("#toastContainer").append(toast);
        toast.toast({ delay: 3000 }).toast('show');
    }

    function updateNotificationCount() {
        $.get("/Notification/GetNotifications", function (notifications) {
            var unreadCount = notifications.filter(n => !n.isRead).length;
            $("#notificationCount").text(unreadCount);
        });
    }

    // Load initial notifications
    $.get("/Notification/GetNotifications", function (notifications) {
        notifications.forEach(function (notification) {
            addNotificationToList(notification);
        });
        updateNotificationCount();
    });
});

function markAsRead(notificationId) {
    $.post("/Notification/MarkAsRead", { notificationId: notificationId }, function () {
        $(`.notification-item[data-id="${notificationId}"]`).addClass("read");
        updateNotificationCount();
    });
}