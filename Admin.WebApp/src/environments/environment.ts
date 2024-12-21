export const environment = {
    production: false,

    // API URLs
    apiUrls: {
        // Admin Portal API
        admin: {
            baseUrl: 'https://localhost:7048/api',
            products: 'https://localhost:7048/api/products',
            categories: 'https://localhost:7048/api/categories',
            statistics: 'https://localhost:7048/api/statistics',
            auth: 'https://localhost:7048/api/auth'
        },
        // Shop Portal API
        shop: {
            baseUrl: 'http://localhost:5002/api',
            products: 'http://localhost:5002/api/products',
            cart: 'http://localhost:5002/api/cart',
            orders: 'http://localhost:5002/api/orders'
        },
        // Shipping Portal API
        shipping: {
            baseUrl: 'http://localhost:5003/api',
            shipments: 'http://localhost:5003/api/shipments',
            tracking: 'http://localhost:5003/api/tracking'
        }
    },

    // Azure Storage
    azure: {
        blobStorage: {
            containerUrl: 'https://yourstorageaccount.blob.core.windows.net',
            productsContainer: 'products-images'
        }
    },

    // Authentication
    auth: {
        authority: 'https://localhost:5001',
        clientId: 'admin_portal', // Note: Using the SPA client ID
        redirectUri: window.location.origin + '/callback',
        postLogoutRedirectUri: window.location.origin,
        responseType: 'code',
        scope: 'openid profile email api.full',
        requireHttps: true
    },

    // Feature Flags
    features: {
        enableAnalytics: true,
        enableNotifications: true,
        useSignalR: true
    },

    // SignalR
    signalR: {
        hubUrl: 'http://localhost:5001/hubs/notifications'
    },

    // Cache Configuration
    cache: {
        defaultTTL: 300, // 5 minutes
        productsTTL: 600 // 10 minutes
    }
};