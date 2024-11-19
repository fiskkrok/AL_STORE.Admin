export interface Product {
    id: string;
    name: string;
    description: string;
    price: number;
    currency: string;
    stock: number;
    category: {
        id: string;
        name: string;
        description: string;
    };
    subCategory: any;
    images: {
        id: string;
        url: string;
        fileName: string;
        size: number;
    }[];
    createdAt: string;
    createdBy: any;
    lastModifiedAt: any;
    lastModifiedBy: any;
}
