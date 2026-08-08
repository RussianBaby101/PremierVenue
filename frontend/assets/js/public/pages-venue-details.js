// Public venue details: shows detailed venue information with photo slideshow for public visitors.
document.addEventListener('DOMContentLoaded', async function () {
            await VenuesPages.init({ mode: 'public', page: 'detail' });

            // Initialize venue slideshow
            initVenueSlideshow();

        });

        const initVenueSlideshow = window.initVenueSlideshow;
