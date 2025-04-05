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
            stock: 'https://localhost:7048/api/stock',
            auth: 'https://localhost:7048/api/auth',
            orders: 'https://localhost:7048/api/orders',
        },
        // Shop Portal API
        shop: {
            baseUrl: 'http://localhost:7002/api',
            products: 'http://localhost:7002/api/products',
            cart: 'http://localhost:7002/api/cart',
            orders: 'http://localhost:7002/api/orders'
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
            containerUrl: 'http://localhost:10000/devstoreaccount1/alstoreblob',
            productsContainer: 'Product_Pictures'
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
        useSignalR: true,
        enableCache: true,
        enableAuth: true,
        enableShoppingCart: true,
        enableShipping: true,

    },

    // SignalR
    signalR: {
        hubUrlnotifications: 'https://localhost:7048/hubs/notifications',
        product: 'https://localhost:7048/hubs/products',
        category: 'https://localhost:7048/hubs/categories',
        stock: 'https://localhost:7048/hubs/stock',
        order: 'https://localhost:7048/hubs/orders'
    },

    // Cache Configuration
    cache: {
        defaultTTL: 300, // 5 minutes
        productsTTL: 600 // 10 minutes
    }
};