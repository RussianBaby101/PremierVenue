// Staff reports: loads booking data and renders revenue, booking mix, and history charts.
document.addEventListener('DOMContentLoaded', loadReports);

        async function loadReports() {
            try {
                const response = await BookingApi.getAll(1, 1000);
                const bookings = response.data || [];
                const closed = bookings.filter(booking => ['Completed', 'Cancelled', 'Rejected', 'QuoteRejected'].includes(booking.status));
                const completed = bookings.filter(booking => booking.status === 'Completed');
                const cancelled = bookings.filter(booking => ['Cancelled', 'Rejected', 'QuoteRejected'].includes(booking.status));
                const paidBookings = bookings.filter(booking => ['DepositPaid', 'FullyPaid', 'Completed'].includes(booking.status) || (booking.payments || []).length > 0);
                const revenue = paidBookings.reduce((total, booking) => total + collectedAmount(booking), 0);

                document.getElementById('reportRevenue').textContent = formatCurrency(revenue);
                document.getElementById('reportCompleted').textContent = completed.length;
                document.getElementById('reportCancelled').textContent = cancelled.length;
                document.getElementById('reportRate').textContent = bookings.length ? `${Math.round((completed.length / bookings.length) * 100)}%` : '0%';
                document.getElementById('reportCount').textContent = `${closed.length} closed booking${closed.length === 1 ? '' : 's'}`;

                renderBookingMix(completed.length, cancelled.length, bookings.length - completed.length - cancelled.length);
                renderRevenueChart(paidBookings);
                renderHistory(closed);
            } catch (error) {
                console.error('Error loading reports:', error);
                document.getElementById('reportBookings').innerHTML = '<tr><td colspan="6" class="text-center text-muted py-4">Reports could not be loaded right now.</td></tr>';
                document.getElementById('reportCount').textContent = 'Unable to load';
            }
        }

        function renderBookingMix(completed, cancelled, open) {
            const total = completed + cancelled + open;
            const percent = value => total ? Math.round((value / total) * 100) : 0;
            document.getElementById('bookingMix').innerHTML = `
                <div class="mix-row"><span><i class="mix-dot completed"></i>Completed</span><strong>${completed} <small>${percent(completed)}%</small></strong></div>
                <div class="mix-bar"><span class="completed" style="width: ${percent(completed)}%"></span></div>
                <div class="mix-row"><span><i class="mix-dot pending"></i>Open / in progress</span><strong>${open} <small>${percent(open)}%</small></strong></div>
                <div class="mix-bar"><span class="pending" style="width: ${percent(open)}%"></span></div>
                <div class="mix-row"><span><i class="mix-dot cancelled"></i>Cancelled</span><strong>${cancelled} <small>${percent(cancelled)}%</small></strong></div>
                <div class="mix-bar"><span class="cancelled" style="width: ${percent(cancelled)}%"></span></div>`;
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

        function renderRevenueChart(paidBookings) {
            const months = [];
            for (let index = 5; index >= 0; index--) {
                const date = new Date();
                date.setMonth(date.getMonth() - index, 1);
                months.push({ label: date.toLocaleDateString('en-ZA', { month: 'short' }), year: date.getFullYear(), month: date.getMonth(), value: 0 });
            }
            paidBookings.forEach(booking => {
                const date = new Date(booking.updatedAt || booking.completedAt || booking.endDate || booking.createdAt);
                const point = months.find(item => item.year === date.getFullYear() && item.month === date.getMonth());
                if (point) point.value += collectedAmount(booking);
            });
            new Chart(document.getElementById('revenueChart'), { type: 'line', data: { labels: months.map(item => item.label), datasets: [{ data: months.map(item => item.value), borderColor: '#1769e0', backgroundColor: 'rgba(23, 105, 224, 0.13)', fill: true, tension: 0.4, pointBackgroundColor: '#fff', pointBorderColor: '#1769e0', pointBorderWidth: 3, pointRadius: 4 }] }, options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { display: false }, tooltip: { callbacks: { label: context => formatCurrency(context.raw) } } }, scales: { y: { beginAtZero: true, ticks: { callback: value => `R${Number(value).toLocaleString('en-ZA')}` }, grid: { color: '#e5eef9' } }, x: { grid: { display: false } } } } });
        }

        function renderHistory(bookings) {
            const rows = bookings.sort((a, b) => new Date(b.updatedAt || b.createdAt) - new Date(a.updatedAt || a.createdAt)).map(booking => `<tr><td><strong>${booking.referenceNumber || '-'}</strong></td><td>${booking.venueName || '-'}</td><td>${booking.clientName || '-'}</td><td>${formatDateRange(booking)}</td><td>${formatCurrency(collectedAmount(booking))}</td><td><span class="badge ${booking.status === 'Completed' ? 'bg-success' : 'bg-danger'}">${booking.status}</span></td></tr>`).join('');
            document.getElementById('reportBookings').innerHTML = rows || '<tr><td colspan="6" class="text-center text-muted py-4">No completed or cancelled bookings yet.</td></tr>';
        }

        function formatDateRange(booking) { return `${new Date(booking.startDate).toLocaleDateString('en-ZA')} - ${new Date(booking.endDate).toLocaleDateString('en-ZA')}`; }
