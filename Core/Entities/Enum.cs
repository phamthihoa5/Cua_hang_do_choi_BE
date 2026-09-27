using System;

namespace Core.Entities
{
    public class Enum
    {
        public enum Gender
        {
            Male, Female, Other
        }

        public enum TrangThaiSanPham
        {
            ConHang, HetHang
        }

        public enum TrangThaiDonHang
        {
            Chờ_xử_lý,
            Đã_xác_nhận,
            Đang_giao_hàng,
            Đã_giao_hàng,
            Đã_hủy
        }

        public enum TrangThaiNhanVien
        {
            DangLam,
            Nghi
        }

        public enum TrangThaiKhoHang
        {
            ConHang,
            HetHang
        }

        public enum TrangThaiThanhToan
        {
            Chưa_thanh_toán,
            Thanh_toán_thất_bại,
            Đã_thanh_toán
        }

        public enum StaffType
        {
            None = 0,       // Mặc định (chưa phân loại)
            Sales = 1,      // Nhân viên bán hàng
            Warehouse = 2   // Nhân viên kho
        }
    }
}
