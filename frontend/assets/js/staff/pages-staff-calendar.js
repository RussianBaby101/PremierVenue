// Staff calendar: renders an interactive calendar of all bookings, restricted to admin users.
document.addEventListener('DOMContentLoaded', initStaffCalendar);

        async function initStaffCalendar() {
            const user = getCurrentUser();
            if (!user || user.role !== 'Admin') {
                window.location.href = '/pages/public/login.html';
                return;
            }

            const calendarContainer = document.getElementById('staffCalendar');
            if (!calendarContainer) return;

            let bookings = [];
            try {
                const response = await BookingApi.getAll(1, 1000);
                if (response && response.success && response.data) {
                    bookings = response.data;
                }
            } catch (error) {
                console.error('Error loading all bookings for calendar:', error);
            }

            const venueColors = buildVenueColors(bookings);
            renderVenueLegend(venueColors);

            let currentDate = new Date();
            let currentMonth = currentDate.getMonth();
            let currentYear = currentDate.getFullYear();

            function normalizeDate(date) {
                return new Date(date.getFullYear(), date.getMonth(), date.getDate());
            }

            function buildVenueColors(allBookings) {
                const colors = [
                    '#667eea', '#764ba2', '#ef476f', '#f78c6b', '#ffd166',
                    '#06d6a0', '#118ab2', '#073b4c', '#9b5de5', '#f15bb5',
                    '#00bbf9', '#00f5d4', '#fee440', '#f8961e', '#90be6d',
                    '#43aa8b', '#577590', '#f94144', '#277da1', '#bc6c25'
                ];
                const map = {};
                let nextIndex = 0;

                allBookings.forEach(booking => {
                    const venue = booking.venueName || 'Unknown venue';
                    if (!(venue in map)) {
                        if (nextIndex < colors.length) {
                            map[venue] = colors[nextIndex++];
                        } else {
                            const hue = (venue.split('').reduce((sum, c) => sum + c.charCodeAt(0), 0) * 17) % 360;
                            map[venue] = `hsl(${hue}, 70%, 55%)`;
                        }
                    }
                });

                return map;
            }

            function getVenueColor(venue) {
                return venueColors[venue] || '#adb5bd';
            }

            function renderVenueLegend(map) {
                const legendContainer = document.getElementById('venueLegend');
                if (!legendContainer) return;

                const venues = Object.keys(map).sort();
                if (venues.length === 0) {
                    legendContainer.innerHTML = '<span class="text-muted">No venues with bookings to display.</span>';
                    return;
                }

                legendContainer.innerHTML = venues.map(venue => `
                    <span class="legend-item">
                        <span class="legend-color" style="background-color: ${map[venue]};"></span>
                        ${venue}
                    </span>
                `).join('');
            }

            function getBookingsForDate(date) {
                const normalized = normalizeDate(date).getTime();
                return bookings.filter(booking => {
                    const start = normalizeDate(new Date(booking.startDate)).getTime();
                    const end = normalizeDate(new Date(booking.endDate)).getTime();
                    return normalized >= start && normalized <= end;
                });
            }

            function formatTooltipBooking(booking) {
                const start = new Date(booking.startDate).toLocaleDateString('en-ZA');
                const end = new Date(booking.endDate).toLocaleDateString('en-ZA');
                const color = getVenueColor(booking.venueName);
                return `
                    <div>
                        <strong><span style="display:inline-block;width:10px;height:10px;border-radius:50%;background-color:${color};margin-right:5px;"></span>${booking.venueName || 'Unknown venue'}</strong><br>
                        Ref: ${booking.referenceNumber || 'N/A'}<br>
                        Client: ${booking.clientName || 'N/A'}<br>
                        Event: ${booking.eventType || 'N/A'}<br>
                        Status: ${booking.status}<br>
                        Dates: ${start} - ${end}<br>
                        Guests: ${booking.expectedGuests || 'N/A'}
                    </div>
                `;
            }

            function formatTooltip(dayBookings) {
                return dayBookings.map(formatTooltipBooking).join('<hr>');
            }

            function renderChips(dayBookings) {
                const maxVisible = 5;
                const visible = dayBookings.slice(0, maxVisible);
                const hidden = dayBookings.length - maxVisible;
                let html = visible.map(booking =>
                    `<span class="booking-chip" style="background-color: ${getVenueColor(booking.venueName)};"></span>`
                ).join('');
                if (hidden > 0) {
                    html += `<span class="more-chips">+${hidden}</span>`;
                }
                return `<div class="calendar-chips">${html}</div>`;
            }

            function renderCalendar(month, year) {
                const firstDay = new Date(year, month, 1);
                const lastDay = new Date(year, month + 1, 0);
                const startingDay = firstDay.getDay();
                const totalDays = lastDay.getDate();

                const monthNames = ['January', 'February', 'March', 'April', 'May', 'June',
                    'July', 'August', 'September', 'October', 'November', 'December'];

                const dayHeaders = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

                let calendarHTML = `
                    <div class="calendar-header">
                        <button id="prevMonth" type="button" class="calendar-nav-btn">&laquo; Previous</button>
                        <h3>${monthNames[month]} ${year}</h3>
                        <button id="nextMonth" type="button" class="calendar-nav-btn">Next &raquo;</button>
                    </div>
                    <div class="calendar-grid">
                `;

                dayHeaders.forEach(day => {
                    calendarHTML += `<div class="calendar-day-header">${day}</div>`;
                });

                for (let i = 0; i < startingDay; i++) {
                    calendarHTML += `<div class="calendar-day empty"></div>`;
                }

                const today = new Date();
                today.setHours(0, 0, 0, 0);

                const totalCells = startingDay + totalDays;
                const lastRowStart = Math.floor((totalCells - 1) / 7) * 7;

                for (let day = 1; day <= totalDays; day++) {
                    const date = new Date(year, month, day);
                    const normalizedDate = normalizeDate(date);
                    const isToday = normalizedDate.getTime() === today.getTime();
                    const dayBookings = getBookingsForDate(date);
                    const hasBookings = dayBookings.length > 0;

                    const cellIndex = startingDay + (day - 1);
                    const col = cellIndex % 7;
                    const isFirstCol = col === 0;
                    const isLastCol = col === 6;
                    const isLastRow = cellIndex >= lastRowStart;

                    const dayClass = hasBookings ? 'has-bookings' : 'no-request';
                    const todayClass = isToday ? 'today' : '';
                    const colClass = isFirstCol ? 'col-first' : isLastCol ? 'col-last' : '';
                    const rowClass = isLastRow ? 'row-last' : '';

                    const bookingExtras = hasBookings
                        ? renderChips(dayBookings) + `
                            <div class="calendar-tooltip" role="tooltip">
                                ${formatTooltip(dayBookings)}
                            </div>
                        `
                        : '';

                    calendarHTML += `
                        <div class="calendar-day ${dayClass} ${todayClass} ${colClass} ${rowClass}">
                            ${day}
                            ${bookingExtras}
                        </div>
                    `;
                }

                calendarHTML += `</div>`;
                calendarContainer.innerHTML = calendarHTML;

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
            }

            renderCalendar(currentMonth, currentYear);
        }
