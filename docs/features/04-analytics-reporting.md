# Feature Spec: Analytics & Reporting

This feature specification details the aggregation APIs used to calculate summaries, categorical breakdown, and transaction trends over weekly, monthly, and yearly intervals.

## 1. API Endpoints

All endpoints require JWT Bearer Authentication and are prefixed with `/api/v1/transactions/analytics`.

### A. General Summary
Returns overall stats (Total Income, Total Expenses, Net Savings/Balance) for a given time window.

*   **Path:** `/summary`
*   **Method:** `GET`
*   **Query Parameters:**
    *   `startDate` (ISO 8601 Date, Required)
    *   `endDate` (ISO 8601 Date, Required)
*   **Response (200 OK):**
    ```json
    {
      "totalIncome": 54200.00,
      "totalExpense": 28150.50,
      "netBalance": 26049.50,
      "startDate": "2026-08-01T00:00:00Z",
      "endDate": "2026-08-31T23:59:59Z"
    }
    ```

### B. Category Breakdown
Returns totals grouped by category. Useful for displaying doughnut/pie charts.

*   **Path:** `/category-breakdown`
*   **Method:** `GET`
*   **Query Parameters:**
    *   `startDate` (ISO 8601 Date, Required)
    *   `endDate` (ISO 8601 Date, Required)
    *   `type` (Optional: `'income'` or `'expense'`)
*   **Response (200 OK):**
    ```json
    [
      {
        "categoryId": "e30e1372-881b-4b21-8255-6677f884fbc9",
        "categoryName": "Food & Drinks",
        "iconName": "Utensils",
        "colorHex": "#EF4444",
        "totalAmount": 12450.00,
        "percentage": 44.22
      },
      {
        "categoryId": "c92d5271-118b-4b21-a18a-bb77f984fc11",
        "categoryName": "Transportation",
        "iconName": "Car",
        "colorHex": "#3B82F6",
        "totalAmount": 5200.50,
        "percentage": 18.47
      }
    ]
    ```

### C. Trend Analytics
Returns periodic data points grouped by days, weeks, or months to draw line or bar charts.

*   **Path:** `/trend`
*   **Method:** `GET`
*   **Query Parameters:**
    *   `startDate` (ISO 8601 Date, Required)
    *   `endDate` (ISO 8601 Date, Required)
    *   `interval` (Required, values: `'daily'`, `'weekly'`, `'monthly'`)
*   **Response (200 OK):**
    ```json
    {
      "interval": "daily",
      "dataPoints": [
        {
          "label": "2026-08-25",
          "income": 0.00,
          "expense": 450.00
        },
        {
          "label": "2026-08-26",
          "income": 15000.00,
          "expense": 120.00
        }
      ]
    }
    ```

---

## 2. Timezone & Date Bounds Handling

To prevent local timezone offset shifts from omitting records belonging to the boundaries of the query window (such as transactions added on the current day), the backend normalizes all date filters:
1. **Start Date Boundary:** Extracted date component is specified as UTC: `DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc)`. This sets the boundary to `00:00:00 UTC`.
2. **End Date Boundary:** Extracted date component is extended to include the entire day in UTC: `DateTime.SpecifyKind(endDate.Date, DateTimeKind.Utc).AddDays(1).AddTicks(-1)`. This sets the boundary to `23:59:59.9999999 UTC`.

All database filters apply `TransactionDate >= startUtc && TransactionDate <= endUtc` to safely retrieve matching transactions.

