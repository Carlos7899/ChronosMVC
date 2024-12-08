const CACHE_NAME = 'chronos-cache-v1';
const urlsToCache = [
    '/', // Página inicial
    '/manifest.json',
    '/icon512_maskable.png',
    '/icon512_rounded.png',
    '/css/styles.css', // Adicione outros arquivos CSS, se houver
    '/js/scripts.js', // Adicione outros arquivos JS, se houver
];

self.addEventListener('install', (event) => {
    console.log('[Service Worker] Instalando...');
    event.waitUntil(
        caches.open(CACHE_NAME).then((cache) => {
            console.log('[Service Worker] Fazendo cache dos recursos...');
            return cache.addAll(urlsToCache);
        })
    );
});

self.addEventListener('activate', (event) => {
    console.log('[Service Worker] Ativando...');
    event.waitUntil(
        caches.keys().then((cacheNames) => {
            return Promise.all(
                cacheNames.map((cacheName) => {
                    if (cacheName !== CACHE_NAME) {
                        console.log('[Service Worker] Removendo cache antigo:', cacheName);
                        return caches.delete(cacheName);
                    }
                })
            );
        })
    );
});

self.addEventListener('fetch', (event) => {
    console.log('[Service Worker] Interceptando requisição para:', event.request.url);
    event.respondWith(
        caches.match(event.request).then((response) => {
            return response || fetch(event.request);
        })
    );
});
