// Client venue details: shows detailed venue information with slideshow and availability in the client portal.
document.addEventListener('DOMContentLoaded', async function () {
            await VenuesPages.init({ mode: 'portal', page: 'detail' });

            // Initialize venue slideshow
            initVenueSlideshow();

            // Initialize venue calendar
            initVenueCalendar();
        });

        // Venue Slideshow
        function initVenueSlideshow() {
            // Placeholder images - will be replaced with actual venue images from API
            const uploadedImages = (window.currentVenue?.photos || []).map(photo => window.resolveVenueImage(photo.url)).filter(Boolean);
            const venueImages = uploadedImages.length ? uploadedImages : [
                window.resolveVenueImage(window.currentVenue?.imageUrl),
                'https://images.unsplash.com/photo-1519167758481-83f550bb49b3?w=800&h=600&fit=crop',
                'https://images.unsplash.com/photo-1464366400600-7168b8af9bc3?w=800&h=600&fit=crop'
            ].filter(Boolean);

            let currentImageIndex = 0;
            const mainImage = document.getElementById('mainImage');
            const thumbnailContainer = document.querySelector('.thumbnail-container');

            // Set initial main image
            if (mainImage && venueImages.length > 0) {
                mainImage.src = venueImages[0];
            }

            // Create thumbnails
            if (thumbnailContainer) {
                venueImages.forEach((imageSrc, index) => {
                    const thumbnail = document.createElement('img');
                    thumbnail.src = imageSrc;
                    thumbnail.alt = `Venue image ${index + 1}`;
                    thumbnail.className = `thumbnail ${index === 0 ? 'active' : ''}`;
                    thumbnail.addEventListener('click', () => {
                        setCurrentImage(index);
                    });
                    thumbnailContainer.appendChild(thumbnail);
                });
            }

            // Set current image
            function setCurrentImage(index) {
                currentImageIndex = index;
                if (mainImage) {
                    mainImage.style.opacity = '0';
                    setTimeout(() => {
                        mainImage.src = venueImages[index];
                        mainImage.style.opacity = '1';
                    }, 300);
                }

                // Update thumbnail active states
                const thumbnails = thumbnailContainer.querySelectorAll('.thumbnail');
                thumbnails.forEach((thumb, i) => {
                    thumb.classList.toggle('active', i === index);
                });
            }

            // Auto-rotate images every 5 seconds
            setInterval(() => {
                const nextIndex = (currentImageIndex + 1) % venueImages.length;
                setCurrentImage(nextIndex);
            }, 5000);
        }

        // Venue Calendar
        function initVenueCalendar() {
            const calendarContainer = document.getElementById('venueCalendar');
            const startDateInput = document.getElementById('selectedStartDate');
            const startDateValueInput = document.getElementById('selectedStartDateValue');
            const endDateInput = document.getElementById('selectedEndDate');
            const endDateValueInput = document.getElementById('selectedEndDateValue');
            const requestForm = document.getElementById('venueRequestForm');
            const requestStatus = document.getElementById('requestStatus');
            if (!calendarContainer) return;

            let currentDate = new Date();
            let currentMonth = currentDate.getMonth();
            let currentYear = currentDate.getFullYear();
            let startDate = null;
            let endDate = null;

            const today = new Date();
            today.setHours(0, 0, 0, 0);

            const unavailableDates = new Set((window.currentVenue?.availabilities || [])
                .filter(availability => !availability.isAvailable)
                .map(availability => availability.date.slice(0, 10)));

            function renderServiceOptions() {
                const container = document.getElementById('requestedServicesOptions');
                if (!container) return;

                const defaults = ['Catering', 'Staffing & security', 'Setup & cleanup'];
                const configured = Array.isArray(window.currentVenue?.supportedServices) && window.currentVenue.supportedServices.length
                    ? window.currentVenue.supportedServices
                    : defaults;

                container.innerHTML = configured.map((service, index) => {
                    const safeService = String(service).replace(/</g, '&lt;').replace(/>/g, '&gt;');
                    const id = `requestedService${index}`;
                    return `<div class="col-sm-6"><label class="form-check service-option"><input class="form-check-input requested-service-option" type="checkbox" id="${id}" value="${safeService}"><span><strong>${safeService}</strong><small>Optional service for your event request</small></span></label></div>`;
                }).join('');
            }

            function normalizeDate(date) {
                return new Date(date.getFullYear(), date.getMonth(), date.getDate());
            }

            function formatDate(date) {
                return date.toLocaleDateString('en-ZA', {
                    weekday: 'short',
                    day: 'numeric',
                    month: 'long',
                    year: 'numeric'
                });
            }

            function formatDateValue(date) {
                const year = date.getFullYear();
                const month = String(date.getMonth() + 1).padStart(2, '0');
                const day = String(date.getDate()).padStart(2, '0');
                return `${year}-${month}-${day}`;
            }

            function clearDates() {
                startDate = null;
                endDate = null;
                updateDateInputs();
                if (requestStatus) {
                    requestStatus.textContent = '';
                    requestStatus.className = 'small text-muted mt-3 mb-0';
                }
            }

            function updateDateInputs() {
                if (startDateInput) {
                    startDateInput.value = startDate ? formatDate(startDate) : '';
                }
                if (startDateValueInput) {
                    startDateValueInput.value = startDate ? formatDateValue(startDate) : '';
                }
                if (endDateInput) {
                    endDateInput.value = endDate ? formatDate(endDate) : '';
                }
                if (endDateValueInput) {
                    endDateValueInput.value = endDate ? formatDateValue(endDate) : '';
                }
            }

            function handleDateClick(date) {
                const clicked = normalizeDate(date);

                if (clicked.getTime() < today.getTime()) {
                    return;
                }

                if (unavailableDates.has(formatDateValue(clicked))) {
                    return;
                }

                if (!startDate || (startDate && endDate)) {
                    startDate = clicked;
                    endDate = null;
                } else if (clicked.getTime() < startDate.getTime()) {
                    startDate = clicked;
                    endDate = null;
                } else {
                    endDate = clicked;
                }

                updateDateInputs();

                if (requestStatus) {
                    requestStatus.textContent = '';
                    requestStatus.className = 'small text-muted mt-3 mb-0';
                }
            }

            clearDates();
            renderServiceOptions();

            function renderCalendar(month, year) {
                const firstDay = new Date(year, month, 1);
                const lastDay = new Date(year, month + 1, 0);
                const startingDay = firstDay.getDay();
                const totalDays = lastDay.getDate();

                const monthNames = ['January', 'February', 'March', 'April', 'May', 'June',
                    'July', 'August', 'September', 'October', 'November', 'December'];

                const dayHeaders = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
                const isCurrentMonth = year === today.getFullYear() && month === today.getMonth();

                let calendarHTML = `
                    <div class="calendar-header">
                        <button id="prevMonth" type="button" class="calendar-nav-btn" ${isCurrentMonth ? 'disabled' : ''}>&laquo; Previous</button>
                        <h3>${monthNames[month]} ${year}</h3>
                        <button id="nextMonth" type="button" class="calendar-nav-btn">Next &raquo;</button>
                    </div>
                    <div class="calendar-grid">
                `;

                // Add day headers
                dayHeaders.forEach(day => {
                    calendarHTML += `<div class="calendar-day-header">${day}</div>`;
                });

                // Add empty cells for days before the first day of the month
                for (let i = 0; i < startingDay; i++) {
                    calendarHTML += `<div class="calendar-day empty"></div>`;
                }

                // Add days of the month
                for (let day = 1; day <= totalDays; day++) {
                    const date = new Date(year, month, day);
                    const normalizedDate = normalizeDate(date);
                    const isToday = normalizedDate.getTime() === today.getTime();
                    const isBooked = unavailableDates.has(formatDateValue(normalizedDate));
                    const isPast = normalizedDate.getTime() < today.getTime();

                    const isStart = startDate && normalizedDate.getTime() === startDate.getTime();
                    const isEnd = endDate && normalizedDate.getTime() === endDate.getTime();
                    const isInRange = startDate && endDate &&
                        normalizedDate.getTime() > startDate.getTime() &&
                        normalizedDate.getTime() < endDate.getTime();

                    const isDisabled = isPast || isBooked;

                    let dayClass = isBooked ? 'booked' : 'open';
                    if (isInRange) dayClass = 'selected';
                    const todayClass = isToday ? 'today' : '';
                    const selectedClass = (isStart || isEnd) ? 'selected' : '';
                    const pastClass = isPast ? 'past' : '';
                    const selectableAttrs = isDisabled
                        ? `aria-disabled="true" title="${isBooked ? 'This date is already booked' : 'This date has already passed'}"`
                        : 'role="button" tabindex="0"';

                    calendarHTML += `
                        <div class="calendar-day ${dayClass} ${todayClass} ${selectedClass} ${pastClass}" data-date="${date.toISOString()}" ${selectableAttrs}>
                            ${day}
                        </div>
                    `;
                }

                calendarHTML += `</div>`;
                calendarContainer.innerHTML = calendarHTML;

                // Add event listeners for navigation
                document.getElementById('prevMonth').addEventListener('click', () => {
                    currentMonth--;
                    if (currentMonth < 0) {
                        currentMonth = 11;
                        currentYear--;
                    }
                    renderCalendar(currentMonth, currentYear);
                });

                document.getElementById('nextMonth').addEventListener('click', () => {
                    currentMonth++;
                    if (currentMonth > 11) {
                        currentMonth = 0;
                        currentYear++;
                    }
                    renderCalendar(currentMonth, currentYear);
                });

                calendarContainer.querySelectorAll('.calendar-day[data-date]').forEach(dayElement => {
                    if (dayElement.classList.contains('past') || dayElement.classList.contains('booked')) {
                        return;
                    }

                    const selectDay = () => {
                        const dateValue = dayElement.getAttribute('data-date');
                        if (!dateValue) {
                            return;
                        }

                        handleDateClick(new Date(dateValue));
                        renderCalendar(currentMonth, currentYear);
                    };

                    dayElement.addEventListener('click', selectDay);
                    dayElement.addEventListener('keydown', event => {
                        if (event.key === 'Enter' || event.key === ' ') {
                            event.preventDefault();
                            selectDay();
                        }
                    });
                });

                if (requestForm) {
                    requestForm.addEventListener('submit', async event => {
                        event.preventDefault();

                        if (!startDateValueInput || !startDateValueInput.value || !endDateValueInput || !endDateValueInput.value) {
                            if (requestStatus) {
                                requestStatus.textContent = 'Please select a start date and end date from the calendar.';
                                requestStatus.className = 'small text-danger mt-3 mb-0';
                            }
                            return;
                        }

                        const venueId = document.getElementById('venueId').value;
                        const eventType = document.getElementById('eventType').value;
                        const startDate = startDateValueInput.value;
                        const endDate = endDateValueInput.value;
                        const expectedGuests = parseInt(document.getElementById('expectedGuests').value);
                        const additionalServicesInput = document.getElementById('additionalServices').value.trim();
                        const selectedServices = Array.from(document.querySelectorAll('.requested-service-option:checked')).map(input => input.value.trim());
                        const normalizedServices = selectedServices.map(service => service.toLowerCase());
                        const cateringRequested = normalizedServices.some(service => service.includes('catering'));
                        const staffingSecurityRequested = normalizedServices.some(service => service.includes('staffing') || service.includes('security'));
                        const setupCleanupRequested = normalizedServices.some(service => service.includes('setup') || service.includes('cleanup'));
                        const serviceSummary = selectedServices.length ? `Selected services: ${selectedServices.join(', ')}` : '';
                        const additionalServices = [serviceSummary, additionalServicesInput].filter(Boolean).join('\n');

                        if (!venueId || !eventType || !startDate || !endDate || !expectedGuests) {
                            if (requestStatus) {
                                requestStatus.textContent = 'Please fill in all required fields.';
                                requestStatus.className = 'small text-danger mt-3 mb-0';
                            }
                            return;
                        }

                        const bookingData = {
                            venueId: parseInt(venueId),
                            eventType: eventType,
                            startDate: startDate,
                            endDate: endDate,
                            expectedGuests: expectedGuests,
                            specialRequirements: '',
                            cateringRequested: cateringRequested,
                            staffingSecurityRequested: staffingSecurityRequested,
                            setupCleanupRequested: setupCleanupRequested,
                            additionalServices: additionalServices
                        };

                        try {
                            const result = await BookingApi.create(bookingData);
                            if (requestStatus) {
                                requestStatus.textContent = `Request submitted! Reference: ${result.data?.referenceNumber || ''}`;
                                requestStatus.className = 'small text-success mt-3 mb-0';
                            }
                            SwalUtils.success('Request Submitted', `Reference: ${result.data?.referenceNumber || ''}`);
                            requestForm.reset();
                            clearDates();
                            renderCalendar(currentMonth, currentYear);
                        } catch (error) {
                            console.error('Error submitting booking request:', error);
                            if (requestStatus) {
                                requestStatus.textContent = error.message || 'Error submitting request. Please try again.';
                                requestStatus.className = 'small text-danger mt-3 mb-0';
                            }
                        }
                    });
                }
            }

            // Initial render
            renderCalendar(currentMonth, currentYear);
        }
