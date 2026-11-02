# HỆ THỐNG QUẢN LÝ VÀ BÁN HÀNG MỸ PHẨM

> **Đề tài:** Xây dựng hệ thống quản lý và bán hàng mỹ phẩm  
> **Trường:** Đại học Công nghệ Đồng Nai (DNTU)  
> **Ngành:** Công nghệ thông tin  
> **Nhóm thực hiện:** 
> - **Thành viên 1:** Trần Bảo Trâm – Phát triển Backend (ASP.NET Core Web API).
> - **Thành viên 2:** [Họ tên thành viên] – Phát triển Frontend (ASP.NET Core MVC) và Database.
> - **Thành viên 3:** [Họ tên thành viên nếu có].
>
> **Repository:** https://github.com/tranbaotram2005/HeThongQuanLyVaBanHang_Comestic.git

---

## 📌 GIỚI THIỆU DỰ ÁN

Hệ thống quản lý và bán hàng mỹ phẩm là ứng dụng web hỗ trợ hoạt động kinh doanh mỹ phẩm trực tuyến, đồng thời cung cấp các chức năng quản lý dành cho quản trị viên.

Hệ thống được xây dựng theo mô hình tách biệt giữa giao diện người dùng và tầng xử lý nghiệp vụ thông qua Web API.

### 🌟 Định hướng chức năng

**Đối với khách hàng:**
1. Đăng ký, đăng nhập và quản lý tài khoản.
2. Xem danh sách và thông tin chi tiết sản phẩm.
3. Tìm kiếm và lọc sản phẩm theo danh mục.
4. Quản lý giỏ hàng.
5. Đặt hàng và theo dõi đơn hàng.
6. Quản lý thông tin cá nhân.

**Đối với quản trị viên (Admin):**
1. Quản lý sản phẩm.
2. Quản lý danh mục mỹ phẩm.
3. Quản lý tài khoản khách hàng.
4. Quản lý đơn hàng.
5. Quản lý thông tin và trạng thái đơn hàng.
6. Thống kê doanh thu và tình hình kinh doanh.

*Lưu ý: Các chức năng trên là định hướng ban đầu, sẽ được điều chỉnh theo phân tích và thiết kế hệ thống của nhóm.*

---

## 🛠️ CÔNG NGHỆ SỬ DỤNG

| Thành phần | Công nghệ |
|---|---|
| Backend | ASP.NET Core Web API |
| Frontend | ASP.NET Core MVC |
| Ngôn ngữ lập trình | C# |
| Database | SQL Server |
| ORM | Entity Framework Core |
| IDE | Visual Studio 2022 |
| Quản lý mã nguồn | Git & GitHub |

---

## 🏗️ KIẾN TRÚC HỆ THỐNG

Dự án được chia thành hai project chính:

**1. HeThongBanHang_Comestic_API**
- Xử lý nghiệp vụ hệ thống.
- Cung cấp các RESTful API.
- Tiếp nhận và xử lý request từ MVC.
- Thực hiện các thao tác dữ liệu thông qua Entity Framework Core.
- Xử lý xác thực và phân quyền người dùng.

**2. HeThongBanHang_Comestic_UI**
- Xây dựng giao diện website bằng ASP.NET Core MVC.
- Hiển thị giao diện dành cho khách hàng.
- Xây dựng khu vực quản trị Admin.
- Gửi HTTP Request đến Web API để lấy và xử lý dữ liệu.

### 📂 CẤU TRÚC THƯ MỤC DỰ ÁN

```text
HeThongQuanLyVaBanHang_Comestic/
│
├── HeThongBanHang_Comestic_API/
│   ├── Controllers/
│   ├── Services/
│   ├── Interfaces/
│   ├── Models/
│   ├── DTOs/
│   ├── Data/
│   ├── Profiles/
│   └── Program.cs
│
├── HeThongBanHang_Comestic_UI/
│   ├── Areas/
│   ├── Controllers/
│   ├── Views/
│   ├── Models/
│   ├── wwwroot/
│   └── Program.cs
│
├── HeThongQuanLyVaBanHang_Comestic.sln
├── .gitignore
└── README.md
```

*Cấu trúc bên trong hai project sẽ được bổ sung trong quá trình phát triển.*

---

# 📥 HƯỚNG DẪN LÀM VIỆC VỚI GITHUB

Đây là phần hướng dẫn dành cho tất cả thành viên tham gia phát triển dự án.

## 1. Chuẩn bị môi trường

Trước khi tải project về máy, cần cài đặt:

- **Git:** Công cụ quản lý mã nguồn.
  - Download: https://git-scm.com/downloads
- **Visual Studio 2022:** IDE dùng để mở và phát triển dự án.
  - Download: https://visualstudio.microsoft.com/
- **.NET SDK:** Phiên bản tương thích với project.
  - Download: https://dotnet.microsoft.com/download

Sau khi cài Git, mở PowerShell và kiểm tra:

```powershell
git --version
```

Nếu hiển thị phiên bản Git thì đã cài đặt thành công.

## 2. Chấp nhận lời mời tham gia Repository

Repository được thiết lập ở chế độ Private, vì vậy thành viên cần được chủ repository cấp quyền truy cập.

Các bước thực hiện:

1. Đăng nhập tài khoản GitHub cá nhân.
2. Kiểm tra thông báo lời mời cộng tác từ repository.
3. Nhấn **Accept invitation** để chấp nhận.
4. Truy cập repository:

https://github.com/tranbaotram2005/HeThongQuanLyVaBanHang_Comestic

Sau khi chấp nhận lời mời, thành viên có thể clone và làm việc với source code.

## 3. Clone project về máy tính

**Clone** là thao tác tải toàn bộ source code từ GitHub về máy tính để bắt đầu phát triển.

### Bước 1: Mở PowerShell

Mở PowerShell tại thư mục muốn lưu project.

Ví dụ:

```powershell
cd D:\Projects
```

### Bước 2: Clone repository

Chạy lệnh:

```powershell
git clone https://github.com/tranbaotram2005/HeThongQuanLyVaBanHang_Comestic.git
```

Git sẽ tải source code từ GitHub về máy.

### Bước 3: Di chuyển vào thư mục project

```powershell
cd HeThongQuanLyVaBanHang_Comestic
```

### Bước 4: Mở project bằng Visual Studio

Có thể mở trực tiếp file Solution:

```powershell
start HeThongQuanLyVaBanHang_Comestic.sln
```

Hoặc mở Visual Studio 2022, chọn **Open a project or solution**, sau đó chọn file:

`HeThongQuanLyVaBanHang_Comestic.sln`

Sau khi mở thành công, Solution Explorer sẽ hiển thị hai project API và MVC.

## 4. Kiểm tra branch hiện tại

Tất cả thành viên sử dụng chung branch `main`.

Kiểm tra branch hiện tại:

```powershell
git branch
```

Nếu kết quả hiển thị:

```text
* main
```

Nghĩa là bạn đang làm việc trên branch chính.

Nếu chưa ở branch `main`, chuyển sang bằng lệnh:

```powershell
git checkout main
```

## 5. Pull code mới nhất trước khi bắt đầu làm việc

<mark>QUAN TRỌNG: LUÔN PULL CODE MỚI NHẤT TRƯỚC KHI CODE.</mark>

Vì tất cả thành viên cùng làm việc trên branch `main`, có thể một thành viên khác đã cập nhật source code lên GitHub trước đó.

Để lấy phiên bản mới nhất về máy, chạy:

```powershell
git pull origin main
```

Lệnh này sẽ đồng bộ các commit mới nhất từ GitHub về máy tính.

**Ví dụ:**

- Thành viên A vừa cập nhật chức năng đăng nhập.
- Thành viên B muốn tiếp tục phát triển chức năng sản phẩm.
- Thành viên B cần chạy `git pull origin main` trước khi bắt đầu làm việc để nhận được source code mới nhất.

Không nên code trên source cũ vì có thể dẫn đến xung đột khi push.

## 6. Commit và Push code lên GitHub

Sau khi hoàn thành một chức năng hoặc một phần công việc, thành viên cần lưu thay đổi lên GitHub.

### Bước 1: Kiểm tra các file đã thay đổi

```powershell
git status
```

Lệnh này giúp kiểm tra những file mới, file đã chỉnh sửa hoặc file bị xóa.

### Bước 2: Đưa các thay đổi vào vùng chuẩn bị commit

```powershell
git add .
```

### Bước 3: Commit code

```powershell
git commit -m "Mô tả nội dung công việc"
```

Ví dụ:

```powershell
git commit -m "Add product management API"
```

Một số cách đặt tên commit:

```text
Add account login API
Add product management UI
Update product controller
Fix order validation
Update database models
Add customer management
```

### Bước 4: Push code lên GitHub

```powershell
git push origin main
```

Sau khi push thành công, những thay đổi đã commit sẽ được cập nhật lên repository GitHub tại branch `main`.

### Tóm tắt quy trình Commit và Push

```powershell
git status
git add .
git commit -m "Mô tả công việc"
git push origin main
```

## 7. Quy trình làm việc hằng ngày

Tất cả thành viên cần tuân thủ quy trình sau:

**KHI BẮT ĐẦU LÀM VIỆC:**

```powershell
git pull origin main
```

Sau đó mới mở Visual Studio và bắt đầu code.

**SAU KHI HOÀN THÀNH CÔNG VIỆC:**

```powershell
git status
git add .
git commit -m "Mô tả công việc"
git push origin main
```

**KHI KẾT THÚC BUỔI LÀM VIỆC:**

Kiểm tra lại trạng thái Git:

```powershell
git status
```

Đảm bảo những thay đổi cần lưu đã được commit và push lên GitHub.

## 8. Xử lý một số lỗi Git thường gặp

### Lỗi 1: Push bị từ chối (rejected)

Ví dụ:

```text
! [rejected] main -> main (fetch first)
```

**Nguyên nhân:** Có thành viên khác đã push code mới lên GitHub trước bạn.

**Cách xử lý:**

```powershell
git pull --rebase origin main
git push origin main
```

Nếu có xung đột, cần xử lý các file bị conflict trước khi tiếp tục.

### Lỗi 2: Merge Conflict

**Nguyên nhân:** Hai thành viên cùng chỉnh sửa những phần code bị trùng nhau, Git không thể tự động kết hợp thay đổi.

**Cách xử lý:**

1. Mở file đang bị conflict trong Visual Studio.
2. Tìm các đoạn được Git đánh dấu:

```text
<<<<<<< HEAD
Code hiện tại
=======
Code từ thay đổi khác
>>>>>>> commit
```

3. Kiểm tra và chọn hoặc kết hợp phần code phù hợp.
4. Xóa các dấu đánh dấu conflict.
5. Lưu file và kiểm tra lại code.
6. Nếu đang dùng `git pull --rebase`, chạy:

```powershell
git add .
git rebase --continue
```

7. Sau khi hoàn tất, push lại:

```powershell
git push origin main
```

Nếu chưa hiểu cách xử lý conflict, hãy trao đổi với thành viên đang chỉnh sửa file đó trước khi giải quyết.

### Lỗi 3: Chưa cấu hình tên và email Git

Nếu Git thông báo chưa có thông tin người commit, chạy:

```powershell
git config --global user.name "Tên GitHub"
git config --global user.email "Email GitHub"
```

Thay bằng tên và email của tài khoản GitHub cá nhân.

### Lỗi 4: Không có quyền push repository

**Nguyên nhân:** Chưa được cấp quyền cộng tác hoặc chưa chấp nhận lời mời tham gia repository.

**Cách xử lý:**

- Kiểm tra lời mời cộng tác trên GitHub.
- Đảm bảo đã nhấn Accept invitation.
- Kiểm tra lại tài khoản GitHub đang đăng nhập.

## 9. QUY TẮC LÀM VIỆC NHÓM

Để tránh mất dữ liệu và hạn chế xung đột code, tất cả thành viên cần tuân thủ:

1. Tất cả thành viên làm việc trên branch `main`.
2. Luôn chạy `git pull origin main` trước khi bắt đầu code.
3. Không push code khi chưa kiểm tra và chạy thử phần mình vừa chỉnh sửa.
4. Commit sau khi hoàn thành một chức năng hoặc một phần công việc.
5. Đặt tên commit rõ ràng, thể hiện đúng nội dung thay đổi.
6. Không tự ý xóa hoặc sửa code thuộc phần phụ trách của thành viên khác.
7. Khi cần chỉnh sửa file dùng chung, nên trao đổi trước với các thành viên liên quan.
8. Không commit các thư mục build như `bin/`, `obj/`, `.vs/`.
9. Không đưa mật khẩu, connection string chứa thông tin nhạy cảm hoặc khóa bí mật lên GitHub.
10. Nếu xảy ra conflict, trao đổi và thống nhất cách xử lý trước khi ghi đè code của người khác.

---

## 👥 PHÂN CÔNG CÔNG VIỆC

| Thành viên | Nhiệm vụ |
|---|---|
| Trần Bảo Trâm | Backend – ASP.NET Core Web API |
| [Thành viên 2] | Frontend – ASP.NET Core MVC, Database |
| [Thành viên 3] | [Bổ sung nhiệm vụ] |

---

## 🔗 THÔNG TIN REPOSITORY

GitHub Repository:

https://github.com/tranbaotram2005/HeThongQuanLyVaBanHang_Comestic.git

**Lưu ý:** Đây là repository Private. Chỉ những tài khoản đã được cấp quyền mới có thể truy cập và làm việc với source code.

---

**Dự án được thực hiện bởi nhóm sinh viên – Khoa Công nghệ Thông tin, Đại học Công nghệ Đồng Nai.**
