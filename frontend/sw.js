const CACHE_NAME = 'premiervenue-v5';
const urlsToCache = [
    '/',
    '/index.html',
    '/pages/public/venues.html',
    '/pages/public/contact-us.html',
    '/pages/public/login.html',
    '/pages/public/register.html',
    '/pages/client/dashboard.html',
    '/pages/staff/dashboard.html',
    '/assets/css/styles.css',
    '/assets/js/shared/main.js',
    '/assets/js/shared/api.js',
    '/assets/images/PremierVenueFavicon.png',
    '/assets/images/PremierVenueLogoNoBg.png',
    '/manifest.json'
];

self.addEventListener('install', event => {
    self.skipWaiting();
    event.waitUntil(
        caches.open(CACHE_NAME)
            .then(cache => cache.addAll(urlsToCache))
    );
});

self.addEventListener('fetch', event => {
    event.respondWith(
        caches.match(event.request)
            .then(response => {
                if (response) {
                    return response;
                }
                return fetch(event.request).catch(() => caches.match('/index.html'));
            })
    );
});

self.addEventListener('activate', event => {
    event.waitUntil(self.clients.claim());
    event.waitUntil(
        caches.keys().then(cacheNames => {
            return Promise.all(
                cacheNames.map(cacheName => {
                    if (cacheName !== CACHE_NAME) {
                        return caches.delete(cacheName);
                    }
                })
            );
        })
    );
});