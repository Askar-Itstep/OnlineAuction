using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Repository
{
    interface IRepository<T> where T: class
    {
        T GetById(int? id);
        IQueryable<T> GetAll();    //
        void Create(T item);
        void Update(T item);
        void Save();
        IQueryable<T> Include(params string[] navigationProperty);
        void Delete(int? item);
    }
}
