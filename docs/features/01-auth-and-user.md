# Feature Spec: Authentication & User Management

This feature specification details the registration, login, and JWT-based authentication flows for นับตังค์ (Nap-Tang).

## 1. Database Schema (Auth Schema)

### Users Table (`easytrack_auth.users`)
*   `id`: `UUID` (Primary Key, default: `gen_random_uuid()`)
*   `email`: `VARCHAR(255)` (Unique, Indexed, Required)
*   `password_hash`: `VARCHAR(500)` (Required)
*   `display_name`: `VARCHAR(100)` (Required)
*   `created_at`: `TIMESTAMP WITH TIME ZONE` (default: `NOW()`)
*   `updated_at`: `TIMESTAMP WITH TIME ZONE` (default: `NOW()`)

### Refresh Tokens Table (`easytrack_auth.refresh_tokens`)
*   `id`: `UUID` (Primary Key, default: `gen_random_uuid()`)
*   `user_id`: `UUID` (Foreign Key -> `users.id`, Cascade On Delete)
*   `token`: `VARCHAR(500)` (Unique, Required)
*   `expires_at`: `TIMESTAMP WITH TIME ZONE` (Required)
*   `is_revoked`: `BOOLEAN` (default: `FALSE`)
*   `created_at`: `TIMESTAMP WITH TIME ZONE` (default: `NOW()`)

---

## 2. API Endpoints

All endpoints are prefixed with `/api/v1/auth`.

### A. Register User
*   **Path:** `/register`
*   **Method:** `POST`
*   **Request Body:**
    ```json
    {
      "email": "user@example.com",
      "password": "StrongPassword123!",
      "confirmPassword": "StrongPassword123!",
      "displayName": "John Doe"
    }
    ```
*   **Validation Rules:**
    *   `email` must be a valid email format.
    *   `password` must be at least 8 characters, contain 1 uppercase, 1 lowercase, 1 number, and 1 special character.
    *   `confirmPassword` must match `password`.
    *   `displayName` must be between 2 and 100 characters.
*   **Response (201 Created):**
    ```json
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "email": "user@example.com",
      "displayName": "John Doe"
    }
    ```

### B. Login User
*   **Path:** `/login`
*   **Method:** `POST`
*   **Request Body:**
    ```json
    {
      "email": "user@example.com",
      "password": "StrongPassword123!"
    }
    ```
*   **Response (200 OK):**
    *   *Body:*
        ```json
        {
          "accessToken": "eyJhbGciOi...",
          "expiresInSeconds": 900,
          "user": {
            "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
            "email": "user@example.com",
            "displayName": "John Doe"
          }
        }
        ```
    *   *Cookies:* Sets `refreshToken` as an `HttpOnly`, `Secure`, `SameSite=Lax` cookie with an expiration of 7 days.

### C. Refresh Token
*   **Path:** `/refresh`
*   **Method:** `POST`
*   **Request Body:** None (reads from the `refreshToken` Cookie)
*   **Response (200 OK):**
    *   *Body:*
        ```json
        {
          "accessToken": "eyJhbGciOi...",
          "expiresInSeconds": 900,
          "user": {
            "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
            "email": "user@example.com",
            "displayName": "John Doe"
          }
        }
        ```
    *   *Cookies:* Replaces the existing `refreshToken` cookie with a newly rotated refresh token.

### D. Logout
*   **Path:** `/logout`
*   **Method:** `POST`
*   **Request Body:** None
*   **Response (204 No Content):** Revokes the refresh token in the database and clears the client's `refreshToken` cookie.

### E. Update Profile
*   **Path:** `/profile`
*   **Method:** `PUT`
*   **Request Body:**
    ```json
    {
      "displayName": "John Doe New",
      "newPassword": "NewStrongPassword123!",
      "confirmPassword": "NewStrongPassword123!"
    }
    ```
*   **Response (200 OK):**
    *   *Body:*
        ```json
        {
          "accessToken": "eyJhbGciOi...",
          "expiresInSeconds": 900,
          "user": {
            "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
            "email": "user@example.com",
            "displayName": "John Doe New"
          }
        }
        ```

