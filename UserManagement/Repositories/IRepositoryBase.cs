﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace UserManagement.Data
{
    public interface IRepositoryBase<T>
    {
        IQueryable<T> FindAll();

        IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression);


    }
}
