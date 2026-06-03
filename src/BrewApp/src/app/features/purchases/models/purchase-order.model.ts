import { Price } from '../../../shared/models/price.model';
import { Quantity } from '../../../shared/models/quantity.model';

export interface PurchaseOrderRowJson {
  beerId: string;
  beerName: string;
  quantity: Quantity;
  price: Price;
}

export interface CreatePurchaseOrderJson {
  supplierId: string;
  rows: PurchaseOrderRowJson[];
}
