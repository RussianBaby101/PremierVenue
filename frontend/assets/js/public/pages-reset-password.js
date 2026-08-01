// Reset password: sets a new password using the email from the query string.
const params = new URLSearchParams(window.location.search);
        const emailInput = document.getElementById('email');
        emailInput.value = params.get('email') || '';

        const passwordInput = document.getElementById('password');
        const confirmInput = document.getElementById('confirmPassword');

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

            const matches = confirmInput.value.length > 0 && password === confirmInput.value;
            const matchMessage = document.getElementById('matchMessage');
            matchMessage.textContent = confirmInput.value.length === 0 ? '' : matches ? 'Passwords match.' : 'Passwords do not match.';
            matchMessage.className = `small mt-1 ${matches ? 'text-success' : 'text-danger'}`;
        }

        passwordInput.addEventListener('input', updatePasswordRequirements);
        confirmInput.addEventListener('input', updatePasswordRequirements);

        document.getElementById('resetPasswordForm').addEventListener('submit', async function (event) {
            event.preventDefault();

            const password = passwordInput.value;
            const confirmPassword = confirmInput.value;
            const validPassword = password.length >= 8 && /[A-Z]/.test(password) && /[a-z]/.test(password) && /[0-9]/.test(password);
            if (!validPassword || password !== confirmPassword) {
                SwalUtils.error('Password Requirements', 'Please meet every password requirement and make sure both passwords match.');
                return;
            }

            const submitButton = document.getElementById('submitButton');
            submitButton.disabled = true;
            submitButton.textContent = 'Resetting...';

            try {
                await AuthApi.resetPassword({
                    email: emailInput.value.trim(),
                    otp: document.getElementById('otp').value.trim(),
                    password
                });
                await SwalUtils.success('Password Reset', 'Your password has been reset. You can now log in.');
                window.location.href = 'login.html';
            } catch (error) {
                SwalUtils.error('Unable to Reset Password', error.message || 'The code may be invalid or expired.');
                submitButton.disabled = false;
                submitButton.textContent = 'Reset Password';
            }
        });
