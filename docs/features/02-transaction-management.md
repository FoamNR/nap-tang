# Feature Spec: Transaction & Category Management

This feature specification details the CRUD APIs and database layout for Income/Expense Transactions and Categories.

## 1. Database Schema (Transactions Schema)

### Categories Table (`easytrack_transactions.categories`)
*   `id`: `UUID` (Primary Key, default: `gen_random_uuid()`)
*   `user_id`: `UUID` (Nullable. `NULL` means it is a system-default category; otherwise, it is a user-defined custom category)
*   `name`: `VARCHAR(100)` (Required)
*   `type`: `VARCHAR(10)` (Value must be either `'income'` or `'expense'`)
*   `icon_name`: `VARCHAR(50)` (Name of the Lucide icon to display)
*   `color_hex`: `VARCHAR(7)` (Hexadecimal color representation, e.g., `#FF5733`)
*   `created_at`: `TIMESTAMP WITH TIME ZONE` (default: `NOW()`)

### Transactions Table (`easytrack_transactions.transactions`)
*   `id`: `UUID` (Primary Key, default: `gen_random_uuid()`)
*   `user_id`: `UUID` (Required, Indexed)
*   `category_id`: `UUID` (Foreign Key -> `categories.id`, RESTRICT on Delete)
*   `amount`: `NUMERIC(18,2)` (Required, must be greater than zero)
*   `type`: `VARCHAR(10)` (Value must be either `'income'` or `'expense'`)
*   `description`: `TEXT` (Nullable)
*   `transaction_date`: `TIMESTAMP WITH TIME ZONE` (Required)
*   `slip_url`: `VARCHAR(500)` (Nullable, points to stored receipt/slip image)
*   `created_at`: `TIMESTAMP WITH TIME ZONE` (default: `NOW()`)
*   `updated_at`: `TIMESTAMP WITH TIME ZONE` (default: `NOW()`)

---

## 2. API Endpoints

All endpoints require JWT Bearer Authentication and are prefixed with `/api/v1/transactions`.

### A. Get Categories
*   **Path:** `/categories`
*   **Method:** `GET`
*   **Response (200 OK):**
    ```json
    [
      {
        "id": "e30e1372-881b-4b21-8255-6677f884fbc9",
        "userId": null,
        "name": "Food & Drinks",
        "type": "expense",
        "iconName": "Utensils",
        "colorHex": "#EF4444"
      }
    ]
    ```

### B. Create Custom Category
*   **Path:** `/categories`
*   **Method:** `POST`
*   **Request Body:**
    ```json
    {
      "name": "My Custom Hobby",
      "type": "expense",
      "iconName": "Gamepad",
      "colorHex": "#8B5CF6"
    }
    ```
*   **Response (201 Created):** Created category details containing the user's `userId`.

### C. Get Transactions (Paginated & Filtered)
*   **Path:** `/`
*   **Method:** `GET`
*   **Query Parameters:**
    *   `page` (default: 1)
    *   `pageSize` (default: 20)
    *   `startDate` (Format: ISO 8601, optional)
    *   `endDate` (Format: ISO 8601, optional)
    *   `type` (Optional: `'income'` or `'expense'`)
    *   `categoryId` (Optional)
*   **Response (200 OK):**
    ```json
    {
      "items": [
        {
          "id": "b304c405-c1e1-4560-a23d-ef04bc99f012",
          "amount": 120.50,
          "type": "expense",
          "description": "Lunch at restaurant",
          "transactionDate": "2026-08-28T12:30:00Z",
          "slipUrl": null,
          "category": {
            "id": "e30e1372-881b-4b21-8255-6677f884fbc9",
            "name": "Food & Drinks",
            "iconName": "Utensils",
            "colorHex": "#EF4444"
          }
        }
      ],
      "page": 1,
      "pageSize": 20,
      "totalCount": 1
    }
    ```

### D. Get Single Transaction
*   **Path:** `/{id}`
*   **Method:** `GET`
*   **Response (200 OK):**
    ```json
    {
      "id": "b304c405-c1e1-4560-a23d-ef04bc99f012",
      "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "amount": 120.50,
      "type": "expense",
      "description": "Lunch at restaurant",
      "transactionDate": "2026-08-28T12:30:00Z",
      "slipUrl": null,
      "category": {
        "id": "e30e1372-881b-4b21-8255-6677f884fbc9",
        "userId": null,
        "name": "Food & Drinks",
        "iconName": "Utensils",
        "colorHex": "#EF4444"
      }
    }
    ```

### E. Create Transaction
*   **Path:** `/`
*   **Method:** `POST`
*   **Request Body:**
    ```json
    {
      "amount": 120.50,
      "type": "expense",
      "categoryId": "e30e1372-881b-4b21-8255-6677f884fbc9",
      "description": "Lunch at restaurant",
      "transactionDate": "2026-08-28T12:30:00Z",
      "slipUrl": "https://s3.easytrack.local/slips/abcd-1234.jpg"
    }
    ```
*   **Response (201 Created):** The created transaction object.

### F. Update Transaction
*   **Path:** `/{id}`
*   **Method:** `PUT`
*   **Request Body:** (Same properties as POST)
*   **Response (200 OK):** The updated transaction object.

### G. Delete Transaction
*   **Path:** `/{id}`
*   **Method:** `DELETE`
*   **Response (204 No Content)**

---

## 3. Preseeded & Custom Categories

### System Default Categories (Automatically Seeded)
On backend service startup, the system automatically verifies and seeds the following system-wide default categories (where `userId` is `null`):
*   **Rent** (Expense, Icon: `Home`, Color: `#D97706`)
*   **Water Bill** (Expense, Icon: `Droplet`, Color: `#0284C7`)
*   **Electricity Bill** (Expense, Icon: `Zap`, Color: `#EAB308`)
*   **Other** (Expense, Icon: `MoreHorizontal`, Color: `#64748B`)

### Inline Custom Category Creation
The frontend "Add Transaction" modal contains an inline `+ เพิ่ม...` button in the category selector grid:
1. When clicked, it displays an inline slide-in form.
2. The user inputs their desired custom category name.
3. On submit, it posts to `/api/v1/transactions/categories` to persist the new category.
4. The frontend store receives the saved category with the user's `userId`, appends it, auto-selects it, and hides the input form reactively.

