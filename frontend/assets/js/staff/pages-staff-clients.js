// Staff client management: lists clients and provides activate/deactivate actions.
let clients = [];

function renderClients() {
            const tbody = document.getElementById('clientsTableBody');
            const searchTerm = document.getElementById('clientSearch')?.value.trim().toLowerCase() || '';
            const visibleClients = clients.filter(client => [
                client.firstName, client.lastName, client.email, client.phoneNumber,
                client.isActive ? 'active' : 'inactive', client.createdAt
            ].some(value => String(value || '').toLowerCase().includes(searchTerm)));

            if (visibleClients.length === 0) {
                tbody.innerHTML = '<tr><td colspan="6" class="text-center text-muted">No clients match your search.</td></tr>';
                return;
            }

            tbody.innerHTML = visibleClients.map(client => `
                    <tr>
                        <td>${client.firstName} ${client.lastName}</td>
                        <td>${client.email}</td>
                        <td>${client.phoneNumber || '-'}</td>
                        <td><span class="badge ${client.isActive ? 'bg-success' : 'bg-danger'}">${client.isActive ? 'Active' : 'Inactive'}</span></td>
                        <td>${new Date(client.createdAt).toLocaleDateString('en-ZA')}</td>
                        <td>
                            <button class="btn btn-sm ${client.isActive ? 'btn-outline-danger' : 'btn-outline-success'}" data-action="toggle-client-status" data-client-id="${client.id}" data-is-active="${client.isActive}">
                                ${client.isActive ? 'Deactivate' : 'Activate'}
                            </button>
                        </td>
                    </tr>
                `).join('');
        }

async function loadClients() {
            const tbody = document.getElementById('clientsTableBody');
            tbody.innerHTML = '<tr><td colspan="6" class="text-center text-muted">Loading clients...</td></tr>';

            try {
                const response = await UserApi.getAll('Client');
                clients = response.data || [];
                renderClients();
            } catch (error) {
                console.error('Error loading clients:', error);
                tbody.innerHTML = '<tr><td colspan="6" class="text-center text-muted">Error loading clients.</td></tr>';
            }
        }

        async function toggleClientStatus(id, isActive) {
            const action = isActive ? 'deactivate' : 'activate';
            const result = await SwalUtils.confirm(
                `${action.charAt(0).toUpperCase() + action.slice(1)} Client`,
                `Are you sure you want to ${action} this client?`,
                `${action.charAt(0).toUpperCase() + action.slice(1)}`
            );

            if (!result.isConfirmed) return;

            try {
                await UserApi.toggleStatus(id);
                await SwalUtils.success(
                    `Client ${isActive ? 'Deactivated' : 'Activated'}`,
                    `The client account has been ${isActive ? 'deactivated' : 'activated'} successfully.`
                );
                await loadClients();
            } catch (error) {
                console.error('Error toggling client status:', error);
                await SwalUtils.error('Update Failed', error.message || 'Failed to update client status.');
            }
        }

        document.addEventListener('DOMContentLoaded', () => {
            document.getElementById('clientSearch')?.addEventListener('input', renderClients);
            loadClients();
        });
