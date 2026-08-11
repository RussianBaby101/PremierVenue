// Client portal profile: loads and saves the client's personal details.
document.addEventListener('DOMContentLoaded', async () => {
            let user = JSON.parse(localStorage.getItem('user') || 'null');
            if (!user) return;
            try {
                const response = await UserApi.getMyProfile();
                user = response.data || response;
                localStorage.setItem('user', JSON.stringify(user));
            } catch (error) {
                console.warn('Unable to refresh profile from API', error);
            }

            document.getElementById('firstName').value = user.firstName || '';
            document.getElementById('lastName').value = user.lastName || '';
            document.getElementById('email').value = user.email || '';
            document.getElementById('phoneNumber').value = user.phoneNumber || '';
            document.getElementById('profileEmailSummary').textContent = user.email || '-';
            document.getElementById('profilePhoneSummary').textContent = user.phoneNumber || '-';

            document.getElementById('profileForm').addEventListener('submit', async event => {
                event.preventDefault();
                const alert = document.getElementById('profileAlert');
                const profileData = {
                    firstName: document.getElementById('firstName').value.trim(),
                    lastName: document.getElementById('lastName').value.trim(),
                    phoneNumber: document.getElementById('phoneNumber').value.trim()
                };
                try {
                    const response = await UserApi.updateMyProfile(profileData);
                    const updatedUser = response.data || response;
                    localStorage.setItem('user', JSON.stringify({ ...user, ...updatedUser }));
                    alert.className = 'alert alert-success';
                    alert.textContent = 'Your profile has been updated.';
                    document.getElementById('profilePhoneSummary').textContent = updatedUser.phoneNumber || '-';
                } catch (error) {
                    alert.className = 'alert alert-danger';
                    alert.textContent = error.message || 'Unable to update your profile.';
                }
            });
        });
