// Reset password: sets a new password using the email from the query string.
const params = new URLSearchParams(window.location.search);
        const emailInput = document.getElementById('email');
        const resetForm = document.getElementById('resetPasswordForm');
        const resetLinkError = document.getElementById('resetLinkError');
        const resetToken = params.get('token') || '';
        emailInput.value = params.get('email') || '';

        if (resetToken) {
            resetForm?.classList.remove('d-none');
        } else {
            resetLinkError?.classList.remove('d-none');
        }

        const passwordInput = document.getElementById('password');
        const confirmInput = document.getElementById('confirmPassword');
        const otpInput = document.getElementById('otp');
        const progressSteps = [...document.querySelectorAll('.auth-progress span')];

        function updateProgress() {
            const passwordComplete = passwordInput.value.length >= 8 && /[A-Z]/.test(passwordInput.value) && /[a-z]/.test(passwordInput.value) && /[0-9]/.test(passwordInput.value) && passwordInput.value === confirmInput.value;
            const currentStep = passwordComplete ? 3 : 2;
            progressSteps.forEach(step => {
                const stepNumber = Number(step.querySelector('b')?.textContent || 0);
                step.classList.toggle('active', stepNumber === currentStep);
                step.classList.toggle('complete', stepNumber < currentStep);
            });
        }

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

        passwordInput.addEventListener('input', () => {
            updatePasswordRequirements();
            updateProgress();
        });
        confirmInput.addEventListener('input', () => {
            updatePasswordRequirements();
            updateProgress();
        });
        otpInput.addEventListener('input', updateProgress);
        updateProgress();

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
                    token: resetToken,
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
