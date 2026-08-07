// Staff venue management: handles venue creation and editing including amenities, photos, and map location.
let venues = [];
        let venueModal = null;
        let venueMap = null;
        let venueMarker = null;
        let selectedGalleryFiles = [];
        let existingPhotos = [];
        let geocodeTimer = null;

        function resolveVenueImage(url) {
            if (!url || !url.startsWith('/')) return url;
            return `${API_BASE_URL.replace(/\/api\/?$/, '')}${url}`;
        }

        function addCustomAmenity(value = '') {
            const wrapper = document.getElementById('customAmenities');
            const row = document.createElement('div');
            row.className = 'input-group mb-2 custom-amenity-row';
            row.innerHTML = `<span class="input-group-text text-primary"><i class="bi bi-check-circle-fill"></i></span><input type="text" class="form-control custom-amenity-input" maxlength="100" placeholder="Enter another venue feature" value="${String(value).replace(/"/g, '&quot;')}"><button type="button" class="btn btn-outline-danger" data-action="remove-custom-amenity"><i class="bi bi-trash"></i></button>`;
            wrapper.appendChild(row);
        }

        function getCustomAmenities() {
            return Array.from(document.querySelectorAll('.custom-amenity-input')).map(input => input.value.trim()).filter(Boolean);
        }

        function setAmenitySelections(amenityIds = [], customAmenities = []) {
            document.querySelectorAll('.amenity-checkbox').forEach(input => input.checked = amenityIds.includes(Number(input.value)));
            document.getElementById('customAmenities').innerHTML = '';
            customAmenities.forEach(addCustomAmenity);
        }

        function addCustomServiceOption(value = '') {
            const wrapper = document.getElementById('customServiceOptions');
            const row = document.createElement('div');
            row.className = 'input-group mb-2 custom-service-option-row';
            row.innerHTML = `<span class="input-group-text text-primary"><i class="bi bi-briefcase-fill"></i></span><input type="text" class="form-control custom-service-option-input" maxlength="100" placeholder="Enter another service clients can request" value="${String(value).replace(/"/g, '&quot;')}"><button type="button" class="btn btn-outline-danger" data-action="remove-custom-service"><i class="bi bi-trash"></i></button>`;
            wrapper.appendChild(row);
        }

        function getSupportedServices() {
            const defaults = Array.from(document.querySelectorAll('.service-option-checkbox:checked')).map(input => input.value.trim());
            const custom = Array.from(document.querySelectorAll('.custom-service-option-input')).map(input => input.value.trim()).filter(Boolean);
            return Array.from(new Set([...defaults, ...custom]));
        }

        function setServiceSelections(supportedServices = []) {
            const normalized = new Set((supportedServices || []).map(service => String(service).trim().toLowerCase()));
            document.querySelectorAll('.service-option-checkbox').forEach(input => {
                input.checked = normalized.has(input.value.trim().toLowerCase());
            });

            const predefined = new Set(Array.from(document.querySelectorAll('.service-option-checkbox')).map(input => input.value.trim().toLowerCase()));
            const custom = (supportedServices || []).filter(service => !predefined.has(String(service).trim().toLowerCase()));
            document.getElementById('customServiceOptions').innerHTML = '';
            custom.forEach(addCustomServiceOption);
        }

        document.addEventListener('DOMContentLoaded', function () {
            venueModal = new bootstrap.Modal(document.getElementById('venueModal'));
            document.querySelectorAll('#venueFilter .filter-pill').forEach(button => button.addEventListener('click', () => {
                document.querySelectorAll('#venueFilter .filter-pill').forEach(item => item.classList.remove('active'));
                button.classList.add('active');
                filterVenues();
            }));
            document.getElementById('searchInput').addEventListener('input', filterVenues);
            document.getElementById('venueModal').addEventListener('shown.bs.modal', function () {
                const lat = parseFloat(document.getElementById('venueLatitude').value);
                const lng = parseFloat(document.getElementById('venueLongitude').value);
                if (Number.isFinite(lat) && Number.isFinite(lng)) {
                    initialiseVenueMap(lat, lng);
                } else {
                    initialiseVenueMap();
                }
            });
            loadVenues();
            document.getElementById('venueImageFiles').addEventListener('change', handleImageSelection);
            document.getElementById('venueLatitude').addEventListener('change', syncMapFromInputs);
            document.getElementById('venueLongitude').addEventListener('change', syncMapFromInputs);
            ['venueAddress', 'venueCity', 'venueProvince', 'venuePostalCode'].forEach(id => document.getElementById(id).addEventListener('blur', scheduleForwardGeocode));
        });

        function initialiseVenueMap(latitude = -30.5595, longitude = 22.9375) {
            if (typeof L === 'undefined') return;
            if (!venueMap) {
                venueMap = L.map('venueLocationMap').setView([latitude, longitude], latitude === -30.5595 ? 5 : 15);
                L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', { attribution: '&copy; OpenStreetMap contributors' }).addTo(venueMap);
                venueMap.on('click', event => setVenueLocation(event.latlng.lat, event.latlng.lng));
            } else {
                venueMap.setView([latitude, longitude], latitude === -30.5595 ? 5 : 15);
            }
            setVenueLocation(latitude, longitude, false);
            setTimeout(() => venueMap.invalidateSize(), 100);
        }

        function setGeocodeStatus(message, isError = false) {
            const status = document.getElementById('geocodeStatus');
            if (status) {
                status.textContent = message;
                status.className = `small mt-1 ${isError ? 'text-danger' : 'text-muted'}`;
            }
        }

        async function reverseGeocode(latitude, longitude) {
            setGeocodeStatus('Finding address…');
            try {
                const response = await fetch(`https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat=${encodeURIComponent(latitude)}&lon=${encodeURIComponent(longitude)}&zoom=18&addressdetails=1`, { headers: { 'Accept-Language': 'en' } });
                if (!response.ok) throw new Error('Geocoding request failed');
                const result = await response.json();
                const address = result.address || {};
                const street = [address.house_number, address.road].filter(Boolean).join(' ');
                if (street) document.getElementById('venueAddress').value = street;
                document.getElementById('venueCity').value = address.city || address.town || address.village || address.municipality || document.getElementById('venueCity').value;
                document.getElementById('venueProvince').value = address.state || address.region || document.getElementById('venueProvince').value;
                document.getElementById('venuePostalCode').value = address.postcode || document.getElementById('venuePostalCode').value;
                setGeocodeStatus(result.display_name ? `Location found: ${result.display_name}` : 'Location selected.');
            } catch (error) {
                console.warn('Reverse geocoding failed:', error);
                setGeocodeStatus('Location selected. Address lookup was unavailable.', true);
            }
        }

        function scheduleForwardGeocode() {
            clearTimeout(geocodeTimer);
            geocodeTimer = setTimeout(forwardGeocode, 500);
        }

        async function forwardGeocode() {
            const query = [
                document.getElementById('venueAddress').value,
                document.getElementById('venueCity').value,
                document.getElementById('venueProvince').value,
                document.getElementById('venuePostalCode').value
            ].filter(Boolean).join(', ');
            if (!query) return;
            setGeocodeStatus('Finding map location…');
            try {
                const response = await fetch(`https://nominatim.openstreetmap.org/search?format=jsonv2&limit=1&addressdetails=1&q=${encodeURIComponent(query)}`, { headers: { 'Accept-Language': 'en' } });
                if (!response.ok) throw new Error('Geocoding request failed');
                const results = await response.json();
                if (!results.length) {
                    setGeocodeStatus('Address could not be located. You can click the map instead.', true);
                    return;
                }
                setVenueLocation(Number(results[0].lat), Number(results[0].lon));
                setGeocodeStatus(`Map location found: ${results[0].display_name}`);
            } catch (error) {
                console.warn('Forward geocoding failed:', error);
                setGeocodeStatus('Address lookup was unavailable. You can click the map instead.', true);
            }
        }

        function setVenueLocation(latitude, longitude, pan = true) {
            document.getElementById('venueLatitude').value = Number(latitude).toFixed(6);
            document.getElementById('venueLongitude').value = Number(longitude).toFixed(6);
            if (!venueMap) return;
            if (!venueMarker) venueMarker = L.marker([latitude, longitude], { draggable: true }).addTo(venueMap);
            else venueMarker.setLatLng([latitude, longitude]);
            venueMarker.off('dragend').on('dragend', event => {
                const position = event.target.getLatLng();
                setVenueLocation(position.lat, position.lng, true);
            });
            if (pan) {
                venueMap.panTo([latitude, longitude]);
                reverseGeocode(latitude, longitude);
            }
        }

        function syncMapFromInputs() {
            const latitude = Number(document.getElementById('venueLatitude').value);
            const longitude = Number(document.getElementById('venueLongitude').value);
            if (Number.isFinite(latitude) && Number.isFinite(longitude)) setVenueLocation(latitude, longitude);
        }

        function handleImageSelection(event) {
            selectedGalleryFiles = Array.from(event.target.files || []);
            const preview = document.getElementById('newImagePreview');
            preview.innerHTML = '';
            selectedGalleryFiles.forEach((file, index) => {
                const url = URL.createObjectURL(file);
                preview.insertAdjacentHTML('beforeend', `<div class="col-6 col-md-3"><img src="${url}" class="img-fluid rounded" style="height:100px;width:100%;object-fit:cover"><div class="form-check"><input class="form-check-input new-primary-photo" type="radio" name="newPrimaryPhoto" value="${index}" ${index === 0 ? 'checked' : ''}><label class="form-check-label small">Primary</label></div></div>`);
            });
        }

        function renderExistingPhotos() {
            const gallery = document.getElementById('venuePhotoGallery');
            gallery.innerHTML = existingPhotos.map(photo => `<div class="col-6 col-md-3 position-relative"><img src="${resolveVenueImage(photo.url)}" class="img-fluid rounded" style="height:100px;width:100%;object-fit:cover"><div class="small mt-1"><label><input type="radio" name="existingPrimaryPhoto" ${photo.isPrimary ? 'checked' : ''} data-action="set-existing-primary" data-photo-id="${photo.id}"> Primary</label><button type="button" class="btn btn-sm btn-outline-danger ms-1" data-action="remove-existing-photo" data-photo-id="${photo.id}"><i class="bi bi-trash"></i></button></div></div>`).join('');
        }

        async function setExistingPrimary(photoId) {
            await VenueApi.setPrimaryPhoto(document.getElementById('venueId').value, photoId);
            existingPhotos = existingPhotos.map(photo => ({ ...photo, isPrimary: photo.id === photoId }));
            renderExistingPhotos();
        }

        async function removeExistingPhoto(photoId) {
            if (!confirm('Delete this venue image?')) return;
            await VenueApi.deletePhoto(document.getElementById('venueId').value, photoId);
            existingPhotos = existingPhotos.filter(photo => photo.id !== photoId);
            renderExistingPhotos();
        }

        async function loadVenues() {
            try {
                const response = await VenueApi.getAll(1, 100, true);
                if (response.success && response.data) {
                    venues = response.data;
                    filterVenues();
                } else {
                    // Fallback to placeholder data if API fails
                    venues = getPlaceholderVenues();
                    filterVenues();
                }
            } catch (error) {
                console.error('Error loading venues:', error);
                // Use placeholder data on error
                venues = getPlaceholderVenues();
                renderVenues(venues);
            }
        }

        function getPlaceholderVenues() {
            return [
                {
                    id: 101,
                    name: 'Lakeside Pavilion',
                    city: 'Cape Town',
                    province: 'Western Cape',
                    capacity: 220,
                    basePricePerDay: 18000,
                    isActive: true,
                    isFeatured: false
                },
                {
                    id: 102,
                    name: 'Summit Conference Hall',
                    city: 'Johannesburg',
                    province: 'Gauteng',
                    capacity: 400,
                    basePricePerDay: 32000,
                    isActive: true,
                    isFeatured: false
                },
                {
                    id: 103,
                    name: 'Garden Terrace Venue',
                    city: 'Durban',
                    province: 'KwaZulu-Natal',
                    capacity: 140,
                    basePricePerDay: 12500,
                    isActive: true,
                    isFeatured: false
                }
            ];
        }

        function renderVenues(venuesToRender) {
            const tbody = document.getElementById('venuesTableBody');

            if (venuesToRender.length === 0) {
                tbody.innerHTML = '<tr><td colspan="6" class="text-center">No venues found</td></tr>';
                return;
            }

            tbody.innerHTML = venuesToRender.map(venue => `
                <tr>
                    <td>
                        <div class="d-flex align-items-center">
                            <div class="venue-thumbnail me-3">
                                <img src="${resolveVenueImage(venue.thumbnailUrl || venue.imageUrl || venue.photos?.find(photo => photo.isPrimary)?.url) || 'https://via.placeholder.com/50'}" 
                                     alt="${venue.name}" 
                                     class="rounded" 
                                     style="width: 50px; height: 50px; object-fit: cover;">
                            </div>
                            <div>
                                <strong>${venue.name}</strong>
                                ${venue.isFeatured ? '<span class="badge bg-primary ms-2"><i class="bi bi-star-fill"></i> Featured</span>' : ''}
                                <div class="text-muted small">${venue.description ? venue.description.substring(0, 50) + '...' : ''}</div>
                            </div>
                        </div>
                    </td>
                    <td>${venue.city}, ${venue.province}</td>
                    <td>${venue.capacity}</td>
                    <td>R ${venue.basePricePerDay.toLocaleString()}</td>
                    <td>
                        <span class="badge ${venue.isActive ? 'bg-success' : 'bg-secondary'}">
                            ${venue.isActive ? 'Active' : 'Inactive'}
                        </span>
                    </td>
                    <td>
                        <button class="btn btn-sm btn-outline-primary me-1" data-action="edit-venue" data-venue-id="${venue.id}">
                            <i class="bi bi-pencil"></i>
                        </button>
                        <button class="btn btn-sm ${venue.isActive ? 'btn-outline-warning' : 'btn-outline-success'}" data-action="toggle-venue-status" data-venue-id="${venue.id}">
                            <i class="bi ${venue.isActive ? 'bi-eye-slash' : 'bi-eye'}"></i>
                        </button>
                    </td>
                </tr>
            `).join('');
        }

        function filterVenues() {
            const searchTerm = document.getElementById('searchInput').value.toLowerCase();
            const filter = document.querySelector('#venueFilter .filter-pill.active')?.dataset.filter || 'active';

            const filtered = venues.filter(venue => {
                const matchesSearch = venue.name.toLowerCase().includes(searchTerm) ||
                    venue.city.toLowerCase().includes(searchTerm);
                const matchesFilter = filter === 'featured'
                    ? venue.isFeatured
                    : filter === 'inactive' ? !venue.isActive : venue.isActive;
                return matchesSearch && matchesFilter;
            });

            renderVenues(filtered);
        }

        function showCreateModal() {
            document.getElementById('venueModalTitle').textContent = 'Add New Venue';
            document.getElementById('venueForm').reset();
            document.getElementById('venueId').value = '';
            document.getElementById('venueStatus').value = 'true';
            document.getElementById('venueFeatured').checked = false;
            document.querySelectorAll('#venueEventTypes input[type="checkbox"]').forEach(checkbox => checkbox.checked = false);
            setAmenitySelections();
            setServiceSelections();
            document.getElementById('venueImageFiles').value = '';
            document.getElementById('newImagePreview').innerHTML = '';
            document.getElementById('venuePhotoGallery').innerHTML = '';
            selectedGalleryFiles = [];
            existingPhotos = [];
            venueModal.show();
        }

        function editVenue(id) {
            const venue = venues.find(v => v.id === id);
            if (!venue) return;

            document.getElementById('venueModalTitle').textContent = 'Edit Venue';
            document.getElementById('venueId').value = venue.id;
            document.getElementById('venueName').value = venue.name;
            document.getElementById('venueCity').value = venue.city;
            document.getElementById('venueProvince').value = venue.province;
            document.getElementById('venuePostalCode').value = venue.postalCode || '';
            document.getElementById('venueAddress').value = venue.address || '';
            document.getElementById('venueDescription').value = venue.description || '';
            document.getElementById('venueCapacity').value = venue.capacity;
            document.getElementById('venuePrice').value = venue.basePricePerDay;
            document.getElementById('venueStatus').value = venue.isActive.toString();
            document.getElementById('venueFeatured').checked = venue.isFeatured === true;
            document.querySelectorAll('#venueEventTypes input[type="checkbox"]').forEach(checkbox => checkbox.checked = (venue.eventTypes || []).includes(Number(checkbox.value)));
            setAmenitySelections((venue.amenities || []).map(amenity => amenity.id), venue.customAmenities || []);
            setServiceSelections(venue.supportedServices || []);
            document.getElementById('venueLatitude').value = venue.latitude || '';
            document.getElementById('venueLongitude').value = venue.longitude || '';
            document.getElementById('venueImageFiles').value = '';
            selectedGalleryFiles = [];
            document.getElementById('newImagePreview').innerHTML = '';
            existingPhotos = venue.photos || [];
            renderExistingPhotos();
            venueModal.show();
        }

        let isSavingVenue = false;

        async function saveVenue() {
            if (isSavingVenue) return;
            isSavingVenue = true;
            const saveButton = document.getElementById('saveVenueButton');
            const originalButtonText = saveButton.innerHTML;
            saveButton.disabled = true;
            saveButton.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Saving...';
            const venueId = document.getElementById('venueId').value;
            const venueData = {
                name: document.getElementById('venueName').value,
                city: document.getElementById('venueCity').value,
                province: document.getElementById('venueProvince').value,
                postalCode: document.getElementById('venuePostalCode').value,
                address: document.getElementById('venueAddress').value,
                description: document.getElementById('venueDescription').value,
                capacity: parseInt(document.getElementById('venueCapacity').value),
                basePricePerDay: parseFloat(document.getElementById('venuePrice').value),
                isActive: document.getElementById('venueStatus').value === 'true',
                isFeatured: document.getElementById('venueFeatured').checked,
                latitude: parseFloat(document.getElementById('venueLatitude').value),
                longitude: parseFloat(document.getElementById('venueLongitude').value),
                imageUrl: null,
                thumbnailUrl: null,
                amenityIds: Array.from(document.querySelectorAll('.amenity-checkbox:checked')).map(checkbox => Number(checkbox.value)),
                customAmenities: getCustomAmenities(),
                supportedServices: getSupportedServices(),
                eventTypes: Array.from(document.querySelectorAll('#venueEventTypes input[type="checkbox"]:checked')).map(checkbox => Number(checkbox.value))
            };

            // Basic validation
            if (!venueData.name || !venueData.city || !venueData.province || !venueData.capacity || !venueData.basePricePerDay) {
                SwalUtils.error('Validation Error', 'Please fill in all required fields');
                isSavingVenue = false;
                saveButton.disabled = false;
                saveButton.innerHTML = originalButtonText;
                return;
            }

            if (!Number.isFinite(venueData.latitude) || !Number.isFinite(venueData.longitude)) {
                SwalUtils.error('Validation Error', 'Choose a location on the map.');
                isSavingVenue = false;
                saveButton.disabled = false;
                saveButton.innerHTML = originalButtonText;
                return;
            }

            try {
                const response = venueId ? await VenueApi.update(venueId, venueData) : await VenueApi.create(venueData);
                const savedVenue = response.data || response;
                const savedVenueId = venueId || savedVenue.id;
                if (selectedGalleryFiles.length) {
                    const primaryIndex = Number(document.querySelector('.new-primary-photo:checked')?.value || 0);
                    await VenueApi.uploadPhotos(savedVenueId, selectedGalleryFiles, primaryIndex);
                }
                SwalUtils.success('Success', venueId ? 'Venue updated successfully' : 'Venue created successfully');
                venueModal.hide();
                loadVenues();
            } catch (error) {
                console.error('Error saving venue:', error);
                const errorMessage = error.message || 'Error saving venue. Please try again.';
                SwalUtils.error('Error', errorMessage);
            } finally {
                isSavingVenue = false;
                saveButton.disabled = false;
                saveButton.innerHTML = originalButtonText;
            }
        }

        async function toggleVenueStatus(id) {
            const venue = venues.find(v => v.id === id);
            if (!venue) return;

            const action = venue.isActive ? 'deactivate' : 'activate';

            const result = await SwalUtils.confirm(
                `Are you sure?`,
                `You want to ${action} this venue`,
                `Yes, ${action} it!`
            );

            if (!result.isConfirmed) {
                return;
            }

            try {
                await VenueApi.toggleStatus(id);
                SwalUtils.success('Success', `Venue ${action}d successfully`);
                loadVenues();
            } catch (error) {
                console.error('Error toggling venue status:', error);
                const errorMessage = error.message || 'Error updating venue status. Please try again.';
                SwalUtils.error('Error', errorMessage);
            }
        }

        // Add VenueApi.toggleStatus method if it doesn't exist
        if (typeof VenueApi !== 'undefined' && !VenueApi.toggleStatus) {
            VenueApi.toggleStatus = async function (id) {
                return ApiClient.patch(`/venues/${id}/toggle-status`);
            };
        }
