export interface Product {
  id: string;
  code: string;
  name: string;
  price: number;
  description: string;
  createdBy: string;
  lastModifiedBy: string;
  createdAt: string;
  modifiedAt: string | null;
}

export interface CreateProduct {
  name: string;
  price: number;
  description: string;
}

export interface UpdateProduct {
  name: string;
  price: number;
  description: string;
}
