// Client portal bookings: loads and filters the client's booking history.
let bookings = [];
        document.addEventListener('DOMContentLoaded', () => {
            document.querySelectorAll('#bookingFilter .filter-pill').forEach(button => button.addEventListener('click', () => {
                document.querySelectorAll('#bookingFilter .filter-pill').forEach(item => item.classList.remove('active'));
                button.classList.add('active');
                renderBookings();
            }));
            document.getElementById('bookingSearch')?.addEventListener('input', renderBookings);
            loadMyBookings();
        });

        async function loadMyBookings() {
            const tbody = document.querySelector('tbody');
            tbody.innerHTML = '<tr><td colspan="5" class="text-center text-muted">Loading bookings...</td></tr>';

            try {
                const response = await BookingApi.getMyBookings(1, 100);

                bookings = response.data || [];
                renderBookings();
            } catch (error) {
                console.error('Error loading bookings:', error);
                tbody.innerHTML = '<tr><td colspan="5" class="text-center text-muted">No bookings yet.</td></tr>';
            }
        }

        function isRecentDocument(document) {
            return new Date(document.createdAt).getTime() >= Date.now() - (7 * 24 * 60 * 60 * 1000);
        }

        function renderBookings() {
            const tbody = document.querySelector('tbody');
            const filter = document.querySelector('#bookingFilter .filter-pill.active')?.dataset.filter || 'active';
            const searchTerm = document.getElementById('bookingSearch')?.value.trim().toLowerCase() || '';
            const visible = bookings.filter(booking => {
                const matchesStatus = filter === 'all' || (filter === 'active' ? !['Completed', 'Cancelled', 'Rejected', 'QuoteRejected'].includes(booking.status) : booking.status === filter);
                const matchesSearch = Object.values(booking).some(value => String(value ?? '').toLowerCase().includes(searchTerm));
                return matchesStatus && matchesSearch;
            });
            if (!visible.length) {
                tbody.innerHTML = '<tr><td colspan="5" class="text-center text-muted">No bookings in this view.</td></tr>';
                return;
            }
            tbody.innerHTML = visible.map(booking => {
                const documents = booking.documents || [];
                const actions = [];
                documents.forEach(document => {
                    const label = document.documentType === 'Invoice' ? 'Invoices' : document.documentType;
                    const newBadge = isRecentDocument(document) ? '<span class="badge bg-warning text-dark">New</span>' : '';
                    actions.push(`<button class="btn btn-sm btn-outline-primary" data-action="download-document" data-document-id="${document.id}"><i class="bi bi-file-earmark-pdf"></i> ${label} ${newBadge}</button>`);
                });
                const hasQuote = Number(booking.finalQuote || 0) > 0 || documents.some(document => document.documentType === 'Quote');
                if (hasQuote) {
                    actions.push(`<button class="btn btn-sm btn-info text-dark" data-action="view-quote-details" data-booking-id="${booking.id}"><i class="bi bi-card-text"></i> Read quote</button>`);
                }
                if (booking.status === 'Quoted') {
                    actions.push(`<button class="btn btn-sm btn-success" data-action="decide-quote" data-booking-id="${booking.id}" data-accepted="true">Accept</button>`);
                    actions.push(`<button class="btn btn-sm btn-outline-danger" data-action="decide-quote" data-booking-id="${booking.id}" data-accepted="false">Reject</button>`);
                }
                actions.push(`<label class="btn btn-sm btn-outline-secondary mb-0"><i class="bi bi-upload"></i> POP<input type="file" accept="application/pdf" hidden data-action="upload-proof" data-booking-id="${booking.id}"></label>`);
                return `<tr><td><strong>${booking.referenceNumber}</strong></td><td>${booking.venueName}</td><td><span class="badge ${getStatusBadgeClass(booking.status)}">${booking.status}</span></td><td>${new Date(booking.startDate).toLocaleDateString('en-ZA')}</td><td><div class="d-flex flex-wrap gap-1">${actions.join('')}</div></td></tr>`;
            }).join('');
        }

        async function viewQuoteDetails(id) {
            const booking = bookings.find(item => item.id === id);
            if (!booking) return;
            const expiry = booking.quoteExpiresAt ? new Date(booking.quoteExpiresAt).toLocaleDateString('en-ZA') : 'Not specified';
            const policy = booking.cancellationPolicy || 'No cancellation policy was supplied.';
            await Swal.fire({
                title: 'Quote details',
                html: `<div class="text-start"><p><strong>Final quote:</strong> ${formatCurrency(booking.finalQuote)}</p><p><strong>Deposit required:</strong> ${formatCurrency(booking.depositAmount)}</p><p><strong>Balance after deposit:</strong> ${formatCurrency(Math.max(0, booking.finalQuote - booking.depositAmount))}</p><p><strong>Quote valid until:</strong> ${expiry}</p><hr><p><strong>Cancellation policy</strong></p><p>${escapeHtml(policy).replace(/\n/g, '<br>')}</p></div>`,
                confirmButtonText: 'Close'
            });
        }

        async function decideQuote(id, accepted) {
            if (!accepted) {
                const result = await SwalUtils.confirm('Reject this quote?', 'The staff member will be notified that you do not accept this quote.', 'Reject quote');
                if (!result.isConfirmed) return;
            }
            try { await BookingApi.decideQuote(id, accepted); await loadMyBookings(); } catch (error) { SwalUtils.error('Quote decision failed', error.message); }
        }

        function escapeHtml(value) {
            return String(value).replace(/[&<>"']/g, character => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#039;' }[character]));
        }

        async function uploadProof(bookingId, file) {
            if (!file) return;
            const formData = new FormData();
            formData.append('bookingId', bookingId);
            formData.append('documentType', 'ProofOfPayment');
            formData.append('description', 'Client proof of payment');
            formData.append('file', file);
            try { await DocumentApi.uploadDocument(formData); SwalUtils.success('Uploaded', 'Proof of payment uploaded.'); await loadMyBookings(); } catch (error) { SwalUtils.error('Upload failed', error.message); }
        }
