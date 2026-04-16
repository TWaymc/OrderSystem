import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { Contact } from '../../../_models/contacts.models';
import { Order, OrderItem } from '../../../_models/orders.models';
import { Product } from '../../../_models/products.models';
import { ContactsService } from '../../../_services/contacts.service';
import { OrdersService } from '../../../_services/orders.service';
import { ProductsService } from '../../../_services/products.service';

interface DraftOrderItem extends OrderItem {}

@Component({
  selector: 'app-orders-product-details-page',
  imports: [DatePipe, FormsModule, RouterLink],
  templateUrl: './orders-product-details-page.html',
  styleUrl: './orders-product-details-page.scss',
})
export class OrdersProductDetailsPage implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly ordersService = inject(OrdersService);
  private readonly contactsService = inject(ContactsService);
  private readonly productsService = inject(ProductsService);

  readonly orderId = this.route.snapshot.paramMap.get('id') ?? '';
  readonly isCreateMode = this.orderId === 'new';
  readonly contacts = signal<Contact[]>([]);
  readonly products = signal<Product[]>([]);
  readonly order = signal<Order | null>(null);
  readonly draftItems = signal<DraftOrderItem[]>([]);
  readonly isLoading = signal(false);
  readonly isSaving = signal(false);
  readonly isAddingItem = signal(false);
  readonly removingItemId = signal<string | null>(null);
  readonly customerDropdownOpen = signal(false);
  readonly productDropdownOpen = signal(false);
  readonly errorMessage = signal('');
  readonly successMessage = signal('');

  customerId = '';
  customerSearch = '';
  itemProductId = '';
  productSearch = '';
  itemQuantity = 1;

  readonly orderItems = computed(() => {
    if (this.isCreateMode) {
      return this.draftItems();
    }
    return this.order()?.orderItems ?? [];
  });

  readonly totalItems = computed(() =>
    this.orderItems().reduce((sum, item) => sum + item.quantity, 0)
  );

  readonly totalAmount = computed(() =>
    this.orderItems().reduce(
      (sum, item) => sum + item.productUnitPrice * item.quantity,
      0
    )
  );

  ngOnInit(): void {
    this.loadReferenceData();

    if (!this.isCreateMode) {
      this.loadOrder();
    }
  }

  loadReferenceData(): void {
    this.contactsService.getAll().subscribe({
      next: (contacts) => this.contacts.set(contacts),
      error: () => this.errorMessage.set('Unable to load contacts.'),
    });

    this.productsService.getAll().subscribe({
      next: (products) => this.products.set(products),
      error: () => this.errorMessage.set('Unable to load products.'),
    });
  }

  loadOrder(): void {
    if (this.isCreateMode) {
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set('');

    this.ordersService
      .getById(this.orderId)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (order) => {
          this.order.set(order);
          this.customerId = order.customerId;
          this.customerSearch = `${order.customerName} ${order.customerSurname}`;
        },
        error: () => {
          this.errorMessage.set('Unable to load order details.');
        },
      });
  }

  submitOrderMainDetails(): void {
    if (this.isSaving()) {
      return;
    }

    if (!this.customerId) {
      this.errorMessage.set('Customer is required.');
      return;
    }

    this.errorMessage.set('');
    this.successMessage.set('');
    this.isSaving.set(true);

    if (this.isCreateMode) {
      if (this.draftItems().length === 0) {
        this.isSaving.set(false);
        this.errorMessage.set('Add at least one item before creating the order.');
        return;
      }

      this.ordersService
        .create({
          customerId: this.customerId,
          orderItems: this.draftItems().map((item) => ({
            productId: item.productId,
            quantity: item.quantity,
          })),
        })
        .pipe(finalize(() => this.isSaving.set(false)))
        .subscribe({
          next: () => {
            this.successMessage.set('Order created successfully.');
            this.router.navigate(['/orders']);
          },
          error: () => {
            this.errorMessage.set('Unable to create order.');
          },
        });
      return;
    }

    this.ordersService
      .update(this.orderId, { customerId: this.customerId })
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: (order) => {
          this.order.set(order);
          this.successMessage.set('Order updated successfully.');
        },
        error: () => {
          this.errorMessage.set('Unable to update order.');
        },
      });
  }

  addItem(): void {
    if (this.isAddingItem()) {
      return;
    }

    const quantity = Number(this.itemQuantity);
    const selectedProduct = this.products().find((product) => product.id === this.itemProductId);

    if (!selectedProduct || quantity <= 0) {
      this.errorMessage.set('Select a product and valid quantity.');
      return;
    }

    this.errorMessage.set('');
    this.successMessage.set('');

    if (this.isCreateMode) {
      const existing = this.draftItems().find((item) => item.productId === selectedProduct.id);
      if (existing) {
        this.draftItems.set(
          this.draftItems().map((item) =>
            item.productId === selectedProduct.id
              ? { ...item, quantity: item.quantity + quantity }
              : item
          )
        );
      } else {
        this.draftItems.set([
          ...this.draftItems(),
          {
            id: crypto.randomUUID(),
            productId: selectedProduct.id,
            productCode: selectedProduct.code,
            productName: selectedProduct.name,
            productUnitPrice: selectedProduct.price,
            quantity,
            subTotal: selectedProduct.price * quantity,
          },
        ]);
      }

      this.itemProductId = '';
      this.productSearch = '';
      this.itemQuantity = 1;
      return;
    }

    this.isAddingItem.set(true);
    this.ordersService
      .addOrderItem(this.orderId, {
        productId: selectedProduct.id,
        quantity,
      })
      .pipe(finalize(() => this.isAddingItem.set(false)))
      .subscribe({
        next: (order) => {
          this.order.set(order);
          this.itemProductId = '';
          this.productSearch = '';
          this.itemQuantity = 1;
        },
        error: () => {
          this.errorMessage.set('Unable to add order item.');
        },
      });
  }

  onCustomerSearchChange(value: string): void {
    this.customerSearch = value;
    this.customerId = '';
  }

  onProductSearchChange(value: string): void {
    this.productSearch = value;
    this.itemProductId = '';
  }

  filteredContacts(): Contact[] {
    const term = this.customerSearch.trim().toLowerCase();
    if (!term) {
      return this.contacts();
    }

    return this.contacts().filter((contact) => {
      const fullName = `${contact.name} ${contact.surname}`.toLowerCase();
      const email = (contact.email ?? '').toLowerCase();
      const code = contact.code.toLowerCase();
      return fullName.includes(term) || email.includes(term) || code.includes(term);
    });
  }

  filteredProducts(): Product[] {
    const term = this.productSearch.trim().toLowerCase();
    if (!term) {
      return this.products();
    }

    return this.products().filter((product) => {
      const label = `${product.code} ${product.name} ${product.description}`.toLowerCase();
      return label.includes(term);
    });
  }

  getStatusLabel(statusCode: string | number): string {
    if (statusCode === 0 || statusCode === 'Pending') {
      return 'Pending';
    }

    if (statusCode === 1 || statusCode === 'Cancelled') {
      return 'Cancelled';
    }

    if (statusCode === 2 || statusCode === 'Processed') {
      return 'Processed';
    }

    return String(statusCode);
  }

  selectCustomer(contact: Contact): void {
    this.customerId = contact.id;
    this.customerSearch = `${contact.code} - ${contact.name} ${contact.surname}`;
    this.customerDropdownOpen.set(false);
  }

  selectProduct(product: Product): void {
    this.itemProductId = product.id;
    this.productSearch = `${product.code} - ${product.name} (${product.price})`;
    this.productDropdownOpen.set(false);
  }

  openCustomerDropdown(): void {
    this.customerDropdownOpen.set(true);
  }

  openProductDropdown(): void {
    this.productDropdownOpen.set(true);
  }

  closeCustomerDropdownSoon(): void {
    setTimeout(() => this.customerDropdownOpen.set(false), 120);
  }

  closeProductDropdownSoon(): void {
    setTimeout(() => this.productDropdownOpen.set(false), 120);
  }

  isRemovingItem(itemId: string): boolean {
    return this.removingItemId() === itemId;
  }

  removeItem(itemId: string): void {
    if (this.isRemovingItem(itemId)) {
      return;
    }

    this.errorMessage.set('');
    this.successMessage.set('');

    if (this.isCreateMode) {
      this.draftItems.set(this.draftItems().filter((item) => item.id !== itemId));
      return;
    }

    this.removingItemId.set(itemId);
    this.ordersService
      .removeOrderItem(this.orderId, itemId)
      .pipe(finalize(() => this.removingItemId.set(null)))
      .subscribe({
        next: () => {
          this.loadOrder();
        },
        error: () => {
          this.errorMessage.set('Unable to remove order item.');
        },
      });
  }
}
