// Forgot password: sends a password reset request and redirects to the reset page.
document.getElementById('forgotPasswordForm').addEventListener('submit', async function (event) {
            event.preventDefault();

            const email = document.getElementById('email').value.trim();
            const submitButton = document.getElementById('submitButton');
            submitButton.disabled = true;
            submitButton.textContent = 'Sending...';

            try {
                await AuthApi.requestPasswordReset(email);
                await SwalUtils.success('Reset Code Sent', 'Check your email for the six-digit code.');
                window.location.href = `reset-password.html?email=${encodeURIComponent(email)}`;
            } catch (error) {
                SwalUtils.error('Unable to Send Code', error.message || 'Please try again.');
                submitButton.disabled = false;
                submitButton.textContent = 'Send Reset Code';
            }
        });
