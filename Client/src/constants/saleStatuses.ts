export const SHIPPING_STATUS = {
    RECEIVED: 'Sipariş Alındı',
    PREPARING: 'Hazırlanıyor',
    SHIPPED: 'Kargoya Verildi',
    DELIVERED: 'Teslim Edildi',
    RETURNED: 'İade'
} as const;

export type ShippingStatusValue = typeof SHIPPING_STATUS[keyof typeof SHIPPING_STATUS];

export const PAYMENT_STATUS = {
    PENDING: 'Bekliyor',
    COMPLETED: 'Tamamlandı'
} as const;

export type PaymentStatusValue = typeof PAYMENT_STATUS[keyof typeof PAYMENT_STATUS];