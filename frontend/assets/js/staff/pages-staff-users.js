// Staff user management: lists staff users and toggles their active status.
let staffUsers = [];

function renderStaffUsers() {
            const tbody = document.getElementById('usersTableBody');
            const searchTerm = document.getElementById('userSearch')?.value.trim().toLowerCase() || '';
            const visibleUsers = staffUsers.filter(user => [
                user.firstName, user.lastName, user.userName, user.email,
                user.role, user.status, user.createdAt
            ].some(value => String(value || '').toLowerCase().includes(searchTerm)));

            if (visibleUsers.length === 0) {
                tbody.innerHTML = '<tr><td colspan="7" class="text-center text-muted">No users match your search.</td></tr>';
                return;
            }

            tbody.innerHTML = visibleUsers.map(user => `
                    <tr>
                        <td>${user.firstName} ${user.lastName}</td>
                        <td>${user.userName}</td>
                        <td>${user.email}</td>
                        <td>${user.role}</td>
                        <td>${statusBadge(user.status)}</td>
                        <td>${new Date(user.createdAt).toLocaleDateString('en-ZA')}</td>
                        <td class="text-end">${actionButton(user)}</td>
                    </tr>
                `).join('');
        }

function statusBadge(status) {
            switch (status) {
                case 'Active':
                    return '<span class="badge bg-success">Active</span>';
                case 'Inactive':
                    return '<span class="badge bg-danger">Inactive</span>';
                case 'Pending':
                    return '<span class="badge bg-warning text-dark">Pending</span>';
                default:
                    return '<span class="badge bg-secondary">Unknown</span>';
            }
        }

        function actionButton(user) {
            if (user.status === 'Active') {
                return `<button class="btn btn-sm btn-outline-danger" title="Deactivate" data-action="toggle-user-status" data-user-id="${user.id}" data-is-active="true"><i class="bi bi-person-slash"></i></button>`;
            }
            if (user.status === 'Inactive') {
                return `<button class="btn btn-sm btn-outline-success" title="Activate" data-action="toggle-user-status" data-user-id="${user.id}" data-is-active="false"><i class="bi bi-person-check"></i></button>`;
            }
            return `<span class="text-muted" title="Pending invitation"><i class="bi bi-envelope"></i></span>`;
        }

        async function loadStaffUsers() {
            const tbody = document.getElementById('usersTableBody');
            tbody.innerHTML = '<tr><td colspan="7" class="text-center text-muted">Loading staff users...</td></tr>';

            try {
                const response = await UserApi.getAll('Staff');
                staffUsers = response.data || [];
                renderStaffUsers();
            } catch (error) {
                console.error('Error loading staff users:', error);
                tbody.innerHTML = '<tr><td colspan="7" class="text-center text-muted">Error loading staff users.</td></tr>';
            }
        }

        async function toggleUserStatus(id, isActive) {
            const action = isActive ? 'deactivate' : 'activate';
            const result = await SwalUtils.confirm(
                `${action.charAt(0).toUpperCase() + action.slice(1)} Staff Member`,
                `Are you sure you want to ${action} this staff member?`,
                `${action.charAt(0).toUpperCase() + action.slice(1)}`
            );

            if (!result.isConfirmed) return;

            try {
                await UserApi.toggleStatus(id);
                await SwalUtils.success(
                    `Account ${action.charAt(0).toUpperCase() + action.slice(1)}d`,
                    `The staff member was ${action}d successfully.`
                );
                await loadStaffUsers();
            } catch (error) {
                console.error('Error toggling user status:', error);
                await SwalUtils.error('Status Update Failed', error.message || 'Failed to update user status.');
            }
        }

        async function sendInvitation() {
            const fullName = document.getElementById('fullName').value.trim();
            const email = document.getElementById('email').value.trim();

            if (!fullName || !email) {
                SwalUtils.error('Validation Error', 'Please provide a full name and email address.');
                return;
            }

            const btn = document.getElementById('sendInvitationBtn');
            btn.disabled = true;
            btn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Sending...';

            try {
                await UserApi.createStaffInvitation({ fullName, email });
                SwalUtils.success('Success', 'Invitation created successfully.');
                document.getElementById('inviteForm').reset();
                bootstrap.Modal.getInstance(document.getElementById('inviteModal')).hide();
                loadStaffUsers();
            } catch (error) {
                console.error('Error sending invitation:', error);
                SwalUtils.error('Error', error.message || 'Failed to create invitation.');
            } finally {
                btn.disabled = false;
                btn.innerHTML = 'Send Invitation';
            }
        }

        document.addEventListener('DOMContentLoaded', function () {
            if (isAuthenticated()) {
                document.getElementById('userSearch')?.addEventListener('input', renderStaffUsers);
                loadStaffUsers();
            }
        });
