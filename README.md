# 📚 Library.API – .NET 8 Back-End for Scalable Book Management

## 🏗️ โครงสร้างโปรเจกต์
Library.API/
├── Asset/ExcelTemplate # Excel Template สำหรับ Export/Import
├── Common/ # Helper, Constants, และ Utilities ต่าง ๆ
├── Data
    ├── Seeder # การสร้างข้อมูลจำลอง (Seeder)
    └── Migrations/ # EF Core Migration Files
├── DTOs/ # Data Transfer Objects
├── Endpoints/ # Minimal API Entry Points
├── Features/
│ ├── Authors/ # Command/Query + Handler ของ Authors
│ └── Books/ # Command/Query + Handler ของ Books
├── Models/ # Entity Models


---

## 🧠 แนวคิดการพัฒนา Back-End

### ✅ Minimal API (.NET 8)

- ใช้ **Minimal API** แทน Controller เพื่อความกระชับ 
- ลด boilerplate code ทำให้โค้ดสะอาด อ่านง่าย และทดสอบง่าย

### ✅ CQRS + MediatR

- แยกคำสั่ง (Command) และการอ่านข้อมูล (Query)
- ทำงานร่วมกับ `MediatR` เพื่อกระจาย Request ไปยัง Handler ที่เหมาะสม
- ส่งเสริมการแยกความรับผิดชอบ และสามารถ Unit Test ในภายหลังได้ง่าย

### ✅ Clean Architecture

- โครงสร้างโค้ดแยกเป็นชั้น ๆ เช่น Features, DTOs, Models
- แต่ละ Feature มีโฟลเดอร์ของตัวเอง (เช่น Books, Authors)
- เพิ่มความสามารถในการ maintain และ scale ระบบในอนาคต

---

## ⚙️ Performance Optimization

### ⚡ Data Seeder (Mock Data Generator)

- ใช้ **Bogus** สำหรับจำลองข้อมูลในรูปแบบ realistic
- ใช้ **EFCore.BulkExtensions** เพื่อทำ Bulk Insert แบบเร็วมาก
- รัน Seeder ได้จากในโปรเจกต์โดยตรง ไม่ต้องพึ่ง SQL Script

### ⚡ LINQ Optimization

- ใช้ `.AsNoTracking()` กับ Query ที่ไม่ต้องการ Tracking Entity เพื่อลด Overhead
- ใช้ `.Skip()` และ `.Take()` สำหรับแบ่งหน้า (Pagination) เพื่อประสิทธิภาพในการดึงข้อมูล
- ใช้ `EF.Functions.Like()` แทน `StartsWith()` ให้ EF สร้าง SQL ที่มีประสิทธิภาพกว่า

🧠 การใช้ `IQueryable<T>` เพื่อเพิ่มประสิทธิภาพ

โปรเจกต์นี้ใช้ `IQueryable<Book>` สร้าง Query ที่ประมวลผลบนฐานข้อมูลโดยตรง (Server-side) ลดภาระ .NET Server และเพิ่มความเร็ว โดยเฉพาะกับข้อมูลจำนวนมาก

ข้อดีของ `IQueryable<T>` กับ EF Core:
- Deferred Execution: Query จะรันเมื่อเรียก `.ToListAsync()` หรือ `.FirstOrDefaultAsync()`
- รวม Query ทั้งหมดเป็น SQL Statement เดียว
- รองรับการทำ `Where`, `OrderBy`, `Include`, `Skip`, `Take` แบบ Dynamic บนฐานข้อมูล
- เหมาะกับระบบที่ต้องการ Paging, Filtering, Sorting อย่างรวดเร็ว

---

## 🔌 API Design Concept

- รองรับการเชื่อมต่อกับ Front-End เช่น AG-Grid (Server-Side)
- API ถูกออกแบบให้:
  - กรอง (Filter) ได้จาก Server
  - แบ่งหน้า (Paging) และเรียงลำดับ (Sorting) ได้แบบยืดหยุ่น

---

## 🛠️ Tech Stack

| หมวดหมู่        | เทคโนโลยี                         |
|------------------|------------------------------------|
| Language         | C# (.NET 8)                        |
| API              | Minimal API                        |
| Architecture     | CQRS + Clean Architecture          |
| Mediation        | MediatR                            |
| ORM              | Entity Framework Core              |
| Mock Data        | Bogus                              |
| Bulk Insert      | EFCore.BulkExtensions              |
| API Test         | Swagger / Postman                  |

---

## 📌 จุดเด่นของระบบ

- API ออกแบบมาเพื่อรองรับข้อมูลขนาดใหญ่
- Clean Code ตามแนวทาง Uncle Bob
- CQRS และ MediatR ช่วยแยก logic ชัดเจน
- สามารถต่อยอดระบบเพื่อ scale ได้ทันที
- ลด Boilerplate code ด้วย Minimal API
- มี Seeder สำหรับสร้างข้อมูลจำลองจำนวนมากอย่างรวดเร็ว

---



