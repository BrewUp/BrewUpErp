import { Price } from '../../../shared/models/price.model';
import { Quantity } from '../../../shared/models/quantity.model';

export type SalesOrderStatus = 'Open' | 'Closed';

export interface SalesOrderRowJson {
  beerId: string;
  beerName: string;
  quantity: Quantity;
  price: Price;
}

export interface SalesOrderJson {
  id: string;
  orderNumber: string;
  orderDate: string;
  customerId: string;
  customerName: string;
  deliveryDate?: string;
  rows: SalesOrderRowJson[];
  status: SalesOrderStatus;
}

export interface CreateSalesOrderJson {
  orderNumber: string;
  orderDate: string;
  customerId: string;
  customerName: string;
  deliveryDate?: string;
  rows: SalesOrderRowJson[];
}

export interface AddBeersToCartJson {
  orderId: string;
  rows: SalesOrderRowJson[];
}
