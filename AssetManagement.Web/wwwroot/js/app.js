// Asset Management - App Scripts

// Auto-dismiss alerts after 5 seconds
document.addEventListener('DOMContentLoaded', function () {
    setTimeout(function () {
        document.querySelectorAll('.alert.alert-dismissible').forEach(function (el) {
            var bsAlert = bootstrap.Alert.getOrCreateInstance(el);
            bsAlert.close();
        });
    }, 5000);
});

// Ticket row click navigation
document.querySelectorAll('.ticket-row[data-url]').forEach(function (row) {
    row.addEventListener('click', function () {
        window.location.href = row.dataset.url;
    });
});

// Priority badge helper (used in views via inline script blocks)
function priorityBadge(priority) {
    var map = {
        'critical': '<span class="badge badge-priority-critical">Critical</span>',
        'urgent':   '<span class="badge badge-priority-urgent">Urgent</span>',
        'normal':   '<span class="badge badge-priority-normal">Normal</span>',
        'low':      '<span class="badge badge-priority-low">Low</span>'
    };
    return map[(priority || '').toLowerCase()] || '<span class="badge bg-secondary">' + priority + '</span>';
}

function statusBadge(status) {
    var map = {
        'open':        '<span class="badge badge-status-open">Open</span>',
        'in_progress': '<span class="badge badge-status-in_progress">In Progress</span>',
        'closed':      '<span class="badge badge-status-closed">Closed</span>'
    };
    return map[(status || '').toLowerCase()] || '<span class="badge bg-secondary">' + status + '</span>';
}
