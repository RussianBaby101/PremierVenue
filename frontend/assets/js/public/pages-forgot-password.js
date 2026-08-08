// Forgot password: sends a password reset request and keeps the user on a check-email confirmation.
const forgotPasswordForm = document.getElementById('forgotPasswordForm');
const resetEmailConfirmation = document.getElementById('resetEmailConfirmation');
const sendAnotherCode = document.getElementById('sendAnotherCode');

function showResetForm() {
    forgotPasswordForm?.classList.remove('d-none');
    resetEmailConfirmation?.classList.add('d-none');
}

forgotPasswordForm?.addEventListener('submit', async function (event) {
            event.preventDefault();

            const email = document.getElementById('email').value.trim();
            const submitButton = document.getElementById('submitButton');
            submitButton.disabled = true;
            submitButton.innerHTML = '<i class="bi bi-hourglass-split"></i> Sending...';

            try {
                await AuthApi.requestPasswordReset(email);
                forgotPasswordForm.classList.add('d-none');
                resetEmailConfirmation?.classList.remove('d-none');
                await SwalUtils.success('Check Your Email', 'Open the secure reset button in the email to continue.');
            } catch (error) {
                SwalUtils.error('Unable to Send Code', error.message || 'Please try again.');
                submitButton.disabled = false;
                submitButton.innerHTML = '<i class="bi bi-send"></i> Send Reset Code';
            }
        });

sendAnotherCode?.addEventListener('click', () => {
    showResetForm();
    document.getElementById('email')?.focus();
});
