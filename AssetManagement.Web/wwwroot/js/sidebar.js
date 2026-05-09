// Sidebar JavaScript

document.addEventListener('DOMContentLoaded', function () {
    initializeSidebar();
    highlightActivePage();
    addTooltips();
});

// Initialize sidebar functionality
function initializeSidebar() {
    const sidebar = document.getElementById('sidebar');
    const sidebarToggle = document.getElementById('sidebarToggle');
    const sidebarOverlay = document.getElementById('sidebarOverlay');
    const mobileMenuBtn = document.getElementById('mobileMenuBtn');

    if (mobileMenuBtn) {
        mobileMenuBtn.addEventListener('click', function (e) {
            e.stopPropagation();
            toggleSidebar();
        });
    }

    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', function (e) {
            e.stopPropagation();
            toggleSidebar();
        });
    }

    if (sidebarOverlay) {
        sidebarOverlay.addEventListener('click', closeSidebar);
    }

    let resizeTimer;
    window.addEventListener('resize', function () {
        clearTimeout(resizeTimer);
        resizeTimer = setTimeout(function () {
            if (window.innerWidth > 1024) {
                closeSidebar();
            }
        }, 250);
    });

    loadSidebarState();
}

// Toggle sidebar
function toggleSidebar() {
    const sidebar = document.getElementById('sidebar');
    const overlay = document.getElementById('sidebarOverlay');

    if (window.innerWidth <= 1024) {
        sidebar.classList.toggle('open');
        overlay.classList.toggle('active');
    } else {
        sidebar.classList.toggle('collapsed');
        saveSidebarState();
    }
}

// Close sidebar
function closeSidebar() {
    const sidebar = document.getElementById('sidebar');
    const overlay = document.getElementById('sidebarOverlay');

    sidebar.classList.remove('open');
    overlay.classList.remove('active');
}

// Toggle submenu
function toggleSubmenu(event, element) {
    event.preventDefault();
    const parent = element.closest('.nav-item');

    document.querySelectorAll('.nav-item.has-submenu').forEach(item => {
        if (item !== parent) {
            item.classList.remove('open');
        }
    });

    parent.classList.toggle('open');
}

// Highlight active page
function highlightActivePage() {
    const currentPath = window.location.pathname.toLowerCase();
    const navLinks = document.querySelectorAll('.nav-link');

    navLinks.forEach(link => {
        const href = link.getAttribute('href');
        if (href && href !== '/' && currentPath.startsWith(href.toLowerCase())) {
            link.classList.add('active');
        } else if (href === '/' && currentPath === '/') {
            link.classList.add('active');
        }
    });
}

// Save sidebar state to localStorage
function saveSidebarState() {
    const sidebar = document.getElementById('sidebar');
    const isCollapsed = sidebar.classList.contains('collapsed');
    localStorage.setItem('sidebarCollapsed', isCollapsed);
}

// Load sidebar state from localStorage
function loadSidebarState() {
    const sidebar = document.getElementById('sidebar');
    const isCollapsed = localStorage.getItem('sidebarCollapsed') === 'true';

    if (isCollapsed && window.innerWidth > 1024) {
        sidebar.classList.add('collapsed');
    }
}

// Add tooltips to navigation links (for collapsed sidebar)
function addTooltips() {
    const navLinks = document.querySelectorAll('.nav-link');
    navLinks.forEach(link => {
        const text = link.querySelector('.nav-text');
        if (text && !link.hasAttribute('data-tooltip')) {
            link.setAttribute('data-tooltip', text.textContent.trim());
        }
    });
}
