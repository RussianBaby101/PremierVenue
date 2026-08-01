
// Shared UI helpers, auth guards, venue cards, navigation, and staff access control
function initPublicPageReveal() {
    const path = window.location.pathname.toLowerCase();
    const shouldReveal = path === '/' || path.endsWith('/index.html') || path.includes('/pages/public/') || path.endsWith('/pages/client/venues.html') || path.endsWith('/pages/client/venue-details.html');
    if (shouldReveal && !window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
        document.body.classList.add('page-reveal');
    }
}

initPublicPageReveal();

// Utility Functions
function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
    });
}

function formatCurrency(amount) {
    const numericAmount = Number(amount || 0);
    return `R${numericAmount.toLocaleString('en-ZA', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    })}`;
}

function showLoading(elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.innerHTML = `
            <div class="loading-spinner">
                <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">Loading...</span>
                </div>
            </div>
        `;
    }
}

function showError(message) {
    if (typeof SwalUtils !== 'undefined' && SwalUtils.error) {
        SwalUtils.error('Error', message);
        return;
    }

    const alertDiv = document.createElement('div');
    alertDiv.className = 'alert alert-danger alert-dismissible fade show';
    alertDiv.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;

    const container = document.querySelector('.container');
    if (container) {
        container.insertBefore(alertDiv, container.firstChild);

        setTimeout(() => {
            alertDiv.remove();
        }, 5000);
    }
}

function showSuccess(message) {
    if (typeof SwalUtils !== 'undefined' && SwalUtils.success) {
        SwalUtils.success('Success', message);
        return;
    }

    const alertDiv = document.createElement('div');
    alertDiv.className = 'alert alert-success alert-dismissible fade show';
    alertDiv.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;

    const container = document.querySelector('.container');
    if (container) {
        container.insertBefore(alertDiv, container.firstChild);

        setTimeout(() => {
            alertDiv.remove();
        }, 5000);
    }
}

function getStatusBadgeClass(status) {
    const statusMap = {
        'Pending': 'bg-warning text-dark',
        'UnderReview': 'bg-info text-dark',
        'Quoted': 'bg-primary',
        'QuoteAccepted': 'bg-info text-dark',
        'QuoteRejected': 'bg-danger',
        'Confirmed': 'bg-success',
        'DepositPaid': 'bg-success',
        'FullyPaid': 'bg-success',
        'Completed': 'bg-secondary',
        'Cancelled': 'bg-danger',
        'Rejected': 'bg-danger'
    };
    return statusMap[status] || 'bg-secondary';
}

// Check authentication status
function isAuthenticated() {
    return localStorage.getItem('token') !== null;
}

function getCurrentUser() {
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
}

function requireAuth() {
    if (!isAuthenticated()) {
        window.location.href = '/pages/public/login.html';
        return false;
    }
    return true;
}

function requireStaffAuth() {
    const user = getCurrentUser();
    if (!user || (user.role !== 'Admin' && user.role !== 'Staff')) {
        window.location.href = '/pages/public/login.html';
        return false;
    }
    return true;
}

// Toggles the saved state of a venue for the authenticated user
async function toggleSavedVenue(venueId, button) {
    if (!isAuthenticated()) {
        window.location.href = '/pages/public/login.html';
        return;
    }

    try {
        const saved = button.classList.contains('saved');
        if (saved) {
            await SavedVenueApi.remove(venueId);
        } else {
            await SavedVenueApi.save(venueId);
        }
        button.classList.toggle('saved', !saved);
        button.innerHTML = `<i class="bi ${saved ? 'bi-heart' : 'bi-heart-fill'}"></i>`;
    } catch (error) {
        showError(error.message || 'Unable to update saved venues.');
    }
}

async function refreshDetailSavedButton(venueId) {
    const button = document.getElementById('saveVenueButton');
    if (!button || !isAuthenticated() || typeof SavedVenueApi === 'undefined') return;
    try {
        const saved = await SavedVenueApi.isSaved(venueId);
        button.classList.toggle('saved', saved === true || saved?.data === true);
        button.innerHTML = `<i class="bi ${button.classList.contains('saved') ? 'bi-heart-fill' : 'bi-heart'}"></i> ${button.classList.contains('saved') ? 'Saved' : 'Save'}`;
    } catch (error) {
        console.warn('Unable to load saved venue state', error);
    }
}

function toggleSavedVenueFromDetail() {
    const button = document.getElementById('saveVenueButton');
    const venueId = window.currentVenue?.id;
    if (button && venueId) toggleSavedVenue(venueId, button).then(() => {
        button.innerHTML = `<i class="bi ${button.classList.contains('saved') ? 'bi-heart-fill' : 'bi-heart'}"></i> ${button.classList.contains('saved') ? 'Saved' : 'Save'}`;
    });
}

async function shareVenue() {
    const venue = window.currentVenue;
    if (!venue) return;
    const shareData = { title: venue.name, text: `Take a look at ${venue.name}`, url: window.location.href };
    try {
        if (navigator.share) await navigator.share(shareData);
        else if (navigator.clipboard) await navigator.clipboard.writeText(window.location.href);
        showSuccess(navigator.share ? 'Venue shared.' : 'Venue link copied.');
    } catch (error) {
        if (error.name !== 'AbortError') showError('Unable to share this venue.');
    }
}

// Load featured venues on homepage
async function loadFeaturedVenues() {
    const container = document.getElementById('featuredVenues');
    if (!container) return;

    try {
        const response = await VenueApi.getAll(1, 3);

        if (response.success && response.data) {
            container.innerHTML = response.data.map(venue => createVenueCard(venue)).join('');
            if (typeof SavedVenueApi !== 'undefined' && isAuthenticated()) {
                SavedVenueApi.getAll().then(saved => {
                    const savedIds = new Set((saved || []).map(item => item.venueId));
                    container.querySelectorAll('.save-venue-btn').forEach(button => {
                        const isSaved = savedIds.has(Number(button.dataset.venueId));
                        button.classList.toggle('saved', isSaved);
                        button.innerHTML = `<i class="bi ${isSaved ? 'bi-heart-fill' : 'bi-heart'}"></i>`;
                    });
                }).catch(() => {});
            }
        }
    } catch (error) {
        console.error('Error loading featured venues:', error);
        container.innerHTML = '<p class="text-center text-muted">Unable to load venues at this time.</p>';
    }
}

// Builds the HTML card for a single venue listing
function createVenueCard(venue) {
    const primaryPhoto = venue.photos?.find(p => p.isPrimary) || venue.photos?.[0];
    const imageUrl = venue.thumbnailUrl || venue.imageUrl || primaryPhoto?.url || 'https://images.unsplash.com/photo-1519167758481-83f550bb49b3?w=800&h=600&fit=crop';
    const detailsUrl = `pages/public/venue-details.html?id=${venue.id}`;

    return `
        <div class="col-md-4 mb-4">
            <div class="card venue-card h-100">
                <a href="${detailsUrl}" class="venue-image-link">
                    <img src="${imageUrl}" data-fallback-src="https://images.unsplash.com/photo-1519167758481-83f550bb49b3?w=800&h=600&fit=crop" class="card-img-top" alt="${venue.name}">
                </a>
                <div class="card-body">
                    <div class="d-flex justify-content-between align-items-start gap-2">
                        <h5 class="card-title">${venue.name}</h5>
                        <button type="button" class="save-venue-btn" data-action="toggle-saved-venue" data-venue-id="${venue.id}" aria-label="Save ${venue.name}"><i class="bi bi-heart"></i></button>
                    </div>
                    <p class="card-text text-muted">${venue.city}, ${venue.province}</p>
                    <p class="card-text">
                        <i class="bi bi-people"></i> Capacity: ${venue.capacity}
                    </p>
                    <p class="venue-price">${formatCurrency(venue.basePricePerDay)}<small class="text-muted">/day</small></p>
                    <div class="venue-amenities">
                        ${venue.amenities?.slice(0, 3).map(a =>
        `<span class="venue-amenity">${a.name}</span>`
    ).join('') || ''}
                    </div>
                </div>
                <div class="card-footer bg-white border-top-0">
                    <a href="pages/public/venue-details.html?id=${venue.id}" class="btn btn-primary w-100">View Details</a>
                </div>
            </div>
        </div>
    `;
}

function initHeroParticles() {
    const field = document.getElementById('heroParticles');
    if (!field || typeof anime === 'undefined' || window.matchMedia('(prefers-reduced-motion: reduce)').matches) return;

    const particleCount = window.innerWidth < 768 ? 34 : 68;
    for (let index = 0; index < particleCount; index += 1) {
        const particle = document.createElement('span');
        particle.className = 'hero-particle';
        particle.style.left = `${Math.random() * 100}%`;
        particle.style.top = `${Math.random() * 100}%`;
        field.appendChild(particle);

        anime({
            targets: particle,
            translateX: () => anime.random(-170, 170),
            translateY: () => anime.random(-130, 130),
            scale: () => anime.random(80, 145) / 100,
            duration: () => anime.random(4200, 9000),
            delay: () => anime.random(0, 1800),
            direction: 'alternate',
            easing: 'easeInOutSine',
            loop: true
        });
    }
}

// Search functionality
document.addEventListener('DOMContentLoaded', function () {
    initHeroParticles();
    const searchForm = document.getElementById('searchForm');
    if (searchForm) {
        searchForm.addEventListener('submit', function (e) {
            e.preventDefault();

            const searchParams = {
                searchTerm: document.getElementById('location')?.value || '',
                capacity: document.getElementById('guests')?.value || null,
                minPrice: null,
                maxPrice: null
            };

            const budget = document.getElementById('budget')?.value;
            if (budget) {
                const [min, max] = budget.split('-');
                searchParams.minPrice = min ? parseInt(min) : null;
                searchParams.maxPrice = max ? parseInt(max.replace('+', '')) : null;
            }

            // Store search params and redirect
            sessionStorage.setItem('searchParams', JSON.stringify(searchParams));
            window.location.href = '/pages/public/venues.html';
        });
    }

    // Load featured venues on homepage
    if (window.location.pathname.endsWith('index.html') || window.location.pathname === '/') {
        loadFeaturedVenues();
    }

    // Update navbar based on auth status
    updateNavbarAuth();
});

// Updates the public navbar links based on the authenticated user's role
function updateNavbarAuth() {
    const user = getCurrentUser();
    const loginLink = document.querySelector('a[href="pages/public/login.html"], a[href="login.html"]');
    const registerLink = document.querySelector('a[href="pages/public/register.html"], a[href="register.html"]');

    if (user) {
        if (loginLink) {
            if (user.role === 'Admin' || user.role === 'Staff') {
                loginLink.textContent = 'Staff Dashboard';
                loginLink.href = '/pages/staff/dashboard.html';
            } else {
                loginLink.textContent = 'My Portal';
                loginLink.href = '/pages/client/dashboard.html';
            }
        }

        if (registerLink) {
            registerLink.style.display = 'none';
        }
    }
}

// Enforces role-based access on staff pages and hides admin-only nav items
function enforceStaffPageAccess() {
    const path = window.location.pathname.toLowerCase();
    const isStaffArea = path.includes('/pages/staff/');
    if (!isStaffArea) {
        return;
    }

    const user = getCurrentUser();
    if (!user || (user.role !== 'Admin' && user.role !== 'Staff')) {
        window.location.href = '/pages/public/login.html';
        return;
    }

    const adminOnlyPages = [
        '/pages/staff/clients.html',
        '/pages/staff/reports.html',
        '/pages/staff/users.html',
        '/pages/staff/manage-venues.html',
        '/pages/staff/calendar.html'
    ];
    const isAdminOnlyPage = adminOnlyPages.some(adminPath => path.endsWith(adminPath));
    const isAdmin = user.role === 'Admin';

    // Hide admin-only nav items for staff users.
    if (!isAdmin) {
        ['clients.html', 'reports.html', 'users.html', 'manage-venues.html', 'calendar.html'].forEach(href => {
            const link = document.querySelector(`a[href="${href}"]`);
            if (link) {
                const navItem = link.closest('.nav-item');
                if (navItem) {
                    navItem.style.display = 'none';
                }
            }
        });
    }

    // Block direct access to admin-only pages by staff users.
    if (isAdminOnlyPage && !isAdmin) {
        showError('Access denied. Admin only page.');
        setTimeout(() => {
            window.location.href = '/pages/staff/dashboard.html';
        }, 700);
    }
}

// Keep interactions for static and dynamically-rendered markup out of HTML attributes.
document.addEventListener('click', event => {
    const target = event.target.closest('[data-action]');
    if (!target) return;
    const action = target.dataset.action;
    if (action === 'logout') return logout();
    if (action === 'toggle-saved-venue') return toggleSavedVenue(Number(target.dataset.venueId), target);
    if (action === 'toggle-saved-detail') return toggleSavedVenueFromDetail();
    if (action === 'share-venue') return shareVenue();
    if (action === 'show-create-modal') return showCreateModal();
    if (action === 'filter-venues') return filterVenues();
    if (action === 'add-custom-amenity') return addCustomAmenity();
    if (action === 'add-custom-service') return addCustomServiceOption();
    if (action === 'save-venue') return saveVenue();
    if (action === 'send-invitation') return sendInvitation();
    if (action === 'update-booking-status') return updateBookingStatus();
    if (action === 'open-status-modal') { event.stopPropagation(); return openStatusModal(Number(target.dataset.bookingId), target.dataset.status); }
    if (action === 'open-quote-modal') return openQuoteModal(Number(target.dataset.bookingId));
    if (action === 'upload-staff-document') return;

    if (action === 'view-documents') return viewDocuments(Number(target.dataset.bookingId));
    if (action === 'download-document') return DocumentApi.downloadDocument(Number(target.dataset.documentId));
    if (action === 'view-quote-details') return viewQuoteDetails(Number(target.dataset.bookingId));
    if (action === 'decide-quote') return decideQuote(Number(target.dataset.bookingId), target.dataset.accepted === 'true');
    if (action === 'submit-quote') return submitQuote();
    if (action === 'remove-custom-amenity') return target.closest('.custom-amenity-row')?.remove();
    if (action === 'remove-custom-service') return target.closest('.custom-service-option-row')?.remove();
    if (action === 'remove-existing-photo') return removeExistingPhoto(Number(target.dataset.photoId));
    if (action === 'edit-venue') return editVenue(Number(target.dataset.venueId));
    if (action === 'toggle-venue-status') return toggleVenueStatus(Number(target.dataset.venueId));
    if (action === 'toggle-client-status') return toggleClientStatus(Number(target.dataset.clientId), target.dataset.isActive === 'true');
    if (action === 'toggle-user-status') return toggleUserStatus(Number(target.dataset.userId), target.dataset.isActive === 'true');
    if (action === 'remove-saved-venue') return removeSavedVenue(Number(target.dataset.venueId), target);
    if (action === 'fallback-image') {
        const image = target;
        image.src = image.dataset.fallbackSrc;
        image.removeAttribute('data-fallback-src');
    }
});
document.addEventListener('change', event => {
    const target = event.target.closest('[data-action]');
    if (!target) return;
    if (target.dataset.action === 'upload-staff-document') return uploadStaffDocument(Number(target.dataset.bookingId), 'Invoice', target.files[0]);
    if (target.dataset.action === 'upload-proof') return uploadProof(Number(target.dataset.bookingId), target.files[0]);
    if (target.dataset.action === 'set-existing-primary') return setExistingPrimary(Number(target.dataset.photoId));
});

document.addEventListener('error', event => {
    const image = event.target.closest?.('img[data-fallback-src]');
    if (!image) return;
    const fallback = image.dataset.fallbackSrc;
    image.removeAttribute('data-fallback-src');
    image.src = fallback;
}, true);

// Clears stored auth credentials and redirects to the homepage
function logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');

    // Optional API logout if available on pages that load api.js.
    if (typeof AuthApi !== 'undefined' && AuthApi && typeof AuthApi.logout === 'function') {
        try {
            AuthApi.logout();
        } catch (e) {
            console.warn('AuthApi.logout failed:', e);
        }
    }

    window.location.href = '/index.html';
}

document.addEventListener('DOMContentLoaded', function () {
    enforceStaffPageAccess();
});