// SweetAlert2 Utility Functions for consistent alert handling across the application

const SwalUtils = {
    // Success alert
    success: (title, text = 'Operation completed successfully') => {
        return Swal.fire({
            icon: 'success',
            title: title,
            text: text,
            confirmButtonColor: '#4a90e2',
            timer: 2000,
            timerProgressBar: true
        });
    },

    // Error alert
    error: (title, text = 'An error occurred') => {
        return Swal.fire({
            icon: 'error',
            title: title,
            text: text,
            confirmButtonColor: '#4a90e2'
        });
    },

    // Warning alert with confirmation
    confirm: (title, text, confirmText = 'Yes, proceed!') => {
        return Swal.fire({
            title: title,
            text: text,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#4a90e2',
            cancelButtonColor: '#d33',
            confirmButtonText: confirmText,
            cancelButtonText: 'Cancel'
        });
    },

    // Info alert
    info: (title, text) => {
        return Swal.fire({
            icon: 'info',
            title: title,
            text: text,
            confirmButtonColor: '#4a90e2'
        });
    },

    // Loading state
    loading: (title = 'Please wait...', text = 'Processing your request') => {
        return Swal.fire({
            title: title,
            text: text,
            allowOutsideClick: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });
    },

    // Close current alert
    close: () => {
        Swal.close();
    },

    // Toast notification (small popup)
    toast: (icon, title, position = 'top-end') => {
        const Toast = Swal.mixin({
            toast: true,
            position: position,
            showConfirmButton: false,
            timer: 3000,
            timerProgressBar: true,
            didOpen: (toast) => {
                toast.addEventListener('mouseenter', Swal.stopTimer);
                toast.addEventListener('mouseleave', Swal.resumeTimer);
            }
        });

        return Toast.fire({
            icon: icon,
            title: title
        });
    },

    // Form validation error
    validationError: (errors) => {
        let errorMessage = 'Please fix the following errors:\n';
        if (Array.isArray(errors)) {
            errorMessage += errors.join('\n');
        } else {
            errorMessage += errors;
        }

        return Swal.fire({
            icon: 'error',
            title: 'Validation Error',
            text: errorMessage,
            confirmButtonColor: '#4a90e2'
        });
    },

    // Async operation wrapper with loading state
    async withLoading(asyncOperation, title = 'Processing...', text = 'Please wait') {
        try {
            SwalUtils.loading(title, text);
            const result = await asyncOperation();
            SwalUtils.close();
            return result;
        } catch (error) {
            SwalUtils.close();
            throw error;
        }
    }
};

// Make it available globally
if (typeof window !== 'undefined') {
    window.SwalUtils = SwalUtils;
}