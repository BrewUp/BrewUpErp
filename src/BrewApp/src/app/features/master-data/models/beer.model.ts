import { Price } from '../../../shared/models/price.model';

export interface BeerJson {
  beerId: string;
  beerName: string;
  beerStyle: string;
  alcoholByVolume?: number;
  packaging?: string;
  price?: Price;
  isActive: boolean;
}

export interface CreateBeerJson {
  beerName: string;
  beerStyle: string;
  price: Price;
  alcoholByVolume?: number;
  packaging?: string;
  isActive?: boolean;
}
