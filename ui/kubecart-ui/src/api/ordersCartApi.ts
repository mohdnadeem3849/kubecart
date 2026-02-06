import { http } from "./http";

export type ServerCartItem = {
  id?: string;       // some APIs use id
  itemId?: string;   // some APIs use itemId
  productId?: number;
  quantity?: number;
  qty?: number;
};

export type ServerCart = {
  items?: ServerCartItem[];
};

function getItemId(x: ServerCartItem) {
  return x.id ?? x.itemId ?? "";
}

export const ordersCartApi = {
  // Get current server cart
  getCart: async () => {
    return await http<ServerCart>("/api/orders/cart");
  },

  // Remove one item from server cart
  removeItem: async (id: string) => {
    return await http<void>(`/api/orders/cart/items/${id}`, { method: "DELETE" });
  },

  // Add item to server cart (payload might be productId/qty or productId/quantity)
  addItem: async (productId: number, qty: number) => {
    // ✅ Most common payload (works in many APIs):
    return await http<void>("/api/orders/cart/items", {
      method: "POST",
      body: JSON.stringify({ productId, qty }),
    });

    // If your swagger shows different, change ONLY the JSON keys above.
    // Examples:
    // { productId, quantity: qty }
    // { productId }  (if it always adds 1)
  },

  // Replace server cart with local cart items (clear then add all)
  replaceCartFromLocal: async (localItems: { productId: number; qty: number }[]) => {
    // 1) clear server cart
    const cart = await ordersCartApi.getCart();
    const existing = cart?.items ?? [];

    for (const x of existing) {
      const id = getItemId(x);
      if (id) {
        await ordersCartApi.removeItem(id);
      }
    }

    // 2) add local items
    for (const x of localItems) {
      await ordersCartApi.addItem(x.productId, x.qty);
    }
  },
};
