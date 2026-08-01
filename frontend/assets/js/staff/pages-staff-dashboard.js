// Staff dashboard: shows booking metrics, pending requests, completed bookings, and recent activity.
document.addEventListener('DOMContentLoaded', loadStaffDashboard);
        async function loadStaffDashboard() {
            const user = JSON.parse(localStorage.getItem('user') || '{}');
            const name = user.firstName || user.name || '';
            document.getElementById('staffName').textContent = name ? `, ${name}` : '';
            try {
                const response = await BookingApi.getAll(1, 1000);
                const bookings = response.data || [];
                const pending = bookings.filter(item => ['Pending', 'UnderReview', 'Quoted'].includes(item.status));
                const completed = bookings.filter(item => item.status === 'Completed');
                const revenue = completed.reduce((sum, item) => sum + Number(item.finalQuote || 0), 0);
                document.getElementById('staffTotal').textContent = bookings.length;
                document.getElementById('staffPending').textContent = pending.length;
                document.getElementById('staffCompleted').textContent = completed.length;
                document.getElementById('staffRevenue').textContent = formatCurrency(revenue);
                renderStaffBookings(bookings.slice(0, 5));
            } catch (error) { console.error('Error loading staff dashboard:', error); document.getElementById('staffBookings').innerHTML = '<div class="empty-dashboard"><i class="bi bi-exclamation-circle"></i><p>Dashboard data is temporarily unavailable.</p></div>'; }
        }
        function renderStaffBookings(bookings) { document.getElementById('staffBookings').innerHTML = bookings.length ? bookings.map(item => `<div class="dashboard-list-item"><div class="list-item-icon"><i class="bi bi-calendar-event"></i></div><div class="list-item-content"><strong>${item.referenceNumber || 'Booking request'}</strong><span>${item.clientName || 'Client'} · ${item.venueName || 'Venue'}</span></div><span class="badge ${getStatusBadgeClass(item.status)}">${item.status}</span></div>`).join('') : '<div class="empty-dashboard"><i class="bi bi-inbox"></i><p>No booking requests yet.</p></div>'; }
