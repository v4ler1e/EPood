import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { SaveProductModel } from '../models/save-product.model';
import { CategoryModel } from '../models/category.model';
import { SaveOrderModel } from '../models/save-order.model';

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private productsUrl = 'http://localhost:5298/api/Products';
  private categoriesUrl = 'http://localhost:5298/api/Categories';
  private ordersUrl = 'http://localhost:5298/api/Orders';

  constructor(private http: HttpClient) { }

  getProducts(
    search: string = '',
    page: number = 1,
    pageSize: number = 3,
    sortBy: string = '',
    descending: boolean = false
  ): Observable<any> {
    return this.http.get<any>(
      `${this.productsUrl}?search=${search}&page=${page}&pageSize=${pageSize}&sortBy=${sortBy}&descending=${descending}`
    );
  }

  addProduct(product: SaveProductModel): Observable<any> {
    return this.http.post(this.productsUrl, product);
  }

  updateProduct(product: SaveProductModel): Observable<any> {
    return this.http.put(`${this.productsUrl}/${product.id}`, product);
  }

  deleteProduct(id: number): Observable<any> {
    return this.http.delete(`${this.productsUrl}/${id}`);
  }

  getCategories(): Observable<CategoryModel[]> {
    return this.http.get<CategoryModel[]>(this.categoriesUrl);
  }

  addCategory(category: CategoryModel): Observable<any> {
    return this.http.post(this.categoriesUrl, category);
  }

  updateCategory(category: CategoryModel): Observable<any> {
    return this.http.put(`${this.categoriesUrl}/${category.id}`, category);
  }

  deleteCategory(id: number): Observable<any> {
    return this.http.delete(`${this.categoriesUrl}/${id}`);
  }

  getOrders(
    search: string = '',
    page: number = 1,
    pageSize: number = 5
  ): Observable<any> {
    return this.http.get<any>(
      `${this.ordersUrl}?search=${search}&page=${page}&pageSize=${pageSize}`
    );
  }

  addOrder(order: SaveOrderModel): Observable<any> {
    return this.http.post(this.ordersUrl, order);
  }

  updateOrder(order: SaveOrderModel): Observable<any> {
    return this.http.put(`${this.ordersUrl}/${order.id}`, order);
  }

  deleteOrder(id: number): Observable<any> {
    return this.http.delete(`${this.ordersUrl}/${id}`);
  }
}
