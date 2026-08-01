// Client portal dashboard: displays booking counts, recent activity, and the next upcoming event.
document.addEventListener('DOMContentLoaded', loadClientDashboard);
        async function loadClientDashboard() {
            const user = JSON.parse(localStorage.getItem('user') || '{}');
            const name = user.firstName || user.name || user.email?.split('@')[0] || '';
            document.getElementById('userName').textContent = name ? `, ${name}` : '';
            try {
                const response = await BookingApi.getMyBookings(1, 100);
                const bookings = response.data || [];
                const open = bookings.filter(item => !['Completed', 'Cancelled', 'Rejected'].includes(item.status));
                const confirmed = bookings.filter(item => ['Confirmed', 'DepositPaid', 'FullyPaid'].includes(item.status));
                const pending = bookings.filter(item => ['Pending', 'UnderReview', 'Quoted'].includes(item.status));
                document.getElementById('clientOpen').textContent = open.length;
                document.getElementById('clientConfirmed').textContent = confirmed.length;
                document.getElementById('clientPending').textContent = pending.length;
                document.getElementById('clientTotal').textContent = bookings.length;
                renderClientBookings(bookings.slice(0, 5));
                renderNextEvent(bookings);
            } catch (error) { console.error('Error loading client dashboard:', error); }
        }
        function renderClientBookings(bookings) {
            document.getElementById('clientBookings').innerHTML = bookings.length ? bookings.map(item => `<div class="dashboard-list-item"><div class="list-item-icon"><i class="bi bi-building"></i></div><div class="list-item-content"><strong>${item.venueName || 'Venue booking'}</strong><span>${item.referenceNumber || 'Booking'} · ${new Date(item.startDate).toLocaleDateString('en-ZA')}</span></div><span class="badge ${getStatusBadgeClass(item.status)}">${item.status}</span></div>`).join('') : '<div class="empty-dashboard"><i class="bi bi-calendar2-plus"></i><p>Your booking activity will appear here.</p><a href="venues.html" class="btn btn-outline-primary btn-sm">Find a venue</a></div>';
        }
        function renderNextEvent(bookings) {
            const upcoming = bookings.filter(item => new Date(item.startDate) >= new Date() && !['Cancelled', 'Rejected'].includes(item.status)).sort((a, b) => new Date(a.startDate) - new Date(b.startDate))[0];
            if (!upcoming) return;
            document.getElementById('nextEvent').innerHTML = `<div class="next-event-date"><strong>${new Date(upcoming.startDate).toLocaleDateString('en-ZA', { day: '2-digit' })}</strong><span>${new Date(upcoming.startDate).toLocaleDateString('en-ZA', { month: 'short' })}</span></div><h3>${upcoming.venueName || 'Upcoming booking'}</h3><p><i class="bi bi-calendar3"></i> ${new Date(upcoming.startDate).toLocaleDateString('en-ZA', { weekday: 'long', day: 'numeric', month: 'long' })}</p><span class="badge ${getStatusBadgeClass(upcoming.status)}">${upcoming.status}</span>`;
        }
