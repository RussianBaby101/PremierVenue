// Shared venues pages logic for both public and client shells.
(function () {
    const PLACEHOLDER_VENUES = [
        {
            id: 1,
            name: 'Lakeside Pavilion',
            city: 'Cape Town',
            province: 'Western Cape',
            capacity: 220,
            basePricePerDay: 18000,
            latitude: -33.9249,
            longitude: 18.4241,
            eventTypes: [1, 2],
            amenities: [{ name: 'Parking' }, { name: 'Catering' }, { name: 'WiFi' }]
        },
        {
            id: 2,
            name: 'Summit Conference Hall',
            city: 'Johannesburg',
            province: 'Gauteng',
            capacity: 400,
            basePricePerDay: 32000,
            latitude: -26.1077,
            longitude: 28.0556,
            eventTypes: [2, 4],
            amenities: [{ name: 'AV System' }, { name: 'Security' }, { name: 'Stage' }]
        },
        {
            id: 3,
            name: 'Garden Terrace Venue',
            city: 'Durban',
            province: 'KwaZulu-Natal',
            capacity: 140,
            basePricePerDay: 12500,
            latitude: -29.8485,
            longitude: 31.0184,
            eventTypes: [1, 3, 7],
            amenities: [{ name: 'Outdoor Space' }, { name: 'Bar' }, { name: 'Decor' }]
        }
    ];

    function getVenueById(id) {
        return PLACEHOLDER_VENUES.find(v => String(v.id) === String(id)) || null;
    }

    // Builds the public site navigation markup
    function renderPublicNav(activePage) {
        return `
            <nav class="navbar navbar-expand-lg navbar-dark bg-primary fixed-top public-navbar">
                <div class="container">
                    <a class="navbar-brand" href="/index.html"><img src="/assets/images/PremierVenueLogoNoBg.png" alt="" class="brand-logo"> <span>Premier Venue</span></a>
                    <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav">
                        <span class="navbar-toggler-icon"></span>
                    </button>
                    <div class="collapse navbar-collapse" id="navbarNav">
                        <ul class="navbar-nav me-auto">
                            <li class="nav-item"><a class="nav-link ${activePage === 'home' ? 'active' : ''}" href="/index.html">Home</a></li>
                            <li class="nav-item"><a class="nav-link ${activePage === 'venues' ? 'active' : ''}" href="venues.html">Venues</a></li>
                            <li class="nav-item"><a class="nav-link ${activePage === 'contact' ? 'active' : ''}" href="contact-us.html">Contact Us</a></li>
                        </ul>
                        <ul class="navbar-nav">
                            <li class="nav-item"><a class="nav-link" href="login.html">Login</a></li>
                        </ul>
                    </div>
                </div>
            </nav>
        `;
    }

    // Builds the authenticated client portal navigation markup
    function renderPortalNav(activePage) {
        return `
            <nav class="navbar navbar-expand-lg navbar-dark bg-primary fixed-top app-navbar">
                <div class="container">
                    <div class="collapse navbar-collapse" id="navbarNav">
                        <ul class="navbar-nav me-auto">
                            <li class="nav-item"><a class="nav-link ${activePage === 'dashboard' ? 'active' : ''}" href="dashboard.html">Dashboard</a></li>
                            <li class="nav-item"><a class="nav-link ${activePage === 'venues' ? 'active' : ''}" href="venues.html">Venues</a></li>
                            <li class="nav-item"><a class="nav-link" href="saved-venues.html">Saved Venues</a></li>
                            <li class="nav-item"><a class="nav-link ${activePage === 'bookings' ? 'active' : ''}" href="my-bookings.html">My Bookings</a></li>
                            <li class="nav-item"><a class="nav-link ${activePage === 'calendar' ? 'active' : ''}" href="calendar.html">Calendar</a></li>
                            <li class="nav-item"><a class="nav-link ${activePage === 'profile' ? 'active' : ''}" href="profile.html">Profile</a></li>
                        </ul>
                        <button class="btn btn-outline-light" data-action="logout">Logout</button>
                    </div>
                </div>
            </nav>
        `;
    }

    // Injects the correct navigation bar for the requested mode
    function mountNav(mode) {
        const navHost = document.getElementById('venuesNavMount');
        if (!navHost) {
            return;
        }

        const activePage = 'venues';
        navHost.innerHTML = mode === 'portal' ? renderPortalNav(activePage) : renderPublicNav(activePage);
    }

    window.resolveVenueImage = function resolveVenueImage(url) {
        if (!url) return url;
        if (!url.startsWith('/')) return url;
        const apiRoot = typeof API_BASE_URL !== 'undefined' ? API_BASE_URL : '';
        return apiRoot ? new URL(url, apiRoot.replace(/\/api\/?$/, '')).href : url;
    }

    // Builds the HTML card for a venue in either public or portal mode
    function createVenueCard(venue, mode, isFeatured = false) {
        const featured = isFeatured || venue.isFeatured;
        const imagePath = resolveVenueImage(venue.thumbnailUrl || venue.imageUrl || venue.photos?.find(p => p.isPrimary)?.url || venue.photos?.[0]?.url);
        const fallbackImagePaths = [
            'https://images.unsplash.com/photo-1721677337543-37b07e7e28b5?fm=jpg&q=80&w=1200&auto=format&fit=crop',
            'https://images.unsplash.com/photo-1680642915019-fdf790dce634?fm=jpg&q=80&w=1200&auto=format&fit=crop',
            'https://plus.unsplash.com/premium_photo-1661775249446-c56b418d009e?fm=jpg&q=80&w=1200&auto=format&fit=crop',
            'https://images.unsplash.com/photo-1687213280116-234f93b15b44?fm=jpg&q=80&w=1200&auto=format&fit=crop'
        ];
        const fallbackImagePath = fallbackImagePaths[(Number(venue.id) || 0) % fallbackImagePaths.length];
        const detailsUrl = `venue-details.html?id=${venue.id}`;

        return `
            <div class="col-md-4 col-lg-4 mb-4">
                <div class="card venue-card h-100 ${featured ? 'featured-venue-card' : ''}">
                    <a href="${detailsUrl}" class="venue-image-link position-relative">
                        ${featured ? '<span class="featured-venue-badge"><i class="bi bi-star-fill"></i> Featured</span>' : ''}
                        <img src="${imagePath || fallbackImagePath}" data-fallback-src="${fallbackImagePath}" class="card-img-top" alt="${venue.name}">
                    </a>
                    <div class="card-body">
                        <div class="d-flex justify-content-between align-items-start gap-2">
                            <h5 class="card-title">${venue.name}</h5>
                            <button type="button" class="save-venue-btn" data-action="toggle-saved-venue" data-venue-id="${venue.id}" aria-label="Save ${venue.name}"><i class="bi bi-heart"></i></button>
                        </div>
                        <p class="card-text text-muted"><i class="bi bi-geo-alt"></i> ${venue.city}, ${venue.province}</p>
                        <p class="card-text"><i class="bi bi-people"></i> Capacity: ${venue.capacity}</p>
                        <p class="venue-price">From ${formatCurrency(venue.basePricePerDay)}<small class="text-muted">/day</small></p>
                        <div class="venue-amenities">
                            ${venueAmenityChips(venue, 5)}
                        </div>
                    </div>
                    <div class="card-footer bg-white border-top-0">
                        <a class="btn btn-outline-primary w-100" href="${detailsUrl}">View Details</a>
                    </div>
                </div>
            </div>
        `;
    }

    function eventTypeName(value) {
        const names = ['Other', 'Wedding', 'Corporate', 'Birthday', 'Conference', 'Exhibition', 'Concert', 'Private Party', 'Workshop', 'Seminar'];
        let normalized = value;
        if (typeof value === 'object' && value !== null) {
            normalized = value.id ?? value.value ?? value.name ?? '';
        }
        if (typeof normalized === 'number') return names[normalized] || 'Event';
        const str = String(normalized || '');
        return str.replace(/([a-z])([A-Z])/g, '$1 $2');
    }

    function eventTypeIcon(name) {
        const key = String(name || '').toLowerCase().replace(/\s+/g, '');
        const icons = {
            wedding: 'bi-heart',
            corporate: 'bi-briefcase',
            birthday: 'bi-gift',
            conference: 'bi-easel',
            exhibition: 'bi-shop',
            concert: 'bi-music-note-beamed',
            privateparty: 'bi-balloon',
            workshop: 'bi-tools',
            seminar: 'bi-journal-text',
            other: 'bi-calendar-event'
        };
        return icons[key] || 'bi-calendar-event';
    }

    function eventTypeChip(value) {
        const name = eventTypeName(value);
        return `<span class="venue-amenity"><i class="bi ${eventTypeIcon(name)}"></i> ${name}</span>`;
    }

    function amenityChip(amenity) {
        const name = amenity?.name || amenity || '';
        const iconMap = {
            parking: 'bi-p-circle', wifi: 'bi-wifi', catering: 'bi-cup-hot', 'av system': 'bi-display',
            security: 'bi-shield-check', stage: 'bi-music-note-beamed', 'outdoor space': 'bi-tree',
            bar: 'bi-cup-straw', decor: 'bi-palette', 'air conditioning': 'bi-snow'
        };
        const icon = amenity?.isCustom ? 'bi-check-circle-fill' : (iconMap[name.toLowerCase()] || 'bi-check-circle-fill');
        return `<span class="venue-amenity"><i class="bi ${icon} text-primary"></i> ${name}</span>`;
    }

    function venueAmenityChips(venue, limit = 5) {
        const amenities = [
            ...(venue.amenities || []),
            ...(venue.customAmenities || []).map(name => ({ name, isCustom: true }))
        ];
        return amenities.slice(0, limit).map(amenityChip).join('');
    }

    // Populates the event type filter from the API
    async function loadEventTypes() {
        const select = document.getElementById('eventType');
        if (!select || typeof VenueApi === 'undefined') return;
        try {
            const response = await VenueApi.getEventTypes();
            const types = response.data || response;
            types.forEach(type => {
                const option = document.createElement('option');
                option.value = type.value;
                option.textContent = type.name.replace(/([a-z])([A-Z])/g, '$1 $2');
                select.appendChild(option);
            });
        } catch (error) {
            console.warn('Unable to load event types', error);
        }
    }

    // Syncs the save/unsaved heart state for all visible venue cards
    async function refreshSavedVenueButtons() {
        const buttons = document.querySelectorAll('.save-venue-btn');
        if (!buttons.length || typeof SavedVenueApi === 'undefined' || !isAuthenticated()) return;
        try {
            const response = await SavedVenueApi.getAll();
            const savedIds = new Set((response || []).map(saved => saved.venueId));
            buttons.forEach(button => {
                const saved = savedIds.has(Number(button.dataset.venueId));
                button.classList.toggle('saved', saved);
                button.innerHTML = `<i class="bi ${saved ? 'bi-heart-fill' : 'bi-heart'}"></i>`;
            });
        } catch (error) {
            console.warn('Unable to load saved venues', error);
        }
    }

    async function loadFeaturedVenues(mode) {
        const grid = document.getElementById('featuredVenuesGrid');
        if (!grid) return;

        try {
            const response = await VenueApi.getAll(1, 100, false, 'newest');
            const responseVenues = response?.success ? (Array.isArray(response.data) ? response.data : response.data?.data || []) : [];
            const venues = responseVenues.filter(venue => venue.isFeatured);
            if (!venues.length) {
                document.getElementById('featuredVenuesSection')?.classList.add('d-none');
                return;
            }
            grid.innerHTML = venues.map(venue => createVenueCard(venue, mode, true)).join('');
            refreshSavedVenueButtons();
        } catch (error) {
            document.getElementById('featuredVenuesSection')?.classList.add('d-none');
            console.warn('Unable to load featured venues', error);
        }
    }

    // Renders the paginated, filterable venue list with map and list views
    async function renderVenueList(mode) {
        if (mode === 'portal' && typeof requireAuth === 'function' && !requireAuth()) {
            return;
        }

        const grid = document.getElementById('venuesGrid');
        const resultsCount = document.getElementById('resultsCount');
        const pagination = document.getElementById('pagination');
        const filterForm = document.getElementById('filterForm');

        if (!grid) {
            return;
        }

        let currentPage = 1;
        const pageSize = 9;
        let map = null;
        let markerLayer = null;
        let currentVenues = [];
        let hasLoadedVenues = false;
        let loadRequestId = 0;

        function renderVenueMap(venues) {
            const mapElement = document.getElementById('venueMap');
            if (!mapElement || typeof L === 'undefined') return;

            if (!map) {
                map = L.map(mapElement).setView([-30.5595, 22.9375], 5);
                L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                    attribution: '&copy; OpenStreetMap contributors'
                }).addTo(map);
                markerLayer = L.layerGroup().addTo(map);
            }

            markerLayer.clearLayers();
            const locatedVenues = venues.filter(venue => Number(venue.latitude) && Number(venue.longitude));
            const bounds = [];

            locatedVenues.forEach(venue => {
                const position = [Number(venue.latitude), Number(venue.longitude)];
                bounds.push(position);
                L.marker(position).bindPopup(`
                    <strong>${venue.name}</strong><br>
                    ${venue.city}, ${venue.province}<br>
                    From ${formatCurrency(venue.basePricePerDay)} / day<br>
                    <a href="venue-details.html?id=${venue.id}">View details</a>
                `).addTo(markerLayer);
            });

            if (bounds.length) map.fitBounds(bounds, { padding: [24, 24] });
            map.invalidateSize();
        }

        function setView(view) {
            const grid = document.getElementById('venuesGrid');
            const mapElement = document.getElementById('venueMap');
            const listButton = document.getElementById('listViewToggle');
            const mapButton = document.getElementById('mapViewToggle');
            const showingMap = view === 'map';
            grid?.classList.toggle('d-none', showingMap);
            mapElement?.classList.toggle('d-none', !showingMap);
            listButton?.classList.toggle('active', !showingMap);
            mapButton?.classList.toggle('active', showingMap);
            if (showingMap) renderVenueMap(currentVenues);
        }

        document.getElementById('listViewToggle')?.addEventListener('click', () => setView('list'));
        document.getElementById('mapViewToggle')?.addEventListener('click', () => setView('map'));
        let filterDebounceTimer;

        document.getElementById('sortBy')?.addEventListener('change', () => applyFilters());
        document.getElementById('clearFilters')?.addEventListener('click', () => {
            clearTimeout(filterDebounceTimer);
            filterForm?.reset();
            const sortBy = document.getElementById('sortBy');
            if (sortBy) sortBy.value = '';
            applyFilters();
        });

        async function loadVenues(page, filters) {
            const requestId = ++loadRequestId;
            if (!hasLoadedVenues) {
                showLoading('venuesGrid');
            }
            grid.setAttribute('aria-busy', 'true');

            try {
                let venues = [];
                let totalCount = 0;
                let apiResponseReceived = false;

                if (typeof VenueApi !== 'undefined' && VenueApi) {
                    let response;
                    if (filters && Object.keys(filters).length > 0) {
                        response = await VenueApi.search(filters, page, pageSize);
                    } else {
                        response = await VenueApi.getAll(page, pageSize, false, document.getElementById('sortBy')?.value || '');
                    }

                    if (requestId !== loadRequestId) return;

                    if (response && response.success && response.data) {
                        apiResponseReceived = true;
                        const responseData = Array.isArray(response.data) ? response.data : response.data.data || [];
                        venues = responseData;
                        totalCount = response.totalCount || responseData.length;

                        if (!Array.isArray(response.data) && pagination) {
                            renderPagination(response.currentPage || 1, response.totalPages || 1);
                        } else if (pagination) {
                            pagination.innerHTML = '';
                        }
                    }
                }

                if (requestId !== loadRequestId) return;

                if (!venues.length && !apiResponseReceived) {
                    venues = PLACEHOLDER_VENUES;
                    totalCount = venues.length;
                    if (pagination) {
                        pagination.innerHTML = '';
                    }
                }

                currentVenues = venues;
                hasLoadedVenues = true;
                grid.innerHTML = venues.length
                    ? venues.map(v => createVenueCard(v, mode)).join('')
                    : '<div class="col-12"><div class="alert alert-light border text-center mb-0">No venues match your filters.</div></div>';
                refreshSavedVenueButtons();
                if (document.getElementById('mapViewToggle')?.classList.contains('active')) renderVenueMap(venues);
                if (resultsCount) {
                    resultsCount.textContent = `Showing ${venues.length} of ${totalCount} venues`;
                }
                grid.removeAttribute('aria-busy');
            } catch (err) {
                if (requestId !== loadRequestId) return;
                const fallback = PLACEHOLDER_VENUES;
                currentVenues = fallback;
                hasLoadedVenues = true;
                grid.innerHTML = fallback.map(v => createVenueCard(v, mode)).join('');
                refreshSavedVenueButtons();
                if (resultsCount) {
                    resultsCount.textContent = `Showing ${fallback.length} placeholder venues`;
                }
                if (pagination) {
                    pagination.innerHTML = '';
                }
                grid.removeAttribute('aria-busy');
            }
        }

        function renderPagination(page, totalPages) {
            if (!pagination) {
                return;
            }

            let html = '';
            html += `<li class="page-item ${page === 1 ? 'disabled' : ''}"><a class="page-link" href="#" data-page="${page - 1}">Previous</a></li>`;
            for (let i = 1; i <= totalPages; i += 1) {
                html += `<li class="page-item ${i === page ? 'active' : ''}"><a class="page-link" href="#" data-page="${i}">${i}</a></li>`;
            }
            html += `<li class="page-item ${page === totalPages ? 'disabled' : ''}"><a class="page-link" href="#" data-page="${page + 1}">Next</a></li>`;
            pagination.innerHTML = html;

            pagination.querySelectorAll('a[data-page]').forEach(link => {
                link.addEventListener('click', function (e) {
                    e.preventDefault();
                    const targetPage = parseInt(this.getAttribute('data-page'), 10);
                    if (!Number.isNaN(targetPage) && targetPage > 0) {
                        currentPage = targetPage;
                        loadVenues(currentPage, {});
                    }
                });
            });
        }

        function applyFilters() {
            const filters = {
                searchTerm: document.getElementById('searchTerm')?.value || '',
                capacity: document.getElementById('capacity')?.value ? parseInt(document.getElementById('capacity').value, 10) : null,
                minPrice: document.getElementById('minPrice')?.value ? parseInt(document.getElementById('minPrice').value, 10) : null,
                maxPrice: document.getElementById('maxPrice')?.value ? parseInt(document.getElementById('maxPrice').value, 10) : null,
                eventType: document.getElementById('eventType')?.value ? parseInt(document.getElementById('eventType').value, 10) : null,
                sortBy: document.getElementById('sortBy')?.value || null
            };

            Object.keys(filters).forEach(key => {
                if (filters[key] === null || filters[key] === '') {
                    delete filters[key];
                }
            });

            currentPage = 1;
            loadVenues(currentPage, filters);
        }

        if (filterForm) {
            filterForm.addEventListener('submit', function (e) {
                e.preventDefault();
                applyFilters();
            });

            filterForm.querySelectorAll('input, select').forEach(control => {
                control.addEventListener(control.tagName === 'SELECT' ? 'change' : 'input', () => {
                    clearTimeout(filterDebounceTimer);
                    filterDebounceTimer = setTimeout(applyFilters, 250);
                });
            });
        }

        loadEventTypes();
        loadFeaturedVenues(mode);
        loadVenues(currentPage, {});
    }

    window.initVenueSlideshow = function initVenueSlideshow() {
        const uploadedImages = (window.currentVenue?.photos || []).map(photo => window.resolveVenueImage(photo.url)).filter(Boolean);
        const venueImages = uploadedImages.length ? uploadedImages : [
            window.resolveVenueImage(window.currentVenue?.imageUrl),
            'https://images.unsplash.com/photo-1519167758481-83f550bb49b3?w=800&h=600&fit=crop',
            'https://images.unsplash.com/photo-1464366400600-7168b8af9bc3?w=800&h=600&fit=crop'
        ].filter(Boolean);
        const mainImage = document.getElementById('mainImage');
        const thumbnailContainer = document.querySelector('.thumbnail-container');
        const galleryCount = document.getElementById('galleryCount');
        const previousButton = document.querySelector('.gallery-prev');
        const nextButton = document.querySelector('.gallery-next');
        if (!mainImage || !venueImages.length) return;

        let currentImageIndex = 0;
        let touchStartX = 0;

        function setCurrentImage(index) {
            currentImageIndex = (index + venueImages.length) % venueImages.length;
            mainImage.src = venueImages[currentImageIndex];
            mainImage.alt = `${window.currentVenue?.name || 'Venue'} image ${currentImageIndex + 1}`;
            galleryCount && (galleryCount.textContent = `${venueImages.length} photos`);
            thumbnailContainer?.querySelectorAll('.thumbnail').forEach((thumb, i) => {
                thumb.classList.toggle('active', i === currentImageIndex);
            });
        }

        venueImages.forEach((imageSrc, index) => {
            const thumbnail = document.createElement('img');
            thumbnail.src = imageSrc;
            thumbnail.alt = `View venue image ${index + 1}`;
            thumbnail.className = `thumbnail ${index === 0 ? 'active' : ''}`;
            thumbnail.tabIndex = 0;
            thumbnail.addEventListener('click', () => setCurrentImage(index));
            thumbnail.addEventListener('keydown', event => {
                if (event.key === 'Enter' || event.key === ' ') {
                    event.preventDefault();
                    setCurrentImage(index);
                }
            });
            thumbnailContainer?.appendChild(thumbnail);
        });

        previousButton?.addEventListener('click', () => setCurrentImage(currentImageIndex - 1));
        nextButton?.addEventListener('click', () => setCurrentImage(currentImageIndex + 1));
        mainImage.addEventListener('keydown', event => {
            if (event.key === 'ArrowLeft') setCurrentImage(currentImageIndex - 1);
            if (event.key === 'ArrowRight') setCurrentImage(currentImageIndex + 1);
        });
        mainImage.addEventListener('touchstart', event => { touchStartX = event.changedTouches[0].screenX; }, { passive: true });
        mainImage.addEventListener('touchend', event => {
            const distance = event.changedTouches[0].screenX - touchStartX;
            if (Math.abs(distance) > 40) setCurrentImage(currentImageIndex + (distance < 0 ? 1 : -1));
        }, { passive: true });
        setCurrentImage(0);
    };

    // Loads and renders a single venue's detail page
    async function renderVenueDetails(mode) {
        if (mode === 'portal' && typeof requireAuth === 'function' && !requireAuth()) {
            return;
        }

        const params = new URLSearchParams(window.location.search);
        const id = params.get('id');

        const venueName = document.getElementById('venueName');
        const venueInfo = document.getElementById('venueInfo');
        const venueCapacity = document.getElementById('venueCapacity');
        const venuePrice = document.getElementById('venuePrice');
        const venueLocation = document.getElementById('venueLocation');
        const venueHeadingLocation = document.getElementById('venueHeadingLocation');
        const venueAddress = document.getElementById('venueAddress');
        const venueMapLink = document.getElementById('venueMapLink');
        const requestBudget = document.getElementById('requestBudget');
        const venueIdInput = document.getElementById('venueId');

        let venue = null;

        if (id) {
            try {
                if (typeof VenueApi !== 'undefined' && VenueApi.getById) {
                    const response = await VenueApi.getById(id);
                    if (response && response.success && response.data) {
                        venue = response.data;
                    }
                }
            } catch (err) {
                console.error('Error fetching venue details:', err);
            }
        }

        if (!venue) {
            venue = getVenueById(id) || PLACEHOLDER_VENUES[0];
        }

        window.currentVenue = venue;
        const eventTypesElement = document.getElementById('venueEventTypes');
        if (eventTypesElement) {
            eventTypesElement.innerHTML = (venue.eventTypes || []).map(eventTypeChip).join('');
        }
        const amenitiesElement = document.getElementById('venueAmenities');
        if (amenitiesElement) {
            amenitiesElement.innerHTML = venueAmenityChips(venue, 50);
        }
        const descriptionElement = document.getElementById('venueDescription');
        if (descriptionElement) {
            descriptionElement.textContent = venue.description || 'A beautiful, flexible setting for your most important moments.';
        }
        const detailMap = document.getElementById('venueDetailMap');
        if (detailMap && typeof L !== 'undefined' && Number(venue.latitude) && Number(venue.longitude)) {
            const detailMapInstance = L.map(detailMap).setView([Number(venue.latitude), Number(venue.longitude)], 14);
            L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', { attribution: '&copy; OpenStreetMap contributors' }).addTo(detailMapInstance);
            L.marker([Number(venue.latitude), Number(venue.longitude)]).addTo(detailMapInstance).bindPopup(venue.name).openPopup();
        }
        if (typeof refreshDetailSavedButton === 'function') refreshDetailSavedButton(venue.id);

        if (venueIdInput) {
            venueIdInput.value = venue.id;
        }

        if (venueName) {
            venueName.textContent = venue.name;
        }
        if (venueCapacity) {
            venueCapacity.textContent = `${venue.capacity} guests`;
        }
        if (venuePrice) {
            venuePrice.textContent = `${formatCurrency(venue.basePricePerDay)} / day`;
        }
        const locationText = [venue.city, venue.province].filter(Boolean).join(', ') || 'Location unavailable';
        const addressText = [venue.address, venue.city, venue.province, venue.postalCode].filter(Boolean).join(', ') || locationText;
        if (venueLocation) {
            venueLocation.textContent = locationText;
        }
        if (venueHeadingLocation) {
            venueHeadingLocation.textContent = locationText;
        }
        if (venueAddress) {
            venueAddress.innerHTML = `<i class="bi bi-geo-alt"></i> ${addressText.replace(/</g, '&lt;').replace(/>/g, '&gt;')}`;
        }
        if (venueMapLink && Number(venue.latitude) && Number(venue.longitude)) {
            venueMapLink.href = `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(`${venue.latitude},${venue.longitude}`)}`;
        } else if (venueMapLink) {
            venueMapLink.classList.add('d-none');
        }
        if (requestBudget) {
            requestBudget.placeholder = String(venue.basePricePerDay);
        }
    }

    // Entry point: mounts nav and renders either the list or detail page
    async function init(options) {
        const mode = options?.mode === 'portal' ? 'portal' : 'public';
        const page = options?.page === 'detail' ? 'detail' : 'list';

        mountNav(mode);
        if (page === 'detail') {
            await renderVenueDetails(mode);
        } else {
            renderVenueList(mode);
        }
    }

    window.VenuesPages = {
        init
    };
})();
