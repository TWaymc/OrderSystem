import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../environments/environment';
import { CreateProduct, Product, UpdateProduct } from '../_models/products.models';

@Injectable({
  providedIn: 'root',
})
export class ProductsService {
  private readonly productsUrl = `${environment.apiBaseUrl}/products`;

  constructor(private readonly http: HttpClient) {}

  getAll(): Observable<Product[]> {
    return this.http.get<Product[]>(this.productsUrl);
  }

  getById(id: string): Observable<Product> {
    return this.http.get<Product>(`${this.productsUrl}/${id}`);
  }

  create(dto: CreateProduct): Observable<Product> {
    return this.http.post<Product>(this.productsUrl, dto);
  }

  update(id: string, dto: UpdateProduct): Observable<Product> {
    return this.http.put<Product>(`${this.productsUrl}/${id}`, dto);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.productsUrl}/${id}`);
  }
}
