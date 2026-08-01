// Client saved venues: displays and manages the client's favourite venues.
document.addEventListener('DOMContentLoaded', async function () {
            if (!requireAuth()) return;
            const grid = document.getElementById('savedVenuesGrid');
            try {
                const savedVenues = await SavedVenueApi.getAll();
                if (!savedVenues.length) {
                    grid.innerHTML = '<div class="col-12"><div class="empty-dashboard"><i class="bi bi-heart"></i><p>You have not saved any venues yet.</p><a href="venues.html" class="btn btn-primary">Browse Venues</a></div></div>';
                    return;
                }
                grid.innerHTML = savedVenues.map(saved => {
                    const venue = saved.venue;
                    const imagePath = venue.thumbnailUrl || venue.imageUrl || venue.photos?.[0]?.url;
                    const image = imagePath?.startsWith('/') ? `${API_BASE_URL.replace(/\/api\/?$/, '')}${imagePath}` : imagePath || 'https://images.unsplash.com/photo-1519167758481-83f550bb49b3?w=800&h=600&fit=crop';
                    return `<div class="col-md-6 col-xl-4"><div class="card venue-card h-100"><img src="${image}" class="card-img-top" alt="${venue.name}"><div class="card-body"><div class="d-flex justify-content-between gap-2"><h5 class="card-title">${venue.name}</h5><button class="save-venue-btn saved" aria-label="Remove ${venue.name}" data-action="remove-saved-venue" data-venue-id="${venue.id}"><i class="bi bi-heart-fill"></i></button></div><p class="text-muted"><i class="bi bi-geo-alt"></i> ${venue.city}, ${venue.province}</p><p><i class="bi bi-people"></i> Capacity: ${venue.capacity}</p><p class="venue-price">From ${formatCurrency(venue.basePricePerDay)}<small class="text-muted">/day</small></p></div><div class="card-footer bg-white border-top-0"><a class="btn btn-outline-primary w-100" href="venue-details.html?id=${venue.id}">View Details</a></div></div></div>`;
                }).join('');
            } catch (error) {
                grid.innerHTML = '<div class="col-12"><div class="alert alert-danger">Unable to load saved venues.</div></div>';
            }
        });

        async function removeSavedVenue(venueId, button) {
            try {
                await SavedVenueApi.remove(venueId);
                button.closest('.col-md-6').remove();
            } catch (error) {
                showError(error.message || 'Unable to remove saved venue.');
            }
        }
