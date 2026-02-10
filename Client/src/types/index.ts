// src/types/index.ts

// --- GENEL ---
export interface PaginatedResult<T> {
    items: T[];
    total: number;
    page: number;
    pageSize: number;
    totalPages?: number;
}

// --- AUTH ---
export interface User {
    id: number;
    fullName: string;
    email: string;
    token?: string;
    role?: string;
}

// --- KATEGORİLER (Category) ---  <-- YENİ EKLENDİ
export interface Category {
    id: number;
    name: string;
    type: number; // 0: Standart, 1: Giyim
    productCount?: number;
}

// --- MÜŞTERİLER (Customer) ---
export interface Customer {
    id: number;
    name: string;
    email?: string;
    phone?: string;
    instagramHandle?: string;
    address?: string;
    status?: 'active' | 'passive';
    totalOrders?: number;
    totalSpent?: number;
}

// --- ÜRÜNLER (Product) ---
export interface Product {
    id: number;
    name: string;
    price: number;
    stock: number;
    sku?: string;       
    code?: string;      
    categoryId?: number;
    imageUrl?: string;  
    imagePath?: string; 
    purchasePrice?: number;
    
    // UI ve Tablo için eklenen alanlar
    description?: string;
    categoryName?: string;
    typeName?: string;          
    size?: string;
    color?: string;
    averageUnitCostInTRY?: number;
    discountedPrice?: number;
    shippingType?: number;      
    totalSales?: number;
}

// --- ALIMLAR (Purchase) ---
export interface Purchase {
    id: number;
    productId: number;
    product?: Product; 
    purchaseDate: string; 
    quantityInUnits: number;
    productPricePerUnit: number;
    totalKg: number;
    shippingCostPerKg: number;
    totalCost: number;
    description?: string;
    
    quantity?: number;
    price?: number;
    productName?: string;
}

// --- SİPARİŞLER (Sale/Order) ---
export interface SaleItem {
    productId: number;
    productName: string;
    quantity: number;
    unitPrice: number;
    total: number;
}

export interface Sale {
    id: number;
    customerId?: number;
    customerName?: string;
    date: string;
    totalAmount: number;
    shippingStatus: 'Pending' | 'Shipped' | 'Delivered' | 'Cancelled';
    paymentStatus: 'Paid' | 'Unpaid' | 'Partial';
    items?: SaleItem[];
    notes?: string;
}

// --- FİNANS (Expense/Payment) ---
export interface Expense {
    id: number;
    description: string;
    amount: number;
    date: string;
    category?: string;
    type: 'expense';
}

export interface Payment {
    id: number;
    description: string;
    amount: number;
    date: string;
    customerId?: number;
    customerName?: string;
    type: 'collection';
}

// --- DASHBOARD ---
export interface DashboardStats {
    totalRevenue: number;
    netProfit: number;
    totalOrders: number;
    totalCustomers: number;
    totalProducts: number;
    totalStockCount: number;
    criticalStockCount: number;
    outOfStockCount: number;
    totalInventoryValue: number;
    potentialRevenue: number;
    revenueRate: number;
    orderRate: number;
    customerRate: number;
    totalShippingCost: number;
    totalOperationalExpenses: number;
    last6MonthsLabels?: string[];
    last6MonthsRevenue?: number[];
}