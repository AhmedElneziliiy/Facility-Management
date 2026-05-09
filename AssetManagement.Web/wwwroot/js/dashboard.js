// ─────────────────────────────────────────────────────────────────
//  Dashboard Charts  —  Vision Valley palette
// ─────────────────────────────────────────────────────────────────

const COLORS = {
    purple:  '#667eea',
    violet:  '#764ba2',
    teal:    '#11998e',
    green:   '#38ef7d',
    red:     '#f5576c',
    pink:    '#f093fb',
    amber:   '#f39c12',
    gold:    '#fee140',
    blue:    '#4facfe',
    cyan:    '#00f2fe',
};

const GRADIENTS = {
    primary:  [COLORS.purple, COLORS.violet],
    success:  [COLORS.teal,   COLORS.green],
    danger:   [COLORS.red,    COLORS.pink],
    warning:  [COLORS.amber,  COLORS.gold],
    info:     [COLORS.blue,   COLORS.cyan],
};

// Build a linear gradient for Chart.js canvas
function makeGradient(ctx, color1, color2, vertical = true) {
    const g = vertical
        ? ctx.createLinearGradient(0, 0, 0, 300)
        : ctx.createLinearGradient(0, 0, 300, 0);
    g.addColorStop(0,   color1);
    g.addColorStop(1,   color2);
    return g;
}

// Shared chart defaults
Chart.defaults.font.family = "'Segoe UI', Tahoma, Geneva, Verdana, sans-serif";
Chart.defaults.font.size   = 12;
Chart.defaults.color       = '#718096';

// ─── 1. Priority Donut ────────────────────────────────────────────
function initPriorityChart(data) {
    const el = document.getElementById('priorityChart');
    if (!el) return;
    new Chart(el, {
        type: 'doughnut',
        data: {
            labels: ['Critical', 'Urgent', 'Normal', 'Low'],
            datasets: [{
                data: [data.critical, data.urgent, data.normal, data.low],
                backgroundColor: [COLORS.red, COLORS.amber, COLORS.purple, COLORS.teal],
                borderWidth: 3,
                borderColor: '#fff',
                hoverOffset: 8
            }]
        },
        options: {
            cutout: '70%',
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: { boxWidth: 10, padding: 16, font: { size: 12 } }
                },
                tooltip: {
                    callbacks: {
                        label: ctx => ` ${ctx.label}: ${ctx.parsed} tickets`
                    }
                }
            },
            maintainAspectRatio: false
        }
    });
}

// ─── 2. Status Donut ──────────────────────────────────────────────
function initStatusChart(data) {
    const el = document.getElementById('statusChart');
    if (!el) return;
    new Chart(el, {
        type: 'doughnut',
        data: {
            labels: ['Open', 'In Progress', 'Closed'],
            datasets: [{
                data: [data.open, data.inProgress, data.closed],
                backgroundColor: [COLORS.purple, COLORS.amber, COLORS.teal],
                borderWidth: 3,
                borderColor: '#fff',
                hoverOffset: 8
            }]
        },
        options: {
            cutout: '70%',
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: { boxWidth: 10, padding: 16, font: { size: 12 } }
                },
                tooltip: {
                    callbacks: {
                        label: ctx => ` ${ctx.label}: ${ctx.parsed} tickets`
                    }
                }
            },
            maintainAspectRatio: false
        }
    });
}

// ─── 3. 7-Day Trend Line ──────────────────────────────────────────
function initTrendChart(trend) {
    const el = document.getElementById('trendChart');
    if (!el) return;
    const ctx = el.getContext('2d');

    const gradCreated  = makeGradient(ctx, 'rgba(102,126,234,0.35)', 'rgba(102,126,234,0.02)');
    const gradResolved = makeGradient(ctx, 'rgba(17,153,142,0.3)',  'rgba(17,153,142,0.02)');

    new Chart(el, {
        type: 'line',
        data: {
            labels: trend.map(d => d.label),
            datasets: [
                {
                    label: 'Created',
                    data: trend.map(d => d.created),
                    borderColor: COLORS.purple,
                    backgroundColor: gradCreated,
                    fill: true,
                    tension: 0.45,
                    pointBackgroundColor: COLORS.purple,
                    pointRadius: 5,
                    pointHoverRadius: 7,
                    borderWidth: 2.5
                },
                {
                    label: 'Resolved',
                    data: trend.map(d => d.resolved),
                    borderColor: COLORS.teal,
                    backgroundColor: gradResolved,
                    fill: true,
                    tension: 0.45,
                    pointBackgroundColor: COLORS.teal,
                    pointRadius: 5,
                    pointHoverRadius: 7,
                    borderWidth: 2.5
                }
            ]
        },
        options: {
            maintainAspectRatio: false,
            interaction: { mode: 'index', intersect: false },
            plugins: {
                legend: {
                    position: 'top',
                    align: 'end',
                    labels: { boxWidth: 10, padding: 16 }
                },
                tooltip: {
                    backgroundColor: '#1a202c',
                    titleColor: '#e2e8f0',
                    bodyColor: '#a0aec0',
                    padding: 12,
                    cornerRadius: 8
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: { stepSize: 1, color: '#a0aec0' },
                    grid: { color: 'rgba(0,0,0,0.05)' }
                },
                x: {
                    ticks: { color: '#a0aec0' },
                    grid: { display: false }
                }
            }
        }
    });
}

// ─── 4. Monthly Created vs Resolved Bar ──────────────────────────
function initMonthlyChart(monthly) {
    const el = document.getElementById('monthlyChart');
    if (!el) return;
    const ctx = el.getContext('2d');

    const gradCreated  = makeGradient(ctx, COLORS.purple, COLORS.violet);
    const gradResolved = makeGradient(ctx, COLORS.teal,   COLORS.green);

    new Chart(el, {
        type: 'bar',
        data: {
            labels: monthly.map(m => m.month),
            datasets: [
                {
                    label: 'Created',
                    data: monthly.map(m => m.created),
                    backgroundColor: gradCreated,
                    borderRadius: { topLeft: 6, topRight: 6 },
                    borderSkipped: false,
                    barPercentage: 0.5,
                    categoryPercentage: 0.7
                },
                {
                    label: 'Resolved',
                    data: monthly.map(m => m.resolved),
                    backgroundColor: gradResolved,
                    borderRadius: { topLeft: 6, topRight: 6 },
                    borderSkipped: false,
                    barPercentage: 0.5,
                    categoryPercentage: 0.7
                }
            ]
        },
        options: {
            maintainAspectRatio: false,
            interaction: { mode: 'index', intersect: false },
            plugins: {
                legend: {
                    position: 'top',
                    align: 'end',
                    labels: { boxWidth: 10, padding: 16 }
                },
                tooltip: {
                    backgroundColor: '#1a202c',
                    titleColor: '#e2e8f0',
                    bodyColor: '#a0aec0',
                    padding: 12,
                    cornerRadius: 8
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: { stepSize: 1, color: '#a0aec0' },
                    grid: { color: 'rgba(0,0,0,0.05)' }
                },
                x: {
                    ticks: { color: '#a0aec0' },
                    grid: { display: false }
                }
            }
        }
    });
}

// ─── 5. Building Horizontal Bar ───────────────────────────────────
function initBuildingChart(buildings) {
    const el = document.getElementById('buildingChart');
    if (!el) return;
    const ctx = el.getContext('2d');

    const gradOpen     = makeGradient(ctx, COLORS.purple, COLORS.violet, false);
    const gradCritical = makeGradient(ctx, COLORS.red,    COLORS.pink,   false);

    new Chart(el, {
        type: 'bar',
        data: {
            labels: buildings.map(b => b.name),
            datasets: [
                {
                    label: 'Open Tickets',
                    data: buildings.map(b => b.openTickets),
                    backgroundColor: gradOpen,
                    borderRadius: 6,
                    barPercentage: 0.5,
                    categoryPercentage: 0.75
                },
                {
                    label: 'Critical',
                    data: buildings.map(b => b.criticalTickets),
                    backgroundColor: gradCritical,
                    borderRadius: 6,
                    barPercentage: 0.5,
                    categoryPercentage: 0.75
                }
            ]
        },
        options: {
            indexAxis: 'y',
            maintainAspectRatio: false,
            interaction: { mode: 'index', intersect: false },
            plugins: {
                legend: {
                    position: 'top',
                    align: 'end',
                    labels: { boxWidth: 10, padding: 16 }
                },
                tooltip: {
                    backgroundColor: '#1a202c',
                    titleColor: '#e2e8f0',
                    bodyColor: '#a0aec0',
                    padding: 12,
                    cornerRadius: 8
                }
            },
            scales: {
                x: {
                    beginAtZero: true,
                    ticks: { stepSize: 1, color: '#a0aec0' },
                    grid: { color: 'rgba(0,0,0,0.05)' }
                },
                y: {
                    ticks: { color: '#a0aec0' },
                    grid: { display: false }
                }
            }
        }
    });
}

// ─── Number counter animation ─────────────────────────────────────
function animateCounters() {
    document.querySelectorAll('[data-count]').forEach(el => {
        const target   = parseInt(el.getAttribute('data-count'), 10);
        const duration = 900;
        const step     = 16;
        const steps    = Math.ceil(duration / step);
        let   current  = 0;
        const increment = target / steps;

        const timer = setInterval(() => {
            current += increment;
            if (current >= target) {
                el.textContent = target;
                clearInterval(timer);
            } else {
                el.textContent = Math.floor(current);
            }
        }, step);
    });
}

// ─── Main entry ───────────────────────────────────────────────────
function initDashboardCharts(data) {
    initPriorityChart(data.priority);
    initStatusChart(data.status);
    initTrendChart(data.trend);
    initMonthlyChart(data.monthly);
    initBuildingChart(data.buildings);
    animateCounters();
}
