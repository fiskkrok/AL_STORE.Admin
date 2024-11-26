export const environment = {
    production: true,

    apiUrls: {
        admin: {
            baseUrl: 'https://api.yourcompany.com/admin',
            products: 'https://api.yourcompany.com/admin/products',
            statistics: 'https://api.yourcompany.com/admin/statistics',
            auth: 'https://api.yourcompany.com/admin/auth'
        },
        shop: {
            baseUrl: 'https://api.yourcompany.com/shop',
            products: 'https://api.yourcompany.com/shop/products',
            cart: 'https://api.yourcompany.com/shop/cart',
            orders: 'https://api.yourcompany.com/shop/orders'
        },
        shipping: {
            baseUrl: 'https://api.yourcompany.com/shipping',
            shipments: 'https://api.yourcompany.com/shipping/shipments',
            tracking: 'https://api.yourcompany.com/shipping/tracking'
        }
    },

    azure: {
        blobStorage: {
            containerUrl: 'https://prod-storage.blob.core.windows.net',
            productsContainer: 'products-images'
        }
    },

    identityServer: {
        authority: 'https://identity.al_store.com',
        clientId: 'admin-portal',
        redirectUri: window.location.origin + '/callback',
        postLogoutRedirectUri: window.location.origin,
        responseType: 'code',
        scope: 'openid profile email api.full'
    },

    features: {
        enableAnalytics: true,
        enableNotifications: true,
        useSignalR: true
    },

    signalR: {
        hubUrl: 'https://api.yourcompany.com/hubs/notifications'
    },

    cache: {
        defaultTTL: 300,
        productsTTL: 600
    }
};