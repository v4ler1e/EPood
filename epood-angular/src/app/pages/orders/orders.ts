import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProductService } from '../../services/product';
import { ProductModel } from '../../models/product.model';
import { OrderModel } from '../../models/order.model';
import { SaveOrderModel } from '../../models/save-order.model';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './orders.html',
  styleUrl: './orders.css'
})
export class OrdersComponent implements OnInit {
  orders = signal<OrderModel[]>([]);
  products = signal<ProductModel[]>([]);

  searchText = '';
  page = 1;
  pageSize = 5;
  totalPages = 1;
  sortBy = '';
  descending = false;

  message = '';
  errorMessage = '';
  isLoading = false;

  orderForm: SaveOrderModel = {
    id: 0,
    customerName: '',
    productId: 1,
    quantity: 1
  };

  constructor(private productService: ProductService) { }

  ngOnInit(): void {
    this.loadOrders();
    this.loadProducts();
  }

  loadOrders(): void {
    this.isLoading = true;

    this.productService.getOrders(
      this.searchText,
      this.page,
      this.pageSize,
      this.sortBy,
      this.descending
    )
      .subscribe({
        next: (response: any) => {
          const items = response.items ?? response.Items ?? response;

          this.orders.set(items);
          this.totalPages = response.totalPages ?? response.TotalPages ?? 1;
          this.isLoading = false;
        },
        error: () => {
          this.errorMessage = 'Failed to load orders.';
          this.isLoading = false;
        }
      });
  }

  loadProducts(): void {
    this.productService.getProducts('', 1, 100)
      .subscribe({
        next: (response: any) => {
          const items = response.items ?? response.Items ?? [];
          this.products.set(items);

          if (items.length > 0 && this.orderForm.productId === 1) {
            this.orderForm.productId = items[0].id;
          }
        },
        error: () => {
          this.errorMessage = 'Failed to load products.';
        }
      });
  }

  searchOrders(): void {
    this.page = 1;
    this.loadOrders();
  }

  sortOrders(sortField: string): void {
    if (this.sortBy === sortField) {
      this.descending = !this.descending;
    } else {
      this.sortBy = sortField;
      this.descending = false;
    }

    this.page = 1;
    this.loadOrders();
  }

  nextPage(): void {
    if (this.page < this.totalPages) {
      this.page++;
      this.loadOrders();
    }
  }

  previousPage(): void {
    if (this.page > 1) {
      this.page--;
      this.loadOrders();
    }
  }

  saveOrder(): void {
    this.message = '';
    this.errorMessage = '';

    if (!this.orderForm.customerName.trim()) {
      this.errorMessage = 'Customer name is required.';
      return;
    }

    if (this.orderForm.quantity <= 0) {
      this.errorMessage = 'Quantity must be greater than zero.';
      return;
    }

    if (this.orderForm.id === 0) {
      this.productService.addOrder(this.orderForm)
        .subscribe({
          next: () => {
            this.message = 'Order added successfully.';
            this.clearForm();
            this.loadOrders();
          },
          error: () => {
            this.errorMessage = 'Failed to add order.';
          }
        });
    } else {
      this.productService.updateOrder(this.orderForm)
        .subscribe({
          next: () => {
            this.message = 'Order updated successfully.';
            this.clearForm();
            this.loadOrders();
          },
          error: () => {
            this.errorMessage = 'Failed to update order.';
          }
        });
    }
  }

  editOrder(order: OrderModel): void {
    this.orderForm = {
      id: order.id,
      customerName: order.customerName,
      productId: order.productId,
      quantity: order.quantity
    };
  }

  deleteOrder(id: number): void {
    const confirmed = confirm('Are you sure you want to delete this order?');

    if (!confirmed) {
      return;
    }

    this.productService.deleteOrder(id)
      .subscribe({
        next: () => {
          this.message = 'Order deleted successfully.';
          this.loadOrders();
        },
        error: () => {
          this.errorMessage = 'Failed to delete order.';
        }
      });
  }

  clearForm(): void {
    this.orderForm = {
      id: 0,
      customerName: '',
      productId: this.products().length > 0 ? this.products()[0].id : 1,
      quantity: 1
    };
  }
}
