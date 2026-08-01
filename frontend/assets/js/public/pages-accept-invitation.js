// Staff invitation acceptance: loads invitation details and validates new account password setup.
const token = new URLSearchParams(window.location.search).get('token');

        async function loadInvitation() {
            const message = document.getElementById('inviteMessage');
            if (!token) {
                message.textContent = 'Invalid invitation link. Please request a new invitation.';
                return;
            }

            try {
                const response = await AuthApi.getInvitation(token);
                const invitation = response.data;
                document.getElementById('inviteEmail').value = invitation.email;
                message.innerHTML = `Welcome, <strong>${invitation.fullName}</strong>. Create a password to activate your staff account.`;
                document.getElementById('acceptForm').classList.remove('d-none');
            } catch (error) {
                console.error('Error loading invitation:', error);
                SwalUtils.error('Invalid Invitation', error.message || 'This invitation is invalid or has expired. Please request a new invitation.');
            }
        }

        const passwordInput = document.getElementById('password');
        const confirmPasswordInput = document.getElementById('confirmPassword');

        function updateRequirement(rule, valid) {
            const item = document.querySelector(`[data-rule="${rule}"]`);
            if (!item) return;
            item.classList.toggle('text-success', valid);
            item.classList.toggle('text-danger', !valid);
            item.querySelector('i').className = `bi ${valid ? 'bi-check-circle-fill' : 'bi-x-circle'} me-1`;
        }

        function updatePasswordRequirements() {
            const password = passwordInput.value;
            updateRequirement('length', password.length >= 8);
            updateRequirement('uppercase', /[A-Z]/.test(password));
            updateRequirement('lowercase', /[a-z]/.test(password));
            updateRequirement('digit', /[0-9]/.test(password));

            const matches = confirmPasswordInput.value.length > 0 && password === confirmPasswordInput.value;
            const matchMessage = document.getElementById('matchMessage');
            matchMessage.textContent = confirmPasswordInput.value.length === 0 ? '' : matches ? 'Passwords match.' : 'Passwords do not match.';
            matchMessage.className = `small mt-1 ${matches ? 'text-success' : 'text-danger'}`;
        }

        passwordInput.addEventListener('input', updatePasswordRequirements);
        confirmPasswordInput.addEventListener('input', updatePasswordRequirements);

        document.getElementById('acceptForm').addEventListener('submit', async function (e) {
            e.preventDefault();

            const password = passwordInput.value;
            const confirmPassword = confirmPasswordInput.value;
            const validPassword = password.length >= 8 && /[A-Z]/.test(password) && /[a-z]/.test(password) && /[0-9]/.test(password);
            if (!validPassword || password !== confirmPassword) {
                await SwalUtils.error('Password Requirements', 'Please meet every password requirement and make sure both passwords match.');
                return;
            }

            const submitButton = this.querySelector('button[type="submit"]');
            submitButton.disabled = true;
            submitButton.textContent = 'Activating...';

            try {
                await AuthApi.acceptInvitation({ token, password });
                document.getElementById('acceptForm').classList.add('d-none');
                document.getElementById('inviteMessage').classList.add('d-none');
                await SwalUtils.success('Account Activated', 'Your account has been activated. You can now log in.');
                window.location.href = 'login.html';
            } catch (error) {
                console.error('Error accepting invitation:', error);
                await SwalUtils.error('Activation Failed', error.message || 'Failed to activate account. Please try again.');
                submitButton.disabled = false;
                submitButton.textContent = 'Activate Account';
            }
        });

        document.addEventListener('DOMContentLoaded', loadInvitation);
