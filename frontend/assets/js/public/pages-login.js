// Public login: authenticates users and redirects to the appropriate staff or client dashboard.
document.getElementById('loginForm').addEventListener('submit', async function (e) {
            e.preventDefault();

            const email = document.getElementById('email').value.trim();
            const password = document.getElementById('password').value;

            try {
                const result = await AuthApi.login(email, password);
                if (result.success && result.data) {
                    localStorage.setItem('token', result.data.token);
                    localStorage.setItem('refreshToken', result.data.refreshToken);
                    localStorage.setItem('user', JSON.stringify(result.data.user));

                    showSuccess('Login successful. Redirecting...');

                    setTimeout(() => {
                        const role = result.data.user.role.toLowerCase();
                        if (role === 'staff' || role === 'admin' || role === 'superadmin') {
                            window.location.href = '/pages/staff/dashboard.html';
                        } else {
                            window.location.href = '/pages/client/dashboard.html';
                        }
                    }, 1000);
                } else {
                    showError(result.message || 'Login failed.');
                }
            } catch (error) {
                showError(error.message || 'Login failed. Please try again.');
            }
        });
