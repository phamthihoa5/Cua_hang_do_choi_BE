using Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.Repo.Common
{
    // Lớp BaseRepo là một lớp generic để xử lý các thao tác cơ bản (CRUD) cho mọi entity
    public class BaseRepo<TEntity> where TEntity : class
    {
        // Context dùng để truy cập cơ sở dữ liệu thông qua Entity Framework
        protected readonly DatabaseContext Context;

        // Constructor nhận context và gán vào biến thành viên
        protected BaseRepo(DatabaseContext context) =>
            Context = context;


        // Thêm một thực thể vào DbContext
        public virtual async Task Add(TEntity obj)
        {
            await Context.AddAsync(obj);
            await Context.SaveChangesAsync();
        }

        // ✅ Thêm nhiều thực thể cùng lúc
        public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            await Context.Set<TEntity>().AddRangeAsync(entities);
        }



        // Lấy toàn bộ thực thể dưới dạng IQueryable (có thể truy vấn thêm như filter, sort, paging)
        public virtual IQueryable<TEntity> GetAll() => Context.Set<TEntity>();



        // Xóa một thực thể đã có (theo object)
        public virtual Task Delete(TEntity obj) =>
            Task.FromResult(Context.Set<TEntity>().Remove(obj));

        // Xóa thực thể theo ID (dạng Guid) - kiểm tra tồn tại trước khi xóa
        public virtual async Task Delete(Guid id)
        {
            var obj = await Context.Set<TEntity>().FindAsync(id);
            if (obj == null)
            {
                // Nếu không tìm thấy, ném ngoại lệ
                throw new ResourceNotFoundException("Resource Not Found");
            }
            // Nếu tìm thấy, xóa thực thể
            Context.Set<TEntity>().Remove(obj);
        }

        // ✅ Xóa nhiều thực thể cùng lúc
        public virtual Task DeleteRangeAsync(IEnumerable<TEntity> entities)
        {
            Context.Set<TEntity>().RemoveRange(entities);
            return Task.CompletedTask;
        }


        // Đánh dấu một thực thể là đã được chỉnh sửa, EF sẽ cập nhật khi gọi SaveChanges
        public virtual async Task Update(TEntity obj)
        {
            Context.Entry(obj).State = EntityState.Modified;
            await Context.SaveChangesAsync();
        }


        // ✅ Lưu thay đổi (khi dùng AddRange/DeleteRange)
        public virtual async Task<int> SaveChangesAsync()
        {
            return await Context.SaveChangesAsync();
        }

        // Giải phóng tài nguyên DbContext khi repository không còn sử dụng
        public void Dispose() => Context.Dispose();
    }
}
