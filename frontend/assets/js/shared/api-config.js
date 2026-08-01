// API environment configuration: defines base URLs for development, Docker, and production.
// API Configuration
// Change this based on your environment
const API_CONFIG = {
    // Development: local API
    development: 'https://localhost:5251/api',
    
    // Docker: API running in Docker container
    docker: 'http://localhost:5080/api',
    
    // Production: Replace with your production API URL
    production: 'https://api.premiervenue.com/api'
};

// Auto-detect environment or set manually
const ENVIRONMENT = 'docker'; // Change to 'docker' or 'production' as needed

const API_BASE_URL = API_CONFIG[ENVIRONMENT];

console.log(`API Configuration: ${ENVIRONMENT} -> ${API_BASE_URL}`);