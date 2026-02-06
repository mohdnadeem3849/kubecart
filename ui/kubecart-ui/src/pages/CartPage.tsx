import { useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useCart } from "../features/cart/cartStore";
import { ordersApi } from "../api/ordersApi";

export default function CartPage() {
  const navigate = useNavigate();
  const { items, inc, dec, remove, clear, totalItems, totalPrice } = useCart();

  const [notes, setNotes] = useState("");
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");

  const canCheckout = useMemo(() => items.length > 0 && !busy, [items, busy]);

  async function onCheckout() {
    setError("");

    if (items.length === 0) {
      setError("Your cart is empty.");
      return;
    }

    try {
      setBusy(true);

      // ✅ IMPORTANT:
      // Your UI cart is localStorage, but Orders API checkout uses server cart.
      // So first push local cart items into server cart.
      for (const x of items) {
        const productId = Number(x.product.id);
        const qty = Number(x.qty);

        if (!productId || qty <= 0) continue;

        await ordersApi.addCartItem(productId, qty);
      }

      // Now checkout will work because server cart has items
      await ordersApi.checkout(notes.trim());

      // Clear local cart after successful checkout
      clear();

      // Go to Orders page
      navigate("/orders");
    } catch (e: any) {
      setError(e?.message || "Checkout failed");
    } finally {
      setBusy(false);
    }
  }

  if (items.length === 0) {
    return (
      <div className="p-6">
        <h1 className="text-2xl font-bold mb-2">Cart</h1>
        <div className="text-gray-600">Your cart is empty.</div>
      </div>
    );
  }

  return (
    <div className="p-6">
      <div className="flex items-center justify-between mb-4">
        <h1 className="text-2xl font-bold">Cart</h1>
        <button
          onClick={clear}
          className="px-4 py-2 rounded bg-gray-200 hover:bg-gray-300"
          disabled={busy}
        >
          Clear Cart
        </button>
      </div>

      <div className="space-y-4">
        {items.map((x) => (
          <div
            key={x.product.id}
            className="border rounded-xl p-4 flex items-center justify-between"
          >
            <div>
              <div className="font-semibold text-lg">{x.product.name}</div>
              <div className="text-sm text-gray-500">
                ${Number(x.product.price).toFixed(2)}
              </div>
            </div>

            <div className="flex items-center gap-3">
              <button
                onClick={() => dec(x.product.id)}
                className="px-3 py-1 rounded bg-gray-200 hover:bg-gray-300"
                disabled={busy}
              >
                -
              </button>

              <div className="min-w-[30px] text-center font-semibold">
                {x.qty}
              </div>

              <button
                onClick={() => inc(x.product.id)}
                className="px-3 py-1 rounded bg-gray-200 hover:bg-gray-300"
                disabled={busy}
              >
                +
              </button>

              <button
                onClick={() => remove(x.product.id)}
                className="ml-3 px-3 py-1 rounded bg-red-600 text-white hover:bg-red-700"
                disabled={busy}
              >
                Remove
              </button>
            </div>
          </div>
        ))}
      </div>

      <div className="mt-6 border-t pt-4 flex items-center justify-between">
        <div className="text-lg font-semibold">Total Items: {totalItems}</div>
        <div className="text-lg font-bold">Total: ${totalPrice.toFixed(2)}</div>
      </div>

      {/* Notes + Checkout */}
      <div className="mt-6 space-y-3">
        <input
          value={notes}
          onChange={(e) => setNotes(e.target.value)}
          placeholder="Notes (optional) e.g. Leave at door"
          className="w-full border rounded px-4 py-3"
          disabled={busy}
        />

        {error && <div className="text-red-600 text-sm">{error}</div>}

        <button
          onClick={onCheckout}
          disabled={!canCheckout}
          className="w-full bg-green-600 disabled:bg-gray-400 text-white p-4 rounded text-lg font-semibold"
        >
          {busy ? "Processing..." : "Checkout"}
        </button>
      </div>
    </div>
  );
}
