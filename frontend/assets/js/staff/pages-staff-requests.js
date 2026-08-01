// Staff booking requests: manages booking request listings, status updates, and quoting.
let requests = [];
        let statusModal = null;
        let quoteModal = null;

        document.addEventListener('DOMContentLoaded', function () {
            statusModal = new bootstrap.Modal(document.getElementById('statusModal'));
            quoteModal = new bootstrap.Modal(document.getElementById('quoteModal'));
            document.querySelectorAll('#requestFilter .filter-pill').forEach(button => button.addEventListener('click', () => {
                document.querySelectorAll('#requestFilter .filter-pill').forEach(item => item.classList.remove('active'));
                button.classList.add('active');
                renderRequests();
            }));
            document.getElementById('requestSearch')?.addEventListener('input', renderRequests);
            loadRequests();
        });

        async function loadRequests() {
            const tbody = document.getElementById('requestsTableBody');
            tbody.innerHTML = '<tr><td colspan="8" class="text-center text-muted">Loading requests...</td></tr>';

            try {
                const response = await BookingApi.getAll(1, 100);
                requests = response.data || [];
                renderRequests();
            } catch (error) {
                console.error('Error loading requests:', error);
                tbody.innerHTML = '<tr><td colspan="8" class="text-center text-muted">No booking requests found.</td></tr>';
            }
        }

        function renderRequests() {
            const tbody = document.getElementById('requestsTableBody');
            const filter = document.querySelector('#requestFilter .filter-pill.active')?.dataset.filter || 'active';
            const searchTerm = document.getElementById('requestSearch')?.value.trim().toLowerCase() || '';
            const visible = requests.filter(booking => {
                const matchesStatus = filter === 'all' || (filter === 'active' ? !['Completed', 'Cancelled', 'Rejected', 'QuoteRejected'].includes(booking.status) : booking.status === filter);
                const matchesSearch = Object.values(booking).some(value => String(value ?? '').toLowerCase().includes(searchTerm));
                return matchesStatus && matchesSearch;
            });
            if (!visible.length) {
                tbody.innerHTML = '<tr><td colspan="8" class="text-center text-muted">No bookings in this view.</td></tr>';
                return;
            }
            tbody.innerHTML = visible.map(booking => `
                    <tr>
                        <td><strong>${booking.referenceNumber}</strong></td>
                        <td>${booking.clientName}</td>
                        <td>${booking.venueName}</td>
                        <td>${booking.eventType}</td>
                        <td>${new Date(booking.startDate).toLocaleDateString('en-ZA')} - ${new Date(booking.endDate).toLocaleDateString('en-ZA')}</td>
                        <td>${booking.expectedGuests}</td>
                        <td><span class="badge ${getStatusBadgeClass(booking.status)}">${booking.status}</span></td>
                        <td>
                            <div class="d-flex flex-wrap gap-1">
                                <button class="btn btn-sm btn-outline-primary" data-action="open-status-modal" data-booking-id="${booking.id}" data-status="${booking.status}"><i class="bi bi-eye"></i> Review</button>
                                ${canMutateBooking(booking.status) && ['Pending'].includes(booking.status) ? `<button class="btn btn-sm btn-outline-success" data-action="open-quote-modal" data-booking-id="${booking.id}"><i class="bi bi-file-earmark-plus"></i> Quote</button>` : '<span class="badge bg-light text-dark border align-self-center"><i class="bi bi-lock"></i> Quote sent</span>'}
                                ${canMutateBooking(booking.status) ? `<label class="btn btn-sm btn-outline-secondary mb-0"><i class="bi bi-receipt"></i> Invoices<input type="file" accept="application/pdf" hidden data-action="upload-staff-document" data-booking-id="${booking.id}"></label>` : '<span class="badge bg-light text-dark border align-self-center"><i class="bi bi-lock"></i> Locked</span>'}
                                <button class="btn btn-sm btn-outline-secondary" data-action="view-documents" data-booking-id="${booking.id}"><i class="bi bi-folder2-open"></i> Files (${(booking.documents || []).length}) ${recentDocuments(booking).length ? '<span class="badge bg-warning text-dark">New</span>' : ''}</button>
                            </div>
                        </td>
                    </tr>
                `).join('');
        }

        function canMutateBooking(status) {
            return !['Cancelled', 'Rejected', 'QuoteRejected'].includes(status);
        }

        function getAllowedNextStatuses(status) {
            const transitions = {
                Pending: ['Quoted', 'Rejected', 'Cancelled'],
                Quoted: ['QuoteAccepted', 'QuoteRejected', 'Cancelled'],
                QuoteAccepted: ['Confirmed', 'Cancelled'],
                Confirmed: ['DepositPaid', 'Cancelled'],
                DepositPaid: ['FullyPaid', 'Cancelled'],
                FullyPaid: ['Cancelled']
            };
            return transitions[status] || [];
        }

        function buildRequestSummary(booking) {
            const defaultServices = [
                booking?.cateringRequested && 'Catering',
                booking?.staffingSecurityRequested && 'Staffing & security',
                booking?.setupCleanupRequested && 'Setup & cleanup'
            ].filter(Boolean);

            const additional = String(booking?.additionalServices || '').trim();
            const lines = additional.split(/\r?\n/).map(line => line.trim()).filter(Boolean);
            const selectedLine = lines.find(line => line.toLowerCase().startsWith('selected services:'));
            const noteLines = lines.filter(line => line !== selectedLine);

            const selectedServices = selectedLine
                ? selectedLine.replace(/^selected services:\s*/i, '').split(',').map(service => service.trim()).filter(Boolean)
                : [];

            const mergedServices = Array.from(new Set([...defaultServices, ...selectedServices]));
            const serviceBlock = mergedServices.length ? mergedServices.map(service => `- ${service}`).join('\n') : '- None selected';
            const notesBlock = noteLines.join('\n') || 'No additional notes from client.';

            return `Services selected\n${serviceBlock}\n\nClient notes\n${notesBlock}`;
        }


        function recentDocuments(booking) {
            const since = Date.now() - (7 * 24 * 60 * 60 * 1000);
            return (booking.documents || []).filter(document => new Date(document.createdAt).getTime() >= since);
        }

        function openStatusModal(bookingId, currentStatus) {
            const booking = requests.find(item => item.id === bookingId);
            document.getElementById('statusBookingId').value = bookingId;
            document.getElementById('bookingSpecialRequests').value = buildRequestSummary(booking);
            const statusSelect = document.getElementById('bookingStatus');
            Array.from(statusSelect.options).forEach(option => {
                const enabled = option.value === currentStatus || getAllowedNextStatuses(currentStatus).includes(option.value);
                option.disabled = !enabled;
                option.hidden = !enabled;
            });
            statusSelect.value = currentStatus;
            const readOnly = !canMutateBooking(currentStatus);
            statusSelect.disabled = readOnly;
            document.getElementById('saveStatusButton').disabled = readOnly;
            document.getElementById('saveStatusButton').classList.toggle('d-none', readOnly);
            statusModal.show();
        }

        function openQuoteModal(bookingId) {
            document.getElementById('quoteBookingId').value = bookingId;
            document.getElementById('finalQuote').value = '';
            document.getElementById('depositAmount').value = '';
            document.getElementById('quoteExpiresAt').value = '';
            document.getElementById('cancellationPolicyCode').value = 'Standard';
            document.getElementById('quoteNotes').value = '';
            document.getElementById('quoteFile').value = '';
            document.getElementById('quoteNotes').value = '';
            quoteModal.show();
        }

        async function submitQuote() {
            const bookingId = Number(document.getElementById('quoteBookingId').value);
            const file = document.getElementById('quoteFile').files[0];
            const finalQuote = Number(document.getElementById('finalQuote').value);
            const depositAmount = Number(document.getElementById('depositAmount').value);
            const quoteExpiresAt = document.getElementById('quoteExpiresAt').value;
            const cancellationPolicy = document.getElementById('quoteNotes').value.trim();
            const cancellationPolicyCode = document.getElementById('cancellationPolicyCode').value;
            if (!bookingId || !file || !finalQuote || !quoteExpiresAt || !cancellationPolicy || depositAmount < 0) return SwalUtils.error('Validation Error', 'Complete the quote fields, cancellation policy, and choose a PDF.');
            try {
                await BookingApi.sendQuote({ bookingId, finalQuote, depositAmount, quoteExpiresAt, cancellationPolicy, cancellationPolicyCode });
                const formData = new FormData();
                formData.append('bookingId', bookingId);
                formData.append('documentType', 'Quote');
                formData.append('description', 'Formal booking quote');
                formData.append('file', file);
                await DocumentApi.uploadDocument(formData);
                quoteModal.hide();
                SwalUtils.success('Quote uploaded', 'The client can now review the quote.');
                loadRequests();
            } catch (error) { SwalUtils.error('Quote failed', error.message); }
        }

        async function uploadStaffDocument(bookingId, documentType, file) {
            if (!file) return;
            const formData = new FormData();
            formData.append('bookingId', bookingId);
            formData.append('documentType', documentType);
            formData.append('description', `${documentType} uploaded by staff`);
            formData.append('file', file);
            try { await DocumentApi.uploadDocument(formData); SwalUtils.success('Uploaded', `${documentType} uploaded successfully.`); } catch (error) { SwalUtils.error('Upload failed', error.message); }
        }

        async function viewDocuments(bookingId) {
            try {
                const documents = await DocumentApi.getBookingDocuments(bookingId);
                if (!documents.length) return SwalUtils.info('Documents', 'No documents uploaded yet.');
                await Swal.fire({
                    title: 'Booking documents',
                    html: documents.map(document => `<button class="btn btn-outline-primary w-100 mb-2 document-download" data-document-id="${document.id}"><i class="bi bi-file-earmark-pdf"></i> ${document.documentType}: ${document.fileName}</button>`).join(''),
                    showConfirmButton: false,
                    didOpen: () => document.querySelectorAll('.document-download').forEach(button => button.addEventListener('click', async () => {
                        await DocumentApi.downloadDocument(button.dataset.documentId);
                        Swal.close();
                    }))
                });
            } catch (error) { SwalUtils.error('Documents failed', error.message); }
        }

        async function updateBookingStatus() {
            const bookingId = document.getElementById('statusBookingId').value;
            const status = document.getElementById('bookingStatus').value;
            const booking = requests.find(item => item.id === Number(bookingId));
            if (!bookingId || !status || !booking || (status !== booking.status && !getAllowedNextStatuses(booking.status).includes(status))) {
                SwalUtils.error('Validation Error', 'Please select a valid status');
                return;
            }

            const destructiveStatuses = ['Rejected', 'QuoteRejected', 'Cancelled'];
            if (destructiveStatuses.includes(status)) {
                const result = await SwalUtils.confirm(`${status} this booking?`, 'This action changes the booking status and may release the venue dates.', status);
                if (!result.isConfirmed) return;
            }

            try {
                await BookingApi.updateStatus(bookingId, { status: status });
                SwalUtils.success('Success', 'Status updated successfully');
                statusModal.hide();
                loadRequests();
            } catch (error) {
                console.error('Error updating status:', error);
                SwalUtils.error('Error', error.message || 'Failed to update status');
            }
        }
