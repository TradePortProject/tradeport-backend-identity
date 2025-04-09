using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace UserManagement.Data
{
    public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        protected AppDbContext AppDbContext { get; set; }

        public RepositoryBase(AppDbContext _AppDbContext)
        {
            AppDbContext = _AppDbContext;
        }

        public IQueryable<T> FindAll()
        {
            return AppDbContext.Set<T>().AsNoTracking();
        }

        public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression)
        {
            return AppDbContext.Set<T>().Where(expression).AsNoTracking();
        }
    }
}
