import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useLangStore = defineStore('lang', () => {
  const locale = ref<'th' | 'en'>('th')

  const dictionary = {
    th: {
      // Common
      appName: 'นับตังค์',
      overview: 'ภาพรวม',
      hello: 'สวัสดี',
      dashboard: 'แดชบอร์ด',
      analytics: 'วิเคราะห์',
      logout: 'ออกจากระบบ',
      add_transaction: 'เพิ่มรายการ',
      save_transaction: 'บันทึกรายการ',
      amount: 'จำนวนเงิน',
      category: 'หมวดหมู่',
      date: 'วันที่',
      notes: 'บันทึกย่อ',
      receipt_slip: 'สลิปใบเสร็จ',
      attach_receipt: 'แนบรูปสลิปใบเสร็จ',
      max_size_slip: 'ขนาดสูงสุด: 5 MB (JPEG, PNG, WEBP)',
      uploading_slip: 'กำลังอัปโหลดสลิป...',
      upload_success: 'อัปโหลดสลิปสำเร็จ',
      placeholder_notes: 'เช่น สตาร์บัคส์, โบนัสเงินเดือน...',
      confirm: 'ยืนยัน',
      cancel: 'ยกเลิก',
      ok: 'ตกลง',
      
      // Categories
      cat_food_drinks: 'อาหารและเครื่องดื่ม',
      cat_transportation: 'การเดินทาง',
      cat_shopping: 'ช็อปปิ้ง',
      cat_bills_utilities: 'บิล & สาธารณูปโภค',
      cat_entertainment: 'ความบันเทิง',
      cat_salary: 'เงินเดือน',
      cat_investment: 'การลงทุน',
      cat_other_income: 'รายได้อื่นๆ',
      cat_rent: 'ค่าเช่า',
      cat_water_bill: 'ค่าน้ำ',
      cat_electricity_bill: 'ค่าไฟ',
      cat_other: 'อื่นๆ',
      
      // Custom Category UI
      create_custom_cat: 'สร้างหมวดหมู่ใหม่',
      placeholder_custom_cat: 'ระบุชื่อหมวดหมู่ที่ต้องการ...',
      add_btn: 'เพิ่ม',
      custom_cat_error: 'กรุณาระบุชื่อหมวดหมู่',
      
      // Dashboard Page
      start_date: 'วันที่เริ่มต้น',
      end_date: 'วันที่สิ้นสุด',
      filter_btn: 'ค้นหา',
      reset_btn: 'รีเซ็ต',
      date_range: 'ช่วงเวลา',
      net_balance: 'ยอดเงินคงเหลือสุทธิ',
      income: 'รายรับ',
      expense: 'รายจ่าย',
      recent_transactions: 'รายการล่าสุด',
      this_month: 'เดือนนี้',
      no_transactions: 'ยังไม่มีการบันทึกรายการ',
      no_transactions_sub: 'กดปุ่มเพิ่มเพื่อบันทึกรายรับหรือรายจ่ายแรกของคุณ',
      slip_badge: 'สลิป',
      
      // Transaction Detail
      transaction_details: 'รายละเอียดธุรกรรม',
      attached_receipt: 'สลิปใบเสร็จที่แนบ',
      view_full_size: 'ดูขนาดเต็ม',
      delete_transaction: 'ลบรายการธุรกรรม',
      delete_confirm: 'คุณแน่ใจหรือไม่ว่าต้องการลบรายการธุรกรรมนี้?',
      delete_failed: 'ลบรายการไม่สำเร็จ',
      
      // Validation & Messages
      fill_all_fields: 'กรุณากรอกข้อมูลให้ครบถ้วน',
      enter_display_name: 'กรุณาระบุชื่อที่แสดงของคุณ',
      register_success: 'สมัครสมาชิกสำเร็จ! กรุณาเข้าสู่ระบบ',
      auth_failed: 'การเข้าสู่ระบบล้มเหลว กรุณาลองใหม่อีกครั้ง',
      enter_valid_amount: 'กรุณาระบุจำนวนเงินที่ถูกต้อง (มากกว่าศูนย์)',
      select_category: 'กรุณาเลือกหมวดหมู่',
      save_failed: 'บันทึกรายการไม่สำเร็จ',
      upload_failed: 'อัปโหลดสลิปไม่สำเร็จ',
      edit_account: 'แก้ไขบัญชีของฉัน',
      profile_updated: 'อัปเดตข้อมูลบัญชีสำเร็จ',
      confirm_password: 'ยืนยันรหัสผ่านใหม่',
      password_mismatch: 'รหัสผ่านใหม่ไม่ตรงกัน',
      profile_update_failed: 'อัปเดตข้อมูลล้มเหลว',
      new_password: 'รหัสผ่านใหม่ (ระบุเมื่อต้องการเปลี่ยน)',
      
      // Login/Register Page
      welcome_back: 'ยินดีต้อนรับกลับมา',
      create_account: 'สร้างบัญชีผู้ใช้',
      login_sub: 'เข้าสู่ระบบเพื่อจัดการงบประมาณส่วนตัวของคุณ',
      register_sub: 'สมัครสมาชิกเพื่อเริ่มติดตามสถานะการเงินของคุณ',
      display_name: 'ชื่อที่แสดง',
      email_address: 'ที่อยู่อีเมล',
      password: 'รหัสผ่าน',
      sign_up: 'สมัครสมาชิก',
      log_in: 'เข้าสู่ระบบ',
      already_have_account: 'มีบัญชีผู้ใช้งานอยู่แล้วใช่ไหม?',
      dont_have_account: 'ยังไม่มีบัญชีผู้ใช้งานใช่ไหม?',
      login_here: 'เข้าสู่ระบบที่นี่',
      create_here: 'สร้างบัญชีที่นี่',
      
      // Analytics Page
      reports: 'รายงานสถิติ',
      financial_analytics: 'วิเคราะห์แนวโน้มการเงิน',
      daily: 'รายวัน',
      weekly: 'รายสัปดาห์',
      monthly: 'รายเดือน',
      trend_title: 'แนวโน้มรายรับ & รายจ่าย',
      distribution_title: 'สัดส่วนค่าใช้จ่ายตามหมวดหมู่',
      expenses: 'รายจ่าย',
      no_trend: 'ไม่มีข้อมูลแนวโน้มในช่วงเวลาที่เลือก',
      no_distribution: 'ไม่มีรายการ {type} ในหมวดหมู่บันทึกไว้ในรอบนี้',
      savings_rate: 'อัตราการออม',
      total_period_income: 'รายรับรวมช่วงนี้',
      total_period_expense: 'รายจ่ายรวมช่วงนี้'
    },
    en: {
      // Common
      appName: 'Nap-Tang',
      overview: 'Overview',
      hello: 'Hello',
      dashboard: 'Dashboard',
      analytics: 'Analytics',
      logout: 'Logout',
      add_transaction: 'Add Transaction',
      save_transaction: 'Save Transaction',
      amount: 'Amount',
      category: 'Category',
      date: 'Date',
      notes: 'Notes',
      receipt_slip: 'Receipt Slip',
      attach_receipt: 'Attach Receipt Image',
      max_size_slip: 'Max size: 5 MB (JPEG, PNG, WEBP)',
      uploading_slip: 'Uploading slip...',
      upload_success: 'Uploaded successfully',
      placeholder_notes: 'e.g. Starbucks, Salary bonus...',
      confirm: 'Confirm',
      cancel: 'Cancel',
      ok: 'OK',
      
      // Categories
      cat_food_drinks: 'Food & Drinks',
      cat_transportation: 'Transportation',
      cat_shopping: 'Shopping',
      cat_bills_utilities: 'Bills & Utilities',
      cat_entertainment: 'Entertainment',
      cat_salary: 'Salary',
      cat_investment: 'Investment',
      cat_other_income: 'Other Income',
      cat_rent: 'Rent',
      cat_water_bill: 'Water Bill',
      cat_electricity_bill: 'Electricity Bill',
      cat_other: 'Other',
      
      // Custom Category UI
      create_custom_cat: 'Create Custom Category',
      placeholder_custom_cat: 'Enter category name...',
      add_btn: 'Add',
      custom_cat_error: 'Please enter a category name.',
      
      // Dashboard Page
      start_date: 'Start Date',
      end_date: 'End Date',
      filter_btn: 'Search',
      reset_btn: 'Reset',
      date_range: 'Date Range',
      net_balance: 'Net Balance',
      income: 'Income',
      expense: 'Expense',
      recent_transactions: 'Recent Transactions',
      this_month: 'This month',
      no_transactions: 'No transactions recorded yet',
      no_transactions_sub: 'Tap the Add button to record your first income or expense.',
      slip_badge: 'Slip',
      
      // Transaction Detail
      transaction_details: 'Transaction Details',
      attached_receipt: 'Attached Receipt',
      view_full_size: 'View Full Size',
      delete_transaction: 'Delete Transaction',
      delete_confirm: 'Are you sure you want to delete this transaction?',
      delete_failed: 'Failed to delete transaction.',
      
      // Validation & Messages
      fill_all_fields: 'Please fill in all fields.',
      enter_display_name: 'Please enter your display name.',
      register_success: 'Registration successful! Please login.',
      auth_failed: 'Authentication failed. Please try again.',
      enter_valid_amount: 'Please enter a valid amount.',
      select_category: 'Please select a category.',
      save_failed: 'Failed to save transaction.',
      upload_failed: 'Failed to upload receipt slip.',
      edit_account: 'Edit My Account',
      profile_updated: 'Profile updated successfully!',
      confirm_password: 'Confirm New Password',
      password_mismatch: 'New passwords do not match',
      profile_update_failed: 'Failed to update profile',
      new_password: 'New Password (optional)',
      
      // Login/Register Page
      welcome_back: 'Welcome Back',
      create_account: 'Create Account',
      login_sub: 'Log in to manage your budget',
      register_sub: 'Sign up to start tracking your finances',
      display_name: 'Display Name',
      email_address: 'Email Address',
      password: 'Password',
      sign_up: 'Sign Up',
      log_in: 'Log In',
      already_have_account: 'Already have an account?',
      dont_have_account: "Don't have an account?",
      login_here: 'Log in here',
      create_here: 'Create one here',
      
      // Analytics Page
      reports: 'Reports',
      financial_analytics: 'Financial Analytics',
      daily: 'Daily',
      weekly: 'Weekly',
      monthly: 'Monthly',
      trend_title: 'Income & Expense Trend',
      distribution_title: 'Category Distribution',
      expenses: 'Expenses',
      no_trend: 'No trend data in the selected period.',
      no_distribution: 'No {type} categories recorded in this period.',
      savings_rate: 'Savings Rate',
      total_period_income: 'Total Income',
      total_period_expense: 'Total Expense'
    }
  }

  function t(key: keyof typeof dictionary['th'] | string, args?: Record<string, string>): string {
    const text = dictionary[locale.value][key as keyof typeof dictionary['th']] || key
    if (args) {
      let formattedText = text
      Object.entries(args).forEach(([k, v]) => {
        formattedText = formattedText.replace(`{${k}}`, v)
      })
      return formattedText
    }
    return text
  }

  function toggleLocale() {
    locale.value = locale.value === 'th' ? 'en' : 'th'
  }

  function translateCategory(name: string): string {
    const keyMap: Record<string, string> = {
      'Food & Drinks': 'cat_food_drinks',
      'Transportation': 'cat_transportation',
      'Shopping': 'cat_shopping',
      'Bills & Utilities': 'cat_bills_utilities',
      'Entertainment': 'cat_entertainment',
      'Salary': 'cat_salary',
      'Investment': 'cat_investment',
      'Other Income': 'cat_other_income',
      'Rent': 'cat_rent',
      'Water Bill': 'cat_water_bill',
      'Electricity Bill': 'cat_electricity_bill',
      'Other': 'cat_other'
    }
    const key = keyMap[name]
    return key ? t(key) : name
  }

  return {
    locale,
    t,
    toggleLocale,
    translateCategory
  }
})
