// Staff dashboard: shows booking metrics, pending requests, completed bookings, and recent activity.
document.addEventListener('DOMContentLoaded', loadStaffDashboard);
        async function loadStaffDashboard() {
            const user = JSON.parse(localStorage.getItem('user') || '{}');
            const name = user.firstName || user.name || '';
            document.getElementById('staffName').textContent = name ? `, ${name}` : '';
            const isAdmin = user.role === 'Admin';
            document.getElementById('staffRevenueMetric')?.classList.toggle('d-none', !isAdmin);
            try {
                const response = await BookingApi.getAll(1, 1000);
                const bookings = response.data || [];
                const pending = bookings.filter(item => ['Pending', 'UnderReview', 'Quoted'].includes(item.status));
                const completed = bookings.filter(item => item.status === 'Completed');
                const paidBookings = bookings.filter(item => ['DepositPaid', 'FullyPaid', 'Completed'].includes(item.status) || (item.payments || []).length > 0);
                const revenue = paidBookings.reduce((total, booking) => total + collectedAmount(booking), 0);
                document.getElementById('staffTotal').textContent = bookings.length;
                document.getElementById('staffPending').textContent = pending.length;
                document.getElementById('staffCompleted').textContent = completed.length;
                document.getElementById('staffRevenue').textContent = formatCurrency(revenue);
                renderStaffBookings(bookings.slice(0, 5));
            } catch (error) { console.error('Error loading staff dashboard:', error); document.getElementById('staffBookings').innerHTML = '<div class="empty-dashboard"><i class="bi bi-exclamation-circle"></i><p>Dashboard data is temporarily unavailable.</p></div>'; }
        }
        function collectedAmount(booking) {
            const payments = booking.payments || [];
            if (payments.length) {
                return payments.filter(payment => payment.status === 'Completed').reduce((total, payment) => total + (payment.paymentType === 'Refund' ? -Number(payment.amount || 0) : Number(payment.amount || 0)), 0);
            }
            if (booking.status === 'DepositPaid') return Number(booking.depositAmount || 0);
            if (['FullyPaid', 'Completed'].includes(booking.status)) return Math.max(0, Number(booking.finalQuote || 0) - Number(booking.refundAmount || 0));
            return 0;
        }

        function renderStaffBookings(bookings) { document.getElementById('staffBookings').innerHTML = bookings.length ? bookings.map(item => `<div class="dashboard-list-item"><div class="list-item-icon"><i class="bi bi-calendar-event"></i></div><div class="list-item-content"><strong>${item.referenceNumber || 'Booking request'}</strong><span>${item.clientName || 'Client'} · ${item.venueName || 'Venue'}</span></div><span class="badge ${getStatusBadgeClass(item.status)}">${item.status}</span></div>`).join('') : '<div class="empty-dashboard"><i class="bi bi-inbox"></i><p>No booking requests yet.</p></div>'; }
