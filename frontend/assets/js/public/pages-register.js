// Public registration: validates password requirements and creates a new client account.
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

        document.getElementById('registerForm').addEventListener('submit', async function (e) {
            e.preventDefault();

            const password = passwordInput.value;
            if (password.length < 8 || !/[A-Z]/.test(password) || !/[a-z]/.test(password) || !/[0-9]/.test(password)) {
                await SwalUtils.error('Password Requirements', 'Please meet every password requirement before continuing.');
                return;
            }
            if (password !== confirmPasswordInput.value) {
                await SwalUtils.error('Passwords Do Not Match', 'Please make sure both password fields match.');
                return;
            }

            const userData = {
                firstName: document.getElementById('firstName').value.trim(),
                lastName: document.getElementById('lastName').value.trim(),
                email: document.getElementById('email').value.trim(),
                phoneNumber: document.getElementById('phoneNumber').value.trim(),
                password
            };
            const submitButton = this.querySelector('button[type="submit"]');
            submitButton.disabled = true;
            submitButton.textContent = 'Creating Account...';

            try {
                const result = await AuthApi.register(userData);
                if (result.success && result.data) {
                    localStorage.setItem('token', result.data.token);
                    localStorage.setItem('refreshToken', result.data.refreshToken);
                    localStorage.setItem('user', JSON.stringify(result.data.user));
                    await SwalUtils.success('Account Created', 'Your account was created successfully.');
                    window.location.href = '/pages/client/dashboard.html';
                } else {
                    await SwalUtils.error('Registration Failed', result.message || 'Please try again.');
                }
            } catch (error) {
                await SwalUtils.error('Registration Failed', error.message || 'Please try again.');
            } finally {
                submitButton.disabled = false;
                submitButton.textContent = 'Create Account';
            }
        });
