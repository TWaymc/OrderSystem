import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';

import { CreateProduct, Product, UpdateProduct } from '../../_models/products.models';
import { ProductsService } from '../../_services/products.service';

@Component({
  selector: 'app-products-page',
  imports: [FormsModule],
  templateUrl: './products-page.html',
  styleUrl: './products-page.scss',
})
export class ProductsPage implements OnInit {
  products = signal<Product[]>([]);
  selectedProductId = signal<string | null>(null);
  isLoading = signal(false);
  isSubmitting = signal(false);
  isDeleting = signal(false);
  errorMessage = signal('');

  name = '';
  price: number | null = null;
  description = '';

  constructor(private readonly productsService: ProductsService) {}

  ngOnInit(): void {
    this.loadProducts();
  }

  loadProducts(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.productsService
      .getAll()
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (products) => {
          this.products.set(products);
        },
        error: () => {
          this.errorMessage.set('Unable to load products.');
        },
      });
  }

  get isEditMode(): boolean {
    return this.selectedProductId() !== null;
  }

  selectProduct(product: Product): void {
    this.selectedProductId.set(product.id);
    this.name = product.name;
    this.price = product.price;
    this.description = product.description;
    this.errorMessage.set('');
  }

  clearSelection(): void {
    this.selectedProductId.set(null);
    this.name = '';
    this.price = null;
    this.description = '';
    this.errorMessage.set('');
  }

  save(): void {
    if (this.isSubmitting() || this.isDeleting()) {
      return;
    }

    const name = this.name.trim();
    const description = this.description.trim();
    const price = this.price;

    if (!name || !description || price === null || Number.isNaN(price)) {
      this.errorMessage.set('Name, price and description are required.');
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set('');

    const payload: CreateProduct | UpdateProduct = {
      name,
      price,
      description,
    };

    const selectedId = this.selectedProductId();
    const request$ = selectedId
      ? this.productsService.update(selectedId, payload)
      : this.productsService.create(payload);

    request$
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: (savedProduct) => {
          const current = this.products();
          const index = current.findIndex((product) => product.id === savedProduct.id);

          if (index >= 0) {
            const updated = [...current];
            updated[index] = savedProduct;
            this.products.set(updated);
          } else {
            this.products.set([savedProduct, ...current]);
          }

          this.clearSelection();
        },
        error: () => {
          this.errorMessage.set('Unable to save product.');
        },
      });
  }

  deleteSelected(): void {
    const selectedId = this.selectedProductId();

    if (!selectedId || this.isDeleting() || this.isSubmitting()) {
      return;
    }

    this.isDeleting.set(true);
    this.errorMessage.set('');

    this.productsService
      .delete(selectedId)
      .pipe(finalize(() => this.isDeleting.set(false)))
      .subscribe({
        next: () => {
          this.products.set(this.products().filter((product) => product.id !== selectedId));
          this.clearSelection();
        },
        error: () => {
          this.errorMessage.set('Unable to delete product.');
        },
      });
  }
}
