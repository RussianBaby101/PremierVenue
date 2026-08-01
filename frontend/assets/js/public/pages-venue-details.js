// Public venue details: shows detailed venue information with photo slideshow for public visitors.
document.addEventListener('DOMContentLoaded', async function () {
            await VenuesPages.init({ mode: 'public', page: 'detail' });

            // Initialize venue slideshow
            initVenueSlideshow();

            // Toggle extra details login prompt
            const moreInfoContent = document.getElementById('venueMoreInfoContent');
            const loginPrompt = document.getElementById('venueLoginPrompt');
            if (moreInfoContent && loginPrompt) {
                if (typeof isAuthenticated === 'function' && isAuthenticated()) {
                    moreInfoContent.classList.remove('more-info-locked');
                    loginPrompt.classList.add('d-none');
                } else {
                    moreInfoContent.classList.add('more-info-locked');
                }
            }
        });

        // Venue Slideshow
        function initVenueSlideshow() {
            // Placeholder images - will be replaced with actual venue images from API
            const uploadedImages = (window.currentVenue?.photos || []).map(photo => window.resolveVenueImage(photo.url)).filter(Boolean);
            const venueImages = uploadedImages.length ? uploadedImages : [
                window.resolveVenueImage(window.currentVenue?.imageUrl),
                'https://images.unsplash.com/photo-1519167758481-83f550bb49b3?w=800&h=600&fit=crop',
                'https://images.unsplash.com/photo-1464366400600-7168b8af9bc3?w=800&h=600&fit=crop'
            ].filter(Boolean);

            let currentImageIndex = 0;
            const mainImage = document.getElementById('mainImage');
            const thumbnailContainer = document.querySelector('.thumbnail-container');

            // Set initial main image
            if (mainImage && venueImages.length > 0) {
                mainImage.src = venueImages[0];
            }

            // Create thumbnails
            if (thumbnailContainer) {
                venueImages.forEach((imageSrc, index) => {
                    const thumbnail = document.createElement('img');
                    thumbnail.src = imageSrc;
                    thumbnail.alt = `Venue image ${index + 1}`;
                    thumbnail.className = `thumbnail ${index === 0 ? 'active' : ''}`;
                    thumbnail.addEventListener('click', () => {
                        setCurrentImage(index);
                    });
                    thumbnailContainer.appendChild(thumbnail);
                });
            }

            // Set current image
            function setCurrentImage(index) {
                currentImageIndex = index;
                if (mainImage) {
                    mainImage.style.opacity = '0';
                    setTimeout(() => {
                        mainImage.src = venueImages[index];
                        mainImage.style.opacity = '1';
                    }, 300);
                }

                // Update thumbnail active states
                const thumbnails = thumbnailContainer.querySelectorAll('.thumbnail');
                thumbnails.forEach((thumb, i) => {
                    thumb.classList.toggle('active', i === index);
                });
            }

            // Auto-rotate images every 5 seconds
            setInterval(() => {
                const nextIndex = (currentImageIndex + 1) % venueImages.length;
                setCurrentImage(nextIndex);
            }, 5000);
        }
