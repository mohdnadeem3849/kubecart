import React, { createContext, useContext, useEffect, useMemo, useState } from "react";
import type { Product } from "../../api/catalogApi";

export type CartItem = {
  product: Product;
  qty: number;
};

type CartContextValue = {
  items: CartItem[];
  add: (product: Product) => void;
  remove: (productId: number) => void;
  inc: (productId: number) => void;
  dec: (productId: number) => void;
  clear: () => void;
  totalItems: number;
  totalPrice: number;
};

const CartContext = createContext<CartContextValue | null>(null);

const STORAGE_KEY = "kubecart.cart.v1";

function safeParse<T>(json: string | null): T | null {
  if (!json) return null;
  try {
    return JSON.parse(json) as T;
  } catch {
    return null;
  }
}

export function CartProvider({ children }: { children: React.ReactNode }) {
  const [items, setItems] = useState<CartItem[]>(() => {
    const saved = safeParse<CartItem[]>(localStorage.getItem(STORAGE_KEY));
    return Array.isArray(saved) ? saved : [];
  });

  // Persist to localStorage whenever items change
  useEffect(() => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(items));
  }, [items]);

  const add = (product: Product) => {
    setItems((prev) => {
      const existing = prev.find((x) => x.product.id === product.id);
      if (existing) {
        return prev.map((x) =>
          x.product.id === product.id ? { ...x, qty: x.qty + 1 } : x
        );
      }
      return [...prev, { product, qty: 1 }];
    });
  };

  const remove = (productId: number) => {
    setItems((prev) => prev.filter((x) => x.product.id !== productId));
  };

  const inc = (productId: number) => {
    setItems((prev) =>
      prev.map((x) =>
        x.product.id === productId ? { ...x, qty: x.qty + 1 } : x
      )
    );
  };

  const dec = (productId: number) => {
    setItems((prev) =>
      prev
        .map((x) =>
          x.product.id === productId ? { ...x, qty: x.qty - 1 } : x
        )
        .filter((x) => x.qty > 0)
    );
  };

  const clear = () => setItems([]);

  const totalItems = useMemo(
    () => items.reduce((sum, x) => sum + x.qty, 0),
    [items]
  );

  const totalPrice = useMemo(
    () => items.reduce((sum, x) => sum + Number(x.product.price) * x.qty, 0),
    [items]
  );

  const value: CartContextValue = {
    items,
    add,
    remove,
    inc,
    dec,
    clear,
    totalItems,
    totalPrice,
  };

  return <CartContext.Provider value={value}>{children}</CartContext.Provider>;
}

export function useCart() {
  const ctx = useContext(CartContext);
  if (!ctx) throw new Error("useCart must be used inside <CartProvider>");
  return ctx;
}
