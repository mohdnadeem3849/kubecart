import { http } from "./http";

export type ServerCartItem = {
  id: string;
  userId: string;
  productId: number;
  quantity: number;
  createdAtUtc: string;
};

export type CheckoutRequest = {
  notes: string;
};

export type OrderSummary = {
  id: string;
  userId: string;
  notes: string | null;
  status: string;
  totalAmount: number;
  createdAtUtc: string;
};

export const ordersApi = {
  // server cart
  getCart: () => http<ServerCartItem[]>("/api/orders/cart"),
  addCartItem: (productId: number, quantity: number) =>
    http<ServerCartItem>("/api/orders/cart/items", {
      method: "POST",
      body: JSON.stringify({ productId, quantity }),
    }),
  removeCartItem: (id: string) =>
    http<void>(`/api/orders/cart/items/${id}`, { method: "DELETE" }),

  // checkout
  checkout: (notes: string) =>
    http<void>("/api/orders/checkout", {
      method: "POST",
      body: JSON.stringify({ notes }),
    }),

  // orders
  getOrders: () => http<OrderSummary[]>("/api/orders"),
  getOrderById: (id: string) => http<any>(`/api/orders/${id}`),
};
