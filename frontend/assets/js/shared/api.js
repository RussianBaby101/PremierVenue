// Shared API client and endpoint helpers for Venue, Booking, Auth, User, SavedVenue, Payment, Document and Task APIs
// API Configuration
let _apiBaseUrl = 'https://localhost:5001/api';
let _authRedirecting = false;

try {
    if (typeof API_BASE_URL !== 'undefined') {
        _apiBaseUrl = API_BASE_URL;
    } else if (typeof API_CONFIG !== 'undefined' && typeof ENVIRONMENT !== 'undefined') {
        _apiBaseUrl = API_CONFIG[ENVIRONMENT] || _apiBaseUrl;
    }
} catch (e) {
    console.log('Using default API URL');
}

// API Helper Functions
class ApiClient {
    static getBaseUrl() {
        return _apiBaseUrl;
    }

    static clearSessionAndRedirect() {
        localStorage.removeItem('token');
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('user');
        if (_authRedirecting) return;
        _authRedirecting = true;
        const returnUrl = `${window.location.pathname}${window.location.search}`;
        window.location.href = `/pages/public/login.html?reason=session-expired&returnUrl=${encodeURIComponent(returnUrl)}`;
    }

    static async refreshAccessToken() {
        const refreshToken = localStorage.getItem('refreshToken');
        if (!refreshToken) return false;

        const response = await fetch(`${_apiBaseUrl}/auth/refresh`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ refreshToken })
        });
        if (!response.ok) return false;

        const result = await response.json();
        if (!result.success || !result.data?.token) return false;
        localStorage.setItem('token', result.data.token);
        if (result.data.refreshToken) localStorage.setItem('refreshToken', result.data.refreshToken);
        if (result.data.user) localStorage.setItem('user', JSON.stringify(result.data.user));
        return true;
    }

    static async request(endpoint, options = {}, retryOnUnauthorized = true) {
        const url = `${_apiBaseUrl}${endpoint}`;
        const token = localStorage.getItem('token');
        
        const headers = {
            ...(options.body instanceof FormData ? {} : { 'Content-Type': 'application/json' }),
            ...options.headers
        };
        
        if (token) {
            headers['Authorization'] = `Bearer ${token}`;
        }
        
        try {
            const response = await fetch(url, {
                ...options,
                headers
            });
            
            if (response.status === 401 && token && retryOnUnauthorized && !endpoint.startsWith('/auth/')) {
                try {
                    if (await ApiClient.refreshAccessToken()) {
                        return ApiClient.request(endpoint, options, false);
                    }
                } catch (refreshError) {
                    console.warn('Session refresh failed:', refreshError);
                }
                ApiClient.clearSessionAndRedirect();
                const sessionError = new Error('Your session has expired. Please log in again.');
                sessionError.status = 401;
                sessionError.sessionExpired = true;
                throw sessionError;
            }

            if (!response.ok) {
                const contentType = response.headers.get('content-type');
                let errorMessage = `API request failed: ${response.status} ${response.statusText}`;
                let errorData = null;
                
                if (contentType && contentType.includes('application/json')) {
                    errorData = await response.json();
                    if (errorData.message) {
                        errorMessage = errorData.message;
                    } else if (errorData.errors && Array.isArray(errorData.errors)) {
                        errorMessage = errorData.errors.join('\n');
                    }
                }
                
                const error = new Error(errorMessage);
                error.status = response.status;
                error.data = errorData;
                throw error;
            }
            
            if (response.status === 204) return null;
            return await response.json();
        } catch (error) {
            if (!error.sessionExpired) console.error('API Error:', error);
            throw error;
        }
    }
    
    static async get(endpoint) {
        return this.request(endpoint, { method: 'GET' });
    }
    
    static async post(endpoint, data) {
        return this.request(endpoint, {
            method: 'POST',
            body: JSON.stringify(data)
        });
    }
    
    static async put(endpoint, data) {
        return this.request(endpoint, {
            method: 'PUT',
            body: JSON.stringify(data)
        });
    }
    
    static async patch(endpoint, data) {
        return this.request(endpoint, {
            method: 'PATCH',
            body: JSON.stringify(data)
        });
    }
    
    static async delete(endpoint) {
        return this.request(endpoint, { method: 'DELETE' });
    }
}

// Venue API
const VenueApi = {
    async getAll(page = 1, pageSize = 10, includeInactive = false, sortBy = '') {
        const params = new URLSearchParams({ page, pageSize });
        if (includeInactive) params.set('includeInactive', 'true');
        if (sortBy) params.set('sortBy', sortBy);
        return ApiClient.get(`/venues?${params}`);
    },
    
    async search(searchParams, page = 1, pageSize = 10) {
        return ApiClient.post(`/venues/search?page=${page}&pageSize=${pageSize}`, searchParams);
    },

    async getEventTypes() {
        return ApiClient.get('/event-types');
    },
    
    async getById(id) {
        return ApiClient.get(`/venues/${id}`);
    },
    
    async create(venueData) {
        return ApiClient.post('/venues', venueData);
    },
    
    async update(id, venueData) {
        return ApiClient.put(`/venues/${id}`, venueData);
    },
    
    async toggleStatus(id) {
        return ApiClient.patch(`/venues/${id}/toggle-status`);
    },

    async uploadPhotos(id, files, primaryPhotoIndex = null) {
        const formData = new FormData();
        files.forEach(file => formData.append('files', file));
        if (primaryPhotoIndex !== null) formData.append('primaryPhotoIndex', primaryPhotoIndex);
        return ApiClient.request(`/venues/${id}/photos`, { method: 'POST', body: formData });
    },

    async deletePhoto(venueId, photoId) {
        return ApiClient.delete(`/venues/${venueId}/photos/${photoId}`);
    },

    async setPrimaryPhoto(venueId, photoId) {
        return ApiClient.patch(`/venues/${venueId}/photos/${photoId}/primary`);
    }
};

// Booking API
const BookingApi = {
    async create(bookingData) {
        return ApiClient.post('/bookings', bookingData);
    },
    
    async getByReferenceNumber(referenceNumber) {
        return ApiClient.get(`/bookings/reference/${referenceNumber}`);
    },

    async getById(id) {
        return ApiClient.get(`/bookings/${id}`);
    },
    
    async getClientBookings(clientId, page = 1, pageSize = 10) {
        return ApiClient.get(`/bookings/client/${clientId}?page=${page}&pageSize=${pageSize}`);
    },
    
    async getMyBookings(page = 1, pageSize = 10) {
        return ApiClient.get(`/bookings/my?page=${page}&pageSize=${pageSize}`);
    },
    
    async getAll(page = 1, pageSize = 10) {
        return ApiClient.get(`/bookings?page=${page}&pageSize=${pageSize}`);
    },
    
    async getPending(page = 1, pageSize = 10) {
        return ApiClient.get(`/bookings/pending?page=${page}&pageSize=${pageSize}`);
    },
    
    async updateStatus(bookingId, statusData) {
        return ApiClient.patch(`/bookings/${bookingId}/status`, statusData);
    },

    async sendQuote(quoteData) {
        return ApiClient.post('/bookings/quote', quoteData);
    },

    async decideQuote(bookingId, accepted, notes = '') {
        return ApiClient.post(`/bookings/${bookingId}/quote-decision`, { accepted, notes });
    }
};

// Auth API
const AuthApi = {
    async login(email, password) {
        return ApiClient.post('/auth/login', { email, password });
    },
    
    async register(userData) {
        return ApiClient.post('/auth/register', userData);
    },
    
    async refreshToken(refreshToken) {
        return ApiClient.post('/auth/refresh', { refreshToken });
    },

    async getInvitation(token) {
        return ApiClient.get(`/auth/invitation?token=${encodeURIComponent(token)}`);
    },

    async acceptInvitation(data) {
        return ApiClient.post('/auth/accept-invitation', data);
    },

    async requestPasswordReset(email) {
        return ApiClient.post('/auth/forgot-password', { email });
    },

    async resetPassword(data) {
        return ApiClient.post('/auth/reset-password', data);
    },
    
    async logout() {
        localStorage.removeItem('token');
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('user');
    }
};

// SavedVenue API
const SavedVenueApi = {
    async getAll() {
        return ApiClient.get('/saved-venues');
    },

    async isSaved(venueId) {
        return ApiClient.get(`/saved-venues/${venueId}/exists`);
    },

    async save(venueId) {
        return ApiClient.post(`/saved-venues/${venueId}`);
    },

    async remove(venueId) {
        return ApiClient.delete(`/saved-venues/${venueId}`);
    }
};

// User API
const UserApi = {
    async getMyProfile() {
        return ApiClient.get('/users/me');
    },

    async updateMyProfile(profileData) {
        return ApiClient.put('/users/me', profileData);
    },

    async getAll(role = '') {
        const roleParam = role ? `?role=${encodeURIComponent(role)}` : '';
        return ApiClient.get(`/users${roleParam}`);
    },
    
    async getById(id) {
        return ApiClient.get(`/users/${id}`);
    },
    
    async toggleStatus(id) {
        return ApiClient.patch(`/users/${id}/toggle-status`, {});
    },

    async createStaffInvitation(invitationData) {
        return ApiClient.post('/users/invitations', invitationData);
    }
};

// Payment API
const PaymentApi = {
    async initiatePayment(paymentData) {
        return ApiClient.post('/payments/initiate', paymentData);
    },
    
    async getBookingPayments(bookingId) {
        return ApiClient.get(`/payments/booking/${bookingId}`);
    }
};

// Document API
const DocumentApi = {
    async getBookingDocuments(bookingId) {
        return ApiClient.get(`/documents/booking/${bookingId}`);
    },
    
    async uploadDocument(formData) {
        const token = localStorage.getItem('token');
        const response = await fetch(`${_apiBaseUrl}/documents/upload`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`
            },
            body: formData
        });
        
        if (!response.ok) {
            throw new Error('Document upload failed');
        }
        
        return await response.json();
    },

    async downloadDocument(id) {
        const token = localStorage.getItem('token');
        const response = await fetch(`${_apiBaseUrl}/documents/${id}/download`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        if (!response.ok) throw new Error('Document download failed');
        const blob = await response.blob();
        const url = URL.createObjectURL(blob);
        const anchor = document.createElement('a');
        anchor.href = url;
        anchor.download = 'booking-document.pdf';
        anchor.click();
        URL.revokeObjectURL(url);
    }
};

// Task API
const TaskApi = {
    async getBookingTasks(bookingId) {
        return ApiClient.get(`/tasks/booking/${bookingId}`);
    },
    
    async createTask(taskData) {
        return ApiClient.post('/tasks', taskData);
    },
    
    async updateTask(taskId, taskData) {
        return ApiClient.put(`/tasks/${taskId}`, taskData);
    },
    
    async completeTask(taskId) {
        return ApiClient.put(`/tasks/${taskId}/complete`);
    }
};