import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProductService } from '../../services/product';
import { ProductModel } from '../../models/product.model';
import { SaveProductModel } from '../../models/save-product.model';
import { CategoryModel } from '../../models/category.model';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './products.html',
  styleUrl: './products.css'
})
export class ProductsComponent implements OnInit {
  products = signal<ProductModel[]>([]);
  categories = signal<CategoryModel[]>([]);

  darkMode = false;

  searchText = '';
  page = 1;
  pageSize = 3;
  totalPages = 1;

  sortBy = '';
  descending = false;

  message = '';
  errorMessage = '';
  isLoading = false;

  categoryForm: CategoryModel = {
    id: 0,
    name: ''
  };

  productForm: SaveProductModel = {
    id: 0,
    name: '',
    description: '',
    price: 0,
    stock: 0,
    categoryId: 1
  };

  constructor(private productService: ProductService) { }

  ngOnInit(): void {
    this.loadProducts();
    this.loadCategories();
  }

  toggleDarkMode(): void {
    this.darkMode = !this.darkMode;
  }

  loadProducts(): void {
    this.isLoading = true;

    this.productService.getProducts(
      this.searchText,
      this.page,
      this.pageSize,
      this.sortBy,
      this.descending
    )
      .subscribe({
        next: (response: any) => {
          const items = response.items ?? response.Items ?? [];

          this.products.set(items);
          this.totalPages = response.totalPages ?? response.TotalPages ?? 1;
          this.isLoading = false;
        },
        error: () => {
          this.errorMessage = 'Failed to load products.';
          this.isLoading = false;
        }
      });
  }

  loadCategories(): void {
    this.productService.getCategories()
      .subscribe({
        next: (response: CategoryModel[]) => {
          this.categories.set(response);

          if (response.length > 0 && this.productForm.categoryId === 0) {
            this.productForm.categoryId = response[0].id;
          }
        },
        error: () => {
          this.errorMessage = 'Failed to load categories.';
        }
      });
  }

  saveCategory(): void {
    this.message = '';
    this.errorMessage = '';

    if (!this.categoryForm.name.trim()) {
      this.errorMessage = 'Category name is required.';
      return;
    }

    if (this.categoryForm.id === 0) {
      this.productService.addCategory(this.categoryForm)
        .subscribe({
          next: () => {
            this.message = 'Category added successfully.';
            this.clearCategoryForm();
            this.loadCategories();
          },
          error: () => {
            this.errorMessage = 'Failed to add category.';
          }
        });
    } else {
      this.productService.updateCategory(this.categoryForm)
        .subscribe({
          next: () => {
            this.message = 'Category updated successfully.';
            this.clearCategoryForm();
            this.loadCategories();
            this.loadProducts();
          },
          error: () => {
            this.errorMessage = 'Failed to update category.';
          }
        });
    }
  }

  editCategory(category: CategoryModel): void {
    this.categoryForm = {
      id: category.id,
      name: category.name
    };
  }

  deleteCategory(id: number): void {
    const confirmed = confirm('Are you sure you want to delete this category?');

    if (!confirmed) {
      return;
    }

    this.productService.deleteCategory(id)
      .subscribe({
        next: () => {
          this.message = 'Category deleted successfully.';
          this.loadCategories();
        },
        error: () => {
          this.errorMessage = 'Cannot delete category with products.';
        }
      });
  }

  clearCategoryForm(): void {
    this.categoryForm = {
      id: 0,
      name: ''
    };
  }

  isFormValid(): boolean {
    if (!this.productForm.name.trim()) {
      this.errorMessage = 'Name is required.';
      return false;
    }

    if (this.productForm.price <= 0) {
      this.errorMessage = 'Price must be greater than 0.';
      return false;
    }

    if (this.productForm.stock < 0) {
      this.errorMessage = 'Stock cannot be negative.';
      return false;
    }

    return true;
  }

  searchProducts(): void {
    this.page = 1;
    this.loadProducts();
  }

  sortProducts(sortField: string): void {
    if (this.sortBy === sortField) {
      this.descending = !this.descending;
    } else {
      this.sortBy = sortField;
      this.descending = false;
    }

    this.page = 1;
    this.loadProducts();
  }

  nextPage(): void {
    if (this.page < this.totalPages) {
      this.page++;
      this.loadProducts();
    }
  }

  previousPage(): void {
    if (this.page > 1) {
      this.page--;
      this.loadProducts();
    }
  }

  saveProduct(): void {
    this.message = '';
    this.errorMessage = '';

    if (!this.isFormValid()) {
      return;
    }

    if (this.productForm.id === 0) {
      this.productService.addProduct(this.productForm)
        .subscribe({
          next: () => {
            this.message = 'Product added successfully.';
            this.clearForm();
            this.loadProducts();
          }
        });
    } else {
      this.productService.updateProduct(this.productForm)
        .subscribe({
          next: () => {
            this.message = 'Product updated successfully.';
            this.clearForm();
            this.loadProducts();
          }
        });
    }
  }

  editProduct(product: ProductModel): void {
    this.productForm = {
      id: product.id,
      name: product.name,
      description: product.description,
      price: product.price,
      stock: product.stock,
      categoryId: product.categoryId
    };
  }

  deleteProduct(id: number): void {
    const confirmed = confirm('Are you sure you want to delete this product?');

    if (!confirmed) {
      return;
    }

    this.productService.deleteProduct(id)
      .subscribe({
        next: () => {
          this.message = 'Product deleted successfully.';
          this.loadProducts();
        }
      });
  }

  clearForm(): void {
    this.productForm = {
      id: 0,
      name: '',
      description: '',
      price: 0,
      stock: 0,
      categoryId: this.categories().length > 0 ? this.categories()[0].id : 1
    };
  }
}
