export interface SalesByCustomerJson {
  customerId?: string;
  customerName?: string;
  year?: string;
  totalSales?: number;
  currency?: string;
}

export interface SalesByProductsJson {
  productId?: string;
  productName?: string;
  year?: string;
  totalSales?: number;
  currency?: string;
  quantity?: number;
  unitOfMeasure?: string;
}
