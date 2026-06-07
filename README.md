# 📦 Warehouse Management System

## 📌 Description

Warehouse Management System is a desktop application developed to support warehouse management activities such as inventory control, employee management, import/export operations, and warehouse monitoring.

The system is designed to help businesses organize warehouse data efficiently, reduce manual management processes, and improve the accuracy of inventory operations.

This project was built using **WPF (.NET Framework)** following the **MVVM architectural pattern** combined with **Entity Framework Database First** and **SQL Server** for database management.

The application provides a modern user interface with role-based management for administrators, warehouse staff, and accountants.

---

# 🚀 Features

## 👨‍💼 Employee Management

* Add, edit, and lock/unlock employees
* Search employee information
* Manage employee activity status
* Validate employee data before saving

---

## 🔐 Account & Role Management

* User login/logout system
* Role-based authorization
* Manage accounts and permissions
* Change password
* Lock/unlock accounts

---

## 🏬 Warehouse Management

* Manage warehouse information
* Track warehouse status
* Warehouse inventory monitoring
* Stock quantity management

---

## 📦 Product Management

* Manage product categories
* Manage manufacturers
* Manage product units
* Add, update, and delete products

---

## 📥 Import Warehouse Management

* Create import receipts
* Temporary import saving
* Confirm warehouse import
* Cancel import receipts
* Automatically update inventory quantities

---

## 📤 Export Warehouse Management

* Create export receipts
* Check inventory before exporting
* Confirm export operations
* Cancel export receipts

---

## 📊 Inventory Checking

* Create inventory check receipts
* Compare actual quantity and stock quantity
* Update warehouse inventory after checking

---

## 📝 System Activity Logs

### Login Logs

* Login history
* Logout history
* Failed login attempts

### System Operation Logs

* Add / Edit / Delete operations
* Lock / Unlock actions
* Import / Export activities
* Receipt creation and cancellation

---

# 🛠️ Technologies Used

* C#
* WPF (.NET Framework 4.8)
* MVVM Pattern
* Entity Framework
* SQL Server
* LINQ
* XAML

---

# 📂 Project Structure

```text
Warehouse-Management-System/
│
├── Helper/
│   ├── BaseViewModel
│   ├── RelayCommand
│   └── CurrentUser
│
├── Model/
│   ├── Database
│   └── Entity Framework
│
├── View/
│   ├── Admin/
│   ├── KeToan/
│   ├── NhanVienKho/
│   └── Windows/
│
├── ViewModels/
│   ├── Admin/
│   ├── KeToan/
│   └── NhanVienKho/
│
├── App.config
└── Do_An.sln
```

---

# 🗄️ Database

## Main Tables

* NHANVIEN
* TAIKHOAN
* VAITRO
* KHO
* SANPHAM
* LOAIHANG
* DONVITINH
* NHASANXUAT
* TONKHO
* PHIEUNHAP
* CT_PHIEUNHAP
* PHIEUXUAT
* CT_PHIEUXUAT
* KIEMKE
* NHATKY

---

# 🎨 User Interface

The application interface was designed with:

* Modern warehouse management style
* Beige/Nude color theme
* Sidebar navigation menu
* Custom DataGrid styles
* Separate forms for add/edit operations
* Responsive layout for desktop screens

---

# ⚙️ System Architecture

```text
View
   ↓
ViewModel
   ↓
Entity Framework
   ↓
SQL Server
```

The project follows the MVVM pattern to separate:

* UI logic
* Business logic
* Data access

This helps improve:

* Maintainability
* Scalability
* Code readability

---

# ▶️ How to Run

## 1. Clone the project

```bash
git clone https://github.com/yourusername/Warehouse-Management-System.git
```

---

## 2. Open with Visual Studio

Recommended:

* Visual Studio 2022
* .NET Framework 4.8

---

## 3. Configure Database Connection

Open:

```text
App.config
```

Update the connection string:

```xml
data source=YOUR_SERVER_NAME;
initial catalog=QUANLI_KHOHANG;
integrated security=True
```

---

## 4. Restore Packages

```bash
Update-Package -reinstall
```

---

## 5. Run the Application

```text
Ctrl + F5
```

---

# 🎯 Purpose

This project was developed for educational purposes to practice:

* WPF Desktop Development
* MVVM Architecture
* Entity Framework
* SQL Server Database Design
* CRUD Operations
* Warehouse Management Logic

---

# 👤 Author

**Nguyen Dai Tai**
**Le Thi Minh Chien**

Student Project - Warehouse Management System

---

# 📄 License

This project is intended for learning and academic purposes only.
