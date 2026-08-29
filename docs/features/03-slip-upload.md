# Feature Spec: Slip Upload Management

This feature specification details the S3-compatible slip upload system handled by `MediaService`.

## 1. Upload Workflow

1.  **Frontend request:** The user selects a slip image file and uploads it via the frontend.
2.  **API Gateway:** YARP forwards the request to `MediaService`.
3.  **Validation:** `MediaService` validates the file's size and MIME type.
4.  **Storage:** `MediaService` uploads the file to the S3 bucket (`easytrack-slips`) with a uniquely generated UUID filename.
5.  **Response:** `MediaService` returns the S3 object URI or a presigned URL back to the frontend.
6.  **Transaction creation:** The frontend includes this URL in the payload when calling the Transaction creation/update API.

---

## 2. API Endpoints

All endpoints require JWT Bearer Authentication and are prefixed with `/api/v1/media`.

### A. Upload Slip File
*   **Path:** `/upload-slip`
*   **Method:** `POST`
*   **Content-Type:** `multipart/form-data`
*   **Payload:**
    *   `file`: Binary file (JPEG, PNG, WEBP)
*   **Validation Rules:**
    *   **Maximum File Size:** 5 MB. Protected against DOS attacks with `[RequestSizeLimit]` and `[RequestFormLimits]` restricted to 5 MB + 50 KB padding.
    *   **Allowed Extensions & MIME Types:** `.jpg`, `.jpeg`, `.png`, `.webp` (MIME: `image/jpeg`, `image/png`, `image/webp`).
    *   **File Signature Verification (Magic Numbers):** Verifies binary headers to prevent masquerading and script injection:
        *   **JPEG:** Starts with `FF D8 FF`
        *   **PNG:** Starts with `89 50 4E 47`
        *   **WEBP:** Contains `RIFF` (bytes 0-3) and `WEBP` (bytes 8-11) markers.
*   **Response (200 OK):**
    ```json
    {
      "url": "http://localhost:9000/easytrack-slips/2026/08/bfd31e9c-5353-4318-ae71-9f2d01eaec5b.jpg"
    }
    ```

---

## 3. Object Storage Directory Structure
Slips are organized by year and month to avoid having too many files in a single directory:
`easytrack-slips/{userId}/{yyyy}/{mm}/{uuid}.{ext}`

## 4. Local MinIO Setup
For local development, we configure a MinIO container in `docker-compose.yml`:
*   `MINIO_ROOT_USER`: `easytrack_admin`
*   `MINIO_ROOT_PASSWORD`: `easytrack_secret_pass`
*   Bucket `easytrack-slips` is automatically created on startup using a healthcheck/initialization script.
*   Policy: The bucket is set to **Public Read-Only** or requires **Presigned URLs** to fetch files. For simplicity, we will configure a public policy for local dev.
