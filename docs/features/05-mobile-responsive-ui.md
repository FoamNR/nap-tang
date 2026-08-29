# Feature Spec: Mobile-Responsive UI & UX

This document outlines the UI specifications, responsive breakpoints, layout flow, and style guides for the นับตังค์ (Nap-Tang) frontend built using Nuxt 4, Tailwind CSS, Pinia, and Lucide Icons.

## 1. Visual & Style Design System

To deliver a premium, modern experience, the frontend implements:
*   **Color Palette:**
    *   **Background:** Cute pastel blend gradient (`bg-gradient-to-tr from-[#FFF5F7] via-[#FFFDF9] to-[#EAF2FF]`).
    *   **Primary Accent:** Warm pink (`#FF758F` / `bg-[#FF758F]`, hover `#FF85A1`).
    *   **Secondary Accent:** Sky blue (`#E0F2FE` / `bg-sky-50`, hover `bg-sky-100`, text `text-sky-600`).
    *   **Income Indicators:** Emerald green (`#10B981`).
    *   **Expense Indicators:** Rose/Coral red (`#F43F5E`).
    *   **Glassmorphism:** Use white borders and translucent panels (`bg-white border-2 border-pink-100/70 shadow-xl shadow-pink-100/50`) to mimic a soft cartoon-inspired glass design.
*   **Typography:** Google Fonts **Fredoka** (for bold, rounded headings) and **Nunito** (for readable body text).
*   **Multi-language Support (i18n):**
    *   **Primary Language:** Thai (TH).
    *   **Secondary Language:** English (EN).
    *   **Integration:** Centralized, reactive Pinia store (`lang.ts`) providing translation dictionary lookup. Toggle controls are available inside desktop/mobile menus and login cards.
*   **Micro-animations:** Hover scaling on cards (`transition-all duration-200 hover:scale-[1.01]`), spinning load states, and layout animations.

---

## 2. Key Screen Specifications

### A. Dashboard / Overview (Mobile-First Layout)
*   **Desktop Navigation (Sidebar):** Left-aligned white panel containing logo, dashboard/analytics page links, language selector, user profile card, and logout button.
*   **Mobile Navigation (Bottom Bar):** Fixed bottom menu (`fixed bottom-0 h-16 bg-white/95`) with 5 items: Dashboard, Language switcher, Center Floating Action Button (FAB) for transaction addition, Analytics, and Logout.
*   **Balance Summary Card:** Modern gradient background (Blue to Sky-600) showing:
    *   Net Balance (Large text)
    *   Sub-counters for Monthly Income (+) and Expenses (-)
*   **Quick Actions:** 
    *   Quick-add Floating Action Button (FAB) at the bottom-right of mobile view.
*   **Transaction List:** 
    *   Grouped by date (e.g., "Today", "Yesterday", "24 Aug 2026").
    *   Lists items showing category icon, description, category name, and amount colored by type (emerald for positive, rose for negative).
    *   Horizontal swiping or click to open context menu (Edit/Delete).

### B. Add/Edit Transaction Form
*   **Amount Field:** Large numeric input layout, autofocus.
*   **Type Switcher:** Segmented controls (Income vs. Expense toggle) with smooth transition animation.
*   **Category Grid:** Horizontal scrolling row or a search-friendly dropdown showing icon + category name colored dynamically.
*   **Attachment Component:**
    *   Upload field with Drag & Drop state.
    *   Shows loading indicator when `MediaService` uploads the image.
    *   Once uploaded, displays a small image thumbnail with a "Remove" badge.

### C. Analytics Screen
*   **Period Switcher:** Segmented tabs for "Week", "Month", and "Year".
*   **Interactive Charts:** Responsive line chart for trends and pie/doughnut chart for category breakdown (using library like `chart.js` or `apexcharts` in Vue).
*   **Interactive List:** Tap category in the breakdown list to filter and view the transaction subset.

### D. Profile Screen
*   **User Avatar Graphic:** Displayed at the center using a rounded-3xl container with a gradient color background (Pink to Sky-400), showing the first letter of the display name capitalized.
*   **User Email Label:** Read-only email address displayed below the avatar.
*   **Form Fields:**
    *   **Display Name:** Required text input pre-filled with the current name.
    *   **New Password:** Optional password field (must be at least 8 characters).
    *   **Confirm Password:** Dynamically appears when a new password is keyed in; must match the new password.
*   **Action Buttons:**
    *   **Save Button:** Triggers profile update, shows spinning loader during submission, disables controls, and pops a success/error toast notification.
    *   **Logout Button (Mobile-only):** Displayed at the bottom of the form on mobile view as a secondary outline rose button to allow quick logout.

