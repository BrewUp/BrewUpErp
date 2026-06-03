export interface IndirizzoJson {
  via: string;
  numeroCivico: string;
  citta: string;
  provincia: string;
  cap: string;
  nazione: string;
}

export interface CustomerJson {
  customerId: string;
  ragioneSociale: string;
  partitaIva: string;
  consumerLevel?: string;
  indirizzo?: IndirizzoJson;
}

export interface CreateCustomerJson {
  ragioneSociale: string;
  partitaIva: string;
  indirizzo?: IndirizzoJson;
}

export interface EditCustomerJson {
  customerId: string;
  ragioneSociale: string;
  partitaIva: string;
  indirizzo?: IndirizzoJson;
}
