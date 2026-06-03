import { Quantity } from '../../../shared/models/quantity.model';

export interface OrderRowDto {
  beerId?: string;
  beerName?: string;
  quantity?: Quantity;
}

export interface ShipmentJson {
  id: string;
  salesOrderId?: string;
  customerId?: string;
  deliveryDate?: string;
  rows?: OrderRowDto[];
  shipmentState?: string;
}
