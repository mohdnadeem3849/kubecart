import { useEffect, useState } from "react";
import { http } from "../api/http";

type OrderSummary = {
  id: string;
  status?: string | null;
  notes?: string | null;
  totalAmount?: number | null;
  createdAtUtc?: string | null;
};

type OrderItem = {
  // Common
  productId?: number | string | null;
  quantity?: number | null;
  qty?: number | null;

  // Pricing (multiple APIs use different names)
  unitPrice?: number | null;
  price?: number | null;

  // Name (multiple APIs use different names)
  productName?: string | null;
  name?: string | null;

  // ✅ Snapshot fields (your Orders DB shows these)
  productNameSnapshot?: string | null;
  unitPriceSnapshot?: number | null;
};

type OrderDetails = {
  id: string;

  // APIs may return items in different property names
  items?: OrderItem[] | null;
  orderItems?: OrderItem[] | null;
};

function money(n: unknown) {
  const v = Number(n);
  if (!Number.isFinite(v)) return "$0.00";
  return `$${v.toFixed(2)}`;
}

function fmtDate(iso?: string | null) {
  if (!iso) return "";
  const d = new Date(iso);
  if (Number.isNaN(d.getTime())) return iso;
  return d.toLocaleString();
}

function toNum(v: unknown) {
  const n = Number(v);
  return Number.isFinite(n) ? n : 0;
}

function getQty(item: OrderItem) {
  return toNum(item.quantity ?? item.qty ?? 0);
}

function getUnitPrice(item: OrderItem) {
  // ✅ support snapshot price
  return toNum(item.unitPriceSnapshot ?? item.unitPrice ?? item.price ?? 0);
}

function getName(item: OrderItem) {
  // ✅ support snapshot name
  return (
    item.productNameSnapshot ||
    item.productName ||
    item.name ||
    (item.productId != null ? `Item #${item.productId}` : "Item")
  );
}

export default function OrdersPage() {
  const [orders, setOrders] = useState<OrderSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const [openId, setOpenId] = useState<string | null>(null);
  const [details, setDetails] = useState<Record<string, OrderDetails | null>>(
    {}
  );
  const [detailsLoading, setDetailsLoading] = useState<Record<string, boolean>>(
    {}
  );

  useEffect(() => {
    (async () => {
      try {
        setError("");
        const data = await http<OrderSummary[]>("/api/orders");
        setOrders(data || []);
      } catch (e: any) {
        setError(e?.message ?? "Failed to load orders");
      } finally {
        setLoading(false);
      }
    })();
  }, []);

  async function toggleDetails(id: string) {
    if (openId === id) {
      setOpenId(null);
      return;
    }

    setOpenId(id);

    if (details[id]) return;

    setDetailsLoading((prev) => ({ ...prev, [id]: true }));
    try {
      const d = await http<{ order: OrderSummary; items: OrderItem[] }>(
  `/api/orders/${id}`
);

setDetails((prev) => ({
  ...prev,
  [id]: { id: d.order.id, items: d.items },
}));

    } catch (e) {
      setDetails((prev) => ({ ...prev, [id]: null }));
      console.error(e);
    } finally {
      setDetailsLoading((prev) => ({ ...prev, [id]: false }));
    }
  }

  if (loading) return <div className="p-6">Loading orders...</div>;
  if (error) return <div className="p-6 text-red-600">{error}</div>;

  return (
    <div className="p-6 space-y-6">
      <h1 className="text-3xl font-bold">Orders</h1>

      {orders.length === 0 && (
        <div className="text-gray-600">No orders found.</div>
      )}

      {orders.map((o) => {
        const isOpen = openId === o.id;
        const d = details[o.id];
        const isLoadingDetails = !!detailsLoading[o.id];

        const itemsRaw =
          d?.items ?? d?.orderItems ?? []; // ✅ handle both names

        // ✅ Only require qty > 0. Do NOT filter on price.
        const items =
          Array.isArray(itemsRaw) ? itemsRaw.filter((it) => getQty(it) > 0) : [];

        return (
          <div key={o.id} className="border rounded-2xl p-6 shadow-sm bg-white">
            <div className="flex items-start justify-between gap-4">
              <div>
                <div className="text-xl font-semibold break-all">
                  Order #{o.id}
                </div>

                <div className="mt-2 font-semibold">
                  Total: {money(o.totalAmount)}
                </div>

                {o.status && (
                  <div className="text-sm text-gray-600 mt-1">
                    Status: {o.status}
                  </div>
                )}

                {o.notes && (
                  <div className="text-sm text-gray-500 mt-1 break-words">
                    Notes: {o.notes}
                  </div>
                )}
              </div>

              <div className="text-right">
                <div className="text-gray-600">{fmtDate(o.createdAtUtc)}</div>

                <button
                  onClick={() => toggleDetails(o.id)}
                  className="mt-3 px-4 py-2 rounded bg-gray-100 hover:bg-gray-200"
                >
                  {isOpen ? "Hide Details" : "View Details"}
                </button>
              </div>
            </div>

            {isOpen && (
              <div className="mt-4 border-t pt-4">
                {isLoadingDetails && (
                  <div className="text-gray-600">Loading details...</div>
                )}

                {!isLoadingDetails && d === null && (
                  <div className="text-gray-600">
                    Could not load order details.
                  </div>
                )}

                {!isLoadingDetails && d && items.length === 0 && (
                  <div className="text-gray-600">No items returned.</div>
                )}

                {!isLoadingDetails && d && items.length > 0 && (
                  <div className="space-y-2">
                    {items.map((it, idx) => {
                      const qty = getQty(it);
                      const unit = getUnitPrice(it);
                      const lineTotal = qty * unit;

                      return (
                        <div
                          key={`${o.id}-${idx}`}
                          className="flex items-center justify-between"
                        >
                          <div className="text-gray-800">
                            {getName(it)}{" "}
                            <span className="text-gray-500">x{qty}</span>
                            <span className="text-gray-400">
                              {" "}
                              • {money(unit)} each
                            </span>
                          </div>

                          <div className="font-medium">{money(lineTotal)}</div>
                        </div>
                      );
                    })}
                  </div>
                )}
              </div>
            )}
          </div>
        );
      })}
    </div>
  );
}
